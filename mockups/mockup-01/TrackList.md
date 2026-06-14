# TrackList — Guia de Dados (atualizado 11/06/2026)

**Regra de ouro:** o conteúdo (números, textos, %, status) fica **todo** no arquivo `data.jsx`.
O design (cores, bordas, espaçamentos) fica nos arquivos das telas e **nunca** precisa ser tocado pra mudar conteúdo.

Todo o conteúdo é exposto em `window.TL`:

| Chave           | Para que serve                            |
| --------------- | ----------------------------------------- |
| `MEDIA`         | catálogo de filmes/séries                 |
| `HOME`          | trilhos da tela Início                    |
| `STATS`         | números da tela Estatísticas              |
| `SYNC`          | dados da tela Sincronização               |
| `DISCOVER`      | buscar: chips recentes + grade de gêneros |
| `DIAGNOSTIC`    | aba Diagnóstico dentro de Sync            |
| `HISTORY`       | histórico de atividade                    |
| `LISTS`         | listas pessoais iniciais                  |
| `NOTIFICATIONS` | notificações do sino                      |

---

## 1. Catálogo — `MEDIA`

Cada item tem estes campos:

| Campo      | Exemplo                         | Onde aparece                                              |
| ---------- | ------------------------------- | --------------------------------------------------------- |
| `id`       | `"archive"`                     | chave interna — deve ser único, sem espaços               |
| `type`     | `"serie"` / `"filme"`           | tag, ícone do pôster, comportamento                       |
| `title`    | `"The Last Archive"`            | título principal em toda a interface                      |
| `original` | `"Arquivo Final"`               | subtítulo em itálico (omitido se igual a `title`)         |
| `year`     | `2025`                          | ano de lançamento                                         |
| `runtime`  | `"52 min"`                      | duração por ep (série) ou total (filme)                   |
| `tint`     | `"cool"` / `"warm"` / `"green"` | cor do gradiente do pôster e backdrop                     |
| `imdb`     | `8.6`                           | card IMDb com estrela                                     |
| `tomato`   | `94`                            | card Crítica → `94%`                                      |
| `popcorn`  | `91`                            | card Público → `91%`                                      |
| `mine`     | `4` / `null`                    | card Minha nota; `null` → `—`                             |
| `genres`   | `["Drama","Sci-Fi"]`            | chips de gênero no detalhe                                |
| `status`   | `"assistindo"`                  | etiqueta colorida (ver tabela abaixo)                     |
| `progress` | `{ done: 18, total: 28 }`       | barra de progresso (só séries)                            |
| `synopsis` | string                          | texto da sinopse (exibe "Ver mais" se > 4 linhas)         |
| `removed`  | `true`                          | se presente, abrir o título cai em "Mídia não encontrada" |
| `seasons`  | array                           | temporadas e episódios (só séries)                        |

### Status → cor automática

| Valor          | Rótulo     | Cor      |
| -------------- | ---------- | -------- |
| `"assistindo"` | ASSISTINDO | âmbar    |
| `"assistido"`  | ASSISTIDO  | verde    |
| `"pendente"`   | PENDENTE   | cinza    |
| `"desistido"`  | DESISTIR   | vermelho |

### Temporadas — `seasons`

```js
seasons: [
    {
        n: 1,
        episodes: [
            { e: 1, title: "O Índice", watched: true, rating: 5 }, // ✓ visto + nota
            { e: 3, title: "Ruído de Fundo", watched: false, current: true }, // próximo a ver
        ],
    },
];
```

- `watched: true` → círculo âmbar com ✓; `false` → círculo vazio
- `current: true` → marca o próximo episódio (alimenta o widget da home)
- `rating` → estrelinhas inline (1–5); omita se não avaliado

---

## 2. Início — `HOME`

```js
const HOME = {
    continue: ["archive", "orbital"], // ← o 1º vira o HERO (destaque grande)
    suggest: ["solar", "northline", "vento"], // trilho "Sugestões"
    recent: ["cinzas", "estacao", "archive"], // trilho "Recentemente assistidos"
};
```

Cada lista usa `id`s do `MEDIA`. O **widget "Próximo episódio"** usa automaticamente o 1º item de `continue` que seja série com episódio `current: true`.

---

## 3. Estatísticas — `STATS`

| Campo                         | Card na tela                                  |
| ----------------------------- | --------------------------------------------- |
| `filmes` `series` `episodios` | primeiros 3 cards                             |
| `epsAssistidos`               | "Eps. assistidos" (verde)                     |
| `tempo`                       | "Tempo assistido" (texto livre, ex: `"312h"`) |
| `completas`                   | "Séries completas" (âmbar)                    |
| `avg`                         | "Nota média" → `7.9 /10` + estrelas           |
| `month`                       | `{ titles: 9, hours: 41 }` → "Este mês"       |
| `dist`                        | barras "Distribuição por status"              |
| `genres`                      | barras "Gêneros favoritos"                    |
| `lastSync`                    | rodapé "Última sincronização"                 |

