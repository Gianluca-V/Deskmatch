FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]

COPY ["apps/auth-service/AuthService.csproj", "apps/auth-service/"]
COPY ["shared/domain/DeskMatch.Domain.csproj", "shared/domain/"]
COPY ["shared/building-blocks/DeskMatch.BuildingBlocks.csproj", "shared/building-blocks/"]
COPY ["shared/sdk/src/Notification/DeskMatch.SDK.Notification.csproj", "shared/sdk/src/Notification/"]

RUN dotnet restore "apps/auth-service/AuthService.csproj"

COPY . .

WORKDIR /src/apps/auth-service
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DeskMatch.AuthService.dll"]