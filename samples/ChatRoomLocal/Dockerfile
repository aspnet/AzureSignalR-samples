FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/ChatRoomLocal

COPY ./. ./
RUN dotnet restore && dotnet build
ENTRYPOINT ["bash", "-c", "dotnet run --Azure:SignalR:ConnectionString $SIGNALR_CONN_STRING --urls http://0.0.0.0:80"]