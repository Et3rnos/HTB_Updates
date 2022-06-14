FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build

WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine as runtime

RUN apk add --no-cache icu-libs

#You might want to change next params for "culture-specific data and behavior"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_GB.UTF-8 \
    LANG=en_GB.UTF-8

WORKDIR /app
COPY --from=build /app/published-app /app
ENTRYPOINT [ "dotnet", "/app/HTB Updates Discord Bot.dll" ]
