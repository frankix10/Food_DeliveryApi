# Stage 1: Build the application
FROM ://microsoft.com AS build-env
WORKDIR /app

COPY *.slnx ./
COPY E_CommerceApi/*.csproj ./E_CommerceApi/
RUN dotnet restore E_CommerceProject.slnx
COPY . ./
RUN dotnet publish E_CommerceProject.slnx -c Release -o out

# Stage 2: Runtime environment
FROM ://microsoft.com
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "E_CommerceApi.dll"]
EOF
