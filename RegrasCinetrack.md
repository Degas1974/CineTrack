# RegrasCinetrack

## Leitura obrigatória antes de codificar
- Antes de criar, alterar ou revisar código neste workspace, leia este arquivo por completo.
- Use `CineTrack-Analise.md` como referência inicial para entender a arquitetura, os fluxos e a operação do sistema.
- A solução principal deste workspace é `CineTrack.sln`.

## Bootstrap de sessão
- Antes de iniciar uma nova tarefa, releia `RegrasCinetrack.md` e consulte `CineTrack-Analise.md` se a demanda tocar arquitetura, fluxos, integração ou operação.
- Confirme que o `dotnet` na raiz do workspace está respeitando o SDK fixado em `global.json` antes de investigar falhas de build ou restore.
- Confirme que o escopo de trabalho está dentro da solução principal `CineTrack.sln` e priorize os projetos da raiz vinculados a ela.
- Identifique quais camadas serão impactadas pela tarefa: API, Mobile, Shared, worker local, utilitários, scripts e banco.
- Verifique se a mudança exige compatibilização entre contrato de API, consumo no app, persistência Dapper, SQL script, worker local ou configuração de API direta.
- Antes de propor nova estrutura, confirme se já existe implementação ou padrão equivalente no código atual.
- Ao finalizar uma tarefa que gere regra duradoura, registre a regra neste arquivo e atualize `CineTrack-Analise.md` se houver impacto arquitetural ou operacional.

## Escopo principal
- Priorize os projetos na raiz do workspace vinculados à solução principal:
  - `CineTrack.API`
  - `CineTrack.Mobile`
  - `CineTrack.Shared`
  - `CineTrack.Functions` (worker local)
  - `CineTrackTokenGen`
- Trate os projetos e arquivos da raiz vinculados a `CineTrack.sln` como fonte primária de desenvolvimento.

## Arquitetura e responsabilidades
- `CineTrack.API`: expõe a API REST e concentra a superfície HTTP da aplicação.
- `CineTrack.Mobile`: interface MAUI + Blazor; deve consumir a API, sem acesso direto ao banco.
- `CineTrack.Shared`: modelos, repositórios Dapper, enums e serviços compartilhados; concentre aqui lógica de domínio, acesso a dados e componentes reutilizáveis.
- `CineTrack.Functions`: worker local/console para automações e scraping manual ou agendado fora de runtime de nuvem.
- `CineTrackRelayListener`: legado desativado; não deve ser usado no fluxo oficial.
- `CineTrackTokenGen`: utilitário isolado para geração de `Security:ApiKey` da API direta.

## Regras de desenvolvimento
- Preservar a separação de responsabilidades entre os projetos; evitar duplicação de lógica entre API, worker local e Mobile.
- Preferir reutilização em `CineTrack.Shared` quando a lógica for de domínio, scraping compartilhado, acesso a dados ou modelo comum.
- Manter o app mobile desacoplado do banco; integrações devem passar pela API ou por clientes de serviço explicitamente definidos.
- Em mudanças de scraping, isolar parsing e considerar que IMDb e SceneSource podem mudar o HTML com frequência.
- A tradução PT-BR de descrições vindas do IMDb usa `LibreTranslate` por padrão, com `Translation:Provider=Disabled` permitido para manter texto original em ambientes sem tradutor local.
- Em mudanças de integração e infraestrutura, evitar novos segredos hardcoded; preferir configuração externa, variáveis de ambiente ou Secret Manager.
- Respeitar a base de dados e objetos já existentes em `CineTrack-Database.sql` antes de propor renomeações ou recriações de tabelas, views e procedures.
- Manter compatibilidade com o stack atual descrito em `CineTrack-Analise.md`: API ASP.NET Core, MAUI + Blazor, Dapper, worker local, API HTTP direta, LibreTranslate e SQL Server.

## Diretrizes práticas
- Antes de mudanças estruturais, revisar o impacto entre API, Mobile, Shared, worker local, utilitários e banco.
- Ao adicionar funcionalidade nova, seguir os fluxos já existentes de catálogo, progresso, estatísticas e sync quando aplicável.
- Ao alterar contratos de API, revisar o consumo correspondente no app mobile.
- Ao alterar persistência, revisar repositórios Dapper, views e procedures relacionadas.
- Ao alterar autenticação ou acesso remoto, revisar também mobile, API, `CineTrackTokenGen`, scripts e documentação operacional.
- Ao encontrar divergência entre documentação e código, considerar o código atual e `CineTrack.sln` como fonte de verdade.

## Padrões práticos de implementação

