# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/Ship.Ses.Extractor.Worker/Ship.Ses.Extractor.Worker.csproj", "src/Ship.Ses.Extractor.Worker/"]
COPY ["src/Ship.Ses.Extractor.Application/Ship.Ses.Extractor.Application.csproj", "src/Ship.Ses.Extractor.Application/"]
COPY ["src/Ship.Ses.Extractor.Domain/Ship.Ses.Extractor.Domain.csproj", "src/Ship.Ses.Extractor.Domain/"]
COPY ["src/Ship.Ses.Extractor.Infrastructure/Ship.Ses.Extractor.Infrastructure.csproj", "src/Ship.Ses.Extractor.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Ship.Ses.Extractor.Worker/Ship.Ses.Extractor.Worker.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/Ship.Ses.Extractor.Worker"
RUN dotnet publish "Ship.Ses.Extractor.Worker.csproj" -c Release -o /app/publish

# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Optional: Create a non-root user
# RUN useradd -m appuser && chown -R appuser /app
# USER appuser

COPY --from=build /app/publish .

# Ensure runtime uses Production environment
ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ship.Ses.Extractor.Worker.dll"]
