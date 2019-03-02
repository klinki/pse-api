FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PseApi/PseApi.csproj", "PseApi/"]
RUN dotnet restore "PseApi/PseApi.csproj"
COPY . .
WORKDIR "/src/PseApi"
RUN dotnet build "PseApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PseApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PseApi.dll"]