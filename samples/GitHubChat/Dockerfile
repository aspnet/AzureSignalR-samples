FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/GitHubChat

COPY ./. ./
RUN dotnet restore && dotnet build

ENTRYPOINT ["bash","-c", "dotnet run --GitHubClientId $GITHUB_CLIENT_ID --GitHubClientSecret $GITHUB_CLIENT_SECRET --urls http://0.0.0.0:80"]