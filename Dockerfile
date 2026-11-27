FROM mcr.microsoft.com/dotnet/aspnet:11.0-alpine AS base
WORKDIR /app
# .NET 8+ images listen on 8080 by default; keep port 80 used by the deployment
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
EXPOSE 443
# Required for Time Zone database lookups
RUN apk add --no-cache tzdata

FROM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build
WORKDIR /src
COPY ["PseApi/PseApi.csproj", "PseApi/"]
RUN dotnet restore "PseApi/PseApi.csproj"
COPY . .
WORKDIR "/src/PseApi"
ARG SHORT_COMMIT
# -- VERSION clashes with other VERSION variable name
ARG VERZION
RUN dotnet build "PseApi.csproj" -p:SourceRevisionId=${SHORT_COMMIT} -p:MSBuildGitHashValue=${SHORT_COMMIT} -c Release --no-restore -o /app

FROM build AS publish
ARG SHORT_COMMIT
# -- VERSION clashes with other VERSION variable name
ARG VERZION
RUN dotnet publish "PseApi.csproj" -p:SourceRevisionId=${SHORT_COMMIT} -p:MSBuildGitHashValue=${SHORT_COMMIT} -c Release -o /app

FROM publish AS test
WORKDIR "/src"
RUN dotnet restore "PseApi.IntegrationTests/PseApi.IntegrationTests.fsproj"
RUN dotnet tool restore
RUN dotnet swagger tofile --output PseApi.IntegrationTests/swagger.json /app/PseApi.dll v1
WORKDIR "/src/PseApi.IntegrationTests"
RUN dotnet test --project PseApi.IntegrationTests.fsproj --results-directory /src/PseApi.IntegrationTests --report-xunit-junit --report-xunit-junit-filename test-result.xml

FROM scratch AS export-test-results
COPY --from=test /src/PseApi.IntegrationTests/*.xml .

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PseApi.dll"]
