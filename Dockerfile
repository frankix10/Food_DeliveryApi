# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy solution and project files first (for better layer caching)
COPY *.slnx ./
COPY E_CommerceApi/*.csproj ./E_CommerceApi/
RUN dotnet restore E_CommerceProject.slnx

# Copy everything else and publish
COPY . ./
RUN dotnet publish E_CommerceProject.slnx -c Release -o out

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Create non-root user for security (Render best practice)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build-env /app/out .

# Render sets PORT env var automatically - use it if available, fallback to 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "E_CommerceApi.dll"]
