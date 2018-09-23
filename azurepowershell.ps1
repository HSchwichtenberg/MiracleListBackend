## Azure-Ressourcen für DevOps-Workshop mit PowerShell anlegen

$ErrorActionPreference = "stoP"


#region Installation
#Get-Module -Name PowerShellGet -ListAvailable | Select-Object -Property Name,Version,Path
Install-Module PowerShellGet -Force # geht nur als Admin!
Install-Module -Name AzureRM -force -AllowClobber
Import-Module -Name AzureRM 
Get-Module AzureRM -ListAvailable | Select-Object -Property Name,Version,Path | ft
#endregion

# Anmelden interaktiv!!!
Login-AzureRmAccount 

# Wichtige Festlegungen
$RessourceGroup = "DEMO_MiracleList"
$Serviceplan = "WestEurope-F1Free"
$location="West Europe"

#region Websites
$webappnames="MLBASTABackend-Produktion","MLBASTABackend-Staging","MLBASTAClient-Produktion","MLBASTAClient-Staging"

# Optional: Create a resource group.
#New-AzureRmResourceGroup -Name myResourceGroup -Location $location
#Remove-AzureRmResourceGroup -Name myResourceGroup 
Get-AzureRmResourceGroup $RessourceGroup

# Optional: Service Plan anlegen
#New-AzureRmAppServicePlan -Name $webappname -Location $location -ResourceGroupName myResourceGroup -Tier Free
$sp = Get-AzureRmAppServicePlan -Name Serviceplan


foreach($webappname in $webappnames)
{
Write-Host "Creating $webappname" -ForegroundColor Yellow
# Create a web app.
New-AzureRmWebApp -Name $webappname -Location $location -AppServicePlan $sp -ResourceGroupName $RessourceGroup
$wa = Get-AzureRmWebApp -Name $webappname
if ($wa -eq $null) { Write-Error "WebApp wurde nicht angelegt!"; return; }
else { write-host "OK" -ForegroundColor Green }
}
# Kontrolle:
Get-AzureRmWebApp -ResourceGroupName $RessourceGroup | where RepositorySiteName -like "*basta*" | ft

# Get publishing profile for the web app
#$xml = (Get-AzureRmWebAppPublishingProfile -Name $webappname `
#-ResourceGroupName DEMO_MiracleList `
#-OutputFile null)
#endregion

#region SQL Server
# The data center and resource name for your resources
$resourcegroupname = "DEMO_MiracleList"
$location = "WestEurope"
# The logical server name: Use a random value or replace with your own value (do not capitalize)
$servername = "bastasqlserver"
# Set an admin login and password for your database
# The login information for the server
$adminlogin = "MLBASTAAdmin"
$password = "211e2868-8ad0-4fb1-9d7a-aa94a2e8dc97"
# The ip address range that you want to allow to access your server - change as appropriate
$startip = "0.0.0.0"
$endip = "255.255.255.255"
# The database name

New-AzureRmSqlServer -ResourceGroupName $resourcegroupname `
    -ServerName $servername `
    -Location $location `
    -SqlAdministratorCredentials $(New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $adminlogin, $(ConvertTo-SecureString -String $password -AsPlainText -Force))

$databasename = "MLBASTADB-Staging"
New-AzureRmSqlDatabase  -ResourceGroupName $resourcegroupname -ServerName $servername  -DatabaseName $databasename    -RequestedServiceObjectiveName "S0"

$databasename = "MLBASTADB-Produktion"
New-AzureRmSqlDatabase  -ResourceGroupName $resourcegroupname -ServerName $servername  -DatabaseName $databasename   -RequestedServiceObjectiveName "S0"

# Kontrolle
Get-AzureRmSqlDatabase  -ResourceGroupName $resourcegroupname -ServerName $servername  | ft

#endregion

#region Alles wieder löschen (mit Nachfrage!)
#$webappnames | % { Remove-AzureRmWebApp -name $_ -ResourceGroupName $RessourceGroup }
#Remove-AzureRmSqlServer -ResourceGroupName $resourcegroupname -ServerName $servername 
#endregion