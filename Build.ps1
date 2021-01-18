$file = ".\projects\Hood.Development\wwwroot\hood\package.json"
$json = Get-Content $file | ConvertFrom-Json
$version = "$($json.version)"

$exec = "dotnet build .\projects\Hood.Core\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood.Admin\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood.UI\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood.UI.Core\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood.UI.Bootstrap3\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood.UI.Bootstrap4\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet build .\projects\Hood\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec

$exec = "dotnet pack .\projects\Hood.Core\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood.Admin\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood.UI\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood.UI.Core\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood.UI.Bootstrap3\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood.UI.Bootstrap4\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec
$exec = "dotnet pack .\projects\Hood\ -c Release /p:Version=$version"
Write-Host "$exec"
Invoke-Expression $exec

"//registry.npmjs.org/:_authToken=`$`{NPM_TOKEN`}" | Out-File (Join-Path $ENV:APPVEYOR_BUILD_FOLDER ".npmrc") -Encoding UTF8
$exec = "npm publish .\projects\Hood.Web\wwwroot\hood\"
Write-Host "$exec"
Invoke-Expression $exec