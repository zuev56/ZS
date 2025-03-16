# cd git/ZS
# git pull
# sudo docker buildx build --platform linux/arm64 -t zs-home-jobs-hangfire -f Home/Zs.Home.Jobs.Hangfire/Dockerfile .
# sudo docker run --name home-jobs-hangfire --restart unless-stopped -p 6023:80 -it zs-home-jobs-hangfire

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Home/Zs.Home.Jobs.Hangfire/Zs.Home.Jobs.Hangfire.csproj", "Home/Zs.Home.Jobs.Hangfire/"]
COPY ["Home/Zs.Home.Application/Zs.Home.Application.csproj", "Home/Zs.Home.Application/"]
COPY ["Common/Zs.Common.Services/Zs.Common.Services.csproj", "Common/Zs.Common.Services/"]
COPY ["Common/Zs.Common/Zs.Common.csproj", "Common/Zs.Common/"]
COPY ["Parsers/Zs.Parser.EspMeteo/Zs.Parser.EspMeteo.csproj", "Parsers/Zs.Parser.EspMeteo/"]
RUN dotnet restore "Home/Zs.Home.Jobs.Hangfire/Zs.Home.Jobs.Hangfire.csproj"
COPY . .
WORKDIR "/src/Home/Zs.Home.Jobs.Hangfire"
RUN dotnet build "Zs.Home.Jobs.Hangfire.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.Home.Jobs.Hangfire.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zs.Home.Jobs.Hangfire.dll"]
