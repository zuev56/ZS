﻿# cd git/ZS
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-home-bot -f Home/Zs.Home.Bot/Dockerfile .
# sudo docker run -e HOSTNAME=$(hostname) --name home-bot -p 6022:80 --restart unless-stopped -v /opt/configs:/opt/configs/:ro -it zs-home-bot

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Home/Zs.Home.Bot/Zs.Home.Bot.csproj", "Home/Zs.Home.Bot/"]
COPY ["Bot/Zs.Bot.Data.PostgreSQL/Zs.Bot.Data.PostgreSQL.csproj", "Bot/Zs.Bot.Data.PostgreSQL/"]
COPY ["Bot/Zs.Bot.Data/Zs.Bot.Data.csproj", "Bot/Zs.Bot.Data/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
COPY ["Bot/Zs.Bot.Telegram/Zs.Bot.Telegram.csproj", "Bot/Zs.Bot.Telegram/"]
COPY ["Bot/Zs.Bot.Services/Zs.Bot.Services.csproj", "Bot/Zs.Bot.Services/"]
COPY ["Common/Zs.Common.Services/Zs.Common.Services.csproj", "Common/Zs.Common.Services/"]
COPY ["Common/Zs.Common.Data.Postgres/Zs.Common.Data.Postgres.csproj", "Common/Zs.Common.Data.Postgres/"]
COPY ["Home/Zs.Home.Application/Zs.Home.Application.csproj", "Home/Zs.Home.Application/"]
COPY ["Parsers/Zs.Parser.EspMeteo/Zs.Parser.EspMeteo.csproj", "Parsers/Zs.Parser.EspMeteo/"]
COPY ["Vk/Zs.VkActivity.WebApi.Client/Zs.VkActivity.WebApi.Client.csproj", "Vk/Zs.VkActivity.WebApi.Client/"]


RUN dotnet restore "Home/Zs.Home.Bot/Zs.Home.Bot.csproj"
COPY . .
WORKDIR "/src/Home/Zs.Home.Bot"
RUN dotnet build "Zs.Home.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.Home.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.Home.Bot.dll"]
 # command-line args (global configuration path)
CMD ["/opt/configs/"]

# install iputils-ping, lm-sensors
USER root
RUN apt update && apt install -y iputils-ping lm-sensors && rm -rf /var/lib/apt/lists/*
