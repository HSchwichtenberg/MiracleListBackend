cls
"Start Unit tests for " + $PSScriptRoot

$ENVName = "ConnectionStrings:MiracleListDB"
#$CS = "Data Source=.;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
$CS = "InMemoryDB"
#$CS = "" # löschen!
"Set ConnectionStrings:MiracleListDB = $CS ..."
[Environment]::SetEnvironmentVariable("ConnectionStrings:MiracleListDB", $CS, "Machine")
$env:ConnectionStrings:MiracleListDB = $CS

"Environment Process: " + $env:ConnectionStrings:MiracleListDB
"Environment Machine: " + [Environment]::GetEnvironmentVariable("ConnectionStrings:MiracleListDB","Machine")



dotnet test $PSScriptRoot\Test\UnitTests\UnitTests.csproj --filter Category!=Integration


