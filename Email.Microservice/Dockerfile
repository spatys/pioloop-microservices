FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Email.API/Email.API.csproj", "src/Email.API/"]
COPY ["src/Email.Application/Email.Application.csproj", "src/Email.Application/"]
COPY ["src/Email.Domain/Email.Domain.csproj", "src/Email.Domain/"]
COPY ["src/Email.Infrastructure/Email.Infrastructure.csproj", "src/Email.Infrastructure/"]
RUN dotnet restore "src/Email.API/Email.API.csproj"
COPY . .
WORKDIR "/src/src/Email.API"
RUN dotnet build "Email.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Email.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Email.API.dll"]
