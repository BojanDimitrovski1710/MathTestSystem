FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for all referenced projects so restore can resolve dependencies
COPY ["MathTestSystem.StudentService/MathTestSystem.StudentService.csproj", "MathTestSystem.StudentService/"]
COPY ["MathTestSystem.Domain/MathTestSystem.Domain.csproj", "MathTestSystem.Domain/"]
COPY ["MathTestSystem.Infrastructure/MathTestSystem.Infrastructure.csproj", "MathTestSystem.Infrastructure/"]

RUN dotnet restore "MathTestSystem.StudentService/MathTestSystem.StudentService.csproj"

COPY . .

WORKDIR "/src/MathTestSystem.StudentService"
RUN dotnet build "MathTestSystem.StudentService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MathTestSystem.StudentService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MathTestSystem.StudentService.dll"]
