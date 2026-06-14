param(
    [string]$WorkspaceRoot = "d:\Solucao TaskList",
    [string]$SqlConnectionString = "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    [string]$ApiKey = "",
    [string]$TranslationProvider = "LibreTranslate",
    [string]$TranslationApiKey = "",
    [string]$TranslationBaseUrl = "http://localhost:5000/",
    [string]$SourceLanguage = "en",
    [string]$TargetLanguage = "pt",
    [string]$ImdbDatasetDirectory = ""
)

$ErrorActionPreference = "Stop"

function Set-SecretValue {
    param(
        [string]$ProjectPath,
        [string]$Key,
        [string]$Value
    )

    dotnet user-secrets --project $ProjectPath set $Key $Value | Out-Null
}

Push-Location $WorkspaceRoot

try {
    $apiProject = ".\CineTrack.API\CineTrack.API.csproj"
    $functionsProject = ".\CineTrack.Functions\CineTrack.Functions.csproj"

    Write-Host "==> Configurando User Secrets da API" -ForegroundColor Cyan
    Set-SecretValue -ProjectPath $apiProject -Key "ConnectionStrings:DefaultConnection" -Value $SqlConnectionString
    if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
        Set-SecretValue -ProjectPath $apiProject -Key "Security:ApiKey" -Value $ApiKey
    }
    Set-SecretValue -ProjectPath $apiProject -Key "Translation:Provider" -Value $TranslationProvider
    Set-SecretValue -ProjectPath $apiProject -Key "Translation:BaseUrl" -Value $TranslationBaseUrl
    Set-SecretValue -ProjectPath $apiProject -Key "Translation:SourceLanguage" -Value $SourceLanguage
    Set-SecretValue -ProjectPath $apiProject -Key "Translation:TargetLanguage" -Value $TargetLanguage
    if (-not [string]::IsNullOrWhiteSpace($TranslationApiKey)) {
        Set-SecretValue -ProjectPath $apiProject -Key "Translation:ApiKey" -Value $TranslationApiKey
    }
    if (-not [string]::IsNullOrWhiteSpace($ImdbDatasetDirectory)) {
        Set-SecretValue -ProjectPath $apiProject -Key "ImdbDatasets:Directory" -Value $ImdbDatasetDirectory
    }

    Write-Host "==> Configurando User Secrets do worker local" -ForegroundColor Cyan
    Set-SecretValue -ProjectPath $functionsProject -Key "SqlConnectionString" -Value $SqlConnectionString
    Set-SecretValue -ProjectPath $functionsProject -Key "Translation:Provider" -Value $TranslationProvider
    Set-SecretValue -ProjectPath $functionsProject -Key "Translation:BaseUrl" -Value $TranslationBaseUrl
    Set-SecretValue -ProjectPath $functionsProject -Key "Translation:SourceLanguage" -Value $SourceLanguage
    Set-SecretValue -ProjectPath $functionsProject -Key "Translation:TargetLanguage" -Value $TargetLanguage
    if (-not [string]::IsNullOrWhiteSpace($TranslationApiKey)) {
        Set-SecretValue -ProjectPath $functionsProject -Key "Translation:ApiKey" -Value $TranslationApiKey
    }
    if (-not [string]::IsNullOrWhiteSpace($ImdbDatasetDirectory)) {
        Set-SecretValue -ProjectPath $functionsProject -Key "ImdbDatasets:Directory" -Value $ImdbDatasetDirectory
    }

    Write-Host "==> User Secrets configurados com sucesso" -ForegroundColor Green
}
finally {
    Pop-Location
}
