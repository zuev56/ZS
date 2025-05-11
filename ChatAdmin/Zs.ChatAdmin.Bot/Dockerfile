# cd git/ZS/ChatAdmin/Zs.ChatAdmin.Bot
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-chat-admin-bot -f Dockerfile .
# sudo docker run --name zima-leto-admin-bot -p 6031:80 --restart unless-stopped -v /opt/configs:/opt/configs/:ro -it zs-chat-admin-bot

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH --self-contained true -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app .
ADD /Data /app/Data
ENTRYPOINT ["dotnet", "ChatAdmin.Bot.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]
