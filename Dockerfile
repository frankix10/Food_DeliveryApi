# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY *.slnx ./
COPY E_CommerceApi/*.csproj ./E_CommerceApi/
RUN dotnet restore E_CommerceProject.slnx
COPY . ./
RUN dotnet publish E_CommerceProject.slnx -c Release -o out

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Create non-root user using useradd (works on Alpine and Debian-based images)
RUN useradd -m -s /bin/bash appuser && chown -R appuser /app
USER appuser

COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet", "E_CommerceApi.dll"]
