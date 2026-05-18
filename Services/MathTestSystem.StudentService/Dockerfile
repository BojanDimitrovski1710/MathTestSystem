FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for all referenced projects so restore can resolve dependencies
COPY ["Services/MathTestSystem.StudentService/MathTestSystem.StudentService.csproj", "Services/MathTestSystem.StudentService/"]
COPY ["BuildingBlocks/MathTestSystem.Domain/MathTestSystem.Domain.csproj", "BuildingBlocks/MathTestSystem.Domain/"]
COPY ["BuildingBlocks/MathTestSystem.Infrastructure/MathTestSystem.Infrastructure.csproj", "BuildingBlocks/MathTestSystem.Infrastructure/"]

RUN dotnet restore "Services/MathTestSystem.StudentService/MathTestSystem.StudentService.csproj"

COPY . .

WORKDIR "/src/Services/MathTestSystem.StudentService"
RUN dotnet build "MathTestSystem.StudentService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MathTestSystem.StudentService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MathTestSystem.StudentService.dll"]
