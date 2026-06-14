param(
    [string]$ConnectionString = "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;",
    [switch]$KeepData
)

$ErrorActionPreference = "Stop"

Push-Location "c:\Solucao CineTrack"
try {
    $args = @(
        "run",
        "--project", "c:\Solucao CineTrack\tools\CineTrack.SyncScenarioRunner\CineTrack.SyncScenarioRunner.csproj",
        "--",
        "--connection-string", $ConnectionString
    )

    if ($KeepData) {
        $args += "--keep-data"
    }

    dotnet @args
}
finally {
    Pop-Location
}
