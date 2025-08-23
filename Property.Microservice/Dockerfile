FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Property.API/Property.API.csproj", "src/Property.API/"]
COPY ["src/Property.Application/Property.Application.csproj", "src/Property.Application/"]
COPY ["src/Property.Domain/Property.Domain.csproj", "src/Property.Domain/"]
COPY ["src/Property.Infrastructure/Property.Infrastructure.csproj", "src/Property.Infrastructure/"]
RUN dotnet restore "src/Property.API/Property.API.csproj"
COPY . .
WORKDIR "/src/src/Property.API"
RUN dotnet build "Property.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Property.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Property.API.dll"]
