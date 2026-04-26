# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

COPY DevOpsExample.slnx ./
COPY src/DevOpsService.Api/DevOpsService.Api.csproj ./src/DevOpsService.Api/
COPY tests/DevOpsService.UnitTests/DevOpsService.UnitTests.csproj ./tests/DevOpsService.UnitTests/
COPY tests/DevOpsService.IntegrationTests/DevOpsService.IntegrationTests.csproj ./tests/DevOpsService.IntegrationTests/

RUN dotnet restore

# Copy the rest of the source
COPY . ./

# Run tests
RUN dotnet test --no-restore --configuration Release

# Publish the API project
RUN dotnet publish src/DevOpsService.Api/DevOpsService.Api.csproj \
    --no-restore \
    --configuration Release \
    --output /App/out


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /App

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
USER appuser

COPY --from=build /App/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DevOpsService.Api.dll"]