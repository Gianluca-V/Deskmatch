FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]

COPY ["shared/domain/DeskMatch.Domain.csproj", "shared/domain/"]
COPY ["shared/building-blocks/DeskMatch.BuildingBlocks.csproj", "shared/building-blocks/"]
COPY ["shared/sdk/src/OpenSearch/DeskMatch.SDK.OpenSearch.csproj", "shared/sdk/src/OpenSearch/"]
COPY ["shared/sdk/src/Redis/DeskMatch.SDK.Redis.csproj", "shared/sdk/src/Redis/"]
COPY ["shared/sdk/src/Notification/DeskMatch.SDK.Notification.csproj", "shared/sdk/src/Notification/"]

COPY ["shared/domain/", "shared/domain/"]
COPY ["shared/building-blocks/", "shared/building-blocks/"]
COPY ["shared/sdk/", "shared/sdk/"]

COPY ["apps/core-service/CoreService.csproj", "apps/core-service/"]
RUN dotnet restore "apps/core-service/CoreService.csproj"

COPY ["apps/core-service/", "apps/core-service/"]
RUN dotnet publish "apps/core-service/CoreService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DeskMatch.CoreService.dll"]