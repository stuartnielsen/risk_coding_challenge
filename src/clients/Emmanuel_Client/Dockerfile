#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["clients/Emmanuel_Client/Emmanuel_Client.csproj", "clients/Emmanuel_Client/"]
COPY ["Risk.Shared/Risk.Shared.csproj", "Risk.Shared/"]
RUN dotnet restore "clients/Emmanuel_Client/Emmanuel_Client.csproj"
COPY . .
WORKDIR "/src/clients/Emmanuel_Client"
RUN dotnet build "Emmanuel_Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Emmanuel_Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Emmanuel_Client.dll"]
