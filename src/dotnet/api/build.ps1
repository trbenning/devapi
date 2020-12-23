[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "win-x86", "win-arm", "win-arm64", "linux-x64", "linux-arm", "linux-arm64", "linux-musl-x64")]
    [string]$runtime,
    [Parameter(Mandatory = $true)]
    [string]$artifactsDirectory,
    # Parameter help description
    [Parameter()]
    [Switch]
    $clean
)

[string]$outputPath = Join-Path -Path $artifactsDirectory -ChildPath $runtime
if ($(Test-Path -Path $outputPath)) {
    mkdir -Path $outputPath -Force
}

if ($clean) {
    Remove-Item -Recurse -Force -Path $outputPath/*
}

& dotnet publish ./api.csproj -o $artifactsDirectory/$runtime -r $runtime -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true