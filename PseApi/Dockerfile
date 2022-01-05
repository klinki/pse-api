FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
# Required for Time Zone database lookups
RUN apk add --no-cache tzdata

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["PseApi/PseApi.csproj", "PseApi/"]
RUN dotnet restore "PseApi/PseApi.csproj"
COPY . .
WORKDIR "/src/PseApi"
ARG SHORT_COMMIT
# -- VERSION clashes with other VERSION variable name
ARG VERZION
RUN dotnet build "PseApi.csproj" -c Release -o /app

FROM build AS publish
ARG SHORT_COMMIT
# -- VERSION clashes with other VERSION variable name
ARG VERZION
RUN dotnet publish "PseApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PseApi.dll"]