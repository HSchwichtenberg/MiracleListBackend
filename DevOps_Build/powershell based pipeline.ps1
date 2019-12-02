$dotnetRequiredVersion = "2.2.401"
$env:BUILD_SOURCESDIRECTORY = "H:\ml\MiracleListBackend_TFS"
$env:BUILD_STAGINGDIRECTORY = "t:\build_staging"
cd H:\ml\MiracleListBackend_TFS
$vers = dotnet --version

if ($vers -ne $dotnetRequiredVersion) { throw ".NET SDK $dotnetRequiredVersion nicht vrefügbar!" }
"verwende .NET SDK $dotnetRequiredVersion"

cd $env:BUILD_SOURCESDIRECTORY
dotnet restore

dotnet build 

$tests = "$env:BUILD_SOURCESDIRECTORY\Test\UnitTests\UnitTests.csproj"
$tests
dotnet.exe test $tests

#[command]C:\AzurePiplinesAgent_Extension\_work\_tool\dotnet\dotnet.exe publish C:\AzurePiplinesAgent_Extension\_work\4\s\src\MiracleList\Miraclelist_WebAPI.csproj --configuration release --output C:\AzurePiplinesAgent_Extension\_work\4\a\MiracleList

dotnet.exe publish "$env:BUILD_SOURCESDIRECTORY\src\MiracleList\Miraclelist_WebAPI.csproj" --configuration release --output "$env:BUILD_STAGINGDIRECTORY\MiracleList"

# Upload 'C:\AzurePiplinesAgent_Extension\_work\4\a' to file container: '#/4514802/drop'
