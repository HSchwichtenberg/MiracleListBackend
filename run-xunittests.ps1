cls
"Start Unit tests for " + $PSScriptRoot

$ENVName = "ConnectionStrings:MiracleListDB"
#$CS = "Data Source=.;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
#$CS = "" # l�schen!
#$CS = "Server=tcp:miraclelistdb-staging.database.windows.net,1433;Initial Catalog=MiracleListDB-Staging;Persist Security Info=False;User ID=miraclelistdb-staging;Password=0d4301c6-4955-422a-a2ed-9821bcae3bb2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$CS = "InMemoryDB"

"Set ConnectionStrings:MiracleListDB = $CS ..."
#[Environment]::SetEnvironmentVariable("ConnectionStrings:MiracleListDB", $CS, "Machine")
#[Environment]::SetEnvironmentVariable("ConnectionStrings:MiracleListDB", $CS, "User")
$env:ConnectionStrings:MiracleListDB = $CS

"Environment Process: " + $env:ConnectionStrings:MiracleListDB
"Environment User: " + [Environment]::GetEnvironmentVariable("ConnectionStrings:MiracleListDB","User")
"Environment Machine: " + [Environment]::GetEnvironmentVariable("ConnectionStrings:MiracleListDB","Machine")

"Starting tests..."
dotnet test $PSScriptRoot\Test\UnitTests\UnitTests.csproj --filter Category!=Integration #-v n