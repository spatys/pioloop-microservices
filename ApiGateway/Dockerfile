# Multi-stage build pour optimiser la taille de l'image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet
COPY ["ApiGateway.csproj", "./"]
RUN dotnet restore "ApiGateway.csproj"

# Copier le reste du code source
COPY . .
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

# Publier l'application
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

# Image finale
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Créer un utilisateur non-root pour la sécurité
RUN adduser --disabled-password --gecos '' appuser

# Installer curl pour les health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copier l'application publiée
COPY --from=publish /app/publish .

# (Logs fichiers désactivés; les logs passent par la console et Docker)
RUN chown -R appuser:appuser /app

# Changer vers l'utilisateur non-root
USER appuser

# Exposer le port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Labels pour la documentation
LABEL maintainer="Pioloop Team <support@pioloop.com>"
LABEL version="1.0.0"
LABEL description="Pioloop API Gateway - Gateway unifié pour les microservices"

# Point d'entrée
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
