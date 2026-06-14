// screen-home.jsx — Home screen, two variants.
// A (conservador): rails clássicos com hero compacto pra "continue assistindo"
// B (ousado): hero full-bleed editorial + rails com tipografia serif

function HomeScreen({ variant = 'a', density = 'comfortable', onOpen }) {
  const emAndamento = MEDIA.filter(m => m.status === 'Assistindo');
  const sugestoes = MEDIA.filter(m => m.status === 'Pendente').slice(0, 6);
  const recentes = MEDIA.filter(m => m.status === 'Assistido').slice(0, 6);

  if (variant === 'b') {
    const hero = emAndamento[0];
    return (
      <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 100 }}>
        {/* HERO full-bleed */}
        <div style={{ position: 'relative', height: 380, overflow: 'hidden' }}>
          <img src={hero.backdrop || hero.poster} alt="" style={{
            position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover', filter: 'saturate(0.85)',
          }} />
          <div style={{
            position: 'absolute', inset: 0,
            background: 'linear-gradient(180deg, rgba(10,9,8,0.5) 0%, rgba(10,9,8,0.1) 30%, rgba(10,9,8,0.95) 95%)',
          }} />
          <div style={{ position: 'absolute', top: 16, left: 16, right: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ fontFamily: THEME.serif, fontWeight: 600, fontSize: 18, letterSpacing: 4, color: THEME.gold }}>CINETRACK</div>
            <Icon name="menu" size={20} color={THEME.text2} />
          </div>
          <div style={{ position: 'absolute', left: 16, right: 16, bottom: 20 }}>
            <div style={{ fontSize: 10, color: THEME.gold, fontFamily: THEME.mono, letterSpacing: 2, marginBottom: 6 }}>
              CONTINUE ASSISTINDO · EP {hero.progress.watched}/{hero.progress.total}
            </div>
            <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 36, fontWeight: 600, lineHeight: 1, letterSpacing: -1 }}>{hero.title}</h1>
            <div style={{ marginTop: 6, fontSize: 12, color: THEME.text2, display: 'flex', gap: 10, fontFamily: THEME.mono }}>
              <span>{hero.year}</span>
              <span style={{ color: THEME.imdb }}>★ {hero.imdb}</span>
              <span>{hero.genres?.[0]}</span>
            </div>
            <div style={{ display: 'flex', gap: 8, marginTop: 14 }}>
              <button onClick={() => onOpen?.(hero.id)} style={{
                background: THEME.gold, color: '#1a1410', border: 'none',
                padding: '10px 20px', borderRadius: 100, fontWeight: 700, fontSize: 13,
                display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontFamily: THEME.sans,
              }}><Icon name="play" size={12} /> Continuar</button>
              <button style={{
                background: 'rgba(255,255,255,0.1)', color: THEME.text, border: `1px solid ${THEME.border2}`,
                padding: '10px 16px', borderRadius: 100, fontWeight: 600, fontSize: 13,
                display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontFamily: THEME.sans,
              }}><Icon name="info" size={12} /> Detalhes</button>
            </div>
            <div style={{ marginTop: 14, height: 3, background: 'rgba(255,255,255,0.15)', borderRadius: 2, overflow: 'hidden' }}>
              <div style={{ height: '100%', width: `${hero.progress.watched / hero.progress.total * 100}%`, background: THEME.gold }} />
            </div>
          </div>
        </div>

        {/* Em andamento (resto) */}
        <SectionHeader title="Continue" em="onde parou" variant="b" link="VER TUDO" />
        <div style={{ display: 'flex', gap: 12, padding: '0 16px', overflowX: 'auto' }}>
          {emAndamento.slice(1).map(m => <MediaCard key={m.id} media={m} w={130} showProgress onClick={() => onOpen?.(m.id)} density={density} variant="b" />)}
        </div>

        <SectionHeader title="Sugestões" em="para você" variant="b" link="VER MAIS" />
        <div style={{ display: 'flex', gap: 12, padding: '0 16px', overflowX: 'auto' }}>
          {sugestoes.map(m => <MediaCard key={m.id} media={m} w={130} onClick={() => onOpen?.(m.id)} density={density} variant="b" />)}
        </div>

        <SectionHeader title="Recentemente" em="assistidos" variant="b" link="VER TUDO" />
        <div style={{ display: 'flex', gap: 12, padding: '0 16px', overflowX: 'auto' }}>
          {recentes.map(m => <MediaCard key={m.id} media={m} w={130} onClick={() => onOpen?.(m.id)} density={density} variant="b" />)}
        </div>
      </div>
    );
  }

  // VARIANT A — Conservador
  return (
    <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 90 }}>
      <div style={{ padding: '20px 16px 8px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 22, fontWeight: 600, letterSpacing: 3, color: THEME.gold }}>
          C I N E T R A C K
        </h1>
        <Icon name="menu" size={20} color={THEME.text2} />
      </div>

      {/* Continue assistindo — featured row */}
      <SectionHeader title="CONTINUE" em=" · onde parou" variant="a" icon="play" link="ver tudo" />
      <div style={{ padding: '0 16px' }}>
        {emAndamento.slice(0, 1).map(m => (
          <button key={m.id} onClick={() => onOpen?.(m.id)} style={{
            display: 'flex', gap: 12, padding: 8, background: THEME.bg1,
            border: `1px solid ${THEME.border}`, borderRadius: 10, width: '100%',
            textAlign: 'left', cursor: 'pointer', color: THEME.text, fontFamily: THEME.sans,
          }}>
            <img src={m.poster} style={{ width: 70, height: 105, borderRadius: 6, objectFit: 'cover' }} />
            <div style={{ flex: 1, padding: '4px 0', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
              <div>
                <div style={{ fontSize: 14, fontWeight: 600 }}>{m.title}</div>
                <div style={{ fontSize: 11, color: THEME.text3, marginTop: 2, fontFamily: THEME.mono }}>
                  Próximo: EP {m.progress.watched + 1}
                </div>
              </div>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 10, color: THEME.text3, marginBottom: 4, fontFamily: THEME.mono }}>
                  <span>{m.progress.watched}/{m.progress.total} eps</span>
                  <span style={{ color: THEME.gold }}>{Math.round(m.progress.watched / m.progress.total * 100)}%</span>
                </div>
                <div style={{ height: 4, background: 'rgba(255,255,255,0.08)', borderRadius: 2, overflow: 'hidden' }}>
                  <div style={{ height: '100%', width: `${m.progress.watched / m.progress.total * 100}%`, background: THEME.gold }} />
                </div>
              </div>
            </div>
          </button>
        ))}
        <div style={{ display: 'flex', gap: 10, marginTop: 12, overflowX: 'auto' }}>
          {emAndamento.slice(1).map(m => <MediaCard key={m.id} media={m} w={100} showProgress onClick={() => onOpen?.(m.id)} density={density} variant="a" />)}
        </div>
      </div>

      <SectionHeader title="SUGESTÕES" em=" · para você" variant="a" icon="sparkle" link="ver mais" />
      <div style={{ display: 'flex', gap: 10, padding: '0 16px', overflowX: 'auto' }}>
        {sugestoes.map(m => <MediaCard key={m.id} media={m} w={100} onClick={() => onOpen?.(m.id)} density={density} variant="a" />)}
      </div>

      <SectionHeader title="RECENTES" em=" · assistidos" variant="a" icon="check-circle" link="ver tudo" />
      <div style={{ display: 'flex', gap: 10, padding: '0 16px', overflowX: 'auto' }}>
        {recentes.map(m => <MediaCard key={m.id} media={m} w={100} onClick={() => onOpen?.(m.id)} density={density} variant="a" />)}
      </div>
    </div>
  );
}

Object.assign(window, { HomeScreen });
