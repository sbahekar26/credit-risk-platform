# Stage 1: build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj files and restore (layer caching)
COPY CreditRisk.Api/CreditRisk.Api.csproj CreditRisk.Api/
COPY CreditRisk.Core/CreditRisk.Core.csproj CreditRisk.Core/
RUN dotnet restore CreditRisk.Api/CreditRisk.Api.csproj

# copy everything and publish
COPY . .
RUN dotnet publish CreditRisk.Api/CreditRisk.Api.csproj -c Release -o /app

# Stage 2: runtime-only image (smaller)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CreditRisk.Api.dll"]