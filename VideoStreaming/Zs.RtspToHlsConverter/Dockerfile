# cd git/ZS/VideoStreaming/Zs.RtspToHlsConverter
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-rtsp-to-hls-converter -f Dockerfile .
# docker run --name rtsp-to-hls-converter --mount type=bind,source=/var/lib/videos,target=/var/lib/videos -it zs-rtsp-to-hls-converter

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH --self-contained true -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Zs.RtspToHlsConverter.dll"]

# install ffmpeg and other software
RUN apt update && apt install -y ffmpeg mc && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /var/lib/videos
