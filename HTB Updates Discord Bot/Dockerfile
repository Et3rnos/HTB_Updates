#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["HTB Updates Discord Bot/HTB Updates Discord Bot.csproj", "HTB Updates Discord Bot/"]
COPY ["HTB Updates Shared Resources/HTB Updates Shared Resources.csproj", "HTB Updates Shared Resources/"]
RUN dotnet restore "HTB Updates Discord Bot/HTB Updates Discord Bot.csproj"
COPY . .
WORKDIR "/src/HTB Updates Discord Bot"
RUN dotnet build "HTB Updates Discord Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HTB Updates Discord Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HTB Updates Discord Bot.dll"]