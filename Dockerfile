FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY OfficeDays.sln ./
COPY src/OfficeDays/OfficeDays.csproj src/OfficeDays/
COPY tests/OfficeDays.UnitTests/OfficeDays.UnitTests.csproj tests/OfficeDays.UnitTests/
COPY tests/OfficeDays.IntegrationTests/OfficeDays.IntegrationTests.csproj tests/OfficeDays.IntegrationTests/
RUN dotnet restore src/OfficeDays/OfficeDays.csproj

COPY src/OfficeDays/ src/OfficeDays/
RUN dotnet publish src/OfficeDays/OfficeDays.csproj -c Release --no-restore -o /output

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /app/data && chown -R app:app /app
COPY --from=build --chown=app:app /output/ ./
USER app
ENV ASPNETCORE_HTTP_PORTS=5802 \
    ConnectionStrings__Default="Data Source=/app/data/officedays.db" \
    DataProtection__KeysPath=/app/data/keys
EXPOSE 5802
ENTRYPOINT ["dotnet", "OfficeDays.dll"]
