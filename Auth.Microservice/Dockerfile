# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Auth.API/Auth.API.csproj", "src/Auth.API/"]
COPY ["src/Auth.Application/Auth.Application.csproj", "src/Auth.Application/"]
COPY ["src/Auth.Domain/Auth.Domain.csproj", "src/Auth.Domain/"]
COPY ["src/Auth.Infrastructure/Auth.Infrastructure.csproj", "src/Auth.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Auth.API/Auth.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/Auth.API"

# Build the application
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
LABEL maintainer="Pioloop Team"
LABEL description="Auth Microservice for Pioloop - User Authentication and Management"
LABEL version="1.0"

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure environment
ENV ASPNETCORE_URLS=http://0.0.0.0:80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "Auth.API.dll"]
