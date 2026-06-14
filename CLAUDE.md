# CineTrack — Contexto do Projeto

## O que é
App móvel (.NET MAUI Blazor) para rastrear filmes e séries assistidos. O usuário marca o que assistiu, avalia com estrelas, sincroniza via scraping e acompanha estatísticas.

## Arquivos originais (.razor)
Os mockups originais do desenvolvedor estão em `uploads/`:
- `Main.razor` — shell principal com navegação e roteamento entre páginas
- `Home.razor` — tela inicial com seções "Em andamento", "Sugestões", "Recentes"
- `Search.razor` — busca com filtros por tipo (Filme/Série) e status
- `Detail.razor` — detalhe de uma mídia: ratings, status, avaliação, temporadas/episódios
- `Stats.razor` — estatísticas gerais do usuário
- `Sync.razor` — sincronização via scraping: pendências, logs, diagnóstico do parser
- `MediaCard.razor` — componente de card de mídia reutilizável
- `FaIcon.razor` — componente de ícone FontAwesome via SVG

## Redesign HTML (protótipo navegável)
Abrir `CineTrack.html` — design canvas com 2 variações por tela (A conservador · B ousado).

### Estrutura dos arquivos do redesign
```
CineTrack.html           ← entry point
CineTrack Prompt.html    ← guia de uso com prompts prontos
src/
  data.jsx               ← TODOS OS PLACEHOLDERS estão aqui
  icons.jsx              ← ícones SVG inline
  primitives.jsx         ← tokens de design + PhoneFrame + MediaCard + BottomNav
  screen-home.jsx        ← tela Home (variantes A e B)
  screen-search.jsx      ← tela Search
  screen-detail.jsx      ← tela Detail
  screen-stats.jsx       ← tela Stats
  screen-sync.jsx        ← tela Sync (com 4 abas)
  app.jsx                ← composição final + DesignCanvas + TweaksPanel
```

## Design System
- **Paleta**: `#0a0908` bg, `#e8b94a` gold, `#c64133` red, `#f5ebd6` text
- **Tipografia**: Fraunces (serif, títulos), Inter (sans, UI), JetBrains Mono (meta/código)
- **Tokens**: objeto `THEME` em `src/primitives.jsx` — edite lá para mudar o visual globalmente

## Substituindo placeholders (tudo em src/data.jsx)
- `MEDIA[]` — array de filmes/séries com pôsteres via TMDB (`image.tmdb.org/t/p/w500/...`)
  - Status válidos: `'Assistindo'`, `'Assistido'`, `'Pendente'`, `'Abandonado'`
  - Campo `progress: { watched, total }` — só em séries em andamento
  - Campo `user` (1–5 ou null) → `EstrelaUsuario`
- `STATS{}` — estatísticas: horas, gêneros (`[{ name, count, pct }]`), atividade mensal (`[{ m, v }]`), topRated (array de IDs)
- `SYNC_DATA{}` — isConnected, pendentes, logs, diagnostico

## Instruções para edições futuras
- Sempre manter as 2 variações (A e B) por tela — o prop `variant` controla o visual
- Para adicionar tela nova: criar `src/screen-nova.jsx`, exportar via `Object.assign(window, {...})`, importar em `CineTrack.html`, registrar no `Phone` e no `DesignCanvas` em `app.jsx`
- Pôsteres: `https://image.tmdb.org/t/p/w500/{path}` — trocar função `POSTER()` em `data.jsx` para usar seu CDN
- Adicionar tweak: acrescentar no `TWEAK_DEFAULTS`, no `TweaksPanel` e passar como prop para `Phone`
- Para ver prompts prontos de como pedir alterações com IA: abrir `CineTrack Prompt.html`
