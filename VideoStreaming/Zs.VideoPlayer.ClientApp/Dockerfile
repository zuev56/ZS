# cd git/ZS/VideoStreaming/Zs.VideoPlayer.ClientApp
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-video-player-client-app -f Dockerfile .
# sudo docker run -e HOSTNAME=$(hostname) --name video-player-client-app -p 6002:80 -it zs-video-player-client-app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VideoStreaming/Zs.VideoPlayer.ClientApp/Zs.VideoPlayer.ClientApp.csproj", "VideoStreaming/Zs.VideoPlayer.ClientApp/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
RUN dotnet restore "VideoStreaming/Zs.VideoPlayer.ClientApp/Zs.VideoPlayer.ClientApp.csproj"
COPY . .
WORKDIR "/src/VideoStreaming/Zs.VideoPlayer.ClientApp"
RUN dotnet build "Zs.VideoPlayer.ClientApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.VideoPlayer.ClientApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.VideoPlayer.ClientApp.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]
