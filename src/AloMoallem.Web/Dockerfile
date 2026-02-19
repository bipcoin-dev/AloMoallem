# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore AloMoallem.Web.csproj
RUN dotnet publish AloMoallem.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

COPY --from=build /app/publish ./
EXPOSE 8080

ENTRYPOINT ["dotnet", "AloMoallem.Web.dll"]
