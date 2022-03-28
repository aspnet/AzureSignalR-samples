FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/ChatRoomLocal
EXPOSE 5000/tcp

COPY ./. ./
RUN dotnet restore && dotnet build
RUN echo 'dotnet run' > run.sh
ENTRYPOINT ["bash", "./run.sh"]
