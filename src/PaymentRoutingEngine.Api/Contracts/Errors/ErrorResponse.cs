namespace PaymentRoutingEngine.Api.Contracts.Errors
{
    public sealed class ErrorResponse
    {
        public string TraceId { get; }
        public IReadOnlyCollection<ApiError> Errors { get; }

        public ErrorResponse(string traceId, IReadOnlyCollection<ApiError> errors)
        {
            TraceId = traceId;
            Errors = errors;
        }

        public static ErrorResponse Single(string traceId, string code, string message)
            => new(traceId, [new ApiError(code, message)]);
    }
}
