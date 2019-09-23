# Azure-Ressourcen für MiracleList-Release-Pipeline mit PowerShell anlegen
# (C) Dr. Holger Schwichtenberg 2018-2019
# Stand: 23.09.2019

# Wichtige Festlegungen
$SubscriptionId = "x4945d71-xadf-4fa5-x6dc-xc2d007c168x" # nur ein Beispiel
$prefix = "ml-" # nur ein Beispiel --> lower case !!!
$RessourceGroup = "RG-DEMO-MiracleList-DevOps" # nur ein Beispiel
$Serviceplan = "SP-DEMO-MiracleList-DevOps-S1" # nur ein Beispiel
$location="West Europe" # nur ein Beispiel

$ErrorActionPreference = "stop"

#region Installation der PowerShell-Module für Azure
Install-Module PowerShellGet -Force -AllowClobber -Scope currentuser # globaler Scope geht nur als Admin!
Install-Module -Name AzureRM -force -AllowClobber -Scope currentuser
Import-Module -Name AzureRM 
Get-Module AzureRM -ListAvailable | Select-Object -Property Name,Version,Path | ft
"Anzahl geladener Azure-Module: " + (Get-Module AzureRM.* -ListAvailable | Select-Object -Property Name,Version,Path).Count
"Anzahl verfügbarer Azure-Befehle: " + (Get-Command -module AzureRM*).Count

#endregion


#region Anmelden interaktiv!!!
#Login-AzureRmAccount 
Connect-AzureRmAccount -SubscriptionId $SubscriptionId  
Get-AzureRmContext | fl *
#endregion

#region ---------------- Ressourcen prüfen
# Optional: Create a resource group.
#New-AzureRmResourceGroup -Name myResourceGroup -Location $location
#Remove-AzureRmResourceGroup -Name myResourceGroup 
$rg = Get-AzureRmResourceGroup $RessourceGroup
if (-not $rg) { Write-Error "Resource Group nicht gefunden!" ; return }
# Optional: Service Plan anlegen
#New-AzureRmAppServicePlan -Name $webappname -Location $location -ResourceGroupName myResourceGroup -Tier Free
$sp = Get-AzureRmAppServicePlan -Name $Serviceplan 
if (-not $sp) { Write-Error "Service Plan nicht gefunden!" ; return }

# TODO:
#$lo = Get-AzureLocation $location
#if (-not $lo) { Write-Error "Location nicht gefunden!" : return }
#endregion

#region ---------------- Websites anlegen
$webappnames="$($prefix)MLBackend-Production","$($prefix)MLBackend-Staging" ,"$($prefix)MLClient-Production","$($prefix)MLClient-Staging"

foreach($webappname in $webappnames)
{
Write-Host "Creating $webappname" -ForegroundColor Yellow
# Create a web app.
New-AzureRmWebApp -Name $webappname -Location $location -AppServicePlan $Serviceplan -ResourceGroupName $RessourceGroup
$wa = Get-AzureRmWebApp -Name $webappname
if ($wa -eq $null) { Write-Error "WebApp wurde nicht angelegt!"; return; }
else { write-host "OK" -ForegroundColor Green }
}
# Kontrolle:
Get-AzureRmWebApp -ResourceGroupName $RessourceGroup | where RepositorySiteName -like "*$prefix*" | ft RepositorySiteName, State

# Create Deployment Slots
foreach($webappname in ($webappnames | where { $_ -like "*production*" }))
{
$webappname
New-AzureRmWebAppSlot -ResourceGroupName $RessourceGroup -name $webappname -slot ProdStagingSlot
Get-AzureRmWebAppSlot -ResourceGroupName $RessourceGroup -name $webappname   
}

# Einstellungen für WebApps
foreach($webappname in ($webappnames))
{
$webappname

#Always On
$WebAppResourceType = 'microsoft.web/sites'
$WebAppPropertiesObject = @{"siteConfig" = @{"AlwaysOn" = $true}}
$webApp= Get-AzureRmResource -ResourceType $WebAppResourceType -ResourceGroupName $RessourceGroup -ResourceName $webappname
$webApp | set-AzureRMResource  -PropertyObject $WebAppPropertiesObject -force

# AppSettings
$WebApp = Get-AzureRMWebApp -ResourceGroupName $RessourceGroup -Name $webappname
$webApp.SiteConfig.AppSettings
$newAppSettings = @{"ASPNETCORE_ENVIRONMENT"="Production"}
Set-AzureRMWebApp -AppSettings $newAppSettings -ResourceGroupName $RessourceGroup -Name $webappname
$webApp.SiteConfig.AppSettings
}

# Get publishing profile for the web app
#$xml = (Get-AzureRmWebAppPublishingProfile -Name $webappname `
#-ResourceGroupName DEMO_MiracleList `
#-OutputFile null)
#endregion
return

#region ---------------- SQL Server anlegen
$dbnames="$($prefix)MLDB-Staging", "$($prefix)MLDB-Produktion"

# The logical server name: Use a random value or replace with your own value (do not capitalize)
$servername = "$($prefix)mssqlserver"
# Set an admin login and password for your database
# The login information for the server
$adminlogin = "MLSQLAdmin"
$password = new-guid
Write-Host "Your SQL Server password is: $password" -ForegroundColor Green
# The ip address range that you want to allow to access your server - change as appropriate
$startip = "0.0.0.0"
$endip = "255.255.255.255"
# The database name

New-AzureRmSqlServer -ResourceGroupName $RessourceGroup `
    -ServerName $servername `
    -Location $location `
    -SqlAdministratorCredentials $(New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $adminlogin, $(ConvertTo-SecureString -String $password -AsPlainText -Force))

New-AzureRmSqlServerFirewallRule -ResourceGroupName $RessourceGroup `
    -ServerName $servername `
    -FirewallRuleName "AllowAll" -StartIpAddress "0.0.0.0" -EndIpAddress $endip

foreach($databasename in $dbnames)
{ 
New-AzureRmSqlDatabase  -ResourceGroupName $RessourceGroup -ServerName $servername  -DatabaseName $databasename    -RequestedServiceObjectiveName "S0"
}

# Kontrolle
Get-AzureRmSqlDatabase  -ResourceGroupName $RessourceGroup -ServerName $servername  | ft

#endregion
return

# ############################################ Aufräumen

#region ------------------------ Alles wieder löschen (mit Nachfrage!)
"Lösche Web Apps..."
$webappnames
$webappnames | % { Remove-AzureRmWebApp -name $_ -ResourceGroupName $RessourceGroup -confirm }
"Lösche SQL Server..."
Remove-AzureRmSqlServer -ResourceGroupName $resourcegroupname -ServerName $servername -Confirm
#endregion