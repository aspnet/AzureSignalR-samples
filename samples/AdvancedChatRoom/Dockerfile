FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ./. ./
RUN dotnet restore "AdvancedChatRoom.csproj"
RUN dotnet build "AdvancedChatRoom.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdvancedChatRoom.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvancedChatRoom.dll", "--urls", "http://0.0.0.0:80"]