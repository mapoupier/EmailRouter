FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MAPEK.EmailRouter/MAPEK.EmailRouter.csproj", "MAPEK.EmailRouter/"]
RUN dotnet restore "MAPEK.EmailRouter/MAPEK.EmailRouter.csproj"
COPY . .
WORKDIR "/src/MAPEK.EmailRouter"
RUN dotnet build "MAPEK.EmailRouter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MAPEK.EmailRouter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MAPEK.EmailRouter.dll"]
