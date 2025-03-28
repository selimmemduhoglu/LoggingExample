FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LoggingExample.Web/LoggingExample.Web.csproj", "LoggingExample.Web/"]
RUN dotnet restore "LoggingExample.Web/LoggingExample.Web.csproj"
COPY . .
WORKDIR "/src/LoggingExample.Web"
RUN dotnet build "LoggingExample.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoggingExample.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoggingExample.Web.dll"]