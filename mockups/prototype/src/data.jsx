// data.jsx — sample CineTrack data: posters from TMDB CDN, plus stats/sync mocks.
// Posters are public TMDB image URLs (image.tmdb.org/t/p/w500/...).

const POSTER = (path) => `https://image.tmdb.org/t/p/w500${path}`;

const MEDIA = [
  // EM ANDAMENTO (séries que está assistindo)
  { id: 1, title: 'Severance', original: 'Severance', year: 2022, type: 'serie', imdb: 8.7, tomato: 95, popcorn: 84, user: 5, status: 'Assistindo',
    poster: POSTER('/lFf6LLrQjYldcZItzOkGmMMigP7.jpg'),
    backdrop: POSTER('/n91kGHMSdsVQs9LMmQyEgcXG7XW.jpg'),
    duration: 55, genres: ['Drama','Mistério','Sci-Fi'],
    desc: 'Mark lidera uma equipe de funcionários de escritório cujas memórias foram cirurgicamente divididas entre suas vidas profissional e pessoal.',
    progress: { watched: 14, total: 19 } },
  { id: 2, title: 'O Urso', original: 'The Bear', year: 2022, type: 'serie', imdb: 8.6, tomato: 99, popcorn: 87, user: 4, status: 'Assistindo',
    poster: POSTER('/zPyHvRUjAwjr2KkLD1198BClMOq.jpg'),
    backdrop: POSTER('/v8d4Twd6dG6tWpa3DyjwdKlfZx2.jpg'),
    duration: 30, genres: ['Drama','Comédia'],
    desc: 'Um jovem chef volta a Chicago para administrar a lanchonete da sua família após uma tragédia.',
    progress: { watched: 18, total: 28 } },
  { id: 3, title: 'Shōgun', original: 'Shōgun', year: 2024, type: 'serie', imdb: 8.7, tomato: 99, popcorn: 91, user: null, status: 'Assistindo',
    poster: POSTER('/7O4iVfOMQmdCSxhOg1WnzG1AjwR.jpg'),
    backdrop: POSTER('/tiPXfKKGmTSj9Pf6X7DJTzSLuAS.jpg'),
    duration: 60, genres: ['Drama','História','Aventura'],
    desc: 'No Japão feudal de 1600, um navegador inglês se vê preso em uma terra cujos costumes ele não compreende.',
    progress: { watched: 4, total: 10 } },

  // SUGESTÕES
  { id: 4, title: 'Duna: Parte Dois', original: 'Dune: Part Two', year: 2024, type: 'filme', imdb: 8.5, tomato: 92, popcorn: 95, user: null, status: 'Pendente',
    poster: POSTER('/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg'),
    backdrop: POSTER('/87ngrAlPneGzgwDdebCkeHQDtNN.jpg'),
    duration: 167, genres: ['Sci-Fi','Aventura'],
    desc: 'Paul Atreides se une aos Fremen para buscar vingança contra os conspiradores que destruíram sua família.' },
  { id: 5, title: 'Pobres Criaturas', original: 'Poor Things', year: 2023, type: 'filme', imdb: 7.8, tomato: 92, popcorn: 76, user: null, status: 'Pendente',
    poster: POSTER('/kCGlIMHnOm8JPXq3rXM6c5wMxcT.jpg'),
    backdrop: POSTER('/5JUtvvQOyZmhpYyRBTORQE5krBQ.jpg'),
    duration: 141, genres: ['Comédia','Drama','Romance'] },
  { id: 6, title: 'Anora', original: 'Anora', year: 2024, type: 'filme', imdb: 7.6, tomato: 89, popcorn: 75, user: null, status: 'Pendente',
    poster: POSTER('/qxMK1V0o6kgkH85b8iDrcMkmqLW.jpg'),
    backdrop: POSTER('/eOnLPDsw3veMGvVT4EJ41xFAtjE.jpg'),
    duration: 139, genres: ['Drama','Comédia'] },
  { id: 7, title: 'A Substância', original: 'The Substance', year: 2024, type: 'filme', imdb: 7.5, tomato: 90, popcorn: 71, user: null, status: 'Pendente',
    poster: POSTER('/lqoMzCcZYEFK729d6qzt349fB4o.jpg'),
    backdrop: POSTER('/t98L9uphqBSNn2Mkvdm3xUDWuzd.jpg'),
    duration: 141, genres: ['Horror','Sci-Fi'] },

  // RECENTES (assistidos)
  { id: 8, title: 'Oppenheimer', original: 'Oppenheimer', year: 2023, type: 'filme', imdb: 8.3, tomato: 93, popcorn: 91, user: 5, status: 'Assistido',
    poster: POSTER('/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg'),
    backdrop: POSTER('/rLb2cwF3Pazuxaj0sRXQ037tGI1.jpg'),
    duration: 181, genres: ['Drama','História'] },
  { id: 9, title: 'Succession', original: 'Succession', year: 2018, type: 'serie', imdb: 8.9, tomato: 94, popcorn: 86, user: 5, status: 'Assistido',
    poster: POSTER('/7HW47XbkNQ5fiwQFYGWdw9gs144.jpg'),
    backdrop: POSTER('/vjnVXopAcU9eLly3rDQxnpXadRr.jpg'),
    duration: 60, genres: ['Drama'] },
  { id: 10, title: 'Anatomia de uma Queda', original: 'Anatomie d\'une chute', year: 2023, type: 'filme', imdb: 7.7, tomato: 96, popcorn: 79, user: 4, status: 'Assistido',
    poster: POSTER('/kQs6keheMwCxJxrzV83VUwFtHkB.jpg'),
    duration: 152, genres: ['Drama','Mistério'] },
  { id: 11, title: 'True Detective', original: 'True Detective', year: 2014, type: 'serie', imdb: 8.9, tomato: 81, popcorn: 81, user: 5, status: 'Assistido',
    poster: POSTER('/eIVeQDKvxaJfLLYuKKkrXlYMxBs.jpg'),
    duration: 55, genres: ['Crime','Drama','Mistério'] },
  { id: 12, title: 'Past Lives', original: 'Past Lives', year: 2023, type: 'filme', imdb: 7.8, tomato: 96, popcorn: 86, user: 4, status: 'Assistido',
    poster: POSTER('/k3waqVXSnvCZWfJYNtdamTgTtTA.jpg'),
    duration: 105, genres: ['Romance','Drama'] },
];

