$ErrorActionPreference = "stop"

$Ziel = "\\192.168.23.129\Documents\" # !!! Vorher authentifizieren

if (-not (test-path $ziel)) { Write-Warning "Ziel nicht gefunden!" ; return }

cd $PSScriptRoot\src\Miraclelist
#dotnet publish -h
$temp = "t:\mlb2_SCA"
# Self-contained deployment (SCD) / Self-contained application (SCA)
dotnet publish -c release --runtime ubuntu.14.04-x64 --self-contained --framework netcoreapp2.0 -o $temp

#SCA kopieren in VM
robocopy $temp $Ziel\MLB2_SCA /e

# Framework-dependent deployments (FDD) / Portable applications (PA) 
$temp = "t:\mlb2_PA"
dotnet publish -c release  --framework netcoreapp2.0 -o $temp


#PA kopieren in VM
robocopy $temp $Ziel\MLB2_PA /e


#Quellcode auch kopieren in VM
robocopy $PSScriptRoot\ $Ziel\MLB_Source /e /XD "Packages" /xd ".vs"

Write-Host "FERTIG!" -ForegroundColor Green

