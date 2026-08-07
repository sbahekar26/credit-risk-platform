FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CreditRisk.Web/CreditRisk.Web.csproj CreditRisk.Web/
COPY CreditRisk.Core/CreditRisk.Core.csproj CreditRisk.Core/
RUN dotnet restore CreditRisk.Web/CreditRisk.Web.csproj

COPY . .
RUN dotnet publish CreditRisk.Web/CreditRisk.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CreditRisk.Web.dll"]