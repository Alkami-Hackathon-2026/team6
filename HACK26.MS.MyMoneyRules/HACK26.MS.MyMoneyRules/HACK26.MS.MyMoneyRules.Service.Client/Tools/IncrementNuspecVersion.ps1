Param($projectName, $projectDir, $nuspecFileName)

## Code to update the version files:


    
    $projectFilePath = Join-Path $projectDir "$projectName.csproj"    
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
        if (!(Test-Path $assemblyFilePath)) {
            Write-Output "Could not find AssemblyInfo.cs"
         } else {
            (Get-Content $assemblyFilePath) | ForEach-Object{
            if($_ -match $pattern){
                # We have found the matching line
                # Edit the version number and put back.
                $version = [version]$matches[1]
                }
            }
         }
    }

    Write-Output "Read version as:"
    Write-Output $version.ToString()

    
    $semverPath = (Join-Path $projectDir "sem.ver")
    Write-Output $semverPath
    if (!(Test-Path $semverPath)) {
        Write-Output "No sem.ver file found"
    } else {

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
        $semVersionOutput | Set-Content -Path $semverPath
    }
    
    $nuspecPath = Join-Path $projectDir $nuspecFileName
    Write-Output $nuspecPath
    if (!(Test-Path $nuspecPath)) {
        Write-Output "No .nuspec file found at $nuspecPath"
    } else {
        [xml] $nuspecFile = Get-Content $nuspecPath
        $nuspecFile.Package.Metadata.Version = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString()

        Write-Output "Updating .nuspec version"
        ## Write-Output $nuspecFile.Package.Metadata.Version

        $nuspecFile.Save($nuspecPath)
    }

## end code