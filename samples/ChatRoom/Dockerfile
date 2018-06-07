# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

FROM microsoft/dotnet:2.1-sdk-stretch AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
RUN mkdir ChatRoom && cd ChatRoom/
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY ./ ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ChatRoom.dll"]
