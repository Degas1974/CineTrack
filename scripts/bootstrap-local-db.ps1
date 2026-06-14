param(
    [string]$ServerInstance = "localhost",
    [string]$DatabaseName = "CineTrackDb",
    [string]$SqlScriptPath = "c:\Solucao CineTrack\CineTrack-Database.sql",
    [int]$CommandTimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-SqlScalar {
    param(
        [string]$Database,
        [string]$Query
    )

    & sqlcmd -S $ServerInstance -d $Database -b -h -1 -W -Q $Query | Out-String
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd não foi encontrado. Instale o SQL Server Command Line Utilities para rodar o bootstrap automático."
}

if (-not (Test-Path $SqlScriptPath)) {
    throw "Script SQL não encontrado em '$SqlScriptPath'."
}

Write-Step "Validando se o banco '$DatabaseName' existe em '$ServerInstance'"
$databaseExists = (Invoke-SqlScalar -Database "master" -Query "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$DatabaseName') IS NULL THEN 0 ELSE 1 END;").Trim()

if ($databaseExists -ne "1") {
    Write-Step "Criando banco '$DatabaseName'"
    & sqlcmd -S $ServerInstance -d "master" -b -Q "CREATE DATABASE [$DatabaseName];"
}

Write-Step "Verificando se o schema principal já foi aplicado"
$hasSchema = (Invoke-SqlScalar -Database $DatabaseName -Query "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID(N'dbo.Midia', N'U') IS NULL THEN 0 ELSE 1 END;").Trim()

if ($hasSchema -eq "1") {
    Write-Step "Schema já existente; mantendo banco atual"
}
else {
    Write-Step "Aplicando script base '$SqlScriptPath'"
    & sqlcmd -S $ServerInstance -d $DatabaseName -b -t $CommandTimeoutSeconds -i $SqlScriptPath
}

Write-Step "Coletando resumo do banco"
$summary = & sqlcmd -S $ServerInstance -d $DatabaseName -b -W -s "," -Q "SET NOCOUNT ON; SELECT DB_NAME() AS DatabaseName, (SELECT COUNT(*) FROM dbo.Midia) AS Midias, (SELECT COUNT(*) FROM dbo.Temporada) AS Temporadas, (SELECT COUNT(*) FROM dbo.Episodio) AS Episodios;"
$summary | ForEach-Object { Write-Host $_ }

Write-Host "Banco local pronto para uso." -ForegroundColor Green