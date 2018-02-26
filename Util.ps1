cls
$ENVName = "ConnectionStrings:MiracleListDB"
$CS = "Data Source=D120;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
$CS = "InMemoryDB"
"Set ConnectionStrings:MiracleListDB = $CS ..."
[Environment]::SetEnvironmentVariable("ConnectionStrings:MiracleListDB", $CS, "Machine")
$env:ConnectionStrings:MiracleListDB = $CS
$PSScriptRoot
dotnet test $PSScriptRoot\Test\UnitTests\UnitTests.csproj --filter Category!=Integration
