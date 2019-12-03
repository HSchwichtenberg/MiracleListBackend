# Set Version number in all .csproj files
# (C) Dr. Holger Schwichtenberg 2018-2019

#region Constructing parameters
$buildid = $env:Build_BuildId
if (-not($buildid)) { $buildid = 0 } # for lokal Test
$version = $env:version
if (-not($version)) { $version = "1.2.3" } # for lokal Test
$version = "$version" + "." + $buildid
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
if (-not($sourcesDirectory)) { $sourcesDirectory = $PSScriptRoot } # for lokal Test
#endregion

Function Set-NodeValue($rootNode, [string]$nodeName, [string]$value)
{   
    $nodePath = "PropertyGroup/$($nodeName)"  
    $node = $rootNode.Node.SelectSingleNode($nodePath)
    if ($node -eq $null) {
        Write-Host "  Add <$($nodeName)>"

        $group = $rootNode.Node.SelectSingleNode("PropertyGroup")
        $node = $group.OwnerDocument.CreateElement($nodeName)
        $group.AppendChild($node) | Out-Null
    }
    $node.InnerText = $value
    Write-Host "  Set <$($nodeName)> = $($value)"
}

Write-Host "---> Searching for *.csproj in $($sourcesDirectory)"
$count = 0
Get-ChildItem -Path $sourcesDirectory -Filter "*.csproj" -Recurse -File | 
    ForEach-Object { 
        $count++
        Write-Host "- Project #$($count):" $($_.FullName)

        $projectPath = $_.FullName
        $project = Select-Xml $projectPath -XPath "//Project"
        
        Set-NodeValue $project "Version" $version
        Set-NodeValue $project "AssemblyVersion" $version
        Set-NodeValue $project "FileVersion" $version
        Set-NodeValue $project "InformationalVersion" "$version-$(Get-Date)" 

        $document = $project.Node.OwnerDocument
        $document.PreserveWhitespace = $true

        $document.Save($projectPath)

        Write-Host ""
    }

Write-Host "##vso[build.updatebuildnumber]$($version)"