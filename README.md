# TrackList

## Primeiros passos para qualquer editor ou IA
- Antes de criar, alterar ou revisar código neste workspace, leia `RegrasCinetrack.md`.
- Use `CineTrack-Analise.md` como referência de arquitetura, fluxos e operação.
- Considere `CineTrack.sln` como a solução principal deste workspace.

## Ordem de leitura recomendada
1. `RegrasCinetrack.md`
2. `CineTrack-Analise.md`
3. `CineTrack.sln`

## Fonte central de regras
- Todas as regras permanentes de desenvolvimento devem ser centralizadas em `RegrasCinetrack.md`.
- Evite duplicar regras em arquivos auxiliares; esses arquivos devem apenas apontar para a fonte central.

## Hierarquia de referência
- `RegrasCinetrack.md`: regras permanentes e bootstrap de sessão.
- `CineTrack-Analise.md`: contexto arquitetural e operacional.
- `CineTrack.sln`: solução principal a ser priorizada no desenvolvimento.

## Checklist de build e limpeza
- SDK fixado do workspace: `.NET SDK 10.0.300` via `global.json`
- Build principal da solução: `dotnet build CineTrack.sln`
- Build isolado da API: `dotnet build CineTrack.API/CineTrack.API.csproj`
- Build isolado do worker local: `dotnet build CineTrack.Functions/CineTrack.Functions.csproj`
- Build isolado do mobile: `dotnet build CineTrack.Mobile/CineTrack.Mobile.csproj`
- Build do runner controlado de sync: `dotnet build tools/CineTrack.SyncScenarioRunner/CineTrack.SyncScenarioRunner.csproj`
- Smoke check de build: `./scripts/smoke-check.ps1 -SkipHttp`
- Smoke check com validação HTTP: `./scripts/smoke-check.ps1`
- Teste controlado de auto-confirmação: `./scripts/test-auto-confirm-controlado.ps1`

### Quando o build Android falhar por acesso negado no `obj`
- Feche processos que possam estar usando artefatos do Android, se houver.
- Remova a pasta intermediária do mobile: `CineTrack.Mobile/obj/Debug/net10.0-android`
- Rode novamente: `dotnet build CineTrack.Mobile/CineTrack.Mobile.csproj`
- Se necessário, repita o build da solução completa após a limpeza: `dotnet build CineTrack.sln`

### Observações operacionais
- O workspace fixa o SDK em `global.json` para evitar oscilação entre `10.0.201` e `10.0.300-preview`.
- O arquivo `Directory.Build.rsp` aplica `AllowMissingPrunePackageData=true` para contornar `NETSDK1226` no restore/build com o SDK .NET 10 atual.
- `CineTrack.Functions` agora é um worker local/console e depende de `SqlConnectionString` configurada em ambiente/local settings.
- `CineTrackRelayListener` fica legado/desativado; o fluxo oficial usa API HTTP direta.
- `CineTrackTokenGen` gera `Security:ApiKey` para API direta.
- Configurações sensíveis devem ser fornecidas via arquivos de configuração apropriados ou variáveis de ambiente, não em código-fonte.
- Arquivos versionados como `appsettings.json` e `local.settings.json` podem conter placeholders e exigem preenchimento antes da execução real dos serviços.
- Em desenvolvimento, `CineTrack.API` usa `CINETRACK_SQL_CONNECTION` quando presente; se o placeholder continuar no `appsettings.json`, a API cai para `Server=(localdb)\MSSQLLocalDB;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True`.
- Em desenvolvimento, a API escuta em `http://0.0.0.0:5050` para permitir acesso do emulador Android via `http://10.0.2.2:5050` e do host via `http://localhost:5050`.
- Em desenvolvimento, o app mobile usa `CineTrackApi:BaseUrl` quando configurada; caso contrário, assume `http://10.0.2.2:5050/` no Android e `http://localhost:5050/` nas demais plataformas. Se `CineTrackApi:ApiKey` estiver preenchida, o app envia `X-Api-Key`.
- O scraping atual do SceneSource usa as categorias `tv`, `films/bluray` e `tv/miniseries`, guardando apenas URL do post e metadados.
- A paginação do SceneSource é controlada por `Scraping:SceneSource:MaxPagesPerCategory` e `Scraping:SceneSource:MaxPostsPerPage` em `CineTrack.API/appsettings.json` e também pode ser sobrescrita localmente em `CineTrack.Functions/local.settings.json`.
- A tradução PT-BR usa `LibreTranslate` por padrão (`Translation:Provider=LibreTranslate`, `Translation:BaseUrl=http://localhost:5000/`). Use `Translation:Provider=Disabled` para manter o texto original sem traduzir.
- Para desenvolvimento local, a API e o worker local aceitam `User Secrets`; mantenha os placeholders versionados no repositório e injete os segredos com `dotnet user-secrets`.

### User Secrets local
- Inicializar secrets da API: `dotnet user-secrets --project CineTrack.API/CineTrack.API.csproj set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"`
- Inicializar chave da API direta: `dotnet run --project CineTrackTokenGen/CineTrackTokenGen.csproj`
- Configurar chave na API: `dotnet user-secrets --project CineTrack.API/CineTrack.API.csproj set "Security:ApiKey" "SUA_CHAVE_GERADA"`
- Configurar datasets IMDb: `dotnet user-secrets --project CineTrack.API/CineTrack.API.csproj set "ImdbDatasets:Directory" "D:\Datasets\imdb"`
- Inicializar secrets do worker local: `dotnet user-secrets --project CineTrack.Functions/CineTrack.Functions.csproj set "SqlConnectionString" "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"`
- Exemplo completo de chaves locais: ver `secrets.example.json` na raiz do workspace.

## Roteiro local executável
- Bootstrap seguro do banco local: `./scripts/bootstrap-local-db.ps1`
- Subida da API com SQL local: `./scripts/start-local-api.ps1`
- Simulação de backfill de tradução: `./scripts/simulate-translation-backfill.ps1`
- Build do Android: `./scripts/run-mobile-android.ps1`
- Build do Android limpando `obj`: `./scripts/run-mobile-android.ps1 -CleanObj`
- Rodar app Android em emulador/device: `./scripts/run-mobile-android.ps1 -Run`
- Preparar tudo e imprimir próximos passos: `./scripts/start-local-stack.ps1`

### Connection string local padrão
- `Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True`

### Sequência recomendada
1. `./scripts/bootstrap-local-db.ps1`
2. `./scripts/start-local-api.ps1`
3. Em outro terminal: `Invoke-WebRequest http://localhost:5050/health`
4. Em outro terminal: `./scripts/run-mobile-android.ps1 -Run`

### Exemplo de simulação de custo de tradução
- Amostra rápida de 10 grupos do SceneSource: `./scripts/simulate-translation-backfill.ps1 -MaxGrupos 10`
- Ajuste de preço/franquia: `./scripts/simulate-translation-backfill.ps1 -PrecoUsdPorMilhao 0 -FranquiaGratisCaracteres 0`
- Ajuste de fallback quando IMDb não devolver sinopse/descrição: `./scripts/simulate-translation-backfill.ps1 -EstimativaCaracteresMidiaSemTexto 600 -EstimativaCaracteresEpisodioSemTexto 350`
