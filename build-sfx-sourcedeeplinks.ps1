function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
    New-Item -ItemType Directory -Path (Join-Path $parent "VSIX$name")
}

$cwd = (Get-Location).Path

$vsix = "$cwd\SourceControlDeepLinks\bin\Debug\SourceControlDeepLinks.vsix"
$sed = "SourceControlDeepLinksSSMS.SED"

If ( Test-Path $vsix -PathType Leaf ){
    # Src is VSIX and SED template file
    $tempDirSrc = New-TemporaryDirectory
    $zipfile = "$tempDirSrc\vsix.zip"
    Copy-Item $vsix -Destination $zipfile

    $sedTemplate = "$cwd\$sed"
    $tempSed = "$tempDirSrc\$sed"
    Copy-Item $sedTemplate -Destination $tempSed

    # Dest contains files extracted from VSIX
    $tempDirDest = New-TemporaryDirectory
    Expand-Archive -LiteralPath $zipfile -DestinationPath $tempDirDest

    $destPathOffset = $tempDirDest.FullName.Length + 1
    $files = (Get-ChildItem -Path $tempDirDest -File -Recurse )
   
    # Add the files and folders to the SED file
    0 .. ($files.length - 1) |
        ForEach-Object { 
            $file = $files[$_].FullName
            $fileWithinFolder = $file.Substring($destPathOffset)
            Add-Content -Path $tempSed -Value "FILE$_=""$fileWithinFolder"""
        }

    # Add the source folders
    $sourceFiles = "[SourceFiles]`r`n" + 
        "SourceFiles0=$tempDirDest\`r`n" +
        "[SourceFiles0]"
    Add-Content -Path $tempSed -Value $sourceFiles

    $outputFile = "$cwd\SourceControlDeepLinksSSMS.exe"

    # Add the empty file references
    0 .. ($files.length - 1) | 
        ForEach-Object { Add-Content -Path $tempSed -Value "%FILE$_%=" }

    # Create the Self Extracting Archive
    # https://ss64.com/nt/iexpress-sed.html
    $parms = "/N /Q $tempSed"
    & iexpress.exe $parms.Split(" ") | Out-Null

    Remove-Item $tempDirSrc -Recurse -Force -Confirm:$false
    Remove-Item $tempDirDest -Recurse -Force -Confirm:$false
}
Else 
{
    Write-Host "VSIX is missing $vsix"
}
