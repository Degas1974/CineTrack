// screen-stats.jsx — Wrapped-style dashboard, two variants.

function StatsScreen({ variant = 'a', density = 'comfortable' }) {
  const s = STATS;
  const formatHoras = (h) => h < 24 ? `${h}h` : `${Math.floor(h/24)}d ${h%24}h`;
  const maxAtividade = Math.max(...s.atividade.map(a => a.v));

  if (variant === 'b') {
    // BOLD — Wrapped editorial style with big type
    const topGenero = s.generos[0];
    return (
      <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 100 }}>
        <div style={{ padding: '20px 16px 8px' }}>
          <div style={{ fontFamily: THEME.mono, fontSize: 10, color: THEME.gold, letterSpacing: 3 }}>SEU 2026 NO CINEMA</div>
          <h1 style={{ margin: '4px 0 0', fontFamily: THEME.serif, fontSize: 38, fontWeight: 600, lineHeight: 0.95, letterSpacing: -1.5 }}>
            Recapitulando<em style={{ color: THEME.gold, fontStyle: 'italic', fontWeight: 400 }}>.</em>
          </h1>
        </div>

        {/* Hero stat */}
        <div style={{ padding: '24px 16px', borderTop: `1px solid ${THEME.border}`, borderBottom: `1px solid ${THEME.border}`, marginTop: 16 }}>
          <div style={{ fontFamily: THEME.mono, fontSize: 10, color: THEME.text3, letterSpacing: 2 }}>TEMPO TOTAL</div>
          <div style={{ fontFamily: THEME.serif, fontSize: 76, fontWeight: 600, lineHeight: 1, color: THEME.gold, letterSpacing: -3, marginTop: 4 }}>
            {formatHoras(s.horasAssistidas)}
          </div>
          <div style={{ marginTop: 6, fontSize: 13, color: THEME.text2 }}>
            assistindo {s.totalFilmes} filmes e {s.totalSeries} séries
          </div>
        </div>

        {/* Big numbers grid */}
        <div style={{ padding: '20px 16px 0', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          <div>
            <div style={{ fontFamily: THEME.serif, fontSize: 48, color: THEME.text, fontWeight: 600, lineHeight: 1 }}>{s.totalEpisodios}</div>
            <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 4 }}>EPISÓDIOS NO CATÁLOGO</div>
          </div>
          <div>
            <div style={{ fontFamily: THEME.serif, fontSize: 48, color: THEME.red, fontWeight: 600, lineHeight: 1 }}>{s.episodiosAssistidos}</div>
            <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 4 }}>VOCÊ JÁ ASSISTIU</div>
          </div>
          <div>
            <div style={{ fontFamily: THEME.serif, fontSize: 48, color: THEME.text, fontWeight: 600, lineHeight: 1 }}>{s.seriesCompletas}</div>
            <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 4 }}>SÉRIES COMPLETAS</div>
          </div>
          <div>
            <div style={{ fontFamily: THEME.serif, fontSize: 48, color: THEME.text, fontWeight: 600, lineHeight: 1 }}>{s.emAndamento}</div>
            <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 4 }}>EM ANDAMENTO</div>
          </div>
        </div>

        {/* Top género */}
        <div style={{ padding: '24px 16px', marginTop: 12, borderTop: `1px solid ${THEME.border}` }}>
          <div style={{ fontFamily: THEME.mono, fontSize: 10, color: THEME.text3, letterSpacing: 2 }}>SEU GÊNERO Nº1</div>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, marginTop: 4 }}>
            <div style={{ fontFamily: THEME.serif, fontSize: 44, color: THEME.gold, fontWeight: 600, fontStyle: 'italic', letterSpacing: -1 }}>
              {topGenero.name}
            </div>
            <div style={{ fontSize: 12, color: THEME.text3, fontFamily: THEME.mono }}>{topGenero.pct}%</div>
          </div>
          <div style={{ marginTop: 14, display: 'flex', flexDirection: 'column', gap: 6 }}>
            {s.generos.slice(1).map(g => (
              <div key={g.name} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <span style={{ width: 70, fontSize: 11, color: THEME.text2 }}>{g.name}</span>
                <div style={{ flex: 1, height: 4, background: THEME.bg2, borderRadius: 2, overflow: 'hidden' }}>
                  <div style={{ height: '100%', width: `${g.pct}%`, background: THEME.text2 }} />
                </div>
                <span style={{ fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, width: 30, textAlign: 'right' }}>{g.pct}%</span>
              </div>
            ))}
          </div>
        </div>

        {/* Atividade mensal */}
        <div style={{ padding: '20px 16px', borderTop: `1px solid ${THEME.border}` }}>
          <div style={{ fontFamily: THEME.mono, fontSize: 10, color: THEME.text3, letterSpacing: 2, marginBottom: 14 }}>ATIVIDADE MENSAL · 12 MESES</div>
          <div style={{ display: 'flex', alignItems: 'flex-end', gap: 4, height: 110 }}>
            {s.atividade.map((a, i) => {
              const isMax = a.v === maxAtividade;
              return (
                <div key={i} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
                  <div style={{ flex: 1, width: '100%', display: 'flex', alignItems: 'flex-end' }}>
                    <div style={{
                      width: '100%',
                      height: `${a.v / maxAtividade * 100}%`,
                      background: isMax ? THEME.gold : THEME.text3,
                      borderRadius: 1,
                    }} />
                  </div>
                  <span style={{ fontSize: 8, color: THEME.text3, fontFamily: THEME.mono }}>{a.m}</span>
                </div>
              );
            })}
          </div>
        </div>

        {/* Top rated */}
        <div style={{ padding: '20px 16px', borderTop: `1px solid ${THEME.border}` }}>
          <div style={{ fontFamily: THEME.mono, fontSize: 10, color: THEME.text3, letterSpacing: 2, marginBottom: 12 }}>SUAS NOTAS MAIS ALTAS</div>
          <div style={{ display: 'flex', gap: 8, overflowX: 'auto' }}>
            {s.topRated.map((id, i) => {
              const m = FIND_MEDIA(id);
              return (
                <div key={id} style={{ flexShrink: 0, width: 90, position: 'relative' }}>
                  <div style={{ position: 'absolute', top: -10, left: -6, fontFamily: THEME.serif, fontSize: 56, color: THEME.gold, fontWeight: 700, lineHeight: 1, fontStyle: 'italic', textShadow: '0 0 12px rgba(0,0,0,0.6)', zIndex: 1 }}>{i+1}</div>
                  <img src={m.poster} style={{ width: 90, height: 135, objectFit: 'cover', borderRadius: 4 }} />
                  <div style={{ marginTop: 6, fontSize: 10, fontWeight: 600, lineHeight: 1.2, overflow: 'hidden', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>{m.title}</div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    );
  }

  // VARIANT A — Conservador with cards
  return (
    <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 90 }}>
      <div style={{ padding: '20px 16px 8px', display: 'flex', alignItems: 'center', gap: 8 }}>
        <Icon name="chart" size={20} color={THEME.gold} />
        <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 22, fontWeight: 600 }}>Estatísticas</h1>
      </div>
      <div style={{ padding: '0 16px', fontSize: 11, color: THEME.text3, fontFamily: THEME.mono }}>
        Última sync: {s.ultimaCaptura}
      </div>

      {/* hero card */}
      <div style={{
        margin: '14px 16px', padding: 16,
        background: `linear-gradient(135deg, ${THEME.bg1} 0%, ${THEME.bg2} 100%)`,
        border: `1px solid ${THEME.border2}`, borderRadius: 10,
        display: 'flex', alignItems: 'center', gap: 14,
      }}>
        <div style={{
          width: 56, height: 56, borderRadius: 12, background: THEME.gold + '20',
          display: 'flex', alignItems: 'center', justifyContent: 'center', color: THEME.gold,
        }}><Icon name="clock" size={28} color={THEME.gold} /></div>
        <div>
          <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1 }}>TEMPO ASSISTIDO</div>
          <div style={{ fontSize: 28, fontWeight: 700, fontFamily: THEME.serif, color: THEME.gold }}>{formatHoras(s.horasAssistidas)}</div>
          <div style={{ fontSize: 11, color: THEME.text2 }}>{s.episodiosAssistidos} episódios concluídos</div>
        </div>
      </div>

      {/* grid cards */}
      <div style={{ padding: '0 16px', display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 8 }}>
        {[
          { icon: 'film', value: s.totalFilmes, label: 'Filmes' },
          { icon: 'tv', value: s.totalSeries, label: 'Séries' },
          { icon: 'list', value: s.totalEpisodios, label: 'Eps total' },
          { icon: 'check-circle', value: s.episodiosAssistidos, label: 'Eps assistidos', highlight: true },
          { icon: 'trophy', value: s.seriesCompletas, label: 'Completas' },
          { icon: 'flame', value: s.emAndamento, label: 'Em andamento' },
        ].map((c, i) => (
          <div key={i} style={{
            padding: 10, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8,
            display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4,
          }}>
            <Icon name={c.icon} size={18} color={c.highlight ? THEME.gold : THEME.text2} />
            <div style={{ fontSize: 18, fontWeight: 700, color: c.highlight ? THEME.gold : THEME.text }}>{c.value}</div>
            <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 0.5, textAlign: 'center' }}>{c.label.toUpperCase()}</div>
          </div>
        ))}
      </div>

      {/* gêneros */}
      <div style={{ margin: '16px 16px 0', padding: 14, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 10 }}>
        <div style={{ fontSize: 11, color: THEME.gold, fontFamily: THEME.sans, fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginBottom: 12 }}>
          Gêneros favoritos
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {s.generos.map((g, i) => (
            <div key={g.name} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ width: 70, fontSize: 11, color: THEME.text }}>{g.name}</span>
              <div style={{ flex: 1, height: 8, background: THEME.bg2, borderRadius: 4, overflow: 'hidden' }}>
                <div style={{ height: '100%', width: `${g.pct}%`, background: i === 0 ? THEME.gold : THEME.text3 }} />
              </div>
              <span style={{ fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, width: 24, textAlign: 'right' }}>{g.count}</span>
            </div>
          ))}
        </div>
      </div>

      {/* atividade */}
      <div style={{ margin: '12px 16px 0', padding: 14, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 10 }}>
        <div style={{ fontSize: 11, color: THEME.gold, fontFamily: THEME.sans, fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginBottom: 12 }}>
          Atividade · 12 meses
        </div>
        <div style={{ display: 'flex', alignItems: 'flex-end', gap: 3, height: 80 }}>
          {s.atividade.map((a, i) => (
            <div key={i} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3 }}>
              <div style={{ flex: 1, width: '100%', display: 'flex', alignItems: 'flex-end' }}>
                <div style={{ width: '100%', height: `${a.v / maxAtividade * 100}%`, background: a.v === maxAtividade ? THEME.gold : THEME.text3, borderRadius: 1 }} />
              </div>
              <span style={{ fontSize: 8, color: THEME.text3, fontFamily: THEME.mono }}>{a.m}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { StatsScreen });
