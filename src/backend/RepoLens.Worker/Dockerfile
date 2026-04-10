# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["RepoLens.Worker/RepoLens.Worker.csproj", "RepoLens.Worker/"]
COPY ["RepoLens.Core/RepoLens.Core.csproj", "RepoLens.Core/"]
COPY ["RepoLens.Infrastructure/RepoLens.Infrastructure.csproj", "RepoLens.Infrastructure/"]

RUN dotnet restore "RepoLens.Worker/RepoLens.Worker.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/RepoLens.Worker"

RUN dotnet build "RepoLens.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RepoLens.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install additional tools for background processing and code analysis
RUN apt-get update && apt-get install -y \
    git \
    curl \
    python3 \
    python3-pip \
    nodejs \
    npm \
    && rm -rf /var/lib/apt/lists/*

# Create directories for worker operations
RUN mkdir -p /app/logs /app/repositories /app/temp

# Copy published application
COPY --from=publish /app/publish .

# Set permissions
RUN chown -R app:app /app
USER app

# Health check for worker service
HEALTHCHECK --interval=60s --timeout=30s --start-period=10s --retries=3 \
    CMD ps aux | grep -q '[R]epoLens.Worker' || exit 1

ENTRYPOINT ["dotnet", "RepoLens.Worker.dll"]
