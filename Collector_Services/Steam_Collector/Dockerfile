#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Steam_Collector/Steam_Collector.csproj", "Steam_Collector/"]
RUN dotnet restore "Steam_Collector/Steam_Collector.csproj"
COPY . .
WORKDIR "/src/Steam_Collector"
RUN dotnet build "Steam_Collector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Steam_Collector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Steam_Collector.dll"]