const FIND_MEDIA = (id) => MEDIA.find(m => m.id === id) || MEDIA[0];

// Stats data (Wrapped-style)
const STATS = {
  totalFilmes: 87,
  totalSeries: 24,
  totalEpisodios: 612,
  episodiosAssistidos: 487,
  horasAssistidas: 412, // hours
  seriesCompletas: 14,
  filmesAssistidos: 73,
  emAndamento: 3,
  pendentes: 18,
  // genres distribution
  generos: [
    { name: 'Drama', count: 42, pct: 38 },
    { name: 'Sci-Fi', count: 28, pct: 25 },
    { name: 'Mistério', count: 18, pct: 16 },
    { name: 'Comédia', count: 12, pct: 11 },
    { name: 'Horror', count: 7, pct: 6 },
    { name: 'Romance', count: 4, pct: 4 },
  ],
  // monthly activity (last 12 months)
  atividade: [
    { m: 'Mai', v: 8 }, { m: 'Jun', v: 14 }, { m: 'Jul', v: 22 },
    { m: 'Ago', v: 18 }, { m: 'Set', v: 11 }, { m: 'Out', v: 24 },
    { m: 'Nov', v: 31 }, { m: 'Dez', v: 28 }, { m: 'Jan', v: 19 },
    { m: 'Fev', v: 23 }, { m: 'Mar', v: 35 }, { m: 'Abr', v: 17 },
  ],
  // top rated by user
  topRated: [8, 9, 11, 1, 2],
  // longest binges
  binges: [
    { mediaId: 9, eps: 39, days: 11 },
    { mediaId: 11, eps: 24, days: 8 },
    { mediaId: 1, eps: 14, days: 4 },
  ],
  ultimaCaptura: '27/04/2026 21:14',
  decade: { '2020': 64, '2010': 32, '2000': 11, '1990': 4 },
};

