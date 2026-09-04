# ──────────────────────────────────────────────────────────────────
# BUILD STAGE
# ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY *.sln .
COPY src/ src/
RUN dotnet restore

# Build & publish
RUN dotnet publish src/MedClinic.API/MedClinic.API.csproj \
    -c Release -o /app/publish --no-restore

# ──────────────────────────────────────────────────────────────────
# RUNTIME STAGE
# ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app/publish .

# Uploads directory
RUN mkdir -p uploads && chown appuser:appuser uploads

USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "MedClinic.API.dll"]
