FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/AdvancedChatRoom
EXPOSE 5000/tcp

COPY ./. ./
RUN dotnet restore && dotnet build
RUN echo 'dotnet user-secrets set Azure:SignalR:ConnectionString $SIGNALR_CONN_STRING; \
          dotnet user-secrets set ConnectionStrings:AzureStorage $STORAGE_CONN_STRING; \
          dotnet run' > run.sh
ENTRYPOINT ["bash", "./run.sh"]
