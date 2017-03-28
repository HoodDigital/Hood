$version = "dev-$($env:APPVEYOR_REPO_COMMIT.substring(0,7))";
Update-AppveyorBuild -Version "$version"
if ($env:APPVEYOR_REPO_TAG -ne "true")
{
    $version = "dev-build$($env:APPVEYOR_BUILD_NUMBER)"
} 
else 
{ 
    $version = "$($env:APPVEYOR_REPO_TAG_NAME)"
}
Update-AppveyorBuild -Version "$version"