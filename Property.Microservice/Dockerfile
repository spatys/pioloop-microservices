# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Property.API/Property.API.csproj", "src/Property.API/"]
COPY ["src/Property.Application/Property.Application.csproj", "src/Property.Application/"]
COPY ["src/Property.Domain/Property.Domain.csproj", "src/Property.Domain/"]
COPY ["src/Property.Infrastructure/Property.Infrastructure.csproj", "src/Property.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Property.API/Property.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/Property.API"

# Build the application
RUN dotnet build "Property.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Property.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
LABEL maintainer="Pioloop Team"
LABEL description="Property Microservice for Pioloop - Property Management"
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
  CMD curl -f http://localhost/api/health || exit 1

ENTRYPOINT ["dotnet", "Property.API.dll"]
