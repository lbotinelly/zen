FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Sample/SimpleWebApp/SimpleWebApp.csproj", "Sample/SimpleWebApp/"]
COPY ["Base/Base.csproj", "Base/"]
COPY ["Module/Web/Web.csproj", "Module/Web/"]
COPY ["Module/Data/LiteDB/LiteDB.csproj", "Module/Data/LiteDB/"]
RUN dotnet restore "Sample/SimpleWebApp/SimpleWebApp.csproj"
COPY . .
WORKDIR "/src/Sample/SimpleWebApp"
RUN dotnet build "SimpleWebApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleWebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleWebApp.dll"]
