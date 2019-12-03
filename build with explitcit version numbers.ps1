

cd $PSScriptRoot

dotnet build  /p:Version=1.2.3.4

#File Version
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\BL\bin\Debug\netstandard2.0\DAL.dll").FileVersion
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\GO\bin\Debug\netstandard2.0\GO.dll").FileVersion
[System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\src\Miraclelist\bin\Debug\netcoreapp2.2\MiracleList_WebAPI.dll").FileVersion

dir ".\src\Miraclelist\bin\Debug\netcoreapp2.2" | Select-Object -ExpandProperty VersionInfo