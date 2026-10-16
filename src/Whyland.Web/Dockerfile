FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["src/Whyland.Domain/Whyland.Domain.csproj", "src/Whyland.Domain/"]
COPY ["src/Whyland.Application/Whyland.Application.csproj", "src/Whyland.Application/"]
COPY ["src/Whyland.Infrastructure/Whyland.Infrastructure.csproj", "src/Whyland.Infrastructure/"]
COPY ["src/Whyland.Web/Whyland.Web.csproj", "src/Whyland.Web/"]
RUN dotnet restore "src/Whyland.Web/Whyland.Web.csproj"

COPY . .

RUN dotnet publish "src/Whyland.Web/Whyland.Web.csproj" --no-restore -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Whyland.Web.dll"]
