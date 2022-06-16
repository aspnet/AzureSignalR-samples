FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ./. ./
RUN dotnet restore "ChatRoom.csproj"
RUN dotnet build "ChatRoom.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatRoom.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatRoom.dll", "--urls", "http://0.0.0.0:80"]