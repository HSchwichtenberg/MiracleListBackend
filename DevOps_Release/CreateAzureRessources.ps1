# Azure-Ressourcen für MiracleList-Release-Pipeline mit PowerShell anlegen
# (C) Dr. Holger Schwichtenberg 2018-2019
# Stand: 03.12.2019

# Wichtige Festlegungen
$SubscriptionId = "14945d71-xxxx-xxxx-xxxx-2c2d007c168a" # nur ein Beispiel
$prefix = "hs-" # nur ein Beispiel --> lower case !!!
$RessourceGroup = "AzureDevOpsWorkhop" # nur ein Beispiel
$Serviceplan = "SP-AzureDevOpsWorkshopStagingS1" # nur ein Beispiel
$location="West Europe" # nur ein Beispiel
$webappnames="$($prefix)MLBackend-Production","$($prefix)MLBackend-Staging" ,"$($prefix)MLClient-Production","$($prefix)MLClient-Staging"
$servername = "$($prefix)mssqlserver"
$dbnames="$($prefix)MLDB-Staging", "$($prefix)MLDB-Produktion"
$password = "0d17a4de-160e-4bd5-9f97-bbdf3e6aa1f6"

$ErrorActionPreference = "stop"

if ($prefix -ne $prefix.ToLower()) { throw "Prefix muss aus Kleinbuchstaben bestehen, weil SQL Azure nur Kleinbuchstaben als Servernamen erlaubt!" }

#region Installation der PowerShell-Module für Azure
#Install-Module PowerShellGet -Force -AllowClobber -Scope currentuser # globaler Scope geht nur als Admin!
#Install-Module -Name AzureRM -force -AllowClobber -Scope currentuser
#Import-Module -Name AzureRM 
#Get-Module AzureRM -ListAvailable | Select-Object -Property Name,Version,Path | ft
#"Anzahl geladener Azure-Module: " + (Get-Module AzureRM.* -ListAvailable | Select-Object -Property Name,Version,Path).Count
#"Anzahl verfügbarer Azure-Befehle: " + (Get-Command -module AzureRM*).Count

#endregion


#region Anmelden interaktiv!!!
"Anmeldung..."
Login-AzureRmAccount 
Connect-AzureRmAccount -SubscriptionId $SubscriptionId
"Infos..."
Get-AzureRmSubscription  
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


#region ---------------- SQL Server anlegen

# Set an admin login and password for your database
# The login information for the server
$adminlogin = "MLSQLAdmin"
$password = $password
#Write-Host "Your SQL Server password is: $password" -ForegroundColor Green
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
    -FirewallRuleName "AllowAll" -StartIpAddress $startip -EndIpAddress $endip

foreach($databasename in $dbnames)
{ 
New-AzureRmSqlDatabase  -ResourceGroupName $RessourceGroup -ServerName $servername  -DatabaseName $databasename    -RequestedServiceObjectiveName "S0"
}

# Kontrolle
Get-AzureRmSqlDatabase  -ResourceGroupName $RessourceGroup -ServerName $servername  | ft

# Kontrolle: Alle Ressourcen in dieser Ressource Group

Get-AzureRmResource -ResourceGroupName $RessourceGroup | sort name | ft

#endregion
return

# ############################################ Aufräumen

#region ------------------------ Alles wieder löschen (mit Nachfrage!)
"Lösche Web Apps..."
$webappnames
$webappnames | % { Remove-AzureRmWebApp -name $_ -ResourceGroupName $RessourceGroup -confirm }
"Lösche SQL Server $servername ..."
Get-AzureRmSqlDatabase  -ResourceGroupName $RessourceGroup -ServerName $servername  | ft
Remove-AzureRmSqlServer -ResourceGroupName $RessourceGroup -ServerName $servername -Confirm
#endregion