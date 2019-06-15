FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Sample/SimpleConsole/SimpleConsole.csproj", "Sample/SimpleConsole/"]
COPY ["Module/Encryption/AES/AES.csproj", "Module/Encryption/AES/"]
COPY ["Base/Base.csproj", "Base/"]
COPY ["Module/Data/LiteDB/LiteDB.csproj", "Module/Data/LiteDB/"]
RUN dotnet restore "Sample/SimpleConsole/SimpleConsole.csproj"
COPY . .
WORKDIR "/src/Sample/SimpleConsole"
RUN dotnet build "SimpleConsole.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleConsole.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleConsole.dll"]