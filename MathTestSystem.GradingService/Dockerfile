FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for all referenced projects so restore can resolve dependencies
COPY ["MathTestSystem.GradingService/MathTestSystem.GradingService.csproj", "MathTestSystem.GradingService/"]
COPY ["MathTestSystem.Domain/MathTestSystem.Domain.csproj", "MathTestSystem.Domain/"]
COPY ["MathTestSystem.Infrastructure/MathTestSystem.Infrastructure.csproj", "MathTestSystem.Infrastructure/"]
COPY ["MathTestSystem.MathProcessor/MathTestSystem.MathProcessor.csproj", "MathTestSystem.MathProcessor/"]

RUN dotnet restore "MathTestSystem.GradingService/MathTestSystem.GradingService.csproj"

COPY . .

WORKDIR "/src/MathTestSystem.GradingService"
RUN dotnet build "MathTestSystem.GradingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MathTestSystem.GradingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MathTestSystem.GradingService.dll"]
