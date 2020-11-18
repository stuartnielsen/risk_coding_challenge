#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WyattClient/WyattClient.csproj", "WyattClient/"]
COPY ["Risk.Shared/Risk.Shared.csproj", "Risk.Shared/"]
RUN dotnet restore "WyattClient/WyattClient.csproj"
COPY . .
WORKDIR "/src/WyattClient"
RUN dotnet build "WyattClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WyattClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WyattClient.dll"]
