FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 20003

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY WebClient/WebClient.csproj WebClient/
RUN dotnet restore WebClient/WebClient.csproj
COPY . .
WORKDIR /src/WebClient
RUN dotnet build WebClient.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish WebClient.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebClient.dll"]