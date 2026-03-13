# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level files
COPY ["Directory.Packages.props", "./"]

# Copy project files and restore
COPY ["src/CoreCodeCamp.csproj", "src/"]
COPY ["CoreApiFundamentals.ServiceDefaults/CoreApiFundamentals.ServiceDefaults.csproj", "CoreApiFundamentals.ServiceDefaults/"]
RUN dotnet restore "src/CoreCodeCamp.csproj"

# Copy all source code
COPY ["src/", "src/"]
COPY ["CoreApiFundamentals.ServiceDefaults/", "CoreApiFundamentals.ServiceDefaults/"]

# Build the project
WORKDIR /src/src
RUN dotnet build "CoreCodeCamp.csproj" -c Release -o /app/build --verbosity detailed

# Publish stage
FROM build AS publish
RUN dotnet publish "CoreCodeCamp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Azure Container Apps uses PORT environment variable
# Default to 8080 if not set
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CoreCodeCamp.dll"]
