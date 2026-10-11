#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build from the repo root: docker build -f Web_Services/API/Dockerfile .

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Web_Services/API/API.csproj", "Web_Services/API/"]
COPY ["Data_Services/UncoreMetrics.Data/UncoreMetrics.Data.csproj", "Data_Services/UncoreMetrics.Data/"]
RUN dotnet restore "Web_Services/API/API.csproj"
COPY . .
WORKDIR "/src/Web_Services/API"

FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./API"]
