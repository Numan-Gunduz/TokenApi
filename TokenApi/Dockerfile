# Linux tabanlý bir imaj kullanýyoruz
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build aþamasý
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Proje dosyasýný kopyala ve restore yap
COPY ["TokenApi.csproj", "./"]
RUN dotnet restore "./TokenApi.csproj"

# Tüm kaynak kodlarýný kopyala ve build yap
COPY . .
WORKDIR "/src"
RUN dotnet build "TokenApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish aþamasý
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TokenApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final aþama - çalýþtýrýlabilir hale getirme
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TokenApi.dll"]
