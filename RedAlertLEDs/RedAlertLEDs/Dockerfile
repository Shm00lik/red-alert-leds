FROM node:22-alpine AS web
WORKDIR /web
COPY ["web/package.json", "web/package-lock.json", "./"]
RUN npm ci
COPY web/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RedAlertLEDs/RedAlertLEDs.csproj", "RedAlertLEDs/"]
RUN dotnet restore "RedAlertLEDs/RedAlertLEDs.csproj"
COPY RedAlertLEDs/ RedAlertLEDs/
WORKDIR "/src/RedAlertLEDs"
RUN dotnet publish "./RedAlertLEDs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web /web/dist ./wwwroot
ENV DATA_DIR=/app/data
ENTRYPOINT ["dotnet", "RedAlertLEDs.dll"]
