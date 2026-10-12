#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build from the repo root: docker build -f Integration_Services/DiscordBot/Dockerfile .

FROM mcr.microsoft.com/dotnet/runtime:11.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY ["Integration_Services/DiscordBot/DiscordBot.csproj", "Integration_Services/DiscordBot/"]
COPY ["Data_Services/UncoreMetrics.Data/UncoreMetrics.Data.csproj", "Data_Services/UncoreMetrics.Data/"]
RUN dotnet restore "Integration_Services/DiscordBot/DiscordBot.csproj"
COPY . .
WORKDIR "/src/Integration_Services/DiscordBot"

FROM build AS publish
RUN dotnet publish "DiscordBot.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
