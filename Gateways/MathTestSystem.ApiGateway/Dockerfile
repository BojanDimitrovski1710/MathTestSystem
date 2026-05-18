FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Gateways/MathTestSystem.ApiGateway/MathTestSystem.ApiGateway.csproj", "Gateways/MathTestSystem.ApiGateway/"]
COPY ["BuildingBlocks/MathTestSystem.Domain/MathTestSystem.Domain.csproj", "BuildingBlocks/MathTestSystem.Domain/"]
COPY ["BuildingBlocks/MathTestSystem.Infrastructure/MathTestSystem.Infrastructure.csproj", "BuildingBlocks/MathTestSystem.Infrastructure/"]

RUN dotnet restore "Gateways/MathTestSystem.ApiGateway/MathTestSystem.ApiGateway.csproj"

COPY . .

WORKDIR "/src/Gateways/MathTestSystem.ApiGateway"
RUN dotnet build "MathTestSystem.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MathTestSystem.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MathTestSystem.ApiGateway.dll"]
