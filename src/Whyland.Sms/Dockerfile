FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["src/Whyland.Sms/Whyland.Sms.csproj", "src/Whyland.Sms/"]
RUN dotnet restore "src/Whyland.Sms/Whyland.Sms.csproj"

COPY . .

RUN dotnet publish "src/Whyland.Sms/Whyland.Sms.csproj" --no-restore -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Whyland.Sms.dll"]
