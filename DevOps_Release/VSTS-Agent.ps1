"VSTS Agent Dienst-Check/Interaktiver Start"
"Dr. Holger Schwichtenberg 2018-2019"
"------------------------------"

function Check-Admin([switch]$Elevated) {
$currentUser = New-Object Security.Principal.WindowsPrincipal $([Security.Principal.WindowsIdentity]::GetCurrent())
$currentUser.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

# Agent vorhanden als Dienst?

$s = get-service *vsts* 
if ($s -eq $null) 
 {
 "VSTS Agent-Dienst nicht vorhanden!" 
 }
 else
 { 
  "Prüfe Status des VSTS-Agent Dienstes..."
   $s | fl Name, Displayname, Status
  if ($s.Status -eq "Running") { 
    "VSTS Agent läuft!" 
     return
  }
 }

"Agent interaktiv+elevated starten..."
  if ((Check-Admin) -eq $false)  {
    if ($elevated)
    {
    # could not elevate, quit
    }
    else {
    "Starte Prozess erneut elevated..."
    Start-Process powershell.exe -Verb RunAs -ArgumentList ('-noprofile -noexit -file "{0}" -elevated' -f ($myinvocation.MyCommand.Definition))
    }
    exit
    }
  
  $v = C:\Software\DevOpsAgent\bin\Agent.Listener.exe --version
  "VSTS Agent v$v" 

  C:\Software\DevOpsAgent\run.cmd
 

"Ende!"