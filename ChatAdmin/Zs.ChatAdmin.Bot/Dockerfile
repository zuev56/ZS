
# docker build -t zs-chat-admin-bot -f .\ChatAdmin\Zs.ChatAdmin.Bot\Dockerfile .
# docker build buildx --platform linux/amd64,linux/arm64 --build-arg SDKARCH=arm64v8 - produces amd64 and arm64v8 images
# docker buildx build --platform linux/arm64 -t zs-chat-admin-bot -f Dockerfile .

# Enable using docker build --platform switch (easily)
# https://github.com/dotnet/dotnet-docker/issues/4388

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG TARGETARCH
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH --self-contained true -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app .
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["./dotnetapp"]
