#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Ping_Collector_Probe/Ping_Collector_Probe.csproj", "Ping_Collector_Probe/"]
RUN dotnet restore "Ping_Collector_Probe/Ping_Collector_Probe.csproj"
COPY . .
WORKDIR "/src/Ping_Collector_Probe"
RUN dotnet build "Ping_Collector_Probe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ping_Collector_Probe.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ping_Collector_Probe.dll"]