FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.12 as build

WORKDIR /app

COPY . .

RUN dotnet publish -c Release -r linux-musl-x64

# FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine

WORKDIR /app

COPY --from=build /app/bin/Release/net5.0/* /app

RUN apk update && apk add libstdc++ libgcc tzdata --no-cache

CMD ["dotnet", "automate-sender.dll"]