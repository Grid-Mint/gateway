# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Спочатку тільки файли проєктів — шар кешується, поки вони не змінились
COPY Gateway.slnx ./
COPY src/Gateway/Gateway.csproj src/Gateway/
RUN dotnet restore src/Gateway/Gateway.csproj

COPY src/ src/
RUN dotnet publish src/Gateway/Gateway.csproj -c Release -o /app --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=builder /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

USER app

ENTRYPOINT ["dotnet", "Gateway.dll"]
