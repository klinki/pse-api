echo "Build script started"

dotnet restore "PseApi/PseApi.csproj"
dotnet build "PseApi/PseApi.csproj" -c Release
dotnet publish "PseApi/PseApi.csproj" -c Release

dotnet tool restore
dotnet swagger tofile --output PseApi.IntegrationTests/swagger.json PseApi/bin/Release/netcoreapp3.1/PseApi.dll v1
dotnet restore "PseApi.IntegrationTests/PseApi.IntegrationTests.fsproj"
dotnet build "PseApi.IntegrationTests/PseApi.IntegrationTests.fsproj"
dotnet test "PseApi.IntegrationTests/PseApi.IntegrationTests.fsproj"
