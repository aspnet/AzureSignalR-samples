FROM microsoft/dotnet:2.1-sdk-alpine

ADD samples/FullSample /home/FullSample/

ARG SDKVersion

ENV ConnectionString ""

EXPOSE 5000

RUN cd /home/FullSample/ && dotnet build

WORKDIR /home/FullSample/

CMD dotnet user-secrets set Azure:SignalR:ConnectionString ${ConnectionString} \
    && dotnet run --no-build
