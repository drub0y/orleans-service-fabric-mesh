FROM microsoft/dotnet:2.1-runtime-nanoserver-1709 AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SiloHost/SiloHost.csproj SiloHost/
RUN dotnet restore SiloHost/SiloHost.csproj
COPY . .
WORKDIR /src/SiloHost
RUN dotnet build SiloHost.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish SiloHost.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SiloHost.dll"]