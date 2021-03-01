FROM mcr.microsoft.com/dotnet/sdk:5.0.103-buster-slim AS build-env

# Copy csproj and restore as distinct layers
COPY src/Pseudonym.Crypto.Invictus.Funds/Pseudonym.Crypto.Invictus.Funds.csproj ./Pseudonym.Crypto.Invictus.Funds/Pseudonym.Crypto.Invictus.Funds.csproj
COPY src/Pseudonym.Crypto.Invictus.Shared/Pseudonym.Crypto.Invictus.Shared.csproj ./Pseudonym.Crypto.Invictus.Shared/Pseudonym.Crypto.Invictus.Shared.csproj
RUN dotnet restore Pseudonym.Crypto.Invictus.Funds/Pseudonym.Crypto.Invictus.Funds.csproj

# Copy everything else and build
COPY StyleCop.ruleset ./StyleCop.ruleset
COPY NuGet.config ./NuGet.config
COPY src/Pseudonym.Crypto.Invictus.Funds/. ./src/Pseudonym.Crypto.Invictus.Funds/
COPY src/Pseudonym.Crypto.Invictus.Shared/. ./src/Pseudonym.Crypto.Invictus.Shared/
RUN dotnet publish ./src/Pseudonym.Crypto.Invictus.Funds/Pseudonym.Crypto.Invictus.Funds.csproj -c Release -o app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Pseudonym.Crypto.Invictus.Funds.dll"]