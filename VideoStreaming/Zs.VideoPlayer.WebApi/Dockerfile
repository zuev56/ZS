# cd git/ZS/VideoStreaming/Zs.VideoPlayer.WebApi
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-video-player-api -f Dockerfile .
# sudo docker run -e HOSTNAME=$(hostname) --name video-player-api -p 6001:80 -v /opt/configs:/opt/configs/:ro -v /opt/videos:/opt/videos/:ro -it zs-video-player-api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VideoStreaming/Zs.VideoPlayer.WebApi/Zs.VideoPlayer.WebApi.csproj", "VideoStreaming/Zs.VideoPlayer.WebApi/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
RUN dotnet restore "VideoStreaming/Zs.VideoPlayer.WebApi/Zs.VideoPlayer.WebApi.csproj"
COPY . .
WORKDIR "/src/VideoStreaming/Zs.VideoPlayer.WebApi"
RUN dotnet build "Zs.VideoPlayer.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.VideoPlayer.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.VideoPlayer.WebApi.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]
