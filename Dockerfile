# Utiliser l'image .NET Core Runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Utiliser l'image SDK .NET pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

# Restaurer les dépendances
RUN dotnet restore "SLAnimationAPI.sln"

# Construire l'application
RUN dotnet build "SLAnimationAPI.sln" -c Release -o /app/build

# Publier l'application
FROM build AS publish
RUN dotnet publish "SLAnimationAPI.sln" -c Release -o /app/publish

# Étape finale : exécuter l'application publiée
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SLAnimationAPI.dll"]

