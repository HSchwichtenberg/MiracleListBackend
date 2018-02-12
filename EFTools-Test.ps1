
Cd $PSScriptRoot\src\EFCTools\bin\Release\netcoreapp2.0

$env:ConnectionStrings:MiracleListDB = "Data Source=D120;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

dotnet .\EFCTools.dll both