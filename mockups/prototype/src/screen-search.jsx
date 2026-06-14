// screen-search.jsx — Search screen, two variants.

function SearchScreen({ variant = 'a', density = 'comfortable', onOpen }) {
  const [q, setQ] = React.useState('');
  const [tipo, setTipo] = React.useState(null);
  const [status, setStatus] = React.useState(null);

  const filtered = MEDIA.filter(m => {
    if (q && !m.title.toLowerCase().includes(q.toLowerCase()) && !m.original.toLowerCase().includes(q.toLowerCase())) return false;
    if (tipo && m.type !== tipo) return false;
    if (status && m.status !== status) return false;
    return true;
  });

  const Chip = ({ active, onClick, children }) => (
    <button onClick={onClick} style={{
      padding: variant === 'b' ? '6px 12px' : '5px 12px',
      borderRadius: 100,
      background: active ? THEME.gold : 'transparent',
      color: active ? '#1a1410' : THEME.text2,
      border: `1px solid ${active ? THEME.gold : THEME.border2}`,
      fontSize: 12, fontWeight: 600, cursor: 'pointer',
      fontFamily: THEME.sans, whiteSpace: 'nowrap',
    }}>{children}</button>
  );

  if (variant === 'b') {
    return (
      <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 100 }}>
        <div style={{ padding: '20px 16px 12px' }}>
          <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: 32, fontWeight: 600, letterSpacing: -1 }}>
            Buscar<em style={{ color: THEME.gold, fontStyle: 'italic', fontWeight: 400 }}>.</em>
          </h1>
        </div>
        <div style={{ padding: '0 16px' }}>
          <div style={{
            display: 'flex', alignItems: 'center', gap: 10, padding: '14px 16px',
            background: THEME.bg1, border: `1px solid ${THEME.border2}`, borderRadius: 14,
          }}>
            <Icon name="search" size={18} color={THEME.gold} />
            <input value={q} onChange={e => setQ(e.target.value)} placeholder="Título, diretor ou ator"
              style={{
                flex: 1, background: 'transparent', border: 'none', outline: 'none',
                color: THEME.text, fontSize: 15, fontFamily: THEME.sans,
              }} />
            {q && <button onClick={() => setQ('')} style={{ background: 'none', border: 'none', color: THEME.text3, cursor: 'pointer' }}><Icon name="x" size={16} /></button>}
          </div>
        </div>
        <div style={{ display: 'flex', gap: 6, padding: '14px 16px 6px', overflowX: 'auto' }}>
          <Chip active={tipo === null} onClick={() => setTipo(null)}>Tudo</Chip>
          <Chip active={tipo === 'filme'} onClick={() => setTipo('filme')}>Filmes</Chip>
          <Chip active={tipo === 'serie'} onClick={() => setTipo('serie')}>Séries</Chip>
          <span style={{ width: 1, background: THEME.border2, margin: '0 4px' }} />
          <Chip active={status === 'Assistindo'} onClick={() => setStatus(status === 'Assistindo' ? null : 'Assistindo')}>Assistindo</Chip>
          <Chip active={status === 'Assistido'} onClick={() => setStatus(status === 'Assistido' ? null : 'Assistido')}>Assistido</Chip>
        </div>

        <div style={{ padding: '8px 16px', fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1 }}>
          {filtered.length} RESULTADOS
        </div>

        {/* grid layout */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 8, padding: '0 16px' }}>
          {filtered.map(m => (
            <button key={m.id} onClick={() => onOpen?.(m.id)} style={{
              padding: 0, background: 'transparent', border: 'none', cursor: 'pointer', textAlign: 'left', color: THEME.text,
            }}>
              <div style={{ aspectRatio: '2/3', borderRadius: 6, overflow: 'hidden', position: 'relative' }}>
                <img src={m.poster} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                {m.status === 'Assistido' && (
                  <div style={{
                    position: 'absolute', top: 4, right: 4, width: 18, height: 18, borderRadius: '50%',
                    background: THEME.gold, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#1a1410',
                  }}><Icon name="check" size={11} strokeWidth={3} /></div>
                )}
              </div>
              <div style={{ marginTop: 6, fontSize: 11, fontWeight: 600, lineHeight: 1.2, overflow: 'hidden', textOverflow: 'ellipsis', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>{m.title}</div>
              <div style={{ fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, marginTop: 2 }}>{m.year}</div>
            </button>
          ))}
        </div>
      </div>
    );
  }

  // VARIANT A — Conservador
  return (
    <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 90 }}>
      <div style={{ padding: '14px 16px 0' }}>
        <div style={{
          display: 'flex', alignItems: 'center', gap: 10, padding: '10px 14px',
          background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 10,
        }}>
          <Icon name="search" size={16} color={THEME.text3} />
          <input value={q} onChange={e => setQ(e.target.value)} placeholder="Título, diretor ou ator"
            style={{
              flex: 1, background: 'transparent', border: 'none', outline: 'none',
              color: THEME.text, fontSize: 14, fontFamily: THEME.sans,
            }} />
        </div>
      </div>
      <div style={{ padding: '12px 16px 4px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        <div style={{ display: 'flex', gap: 6, overflowX: 'auto' }}>
          <Chip active={tipo === null} onClick={() => setTipo(null)}>Todos</Chip>
          <Chip active={tipo === 'filme'} onClick={() => setTipo('filme')}>Filmes</Chip>
          <Chip active={tipo === 'serie'} onClick={() => setTipo('serie')}>Séries</Chip>
        </div>
        <div style={{ display: 'flex', gap: 6, overflowX: 'auto' }}>
          <Chip active={status === null} onClick={() => setStatus(null)}>Todos</Chip>
          <Chip active={status === 'Assistindo'} onClick={() => setStatus('Assistindo')}>Assistindo</Chip>
          <Chip active={status === 'Assistido'} onClick={() => setStatus('Assistido')}>Assistido</Chip>
          <Chip active={status === 'Pendente'} onClick={() => setStatus('Pendente')}>Pendente</Chip>
        </div>
      </div>

      <div style={{ padding: '4px 16px 0', display: 'flex', flexDirection: 'column', gap: density === 'compact' ? 4 : 8 }}>
        {filtered.map(m => (
          <button key={m.id} onClick={() => onOpen?.(m.id)} style={{
            display: 'flex', gap: 10, padding: density === 'compact' ? 6 : 8,
            background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8,
            cursor: 'pointer', color: THEME.text, fontFamily: THEME.sans, textAlign: 'left',
          }}>
            <img src={m.poster} style={{
              width: density === 'compact' ? 36 : 48,
              height: density === 'compact' ? 54 : 72,
              borderRadius: 4, objectFit: 'cover', flexShrink: 0,
            }} />
            <div style={{ flex: 1, minWidth: 0, padding: '2px 0', display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
              <div style={{ fontSize: 13, fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{m.title}</div>
              {m.original !== m.title && (
                <div style={{ fontSize: 11, color: THEME.text3, fontStyle: 'italic', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{m.original}</div>
              )}
              <div style={{ display: 'flex', gap: 8, marginTop: 4, fontSize: 10, color: THEME.text2, fontFamily: THEME.mono, alignItems: 'center' }}>
                <Icon name={m.type === 'serie' ? 'tv' : 'film'} size={10} />
                <span>{m.type === 'serie' ? 'Série' : 'Filme'}</span>
                <span>·</span>
                <span>{m.year}</span>
                {m.imdb && <><span>·</span><span style={{ color: THEME.imdb }}>★ {m.imdb.toFixed(1)}</span></>}
              </div>
            </div>
            {m.status !== 'Pendente' && (
              <div style={{
                alignSelf: 'center',
                fontSize: 9, fontFamily: THEME.mono, letterSpacing: 1,
                padding: '3px 6px', borderRadius: 3,
                background: m.status === 'Assistido' ? THEME.goldDim + '40' : THEME.bg3,
                color: m.status === 'Assistido' ? THEME.gold : THEME.text2,
              }}>{m.status.toUpperCase()}</div>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}

Object.assign(window, { SearchScreen });
