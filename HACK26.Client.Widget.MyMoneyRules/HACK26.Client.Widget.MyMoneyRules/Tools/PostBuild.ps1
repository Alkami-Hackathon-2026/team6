Param($projectName, $projectDir)

## This file should not be overwritten by future upgrades to the postbuild installer.
## If you want this file to be overwritten you must remove it before upgrading the Alkami.Installer.Widget.PostBuild package in the future.
## This means that your file changes are persistent for yourself.

function Test-IsAdmin {
    try {
        return (New-Object Security.Principal.WindowsPrincipal -ArgumentList ([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole( [Security.Principal.WindowsBuiltInRole]::Administrator )
    } catch {
        return $false;
    }
}

if (!(Test-IsAdmin)) {
    Write-Host "Can only do chocolatey based PostDeploy events when running Visual Studio as admin";
    return;
}

Write-Output "PostBuild initiated for $projectName";

cd $projectDir;

$expectedWorkingPath = (Join-Path $projectDir 'TXNSCU.Client.Widget.LAW.nuspec')

if (!(Test-Path $expectedWorkingPath)) {
	$expectedWorkingPath = (Get-ChildItem (Join-Path $projectDir "*.nuspec") | Sort LastWriteTime | Select -First 1).FullName;
}

$versionPrefix = '1.0.0'
## Write-Output "Looking for version to pack with";

$semverPath = (Join-Path $projectDir "sem.ver")


## Code to update the version files:

    $projectFilePath = Join-Path $projectDir "$projectName.csproj"
    $nuspecPath = Join-Path $projectDir "$projectName.nuspec"
    $assemblyFilePath = Join-Path $projectDir "Properties\AssemblyInfo.cs"
    $pattern = '^\[assembly: AssemblyVersion\("(.*)"\)\]'
    $version = "1.0.0.0"
    ## Write-Output "Getting project file from $projectFilePath"
    [xml]$xml = Get-Content $projectFilePath
    ## Write-Output "Getting target framework"
    $targetFramework = $xml.Project.PropertyGroup.TargetFramework
    ## Write-Output "Target framework is $targetFramework"
    if($targetFramework -eq "net8.0"){
        Write-Output "Getting version from project file"
        $version = [Version] $xml.Project.PropertyGroup.AssemblyVersion
    } else {
        Write-Output "Getting version from AssemblyInfo.cs"
        (Get-Content $assemblyFilePath) | ForEach-Object{
        if($_ -match $pattern){
            # We have found the matching line
            # Edit the version number and put back.
            $version = [version]$matches[1]
            }
        }
    }

    Write-Output "Read version as:"
    Write-Output $version.ToString()


    Write-Output "Creating sem.ver object"
    # Create a custom object with the desired JSON structure
    $semVersion = [ordered]@{
        Version = [ordered]@{
            Major = $version.Major
            Minor = $version.Minor
            Patch = $version.Build
        }
    }
    $semVersionOutput = $semVersion | ConvertTo-Json -Depth 2
    $semVersionOutput = $semVersionOutput -replace "  "," "
    Write-Output "Updating sem.ver"
    ## Write-Output $semVersionOutput
    [xml] $nuspecFile = Get-Content $nuspecPath
    $nuspecFile.Package.Metadata.Version = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString()

    Write-Output "Updating .nuspec version"
    ## Write-Output $nuspecFile.Package.Metadata.Version

    $semVersionOutput | Set-Content -Path $semverPath
    $nuspecFile.Save($nuspecPath)

## end code

if (!(Test-Path $semverPath)) {
    $semverPath = (Join-Path (Split-Path $projectDir) "sem.ver")
}

if (Test-Path $semverPath) {
    $fileVersion = (Get-Content $semverPath) | ConvertFrom-Json;
    $versionPrefix = "{0}.{1}.{2}" -f $fileVersion.Version.Major, $fileVersion.Version.Minor, $fileVersion.Version.Patch
} else {
    try {
        $nuspecContent = [Xml](Get-Content $expectedWorkingPath)
        $version = ([System.Version]$nuspecContent.package.metadata.version);
        $versionPrefix = $version.ToString();
    } catch {
        Write-Warning "Can't find the sem.ver file, defaulting to 1.0.0";
        $versionPrefix = '1.0.0'
    }
}

## Write-Output "Applying version $versionPrefix to $expectedWorkingPath";

## Write-Output "Packing $expectedWorkingPath for deploy";

## Write-Output "Creating .txt copy of current .nuspec manifest"

Copy-Item -Path $expectedWorkingPath -Destination ($expectedWorkingPath + ".txt")

## Write-Output "choco pack $expectedWorkingPath --version $versionPrefix --out $projectDir --no-progress -r;";

choco pack $expectedWorkingPath --version $versionPrefix --out $projectDir --no-progress -r;

## Write-Output "Removing .txt copy of current .nuspec manifest"

Remove-Item -Path ($expectedWorkingPath + ".txt")

$firstFoundNupkg = (Get-ChildItem (Join-Path $projectDir "*.nupkg") | Sort LastWriteTime -Descending | Select -First 1).Name;

Write-Output "Installing $firstFoundNupkg via choco";

## TODO: This `-s .` inhibits finding the upstream required packages because it limits source scope

## Write-Output "choco upgrade $firstFoundNupkg -fry -s . --no-progress;";

choco upgrade $firstFoundNupkg -fy -s . --no-progress;
