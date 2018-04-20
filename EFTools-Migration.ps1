
Cd $PSScriptRoot\src\EFCTools\bin\debug\netcoreapp2.0

$env:ConnectionStrings:MiracleListDB = "Data Source=localhost;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

#dotnet .\EFCTools.dll migrate 


$env:ConnectionStrings:MiracleListDB = "Server=tcp:miraclelistdb-staging.database.windows.net,1433;Initial Catalog=MiracleListDB-Staging;Persist Security Info=False;User ID=miraclelistdb-staging;Password=0d4301c6-4955-422a-a2ed-9821bcae3bb2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"


dotnet .\EFCTools.dll recreate 
