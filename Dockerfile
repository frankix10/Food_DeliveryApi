# Stage 1: Build the application
FROM ://Microsoft.com AS build-env
WORKDIR /app

# Copy the new solution file and the project folder structure
COPY *.slnx ./
COPY E_CommerceApi/*.csproj ./E_CommerceApi/

# Restore dependencies using the .slnx solution
RUN dotnet restore E_CommerceProject.slnx

# Copy the rest of the source code files
COPY . ./

# Build and publish the release output
RUN dotnet publish E_CommerceProject.slnx -c Release -o out

# Stage 2: Runtime environment
FROM ://microsoft.com
WORKDIR /app
COPY --from=build-env /app/out .

# Render runtime port configuration
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E_CommerceApi.dll"]
