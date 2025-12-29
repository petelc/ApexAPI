# ============================================================
# APEX API - Multi-stage Dockerfile
# Optimized for production deployment
# ============================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Apex.API.slnx", "./"]
COPY ["src/Apex.API.Core/Apex.API.Core.csproj", "src/Apex.API.Core/"]
COPY ["src/Apex.API.UseCases/Apex.API.UseCases.csproj", "src/Apex.API.UseCases/"]
COPY ["src/Apex.API.Infrastructure/Apex.API.Infrastructure.csproj", "src/Apex.API.Infrastructure/"]
COPY ["src/Apex.API.Web/Apex.API.Web.csproj", "src/Apex.API.Web/"]

# Restore dependencies (cached layer if csproj files haven't changed)
RUN dotnet restore "src/Apex.API.Web/Apex.API.Web.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/src/Apex.API.Web"
RUN dotnet build "Apex.API.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Apex.API.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r apex && useradd -r -g apex apex

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R apex:apex /app

# Switch to non-root user
USER apex

# Expose port
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "Apex.API.Web.dll"]