### API ASP.NET Core
- Manter controllers enxutos, focados em orquestração HTTP, validação de entrada simples e retorno de `ActionResult` apropriado.
- Preservar o padrão de rotas já adotado em `api/[controller]`, expandindo endpoints de forma consistente com os recursos existentes.
- Preferir injeção por interface para repositórios e serviços registrados no `Program.cs`.
- Ao criar novos endpoints, revisar impacto no cliente mobile e manter contratos previsíveis para query string, payload e códigos HTTP.
- Usar `NotFound`, `Ok`, `BadRequest` e outros retornos HTTP explícitos quando houver cenários distintos de resposta.
- Evitar colocar regra de negócio pesada dentro de controller; quando necessário, concentrar a lógica compartilhável em `CineTrack.Shared`.

### Repositórios Dapper e acesso a dados
- Manter acesso a dados centralizado em `CineTrack.Shared/Data` por meio de interfaces e implementações específicas por agregado ou fluxo.
- Abrir conexões com `SqlConnection` no escopo do método, mantendo o padrão atual de uso curto e explícito.
- Preferir queries parametrizadas com Dapper para qualquer dado variável, principalmente entradas vindas do usuário.
- Evitar interpolação direta de valores em SQL; mesmo quando o valor parecer seguro, priorizar parâmetros para manter consistência e reduzir risco.
- Reutilizar views, stored procedures e estruturas já existentes em `CineTrack-Database.sql` antes de criar novas consultas ou objetos.
- Ao alterar persistência, revisar nomes de colunas, view models e impacto em todos os repositórios e endpoints consumidores.

### MAUI e Blazor
- Manter a UI desacoplada da persistência; o app deve conversar com a API por meio de clientes de serviço, não com SQL diretamente.
- Centralizar chamadas HTTP no `CineTrackApiClient` ou em serviços equivalentes, evitando espalhar acesso à API por componentes Razor.
- Preservar o padrão atual de helpers HTTP e serialização consistente ao adicionar novos métodos no cliente.
- Ao criar páginas ou componentes, alinhar navegação e fluxo com a estrutura já existente em `Components`, `Routes` e páginas principais do app.
- Registrar serviços no `MauiProgram.cs` de forma explícita e manter o startup simples.
- Ao alterar contratos consumidos pelo app, atualizar o cliente e revisar todas as telas impactadas.

### Configuração e segredos
- Não introduzir novos segredos hardcoded em código-fonte, inclusive connection strings, tokens, chaves e endpoints sensíveis.
- Sempre que tocar configurações existentes hoje hardcoded, preferir evoluir para `appsettings`, `local.settings.json`, variáveis de ambiente, User Secrets ou mecanismo equivalente.
- Tratar valores locais de desenvolvimento e valores de produção separadamente.
- Se uma alteração depender de configuração externa, documentar claramente onde ela deve ser fornecida.
- Em mudanças de startup, preservar comportamento local conhecido, mas reduzir acoplamento com valores fixos sempre que possível.
- Tratar `global.json` como a fonte de verdade da versão do SDK .NET usada pelo workspace quando houver múltiplos SDKs instalados na máquina.
- Preservar `Directory.Build.rsp` enquanto o workspace depender do workaround `AllowMissingPrunePackageData=true` para evitar `NETSDK1226` no restore/build com .NET 10.

### Worker local e API direta
- Manter `CineTrack.Functions` focado em automação local, scraping, status e tarefas agendadas fora de runtime de nuvem.
- Não duplicar regra de domínio entre API e worker local; compartilhar o que fizer sentido em `CineTrack.Shared`.
- Manter `CineTrackRelayListener` legado/desativado, sem novas dependências ou fluxos nessa camada.
- Ao alterar headers, autenticação ou roteamento da API direta, validar o impacto na API e no cliente mobile.
- Ao mexer em scraping, considerar resiliência a mudanças de HTML e a falhas externas de rede ou parsing.

### Banco e script SQL
- Tratar `CineTrack-Database.sql` como referência central dos objetos do banco utilizados pela solução.
- O acesso exploratório e operacional aos dados deve ser feito via extensão MSSQL no VS Code, evitando consultas improvisadas fora do fluxo controlado do workspace.
- Sempre que houver necessidade de criar, alterar ou remover estrutura de banco, a mudança deve ser materializada em script SQL versionável.
- Antes de criar tabela, view ou procedure nova, verificar se já existe objeto equivalente ou extensível no script atual.
- Em mudanças de banco, manter alinhamento com repositórios Dapper, view models e endpoints expostos.
- Evitar mudanças destrutivas sem necessidade; preferir evoluções compatíveis com o código atual da solução principal.

### Qualidade de implementação
- Seguir o estilo já presente no projeto: nomes claros, classes pequenas, responsabilidades bem separadas e mínima complexidade acidental.
- Corrigir a causa raiz quando possível, em vez de introduzir remendos locais que espalhem duplicação.
- Manter mudanças focadas no escopo solicitado, sem refatorações amplas não relacionadas.
- Ao introduzir novo padrão duradouro de código, registrar a diretriz neste arquivo.

## Checklists operacionais por tipo de tarefa

