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
$exec = "dotnet pack projects/Hood/Hood.csproj -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec