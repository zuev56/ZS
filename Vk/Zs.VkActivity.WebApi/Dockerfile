# cd git/ZS
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-vk-activity-api -f Vk/Zs.VkActivity.WebApi/Dockerfile .
# sudo docker run --name vk-activity-api --restart unless-stopped -p 6011:80 -it zs-vk-activity-api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Vk/Zs.VkActivity.WebApi/Zs.VkActivity.WebApi.csproj", "Vk/Zs.VkActivity.WebApi/"]
COPY ["Common/Zs.Common.Services/Zs.Common.Services.csproj", "Common/Zs.Common.Services/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
COPY ["Vk/Zs.VkActivity.Common/Zs.VkActivity.Common.csproj", "Vk/Zs.VkActivity.Common/"]
COPY ["Vk/Zs.VkActivity.Data/Zs.VkActivity.Data.csproj", "Vk/Zs.VkActivity.Data/"]
RUN dotnet restore "Vk/Zs.VkActivity.WebApi/Zs.VkActivity.WebApi.csproj"
COPY . .
WORKDIR "/src/Vk/Zs.VkActivity.WebApi"
RUN dotnet build "Zs.VkActivity.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.VkActivity.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.VkActivity.WebApi.dll"]
