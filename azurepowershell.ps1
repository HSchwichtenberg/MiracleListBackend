$ErrorActionPreference = "stoP"
#Get-Module -Name PowerShellGet -ListAvailable | Select-Object -Property Name,Version,Path

Install-Module PowerShellGet -Force

Install-Module -Name AzureRM -AllowClobber

Import-Module -Name AzureRM

Get-Module AzureRM -ListAvailable | Select-Object -Property Name,Version,Path
Login-AzureRmAccount -


# Buchdemo

New-AzureRmWebApp -Name BuchWebsite2 -Location "West Europe" -AppServicePlan BuchDemosFree -ResourceGroupName BuchWebsite


$webappnames="MLBASTABackend-Produktion","MLBASTABackend-Staging","MLBASTAClient-Produktion","MLBASTAClient-Staging"
#$webappnames="BASTA3","BASTA4"
$location="West Europe"
$RessourceGroup = "DEMO_MiracleList"
$Serviceplan = "WestEurope-F1Free"

foreach($webappname in $webappnames)
{
Write-Host $webappname -ForegroundColor Yellow

# Create a resource group.
#New-AzureRmResourceGroup -Name myResourceGroup -Location $location
#Remove-AzureRmResourceGroup -Name myResourceGroup 

Get-AzureRmResourceGroup $RessourceGroup

# Create an App Service plan in `Free` tier.
#New-AzureRmAppServicePlan -Name $webappname -Location $location -ResourceGroupName myResourceGroup -Tier Free
$sp = Get-AzureRmAppServicePlan -Name Serviceplan

# Create a web app.
New-AzureRmWebApp -Name $webappname -Location $location -AppServicePlan $sp -ResourceGroupName $RessourceGroup

$wa = Get-AzureRmWebApp -Name $webappname
if ($wa -eq $null) { return }
else { write-host "OK" -ForegroundColor Green }
}

# Get publishing profile for the web app
#$xml = (Get-AzureRmWebAppPublishingProfile -Name $webappname `
#-ResourceGroupName DEMO_MiracleList `
#-OutputFile null)

# The data center and resource name for your resources
$resourcegroupname = "DEMO_MiracleList"
$location = "WestEurope"
# The logical server name: Use a random value or replace with your own value (do not capitalize)
$servername = "bastasqlserver2"
# Set an admin login and password for your database
# The login information for the server
$adminlogin = "ServerAdmin"
$password = "geheim+123"
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

#"BASTA1","BASTA2","BASTA3","BASTA4" | % { Remove-AzureRmWebApp -name $_ -ResourceGroupName $RessourceGroup }