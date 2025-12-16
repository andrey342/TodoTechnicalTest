# See https://aka.ms/customizecontainer to learn how to customize your container and how Visual Studio uses this Dockerfile to build images for faster debugging.

# This stage is used when running from VS in fast mode (default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Install curl for debugging or health checks
RUN apt-get update && apt-get install -y curl

# Set the user for the application (provided by environment variable)
USER $APP_UID
WORKDIR /app

# Expose application ports
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency restore
COPY ["src/ApiGateways/ApiGateway.AG/ApiGateway.AG.csproj", "src/ApiGateways/ApiGateway.AG/"]
COPY ["src/Shared/Contracts/Contracts.csproj", "src/Shared/Contracts/"]
COPY ["src/Shared/EventBus/EventBus.csproj", "src/Shared/EventBus/"]
# Restore NuGet packages
RUN dotnet restore "./src/ApiGateways/ApiGateway.AG/ApiGateway.AG.csproj"

# Copy all source code
COPY . .

# Set working directory to the API project
WORKDIR "/src/src/ApiGateways/ApiGateway.AG"

# Build the project
RUN dotnet build "./ApiGateway.AG.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project, which will be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish the application to the /app/publish directory
RUN dotnet publish "./ApiGateway.AG.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in normal mode (default when not using Debug configuration)
FROM base AS final
WORKDIR /app

# Copy published output from the publish stage
COPY --from=publish /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "ApiGateway.AG.dll"]