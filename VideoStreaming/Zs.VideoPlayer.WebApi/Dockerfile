# cd git/ZS/VideoStreaming/Zs.VideoPlayer.WebApi
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-video-player-api -f Dockerfile .
# sudo docker run --name video-player-api --mount type=bind,source=/var/lib/videos,target=/var/lib/videos -it zs-video-player-api

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
ENTRYPOINT ["dotnet", "Zs.VideoPlayer.WebApi.dll"]