// Sync data
const SYNC_DATA = {
  isConnected: true,
  ultimaSync: '27/04/2026 21:14',
  novosItens: 7,
  itensAtualizados: 3,
  pendentes: [
    { id: 101, capturado: 'severance.s02e08.cold.harbor.2160p.dsnp.web-dl', match: 'Severance', confianca: 96, releases: [
      { id: 101, label: '2160P • WEB-DL • DSNP • H265', score: 96, raw: 'severance.s02e08.cold.harbor.2160p.dsnp.web-dl.h265-flux' },
      { id: 102, label: '1080P • WEB-DL • DSNP', score: 91, raw: 'severance.s02e08.cold.harbor.1080p.dsnp.web-dl.x264-ntb' },
    ]},
    { id: 103, capturado: 'the.bear.s03e05.children.1080p.hulu.webrip', match: 'O Urso', confianca: 88, releases: [
      { id: 103, label: '1080P • WEBRip • HULU', score: 88, raw: 'the.bear.s03e05.children.1080p.hulu.webrip.x265-mince' },
    ]},
    { id: 104, capturado: 'shogun.2024.s01e09.crimson.sky.1080p', match: 'Shōgun', confianca: 73, releases: [
      { id: 104, label: '1080P • WEB-DL', score: 73, raw: 'shogun.2024.s01e09.crimson.sky.1080p.dsnp.web-dl.ddp5.1.x264-ntb' },
    ]},
    { id: 105, capturado: 'past.lives.2023.1080p.bluray', match: 'Past Lives', confianca: 64, releases: [
      { id: 105, label: '1080P • BluRay', score: 64, raw: 'past.lives.2023.1080p.bluray.x264-rovers' },
    ]},
  ],
  logs: [
    { tipo: 'sucesso', data: '27/04 21:14', msg: '7 novos itens importados' },
    { tipo: 'info', data: '27/04 21:13', msg: 'Conectado ao servidor de scraping' },
    { tipo: 'aviso', data: '27/04 19:02', msg: '3 capturas com confiança baixa (<70%)' },
    { tipo: 'sucesso', data: '27/04 18:30', msg: 'Sincronização agendada concluída' },
    { tipo: 'erro',    data: '26/04 11:42', msg: 'Timeout ao buscar metadata para "shogun.s01e10"' },
    { tipo: 'info',    data: '26/04 09:15', msg: '12 episódios atualizados' },
  ],
  diagnostico: [
    { titulo: 'Severance', episodio: 'Cold Harbor', codigo: 'S02E08', ano: 2025, claridade: 'muito-claro', score: 124, modo: 'SxxEyy', resumo: 'Padrão SxxEyy detectado com alta confiança. Provider e codec identificados.' },
    { titulo: 'The Bear', episodio: 'Children', codigo: 'S03E05', ano: 2024, claridade: 'claro', score: 102, modo: 'SxxEyy', resumo: 'Episódio reconhecido. Título traduzido associado via heurística de slug.' },
    { titulo: 'Shōgun', episodio: 'Crimson Sky', codigo: 'S01E09', ano: 2024, claridade: 'moderado', score: 78, modo: 'SxxEyy', resumo: 'Episódio reconhecido. Caractere especial no título original pode causar conflito de busca.' },
    { titulo: 'Past Lives', episodio: null, codigo: null, ano: 2023, claridade: 'ambiguo', score: 42, modo: 'Generico', resumo: 'Parser caiu em heurística genérica; vale revisar essa captura manualmente.' },
  ],
};

Object.assign(window, { MEDIA, FIND_MEDIA, STATS, SYNC_DATA, POSTER });
