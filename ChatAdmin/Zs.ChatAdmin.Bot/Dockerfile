FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Zs.ChatAdmin.Bot.csproj", "ChatAdmin/Zs.ChatAdmin.Bot/"]
RUN dotnet restore "ChatAdmin/Zs.ChatAdmin.Bot/Zs.ChatAdmin.Bot.csproj"
COPY . .
WORKDIR "/src/ChatAdmin/Zs.ChatAdmin.Bot"
RUN dotnet build "Zs.ChatAdmin.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zs.ChatAdmin.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "ChatAdmin.Bot.dll"]
