#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ARK_Collector/ARK_Collector.csproj", "ARK_Collector/"]
RUN dotnet restore "ARK_Collector/ARK_Collector.csproj"
COPY . .
WORKDIR "/src/ARK_Collector"
RUN dotnet build "ARK_Collector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ARK_Collector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ARK_Collector.dll"]