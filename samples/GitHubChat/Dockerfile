FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/GitHubChat
EXPOSE 5000/tcp

COPY ./. ./
RUN dotnet restore && dotnet build
RUN echo 'dotnet user-secrets set Azure:SignalR:ConnectionString $SIGNALR_CONN_STRING; \
          dotnet user-secrets set GitHubClientId $GITHUB_CLIENT_ID; \
          dotnet user-secrets set GitHubClientSecret $GITHUB_CLIENT_SECRET; \
          dotnet run' > run.sh
ENTRYPOINT ["bash", "./run.sh"]