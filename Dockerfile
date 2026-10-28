# CSCMaster (ASP.NET Core 10) for Linux: two images from one Dockerfile.
#   docker build --target api -t cscmaster-api .
#   docker build --target ui  -t cscmaster-ui  .
# (docker compose builds both; GitHub Actions publishes them to ghcr.io)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so package downloads are cached between builds.
COPY dotnet-tools.json ./
COPY CSCMasterAPI/CSCMasterAPI.csproj CSCMasterAPI/
COPY CSCMasterUI/CSCMasterUI.csproj CSCMasterUI/
COPY tools/CSCMasterDataCopy/CSCMasterDataCopy.csproj tools/CSCMasterDataCopy/
RUN dotnet tool restore \
 && dotnet restore CSCMasterAPI/CSCMasterAPI.csproj \
 && dotnet restore CSCMasterUI/CSCMasterUI.csproj \
 && dotnet restore tools/CSCMasterDataCopy/CSCMasterDataCopy.csproj

COPY CSCMasterAPI/ CSCMasterAPI/
COPY CSCMasterUI/ CSCMasterUI/
COPY tools/CSCMasterDataCopy/ tools/CSCMasterDataCopy/
RUN dotnet publish CSCMasterAPI/CSCMasterAPI.csproj -c Release -o /out/api --no-restore \
 && dotnet publish CSCMasterUI/CSCMasterUI.csproj -c Release -o /out/ui --no-restore \
 && dotnet publish tools/CSCMasterDataCopy/CSCMasterDataCopy.csproj -c Release -o /out/api/datacopy --no-restore
# EF Core migration bundle (used by deploy/update.sh). It contains the SQL Server and the PostgreSQL migrations
# and applies the set that matches DatabaseProvider at run time.
RUN dotnet tool run dotnet-ef migrations bundle --project CSCMasterAPI --configuration Release --output /out/api/efbundle

# ---- API ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
# tzdata: TZ sets the container time zone (log timestamps only).
# libgssapi-krb5-2: loaded by Microsoft.Data.SqlClient (data copy tool); avoids a startup warning.
RUN apt-get update \
 && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends tzdata libgssapi-krb5-2 \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /out/api .
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "CSCMasterAPI.dll"]

# ---- UI ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS ui
RUN apt-get update \
 && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends tzdata \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /out/ui .
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "CSCMasterUI.dll"]
