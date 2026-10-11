#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build from the repo root: docker build -f Collector_Services/Ping_Collector/Ping_Collector_Probe/Dockerfile .

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Collector_Services/Ping_Collector/Ping_Collector_Probe/Ping_Collector_Probe.csproj", "Collector_Services/Ping_Collector/Ping_Collector_Probe/"]
COPY ["Data_Services/UncoreMetrics.Data/UncoreMetrics.Data.csproj", "Data_Services/UncoreMetrics.Data/"]
RUN dotnet restore "Collector_Services/Ping_Collector/Ping_Collector_Probe/Ping_Collector_Probe.csproj"
COPY . .
WORKDIR "/src/Collector_Services/Ping_Collector/Ping_Collector_Probe"

FROM build AS publish
RUN dotnet publish "Ping_Collector_Probe.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ping_Collector_Probe.dll"]
