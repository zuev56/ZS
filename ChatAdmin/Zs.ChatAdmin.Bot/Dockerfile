
# !!! Run from one level up directory !!!
# docker build -t zs-chat-admin-bot -f Dockerfile .


FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
#FROM mcr.microsoft.com/dotnet/runtime:6.0-buster-slim-arm64v8 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#FROM mcr.microsoft.com/dotnet/sdk:6.0-buster AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChatAdmin/Zs.ChatAdmin.Bot/Zs.ChatAdmin.Bot.csproj", "Zs.ChatAdmin.Bot/"]
RUN dotnet restore "Zs.ChatAdmin.Bot/Zs.ChatAdmin.Bot.csproj"
#RUN dotnet restore "Zs.ChatAdmin.Bot/Zs.ChatAdmin.Bot.csproj" -r linux-arm64
COPY . .
WORKDIR "/src/ChatAdmin/Zs.ChatAdmin.Bot"
RUN dotnet build "Zs.ChatAdmin.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.ChatAdmin.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#RUN dotnet publish "Zs.ChatAdmin.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -r linux-arm64 #--self-contained false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "ChatAdmin.Bot.dll"]
