FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/AdvancedChatRoom

COPY ./. ./
RUN dotnet restore && dotnet build
ENTRYPOINT ["bash", "-c", "dotnet run --ConnectionStrings:AzureStorage $STORAGE_CONN_STRING --urls http://0.0.0.0:80"]