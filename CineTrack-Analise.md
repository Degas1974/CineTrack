# TrackList - Analise Arquitetural

## Visao Geral
TrackList e o app pessoal para acompanhar filmes, series, temporadas, episodios, progresso, status, avaliacoes pessoais e ratings externos.

No MVP, os projetos e namespaces continuam como `CineTrack.*` para reduzir risco operacional, mas a marca exibida e a documentacao funcional usam TrackList.

## Fluxo Oficial
```text
Mobile MAUI/Blazor
   -> API HTTP direta (CineTrack.API)
   -> Shared/Dapper
   -> SQL Server (CineTrackDb)
```

O mobile nao acessa o banco diretamente. Toda leitura e escrita passam pela API.

## Projetos
- `CineTrack.API`: API REST, auth por `X-Api-Key` opcional, endpoints de midias, usuario, estatisticas e sync.
- `CineTrack.Mobile`: app MAUI + Blazor, interface TrackList e cliente HTTP centralizado em `CineTrackApiClient`.
- `CineTrack.Shared`: modelos, repositorios Dapper, scrapers, importador IMDb datasets, provider Rotten Tomatoes e orquestracao de sync.
- `CineTrack.Functions`: worker local/console para sync manual ou agendado fora de runtime de nuvem.
- `CineTrackTokenGen`: gera chaves para `Security:ApiKey`.
- `CineTrackRelayListener`: legado desativado; nao faz parte do fluxo oficial.

## Fontes De Dados
- SceneSource: usa categorias `tv`, `films/bluray` e `tv/miniseries`; guarda apenas URL do post e metadados do release.
- IMDb datasets: fonte principal de catalogo, episodios, ratings, direcao e elenco quando os TSV oficiais estao disponiveis localmente.
- IMDb web fallback: desabilitado por padrao via `ImdbDatasets:FallbackWebEnabled=false`.
- Rotten Tomatoes: provider `Disabled/Manual` por padrao; pronto para trocar por API licenciada.
- Traducao: `LibreTranslate` por padrao, com `Translation:Provider=Disabled` para manter texto original.

## Configuracao Local
- API: `CineTrack.API/appsettings.json`
- Mobile: `CineTrack.Mobile/appsettings.json`
- Worker local: `CineTrack.Functions/local.settings.json`
- Secrets: `scripts/configure-user-secrets.ps1`
- API key: `dotnet run --project CineTrackTokenGen/CineTrackTokenGen.csproj`

## Endpoints De Sync
- `GET /api/sync/fontes`
- `GET /api/sync/diagnostico`
- `POST /api/sync/scraping/executar`
- `POST /api/sync/imdb/importar`
- `POST /api/sync/ratings/reprocessar`

## Operacao Recomendada
1. Aplicar/atualizar `CineTrack-Database.sql`.
2. Configurar `ConnectionStrings:DefaultConnection`.
3. Rodar LibreTranslate localmente ou configurar `Translation:Provider=Disabled`.
4. Baixar os datasets IMDb para um diretorio local e configurar `ImdbDatasets:Directory`.
5. Iniciar a API em `http://localhost:5050`.
6. Configurar o mobile com `scripts/prepare-direct-api-mobile-release.ps1`.
7. Validar com `scripts/smoke-test-direct-api.ps1`.

## Regras Importantes
- Nao guardar links diretos de download vindos de fontes de release.
- Nao fazer scraping direto de Rotten Tomatoes por padrao.
- Nao depender de runtime de nuvem para sync.
- Centralizar chamadas HTTP do mobile em `CineTrackApiClient`.
- Registrar mudancas estruturais de banco em SQL versionavel.
