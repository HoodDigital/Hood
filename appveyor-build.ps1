$file = "bower.json"
$json = Get-Content $file | ConvertFrom-Json
if ($env:APPVEYOR_REPO_TAG -ne "true")
{
	$version = "$($json.version)-build$($env:APPVEYOR_BUILD_NUMBER)"
}
else 
{
    $version = "$($env:APPVEYOR_REPO_TAG_NAME)"
}
$exec = "dotnet restore"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec