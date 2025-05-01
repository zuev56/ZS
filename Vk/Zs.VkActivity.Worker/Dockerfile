# cd git/ZS (FROM SOLUTION DIRECTORY!!!)
# git pull
# sudo docker buildx build --platform linux/arm64 --no-cache -t zs-vk-activity-worker -f Vk/Zs.VkActivity.Worker/Dockerfile .
# docker run --name vk-activity-worker --restart unless-stopped -it zs-vk-activity-worker

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Vk/Zs.VkActivity.Worker/Zs.VkActivity.Worker.csproj", "Vk/Zs.VkActivity.Worker/"]
COPY ["Common/Zs.Common.Services/Zs.Common.Services.csproj", "Common/Zs.Common.Services/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
COPY ["Vk/Zs.VkActivity.Common/Zs.VkActivity.Common.csproj", "Vk/Zs.VkActivity.Common/"]
COPY ["Vk/Zs.VkActivity.Data/Zs.VkActivity.Data.csproj", "Vk/Zs.VkActivity.Data/"]
RUN dotnet restore "Vk/Zs.VkActivity.Worker/Zs.VkActivity.Worker.csproj"
COPY . .
WORKDIR "/src/Vk/Zs.VkActivity.Worker"
RUN dotnet build "Zs.VkActivity.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.VkActivity.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.VkActivity.Worker.dll"]

# install iputils-ping
USER root
RUN apt update && apt install -y iputils-ping && rm -rf /var/lib/apt/lists/*
