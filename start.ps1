$ErrorActionPreference = 'Stop'

$projectName = 'Okisioli.Teacups.UI.Demo.Avalonia.Browser'
$project = Join-Path $PSScriptRoot "demo/$projectName/$projectName.csproj"

Push-Location $PSScriptRoot
try {
    dotnet workload restore $project --skip-manifest-update
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    dotnet build $project -m:1
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    dotnet run --project $project --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally {
    Pop-Location
}
