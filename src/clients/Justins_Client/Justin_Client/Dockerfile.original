#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["clients/Justins_Client/Justin_Client/Justin_Client.csproj", "clients/Justins_Client/Justin_Client/"]
RUN dotnet restore "clients/Justins_Client/Justin_Client/Justin_Client.csproj"
COPY . .
WORKDIR "/src/clients/Justins_Client/Justin_Client"
RUN dotnet build "Justin_Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Justin_Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Justin_Client.dll"]