FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["PseApi/PseApi.csproj", "PseApi/"]
RUN dotnet restore "PseApi/PseApi.csproj"
COPY . .
WORKDIR "/src/PseApi"
RUN dotnet build "PseApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PseApi.csproj" -c Release -o /app

FROM publish AS test
WORKDIR "/src"
RUN dotnet restore "PseApi.IntegrationTests/PseApi.IntegrationTests.fsproj"
RUN dotnet tool restore
RUN dotnet swagger tofile --output PseApi.IntegrationTests/swagger.json /app/PseApi.dll v1
WORKDIR "/src/PseApi.IntegrationTests"
RUN dotnet test

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PseApi.dll"]
