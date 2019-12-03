

cd $PSScriptRoot

dotnet build  /p:Version=1.2.3.4

#File Version
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\BL\bin\Debug\netstandard2.0\DAL.dll").FileVersion
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\GO\bin\Debug\netstandard2.0\GO.dll").FileVersion
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\Miraclelist\bin\Debug\netcoreapp2.2\MiracleList_WebAPI.dll").FileVersion

dir ".\src\Miraclelist\bin" -Recurse -Filter "*.dll"  | Select-Object -ExpandProperty VersionInfo


$version = "0.12.1"
$nuGetVersion = $env:GitVersion_NuGetVersionV2
$sourcesDirectory = $PSScriptRoot # $env:BUILD_SOURCESDIRECTORY

Write-Host "Searching for projects under $($sourcesDirectory)"

# Find all the csproj files
if ($nuGetVersion -eq $null) {
    Write-Error ("GitVersion_NuGetVersionV2 environment variable is missing.")
    exit 1
}

if ($env:GitVersion_AssemblySemVer -eq $null) {
    Write-Error ("GitVersion_AssemblySemVer environment variable is missing.")
    exit 1
}

if ($env:GitVersion_MajorMinorPatch -eq $null) {
    Write-Error ("GitVersion_MajorMinorPatch environment variable is missing.")
    exit 1
}

if ($env:GitVersion_InformationalVersion -eq $null) {
    Write-Error ("GitVersion_InformationalVersion environment variable is missing.")
    exit 1
}

if ($sourcesDirectory -eq $null) {
    Write-Error ("BUILD_SOURCESDIRECTORY environment variable is missing.")
    exit 1
}

Function Set-NodeValue($rootNode, [string]$nodeName, [string]$value)
{   
    $nodePath = "PropertyGroup/$($nodeName)"
    
    $node = $rootNode.Node.SelectSingleNode($nodePath)

    if ($node -eq $null) {
        Write-Host "Adding $($nodeName) element to existing PropertyGroup"

        $group = $rootNode.Node.SelectSingleNode("PropertyGroup")
        $node = $group.OwnerDocument.CreateElement($nodeName)
        $group.AppendChild($node) | Out-Null
    }

    $node.InnerText = $value

    Write-Host "Set $($nodeName) to $($value)"
}

# This code snippet gets all the files in $Path that end in ".csproj" and any subdirectories.
Get-ChildItem -Path $sourcesDirectory -Filter "*.csproj" -Recurse -File | 
    ForEach-Object { 
        
        Write-Host "Found project at $($_.FullName)"

        $projectPath = $_.FullName
        $project = Select-Xml $projectPath -XPath "//Project"
        
        Set-NodeValue $project "Version" $nuGetVersion
        Set-NodeValue $project "AssemblyVersion" $env:GitVersion_AssemblySemVer
        Set-NodeValue $project "FileVersion" $env:GitVersion_MajorMinorPatch
        Set-NodeValue $project "InformationalVersion" $env:GitVersion_InformationalVersion 

        $document = $project.Node.OwnerDocument
        $document.PreserveWhitespace = $true

        $document.Save($projectPath)

        Write-Host ""
    }

Write-Host "##vso[build.updatebuildnumber]$($nuGetVersion)"