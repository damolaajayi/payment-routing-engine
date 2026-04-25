using Microsoft.Extensions.Configuration;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PaymentRoutingEngine.Infrastructure.Providers.Paystack
{
    public sealed class PaystackProviderClient : IPaymentProviderClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaystackProviderClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public PaymentProvider Provider => PaymentProvider.Paystack;

        public async Task<ProviderPaymentResponse> ProcessPaymentAsync(
            ProviderPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var secretKey = _configuration["Paystack:SecretKey"];

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", secretKey);

                var payload = new
                {
                    amount = request.AmountMinor,
                    email = $"{request.CustomerId ?? "customer"}@example.com",
                    reference = request.Reference,
                    currency = request.Currency
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "transaction/initialize",
                    content,
                    cancellationToken);

                var raw = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ProviderPaymentResponse
                    {
                        IsSuccessful = false,
                        FailureCategory = MapFailure(response.StatusCode),
                        FailureReason = "Paystack API error",
                        ProviderStatusCode = ((int)response.StatusCode).ToString(),
                        RawResponsePayload = raw
                    };
                }

                var parsed = JsonDocument.Parse(raw);

                var success = parsed.RootElement.GetProperty("status").GetBoolean();

                if (!success)
                {
                    return new ProviderPaymentResponse
                    {
                        IsSuccessful = false,
                        FailureCategory = FailureCategory.ProviderPermanent,
                        FailureReason = parsed.RootElement.GetProperty("message").GetString(),
                        RawResponsePayload = raw
                    };
                }

                var data = parsed.RootElement.GetProperty("data");

                return new ProviderPaymentResponse
                {
                    IsSuccessful = true,
                    ProviderReference = data.GetProperty("reference").GetString(),
                    ProviderStatusCode = "initialized",
                    RawResponsePayload = raw,
                    FailureCategory = FailureCategory.None
                };
            }
            catch (TaskCanceledException)
            {
                return new ProviderPaymentResponse
                {
                    IsSuccessful = false,
                    FailureCategory = FailureCategory.Timeout,
                    FailureReason = "Paystack request timed out"
                };
            }
            catch (HttpRequestException)
            {
                return new ProviderPaymentResponse
                {
                    IsSuccessful = false,
                    FailureCategory = FailureCategory.Network,
                    FailureReason = "Network error calling Paystack"
                };
            }
            catch (Exception ex)
            {
                return new ProviderPaymentResponse
                {
                    IsSuccessful = false,
                    FailureCategory = FailureCategory.Unknown,
                    FailureReason = ex.Message
                };
            }
        }

        private static FailureCategory MapFailure(System.Net.HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => FailureCategory.Validation,
                System.Net.HttpStatusCode.Unauthorized => FailureCategory.ProviderPermanent,
                System.Net.HttpStatusCode.Forbidden => FailureCategory.ProviderPermanent,
                System.Net.HttpStatusCode.RequestTimeout => FailureCategory.Timeout,
                System.Net.HttpStatusCode.InternalServerError => FailureCategory.ProviderTransient,
                _ => FailureCategory.ProviderTransient
            };
        }
    }
}
