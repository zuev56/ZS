# cd git/ZS/VideoStreaming/Zs.RtspToHlsConverter
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-rtsp-to-hls-converter -f Dockerfile .
# docker run -e HOSTNAME=$(hostname) --name rtsp-to-hls-converter -v /opt/configs:/opt/configs/:ro -v /opt/videos:/opt/videos/ -it zs-rtsp-to-hls-converter

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VideoStreaming/Zs.RtspToHlsConverter/Zs.RtspToHlsConverter.csproj", "VideoStreaming/Zs.RtspToHlsConverter/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
RUN dotnet restore "VideoStreaming/Zs.RtspToHlsConverter/Zs.RtspToHlsConverter.csproj"
COPY . .
WORKDIR "/src/VideoStreaming/Zs.RtspToHlsConverter"
RUN dotnet build "Zs.RtspToHlsConverter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.RtspToHlsConverter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.RtspToHlsConverter.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]

RUN mkdir /opt/videos