Barras usam `{ label, value, max }` — `value/max` define o comprimento.

---

## 4. Sincronização — `SYNC`

| Campo                            | Na tela                                                       |
| -------------------------------- | ------------------------------------------------------------- |
| `filmes` `series` `episodios`    | 3 cards de números                                            |
| `pending`                        | cards de associação pendente + contador automático com plural |
| `resolved`                       | lista "Escolhas recentes" (pós-sync)                          |
| `lastSyncOk` / `lastSyncPending` | datas nos banners                                             |

### Item pendente

```js
{ source: "Captura IMDb", target: "Northline", conf: 58, level: "low" }
```

- `conf` → porcentagem exibida
- `level: "low"` → % em **vermelho**; `"ok"` → âmbar

---

## 5. Diagnóstico — `DIAGNOSTIC`

```js
const DIAGNOSTIC = {
  total: 12,                                          // número no canto do header
  filters: ["Todos", "Ambíguo", "Baixa conf.", ...], // chips de filtro
  counts: [{ n: 4, label: "Ambíguos" }, ...],        // 4 cards de resumo
  entries: [
    { head: "…", title: "…", tags: ["…"], body: "…", tone: "warn" }, // barra âmbar
    { head: "…", title: "…", tags: ["…"], body: "…", tone: "info" }, // barra cinza
  ],
};
```

---

## 6. Buscar — `DISCOVER`

```js
const DISCOVER = {
  recent: ["Noite Solar", "sci-fi", "Orbital"], // chips "Buscas recentes" (clicáveis)
  genres: ["Sci-Fi", "Drama", "Thriller", ...], // grade "Explorar gêneros"
};
```

Os **resultados** da busca vêm do `MEDIA` filtrado em tempo real. O contador `"N resultados"` é calculado automaticamente.

---

## 7. Histórico — `HISTORY`

```js
const HISTORY = [
    {
        date: "Hoje", // rótulo exibido (texto livre)
        entries: [
            {
                mediaId: "archive",
                action: "ep",
                detail: "E03 · Ruído de Fundo",
                time: "23:14",
            },
            {
                mediaId: "cinzas",
                action: "rate",
                detail: "9/10 · 5 estrelas",
                time: "22:10",
            },
            {
                mediaId: "solar",
                action: "add",
                detail: "Adicionado",
                time: "18:30",
            },
            {
                mediaId: "orbital",
                action: "status",
                detail: "Status → Assistindo",
                time: "19:15",
            },
        ],
    },
];
```

### Tipos de ação → cor automática

| `action`   | Rótulo    | Cor     |
| ---------- | --------- | ------- |
| `"ep"`     | Assistiu  | âmbar   |
| `"rate"`   | Avaliou   | dourado |
| `"status"` | Status    | verde   |
| `"add"`    | Adicionou | cinza   |

---

## 8. Listas — `LISTS`

```js
const LISTS = [
    {
        id: "fav",
        name: "Favoritos",
        tint: "warm",
        icon: "♥",
        items: ["cinzas", "archive"],
    },
    {
        id: "watchlist",
        name: "Quero Assistir",
        tint: "cool",
        icon: "▷",
        items: ["solar", "northline"],
    },
];
```

- `id` — único, sem espaços
- `tint` — cor do card (`"cool"` / `"warm"` / `"green"`)
- `icon` — emoji ou símbolo exibido no card
- `items` — lista de `id`s do `MEDIA`

O usuário pode criar listas novas e adicionar/remover títulos dentro do app — essas mudanças ficam no estado local (não persistem ao recarregar). Para valores fixos como ponto de partida, edite aqui.

---

## 9. Notificações — `NOTIFICATIONS`

```js
const NOTIFICATIONS = [
    {
        id: 1,
        type: "episode",
        mediaId: "archive",
        title: "Novo episódio disponível",
        detail: "The Last Archive · S02E04 · Marginália",
        time: "2h atrás",
        read: false,
    },
    {
        id: 2,
        type: "sync",
        mediaId: null,
        title: "Sincronização pendente",
        detail: "2 associações aguardam confirmação",
        time: "5h atrás",
        read: false,
    },
];
```

- `type` → `"episode"` (ícone TV âmbar) | `"sync"` (ícone sync) | `"alert"` (ícone verde)
- `read: false` → aparece o badge no sino; `true` → entrada acinzentada
- `mediaId: null` → sem mini-pôster

---

## Como trocar conteúdo (passo a passo)

1. Abra **`data.jsx`**
2. Encontre a seção certa (cada uma tem um comentário de cabeçalho)
3. Mude o **valor** — adicione/remova itens de listas, altere números, edite strings
4. Salve — a tela se redesenha com **o mesmo design e as mesmas cores**

> No app real, este objeto é entregue pela API (IMDb/TMDB/backend). A interface é o molde — só lê os campos.
