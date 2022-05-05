FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/ChatRoom

COPY ./. ./
RUN dotnet restore && dotnet build
ENTRYPOINT ["bash", "-c", "dotnet run --urls http://0.0.0.0:80"]