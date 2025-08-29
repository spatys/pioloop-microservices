# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Email.API/Email.API.csproj", "src/Email.API/"]
COPY ["src/Email.Application/Email.Application.csproj", "src/Email.Application/"]
COPY ["src/Email.Domain/Email.Domain.csproj", "src/Email.Domain/"]
COPY ["src/Email.Infrastructure/Email.Infrastructure.csproj", "src/Email.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Email.API/Email.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/Email.API"

# Build the application
RUN dotnet build "Email.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Email.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
LABEL maintainer="Pioloop Team"
LABEL description="Email Microservice for Pioloop - Email Management"
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

ENTRYPOINT ["dotnet", "Email.API.dll"]
