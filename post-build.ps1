$file = "bower.json"
$json = Get-Content $file | ConvertFrom-Json
$version = "$($json.version)-build$($env:APPVEYOR_BUILD_NUMBER)"

if ($env:APPVEYOR_REPO_TAG -eq "true")
{
    $version = "$($env:APPVEYOR_REPO_TAG_NAME)"
}

Invoke-Expression "dotnet pack projects/Hood/Hood.csproj /p:Version=$version"