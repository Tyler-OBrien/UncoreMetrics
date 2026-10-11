#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build from the repo root: docker build -f Collector_Services/Ping_Collector/Ping_Collector/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Collector_Services/Ping_Collector/Ping_Collector/Ping_Collector.csproj", "Collector_Services/Ping_Collector/Ping_Collector/"]
COPY ["Data_Services/UncoreMetrics.Data/UncoreMetrics.Data.csproj", "Data_Services/UncoreMetrics.Data/"]
RUN dotnet restore "Collector_Services/Ping_Collector/Ping_Collector/Ping_Collector.csproj"
COPY . .
WORKDIR "/src/Collector_Services/Ping_Collector/Ping_Collector"

FROM build AS publish
RUN dotnet publish "Ping_Collector.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ping_Collector.dll"]
