FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["shared/domain/DeskMatch.Domain.csproj", "shared/domain/"]
COPY ["shared/building-blocks/DeskMatch.BuildingBlocks.csproj", "shared/building-blocks/"]
COPY ["shared/sdk/src/Notification/DeskMatch.SDK.Notification.csproj", "shared/sdk/src/Notification/"]

COPY ["apps/notification-service/NotificationService.csproj", "apps/notification-service/"]

RUN dotnet restore "apps/notification-service/NotificationService.csproj"

COPY ["shared/domain/", "shared/domain/"]
COPY ["shared/building-blocks/", "shared/building-blocks/"]
COPY ["shared/sdk/src/Notification/", "shared/sdk/src/Notification/"]

COPY ["apps/notification-service/", "apps/notification-service/"]

WORKDIR "/src/apps/notification-service"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS="http://+:5005"
ENTRYPOINT ["dotnet", "DeskMatch.NotificationService.dll"]