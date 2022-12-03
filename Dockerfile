FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish \ 
    --configuration release \
    --use-current-runtime \
    --self-contained \
    /p:PublishSingleFile=true \
    ./sysinternals.installer/sysinternals.installer.csproj

RUN ls /src/sysinternals.installer/bin/release/net7.0/

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

WORKDIR /bin/sysinternals.installer
COPY --from=build /src/sysinternals.installer/bin/release/net7.0/linux-x64/publish .

ENTRYPOINT [ "./sysinternals.installer"]