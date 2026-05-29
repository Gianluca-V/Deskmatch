FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY nuget.config ./

COPY shared/building-blocks/DeskMatch.BuildingBlocks.csproj shared/building-blocks/
RUN dotnet restore shared/building-blocks/DeskMatch.BuildingBlocks.csproj

COPY apps/api-gateway/ApiGateway.csproj apps/api-gateway/
RUN dotnet restore apps/api-gateway/ApiGateway.csproj

COPY shared/building-blocks/ shared/building-blocks/
COPY apps/api-gateway/ apps/api-gateway/

WORKDIR /src/apps/api-gateway
RUN dotnet publish -c Release -o /app/publish
RUN cp src/Api/appsettings.json /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "DeskMatch.ApiGateway.dll"]