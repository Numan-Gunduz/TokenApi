# Linux tabanl� bir imaj kullan�yoruz
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build a�amas�
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Proje dosyas�n� kopyala ve restore yap
COPY ["TokenApi.csproj", "./"]
RUN dotnet restore "./TokenApi.csproj"

# T�m kaynak kodlar�n� kopyala ve build yap
COPY . .
WORKDIR "/src"
RUN dotnet build "TokenApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish a�amas�
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TokenApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final a�ama - �al��t�r�labilir hale getirme
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TokenApi.dll"]
