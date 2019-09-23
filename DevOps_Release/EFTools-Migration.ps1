Cd $PSScriptRoot\..
cd src\EFCTools\bin\debug\netcoreapp2.2

# Setze Connection String
$env:ConnectionStrings:MiracleListDB = "Data Source=.;Initial Catalog = MiracleList_Test_20190923; Integrated Security = True; Connect Timeout = 15; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Application Name=EntityFramework"

# Starte Migration
dotnet .\EFCTools.dll migrate 

# Alternativ: Komplette Datenbank löschen und wieder anlegen
#dotnet .\EFCTools.dll recreate 