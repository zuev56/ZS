# cd git/ZS
# git pull
# sudo docker buildx build --platform linux/arm64 -t home.web-api -f Home/Zs.Home.WebApi/Dockerfile .
# sudo docker run -e HOSTNAME=$(hostname) --name home-web-api --restart unless-stopped -p 6024:80 \
#    -v /opt/configs:/opt/configs:ro \
#    -v /proc:/host/proc:ro \
#    -v /sys:/host/sys:ro \
#    -v /dev:/host/dev:ro \
#    -v /run:/host/run:ro \
#    -v /usr/sbin/smartctl:/usr/sbin/smartctl:ro \
#    --pid host --privileged --user 0:0 -it home.web-api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Home/Zs.Home.WebApi/Zs.Home.WebApi.csproj", "Home/Zs.Home.WebApi/"]
COPY ["Home/Zs.Home.Application/Zs.Home.Application.csproj", "Home/Zs.Home.Application/"]
COPY ["Common/Zs.Common.Services/Zs.Common.Services.csproj", "Common/Zs.Common.Services/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
COPY ["Parsers/Zs.Parser.EspMeteo/Zs.Parser.EspMeteo.csproj", "Parsers/Zs.Parser.EspMeteo/"]
COPY ["Vk/Zs.VkActivity.WebApi.Client/Zs.VkActivity.WebApi.Client.csproj", "Vk/Zs.VkActivity.WebApi.Client/"]
RUN dotnet restore "Home/Zs.Home.WebApi/Zs.Home.WebApi.csproj"
COPY . .
WORKDIR "/src/Home/Zs.Home.WebApi"
RUN dotnet build "Zs.Home.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.Home.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.Home.WebApi.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]
