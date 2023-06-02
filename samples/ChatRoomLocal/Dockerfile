FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ./. ./
RUN dotnet restore "ChatRoomLocal.csproj"
RUN dotnet build "ChatRoomLocal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatRoomLocal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatRoomLocal.dll", "--urls", "http://0.0.0.0:80"]