cd $PSScriptRoot\src\Miraclelist
dotnet publish -h
$ziel = "t:\mlb"
# Self-contained deployment (SCD) / Self-contained application (SCA)
dotnet publish -c release --runtime ubuntu.14.04-x64 --self-contained --framework netcoreapp2.0 -o $ziel

robocopy $ziel \\192.168.1.170\Documents\MLB /e


robocopy $PSScriptRoot\ \\192.168.1.170\Documents\MLB_Source /e