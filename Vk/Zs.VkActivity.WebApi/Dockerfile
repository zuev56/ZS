# cd git/ZS/Vk/Zs.VkActivity.WebApi
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-vk-activity-api -f Dockerfile .
# sudo docker run --name vk-activity-api -p 6011:80 -it zs-vk-activity-api

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
ENTRYPOINT ["dotnet", "Zs.VkActivity.WebApi.dll"]
