# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["RepoLens.Api/RepoLens.Api.csproj", "RepoLens.Api/"]
COPY ["RepoLens.Core/RepoLens.Core.csproj", "RepoLens.Core/"]
COPY ["RepoLens.Infrastructure/RepoLens.Infrastructure.csproj", "RepoLens.Infrastructure/"]

RUN dotnet restore "RepoLens.Api/RepoLens.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/RepoLens.Api"

RUN dotnet build "RepoLens.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RepoLens.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install additional tools for code analysis
RUN apt-get update && apt-get install -y \
    git \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create directories
RUN mkdir -p /app/logs /app/repositories

# Copy published application
COPY --from=publish /app/publish .

# Set permissions
RUN chown -R app:app /app
USER app

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "RepoLens.Api.dll"]
