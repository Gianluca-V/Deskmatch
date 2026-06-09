FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["shared/domain/DeskMatch.Domain.csproj", "shared/domain/"]
COPY ["shared/building-blocks/DeskMatch.BuildingBlocks.csproj", "shared/building-blocks/"]
COPY ["shared/sdk/src/OpenSearch/DeskMatch.SDK.OpenSearch.csproj", "shared/sdk/src/OpenSearch/"]

COPY ["apps/search-service/SearchService.csproj", "apps/search-service/"]

RUN dotnet restore "apps/search-service/SearchService.csproj"

COPY ["shared/domain/", "shared/domain/"]
COPY ["shared/building-blocks/", "shared/building-blocks/"]
COPY ["shared/sdk/src/OpenSearch/", "shared/sdk/src/OpenSearch/"]

COPY ["apps/search-service/", "apps/search-service/"]

WORKDIR "/src/apps/search-service"
RUN dotnet build "SearchService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SearchService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS="http://+:5004"
ENTRYPOINT ["dotnet", "DeskMatch.SearchService.dll"]