### Ao criar ou alterar endpoint na API
- Confirmar se a funcionalidade pertence mesmo à camada `CineTrack.API`.
- Verificar se já existe endpoint ou fluxo semelhante antes de criar nova rota.
- Manter controller enxuto, com validação simples de entrada e retorno HTTP explícito.
- Confirmar quais repositórios, models e view models serão impactados.
- Revisar o contrato consumido pelo app e atualizar o cliente mobile correspondente.
- Validar impacto em rotas relacionadas, estatísticas, sync ou health quando aplicável.

### Ao mexer no banco ou no acesso a dados
- Confirmar se a necessidade exige mudança em tabela, view, procedure ou apenas ajuste em consulta existente.
- Acessar e inspecionar dados sempre via extensão MSSQL no VS Code quando houver necessidade de consulta ao banco.
- Sempre converter mudanças estruturais de banco em script SQL versionável, mesmo quando a alteração parecer pequena.
- Verificar primeiro `CineTrack-Database.sql` antes de criar novos objetos.
- Atualizar repositórios Dapper e models/view models em conjunto quando houver mudança estrutural.
- Preferir parâmetros Dapper em toda entrada variável.
- Revisar impacto em endpoints, worker local, scraping e telas que consomem os dados alterados.
- Evitar mudanças destrutivas sem necessidade e preservar compatibilidade com o código atual.

### Ao alterar o app mobile
- Confirmar qual endpoint e qual contrato a tela consome.
- Centralizar novas chamadas HTTP no cliente de API ou serviço equivalente.
- Evitar lógica de persistência ou regra de domínio espalhada em componentes Razor.
- Revisar navegação, estados vazios, tratamento de erro e carregamento da tela alterada.
- Garantir alinhamento entre nomes de propriedades, serialização e payloads retornados pela API.
- Se a mudança alterar fluxo principal, refletir isso também em `CineTrack-Analise.md`.

### Ao mexer em scraping ou automações
- Confirmar se a lógica deve ficar em `CineTrack.Functions`, `CineTrack.Shared` ou em ambos.
- Reutilizar scrapers e serviços compartilhados antes de duplicar parsing.
- Considerar falhas de rede, mudanças no HTML externo e cenários de dados incompletos.
- Revisar persistência de logs, associações pendentes e atualização de última sync.
- Validar impacto na API e na tela Sync do app quando o fluxo exposto ao usuário mudar.

### Ao mexer em API direta, tokens ou configuração
- Confirmar se a mudança é de transporte, autenticação ou configuração de ambiente.
- Evitar ampliar o uso de segredos hardcoded; preferir mover para configuração externa.
- Revisar juntos `CineTrackTokenGen`, cliente mobile, API e scripts quando houver alteração de endpoint, chave ou header.
- Documentar onde a configuração deve ser preenchida em ambiente local e produção.
- Preservar o comportamento local esperado enquanto evolui a segurança/configuração.

### Ao concluir qualquer tarefa relevante
- Verificar se houve impacto cruzado entre API, Shared, Mobile, worker local, utilitários, scripts e banco.
- Atualizar `RegrasCinetrack.md` se a tarefa introduzir uma nova regra duradoura.
- Atualizar `CineTrack-Analise.md` se houver mudança arquitetural, operacional ou de fluxo funcional.
- Se a tarefa alterar comportamento de toolchain, restore, workload ou versão do SDK, atualizar também `global.json`, `Directory.Build.rsp`, `README.md` e a seção operacional de `CineTrack-Analise.md` quando aplicável.
- Manter a documentação alinhada ao código real da solução principal `CineTrack.sln`.

## Anti-padrões a evitar
- Não acessar o banco por caminhos paralelos quando a necessidade puder ser atendida pela extensão MSSQL do VS Code.
- Não aplicar mudança estrutural diretamente no banco sem gerar o respectivo script SQL versionável.
- Não duplicar regra de negócio entre `CineTrack.API`, `CineTrack.Functions` (worker local) e `CineTrack.Mobile`.
- Não espalhar chamadas HTTP da API por componentes Razor quando elas puderem ficar centralizadas no cliente de serviço.
- Não ampliar o uso de connection strings, tokens, chaves ou endpoints sensíveis hardcoded no código.
- Não criar rotas, queries, views ou procedures novas sem antes verificar se já existe estrutura equivalente no projeto.
- Não corrigir sintomas no app, na API ou no banco sem investigar a causa raiz do fluxo completo.
- Não depender implicitamente do SDK “ativo da máquina” quando o workspace já tiver `global.json` definindo a versão esperada.

## Memória viva do projeto
- Registre neste arquivo toda nova regra duradoura de desenvolvimento definida ao longo do projeto.
- Atualize `CineTrack-Analise.md` quando houver mudança relevante de arquitetura, fluxo, responsabilidades ou operação.
- Não espalhe regras permanentes em múltiplos arquivos sem necessidade; este arquivo deve ser o ponto central.
