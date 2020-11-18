#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["clients/DJClient/DJClient.csproj", "clients/DJClient/"]
COPY ["Risk.Game/Risk.Game.csproj", "Risk.Game/"]
COPY ["Risk.Shared/Risk.Shared.csproj", "Risk.Shared/"]
COPY ["Risk.Api/Risk.Api.csproj", "Risk.Api/"]
RUN dotnet restore "clients/DJClient/DJClient.csproj"
COPY . .
WORKDIR "/src/clients/DJClient"
RUN dotnet build "DJClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DJClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DJClient.dll"]
