// screen-detail.jsx — Detail screen, two variants.

function DetailScreen({ mediaId, variant = 'a', density = 'comfortable', onBack }) {
  const m = FIND_MEDIA(mediaId);
  const [status, setStatus] = React.useState(m.status);
  const [stars, setStars] = React.useState(m.user || 0);
  const [expanded, setExpanded] = React.useState(true);

  const RatingCard = ({ icon, value, label, color, big }) => (
    <div style={{
      flex: 1, padding: big ? 12 : 8,
      background: variant === 'b' ? 'transparent' : THEME.bg1,
      border: variant === 'b' ? `1px solid ${THEME.border}` : `1px solid ${THEME.border}`,
      borderRadius: variant === 'b' ? 4 : 8,
      display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4,
    }}>
      <Icon name={icon} size={big ? 18 : 14} color={color} />
      <div style={{ fontSize: big ? 18 : 14, fontWeight: 700, fontFamily: THEME.mono, color: THEME.text }}>{value}</div>
      <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 0.5, textTransform: 'uppercase' }}>{label}</div>
    </div>
  );

  if (variant === 'b') {
    return (
      <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 100 }}>
        {/* big poster header */}
        <div style={{ position: 'relative', height: 480, overflow: 'hidden' }}>
          <img src={m.poster} alt="" style={{
            position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover',
          }} />
          <div style={{
            position: 'absolute', inset: 0,
            background: 'linear-gradient(180deg, rgba(10,9,8,0.4) 0%, rgba(10,9,8,0) 30%, rgba(10,9,8,0.6) 65%, rgba(10,9,8,1) 100%)',
          }} />
          <button onClick={onBack} style={{
            position: 'absolute', top: 12, left: 12,
            width: 36, height: 36, borderRadius: 100,
            background: 'rgba(10,9,8,0.6)', backdropFilter: 'blur(12px)',
            border: `1px solid ${THEME.border2}`, color: THEME.text,
            display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer',
          }}><Icon name="chevron-left" size={18} /></button>

          <div style={{ position: 'absolute', left: 16, right: 16, bottom: 20 }}>
            <div style={{ fontSize: 10, color: THEME.gold, fontFamily: THEME.mono, letterSpacing: 2, marginBottom: 6 }}>
              {m.type === 'serie' ? 'SÉRIE' : 'FILME'} · {m.year}
            </div>
            <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 38, fontWeight: 600, lineHeight: 0.95, letterSpacing: -1 }}>{m.title}</h1>
            {m.original !== m.title && (
              <div style={{ marginTop: 6, fontFamily: THEME.serif, fontStyle: 'italic', color: THEME.text3, fontSize: 16 }}>{m.original}</div>
            )}
          </div>
        </div>

        <div style={{ padding: '0 16px' }}>
          {/* ratings */}
          <div style={{ display: 'flex', gap: 0, padding: '20px 0', borderBottom: `1px solid ${THEME.border}`, justifyContent: 'space-between' }}>
            <div style={{ flex: 1, textAlign: 'center' }}>
              <div style={{ fontFamily: THEME.serif, fontSize: 28, fontWeight: 600, color: THEME.imdb }}>{m.imdb?.toFixed(1) || '—'}</div>
              <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 2 }}>IMDb</div>
            </div>
            <div style={{ flex: 1, textAlign: 'center', borderLeft: `1px solid ${THEME.border}`, borderRight: `1px solid ${THEME.border}` }}>
              <div style={{ fontFamily: THEME.serif, fontSize: 28, fontWeight: 600, color: THEME.tomato }}>{m.tomato || '—'}<span style={{ fontSize: 14 }}>%</span></div>
              <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 2 }}>TOMATO</div>
            </div>
            <div style={{ flex: 1, textAlign: 'center' }}>
              <div style={{ fontFamily: THEME.serif, fontSize: 28, fontWeight: 600, color: THEME.popcorn }}>{m.popcorn || '—'}<span style={{ fontSize: 14 }}>%</span></div>
              <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 2 }}>POPCORN</div>
            </div>
          </div>

          {/* meta */}
          <div style={{ display: 'flex', gap: 8, padding: '14px 0', flexWrap: 'wrap', fontSize: 11, color: THEME.text2, fontFamily: THEME.mono }}>
            <span><Icon name="clock" size={11} /> {m.duration}min</span>
            {m.genres?.map(g => <span key={g} style={{ padding: '2px 8px', background: THEME.bg1, borderRadius: 100, fontSize: 10 }}>{g}</span>)}
          </div>

          {/* progress for series */}
          {m.type === 'serie' && m.progress && (
            <div style={{ padding: '14px 0', borderTop: `1px solid ${THEME.border}` }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ fontFamily: THEME.serif, fontSize: 16 }}>Progresso</span>
                <span style={{ fontSize: 11, color: THEME.gold, fontFamily: THEME.mono }}>{m.progress.watched}/{m.progress.total} eps</span>
              </div>
              <div style={{ height: 4, background: THEME.bg2, borderRadius: 2, overflow: 'hidden' }}>
                <div style={{ height: '100%', width: `${m.progress.watched / m.progress.total * 100}%`, background: THEME.gold }} />
              </div>
            </div>
          )}

          {/* status buttons */}
          <div style={{ padding: '14px 0', display: 'flex', gap: 8 }}>
            {[
              { key: 'Assistido', icon: 'check', label: 'Assistido' },
              { key: 'Assistindo', icon: 'play', label: 'Assistindo' },
              { key: 'Abandonado', icon: 'ban', label: 'Desistir' },
            ].map(b => {
              const active = status === b.key;
              return (
                <button key={b.key} onClick={() => setStatus(b.key)} style={{
                  flex: 1, padding: '12px 6px',
                  background: active ? (b.key === 'Abandonado' ? THEME.red : THEME.gold) : 'transparent',
                  color: active ? '#1a1410' : THEME.text2,
                  border: `1px solid ${active ? 'transparent' : THEME.border2}`,
                  borderRadius: 4, fontFamily: THEME.sans, fontSize: 11, fontWeight: 600,
                  display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4, cursor: 'pointer',
                }}><Icon name={b.icon} size={14} />{b.label}</button>
              );
            })}
          </div>

          {/* sinopse */}
          {m.desc && (
            <div style={{ padding: '14px 0', borderTop: `1px solid ${THEME.border}` }}>
              <h3 style={{ margin: '0 0 8px', fontFamily: THEME.serif, fontSize: 18, fontWeight: 500 }}>Sinopse</h3>
              <p style={{ margin: 0, fontSize: 14, lineHeight: 1.55, color: THEME.text2 }}>{m.desc}</p>
            </div>
          )}

          {/* user rating */}
          <div style={{ padding: '14px 0', borderTop: `1px solid ${THEME.border}` }}>
            <h3 style={{ margin: '0 0 4px', fontFamily: THEME.serif, fontSize: 18, fontWeight: 500 }}>Sua avaliação</h3>
            <p style={{ margin: '0 0 10px', fontSize: 12, color: THEME.text3 }}>{stars > 0 ? `${stars}/5 estrelas` : 'Toque para avaliar'}</p>
            <div style={{ display: 'flex', gap: 6 }}>
              {[1,2,3,4,5].map(i => (
                <button key={i} onClick={() => setStars(i)} style={{
                  width: 38, height: 38, borderRadius: 4,
                  background: i <= stars ? THEME.gold + '20' : THEME.bg1,
                  border: `1px solid ${i <= stars ? THEME.gold : THEME.border}`,
                  color: i <= stars ? THEME.gold : THEME.text3,
                  cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center',
                }}><Icon name="star" size={16} /></button>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  // VARIANT A — Conservador (poster + backdrop classic)
  return (
    <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 30 }}>
      <div style={{ position: 'relative', height: 220 }}>
        <img src={m.backdrop || m.poster} style={{ width: '100%', height: '100%', objectFit: 'cover', filter: 'brightness(0.55)' }} />
        <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(180deg, rgba(10,9,8,0.4) 0%, rgba(10,9,8,0) 50%, rgba(10,9,8,1) 100%)' }} />
        <button onClick={onBack} style={{
          position: 'absolute', top: 10, left: 10, width: 32, height: 32, borderRadius: 8,
          background: 'rgba(10,9,8,0.7)', border: `1px solid ${THEME.border2}`, color: THEME.text,
          display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer',
        }}><Icon name="chevron-left" size={16} /></button>
      </div>
      <div style={{ padding: '0 16px', marginTop: -70, position: 'relative' }}>
        <div style={{ display: 'flex', gap: 12, alignItems: 'flex-end' }}>
          <img src={m.poster} style={{
            width: 100, height: 150, borderRadius: 8, objectFit: 'cover',
            boxShadow: '0 4px 16px rgba(0,0,0,0.6)', border: `1px solid ${THEME.border2}`,
          }} />
          <div style={{ flex: 1, paddingBottom: 4 }}>
            <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 20, fontWeight: 600, lineHeight: 1.1 }}>{m.title}</h1>
            {m.original !== m.title && (
              <div style={{ fontSize: 11, color: THEME.text3, fontStyle: 'italic', marginTop: 2 }}>{m.original}</div>
            )}
            <div style={{ display: 'flex', gap: 8, marginTop: 6, fontSize: 10, color: THEME.text2, fontFamily: THEME.mono, flexWrap: 'wrap' }}>
              <span>{m.year}</span>
              <span>·</span>
              <span>{m.type === 'serie' ? 'Série' : 'Filme'}</span>
              {m.duration && <><span>·</span><span>{m.duration}min</span></>}
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 6, marginTop: 16 }}>
          <RatingCard icon="star" value={m.imdb?.toFixed(1) || '—'} label="IMDb" color={THEME.imdb} />
          <RatingCard icon="tomato" value={m.tomato ? m.tomato + '%' : '—'} label="Tomato" color={THEME.tomato} />
          <RatingCard icon="popcorn" value={m.popcorn ? m.popcorn + '%' : '—'} label="Popcorn" color={THEME.popcorn} />
          <RatingCard icon="heart" value={stars || '—'} label="Você" color={THEME.gold} />
        </div>

        {m.genres && (
          <div style={{ display: 'flex', gap: 5, marginTop: 12, flexWrap: 'wrap' }}>
            {m.genres.map(g => (
              <span key={g} style={{
                padding: '3px 10px', borderRadius: 100, fontSize: 11,
                background: THEME.bg1, border: `1px solid ${THEME.border}`, color: THEME.text2,
              }}>{g}</span>
            ))}
          </div>
        )}

        {m.type === 'serie' && m.progress && (
          <div style={{
            marginTop: 14, padding: 12,
            background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8,
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6, fontSize: 12 }}>
              <span style={{ fontWeight: 600 }}>Progresso</span>
              <span style={{ color: THEME.gold, fontFamily: THEME.mono }}>
                {m.progress.watched}/{m.progress.total} eps · {Math.round(m.progress.watched/m.progress.total*100)}%
              </span>
            </div>
            <div style={{ height: 6, background: THEME.bg2, borderRadius: 3, overflow: 'hidden' }}>
              <div style={{ height: '100%', width: `${m.progress.watched / m.progress.total * 100}%`, background: THEME.gold }} />
            </div>
          </div>
        )}

        {/* status buttons */}
        <div style={{
          marginTop: 14, padding: 12,
          background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, fontWeight: 600 }}>Seu status</span>
            <span style={{
              fontSize: 10, fontFamily: THEME.mono, padding: '3px 8px', borderRadius: 100,
              background: THEME.gold + '20', color: THEME.gold,
            }}>{status.toUpperCase()}</span>
          </div>
          <div style={{ display: 'flex', gap: 6 }}>
            {[
              { key: 'Assistido', icon: 'check', label: 'Assistido', col: THEME.success },
              { key: 'Assistindo', icon: 'play', label: 'Assistindo', col: THEME.gold },
              { key: 'Abandonado', icon: 'ban', label: 'Desistir', col: THEME.red },
            ].map(b => {
              const active = status === b.key;
              return (
                <button key={b.key} onClick={() => setStatus(b.key)} style={{
                  flex: 1, padding: '10px 4px',
                  background: active ? b.col : THEME.bg2,
                  color: active ? '#1a1410' : THEME.text2,
                  border: `1px solid ${active ? b.col : THEME.border}`,
                  borderRadius: 6, fontFamily: THEME.sans, fontSize: 11, fontWeight: 600,
                  display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4, cursor: 'pointer',
                }}><Icon name={b.icon} size={13} />{b.label}</button>
              );
            })}
          </div>
        </div>

        {m.desc && (
          <div style={{ marginTop: 14 }}>
            <div style={{ fontSize: 11, fontFamily: THEME.sans, fontWeight: 700, color: THEME.gold, letterSpacing: 1, textTransform: 'uppercase', marginBottom: 6 }}>Sinopse</div>
            <p style={{ margin: 0, fontSize: 13, lineHeight: 1.55, color: THEME.text2 }}>{m.desc}</p>
          </div>
        )}

        <div style={{ marginTop: 14, padding: 12, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8 }}>
          <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>Sua avaliação</div>
          <div style={{ display: 'flex', gap: 4 }}>
            {[1,2,3,4,5].map(i => (
              <button key={i} onClick={() => setStars(i)} style={{
                background: 'transparent', border: 'none', cursor: 'pointer',
                color: i <= stars ? THEME.gold : THEME.text4,
              }}><Icon name="star" size={26} /></button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { DetailScreen });
