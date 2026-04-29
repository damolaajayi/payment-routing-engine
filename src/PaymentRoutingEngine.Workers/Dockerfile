FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PaymentRoutingEngine.sln", "./"]

COPY ["src/PaymentRoutingEngine.Workers/PaymentRoutingEngine.Workers.csproj", "src/PaymentRoutingEngine.Workers/"]
COPY ["src/PaymentRoutingEngine.Application/PaymentRoutingEngine.Application.csproj", "src/PaymentRoutingEngine.Application/"]
COPY ["src/PaymentRoutingEngine.Domain/PaymentRoutingEngine.Domain.csproj", "src/PaymentRoutingEngine.Domain/"]
COPY ["src/PaymentRoutingEngine.Infrastructure/PaymentRoutingEngine.Infrastructure.csproj", "src/PaymentRoutingEngine.Infrastructure/"]

RUN dotnet restore "src/PaymentRoutingEngine.Workers/PaymentRoutingEngine.Workers.csproj"

COPY . .

RUN dotnet publish "src/PaymentRoutingEngine.Workers/PaymentRoutingEngine.Workers.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentRoutingEngine.Workers.dll"]