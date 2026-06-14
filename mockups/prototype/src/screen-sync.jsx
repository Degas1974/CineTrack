// screen-sync.jsx — Sync screen com ABAS, two variants.

function SyncScreen({ variant = 'a', density = 'comfortable' }) {
  const [tab, setTab] = React.useState('status');
  const s = SYNC_DATA;
  const [syncing, setSyncing] = React.useState(false);
  const triggerSync = () => {
    setSyncing(true);
    setTimeout(() => setSyncing(false), 1500);
  };

  const tabs = [
    { id: 'status', label: 'Status' },
    { id: 'pendentes', label: `Pendências`, badge: s.pendentes.length },
    { id: 'logs', label: 'Logs' },
    { id: 'diag', label: 'Diagnóstico' },
  ];

  const headerTitle = (
    <div style={{ padding: '20px 16px 8px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <Icon name="sync" size={20} color={THEME.gold} />
        <h1 style={{ margin: 0, fontFamily: THEME.serif, fontSize: variant === 'b' ? 28 : 22, fontWeight: 600 }}>
          {variant === 'b' ? <>Sincronização<em style={{ color: THEME.gold, fontStyle: 'italic', fontWeight: 400 }}>.</em></> : 'Sincronização'}
        </h1>
      </div>
    </div>
  );

  const tabBar = (
    <div style={{
      display: 'flex', gap: 4, padding: '8px 12px',
      borderBottom: variant === 'b' ? `1px solid ${THEME.border}` : 'none',
      overflowX: 'auto',
    }}>
      {tabs.map(t => {
        const active = tab === t.id;
        return (
          <button key={t.id} onClick={() => setTab(t.id)} style={{
            padding: variant === 'b' ? '8px 4px' : '6px 12px',
            background: variant === 'b' ? 'transparent' : (active ? THEME.gold : 'transparent'),
            color: variant === 'b' ? (active ? THEME.gold : THEME.text3) : (active ? '#1a1410' : THEME.text2),
            border: variant === 'b' ? 'none' : `1px solid ${active ? THEME.gold : THEME.border2}`,
            borderRadius: variant === 'b' ? 0 : 100,
            borderBottom: variant === 'b' ? `2px solid ${active ? THEME.gold : 'transparent'}` : undefined,
            fontFamily: THEME.sans, fontSize: 12, fontWeight: 600,
            whiteSpace: 'nowrap', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 6,
            marginRight: variant === 'b' ? 12 : 0,
          }}>
            {t.label}
            {t.badge ? (
              <span style={{
                background: active ? '#1a1410' : THEME.red,
                color: active ? THEME.gold : '#fff',
                fontSize: 9, padding: '1px 5px', borderRadius: 100, fontFamily: THEME.mono, fontWeight: 700,
              }}>{t.badge}</span>
            ) : null}
          </button>
        );
      })}
    </div>
  );

  // ── STATUS TAB ────────────────────────────────────────────
  const statusContent = (
    <div style={{ padding: '14px 16px' }}>
      <div style={{
        padding: 16, background: s.isConnected ? `${THEME.success}15` : `${THEME.danger}20`,
        border: `1px solid ${s.isConnected ? THEME.success + '40' : THEME.danger + '60'}`,
        borderRadius: 10, display: 'flex', alignItems: 'center', gap: 12,
      }}>
        <div style={{
          width: 40, height: 40, borderRadius: 100,
          background: s.isConnected ? THEME.success + '30' : THEME.danger + '30',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: s.isConnected ? THEME.success : THEME.danger,
        }}>
          <Icon name={s.isConnected ? 'check-circle' : 'x-circle'} size={22} />
        </div>
        <div style={{ flex: 1 }}>
          <div style={{ fontWeight: 700, fontSize: 15 }}>
            {s.isConnected ? 'Tudo sincronizado' : 'Sem conexão'}
          </div>
          <div style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, marginTop: 2 }}>
            Última sync · {s.ultimaSync}
          </div>
        </div>
      </div>

      <button onClick={triggerSync} disabled={syncing} style={{
        marginTop: 12, width: '100%', padding: '14px',
        background: THEME.gold, color: '#1a1410', border: 'none', borderRadius: 10,
        fontFamily: THEME.sans, fontSize: 14, fontWeight: 700,
        display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8,
        cursor: syncing ? 'wait' : 'pointer', opacity: syncing ? 0.7 : 1,
      }}>
        <Icon name="sync" size={16} style={{ animation: syncing ? 'spin 1s linear infinite' : 'none' }} />
        {syncing ? 'Sincronizando…' : 'Sincronizar Agora'}
      </button>

      <div style={{ marginTop: 18, display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 8 }}>
        {[
          { v: 87, l: 'FILMES' }, { v: 24, l: 'SÉRIES' }, { v: 612, l: 'EPISÓDIOS' },
        ].map((d,i) => (
          <div key={i} style={{ padding: 12, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8, textAlign: 'center' }}>
            <div style={{ fontSize: 22, fontWeight: 700, fontFamily: THEME.serif, color: THEME.text }}>{d.v}</div>
            <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 1, marginTop: 2 }}>{d.l}</div>
          </div>
        ))}
      </div>

      <div style={{ marginTop: 18, padding: 12, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 8 }}>
        <div style={{ fontSize: 11, color: THEME.gold, fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginBottom: 8 }}>Última execução</div>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, padding: '4px 0' }}>
          <span style={{ color: THEME.text3 }}>Novos itens</span>
          <span style={{ color: THEME.success, fontFamily: THEME.mono, fontWeight: 600 }}>+{s.novosItens}</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, padding: '4px 0' }}>
          <span style={{ color: THEME.text3 }}>Itens atualizados</span>
          <span style={{ color: THEME.gold, fontFamily: THEME.mono, fontWeight: 600 }}>{s.itensAtualizados}</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, padding: '4px 0' }}>
          <span style={{ color: THEME.text3 }}>Pendências</span>
          <span style={{ color: THEME.warning, fontFamily: THEME.mono, fontWeight: 600 }}>{s.pendentes.length}</span>
        </div>
      </div>
    </div>
  );

  // ── PENDENTES TAB ─────────────────────────────────────────
  const pendentesContent = (
    <div style={{ padding: '14px 16px', display: 'flex', flexDirection: 'column', gap: 10 }}>
      {s.pendentes.map(p => (
        <div key={p.id} style={{
          padding: 12, background: THEME.bg1,
          border: `1px solid ${p.confianca < 70 ? THEME.warning + '60' : THEME.border}`,
          borderRadius: 8,
        }}>
          <div style={{ fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, marginBottom: 4, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {p.capturado}
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
            <span style={{ fontSize: 14, fontWeight: 600 }}>→ {p.match}</span>
            <span style={{
              fontSize: 10, fontFamily: THEME.mono, padding: '2px 8px', borderRadius: 100,
              background: p.confianca < 70 ? THEME.warning + '30' : THEME.gold + '20',
              color: p.confianca < 70 ? THEME.warning : THEME.gold, fontWeight: 700,
            }}>{p.confianca}% match</span>
          </div>
          {p.releases.map(r => (
            <div key={r.id} style={{ padding: 8, background: THEME.bg2, borderRadius: 6, marginTop: 6 }}>
              <div style={{ fontSize: 11, fontWeight: 600, fontFamily: THEME.mono, color: THEME.text }}>{r.label}</div>
              <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, marginTop: 2, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.raw}</div>
              <div style={{ display: 'flex', gap: 6, marginTop: 8 }}>
                <button style={{
                  flex: 1, padding: '6px', background: THEME.success + '30', color: THEME.success,
                  border: `1px solid ${THEME.success}40`, borderRadius: 4, fontSize: 11, fontWeight: 600, cursor: 'pointer',
                  fontFamily: THEME.sans, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 4,
                }}><Icon name="check" size={11} /> Importar</button>
                <button style={{
                  flex: 1, padding: '6px', background: 'transparent', color: THEME.text3,
                  border: `1px solid ${THEME.border2}`, borderRadius: 4, fontSize: 11, fontWeight: 600, cursor: 'pointer',
                  fontFamily: THEME.sans, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 4,
                }}><Icon name="x" size={11} /> Rejeitar</button>
              </div>
            </div>
          ))}
        </div>
      ))}
    </div>
  );

  // ── LOGS TAB ──────────────────────────────────────────────
  const logColors = { sucesso: THEME.success, erro: THEME.danger, aviso: THEME.warning, info: THEME.gold };
  const logIcons = { sucesso: 'check-circle', erro: 'x-circle', aviso: 'warn', info: 'info' };
  const logsContent = (
    <div style={{ padding: '14px 16px', display: 'flex', flexDirection: 'column', gap: 6 }}>
      {s.logs.map((l, i) => (
        <div key={i} style={{
          padding: 10, background: THEME.bg1, borderRadius: 6,
          borderLeft: `3px solid ${logColors[l.tipo]}`,
          display: 'flex', alignItems: 'flex-start', gap: 10,
        }}>
          <Icon name={logIcons[l.tipo]} size={14} color={logColors[l.tipo]} style={{ marginTop: 2 }} />
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontSize: 12, color: THEME.text }}>{l.msg}</div>
            <div style={{ fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, marginTop: 2 }}>{l.data}</div>
          </div>
        </div>
      ))}
    </div>
  );

  // ── DIAGNÓSTICO TAB ───────────────────────────────────────
  const claridadeColor = { 'muito-claro': THEME.success, claro: THEME.gold, moderado: THEME.warning, ambiguo: THEME.danger };
  const claridadeLabel = { 'muito-claro': 'Muito claro', claro: 'Claro', moderado: 'Moderado', ambiguo: 'Ambíguo' };
  const diagContent = (
    <div style={{ padding: '14px 16px' }}>
      <div style={{ fontSize: 11, color: THEME.text3, marginBottom: 10, lineHeight: 1.5 }}>
        Modo dev — qualidade do parsing das capturas.
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: 6, marginBottom: 14 }}>
        {[
          { l: 'Total', v: s.diagnostico.length, c: THEME.text },
          { l: 'Claros', v: s.diagnostico.filter(d => d.claridade === 'muito-claro' || d.claridade === 'claro').length, c: THEME.success },
          { l: 'Moder.', v: s.diagnostico.filter(d => d.claridade === 'moderado').length, c: THEME.warning },
          { l: 'Ambig.', v: s.diagnostico.filter(d => d.claridade === 'ambiguo').length, c: THEME.danger },
        ].map((c,i)=>(
          <div key={i} style={{ padding: 8, background: THEME.bg1, border: `1px solid ${THEME.border}`, borderRadius: 6, textAlign: 'center' }}>
            <div style={{ fontSize: 18, fontWeight: 700, color: c.c, fontFamily: THEME.serif }}>{c.v}</div>
            <div style={{ fontSize: 9, color: THEME.text3, fontFamily: THEME.mono, letterSpacing: 0.5 }}>{c.l.toUpperCase()}</div>
          </div>
        ))}
      </div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {s.diagnostico.map((d, i) => (
          <div key={i} style={{
            padding: 10, background: THEME.bg1, borderRadius: 6,
            borderLeft: `3px solid ${claridadeColor[d.claridade]}`,
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 }}>
              <span style={{ fontSize: 12, fontWeight: 600 }}>{d.titulo}</span>
              <span style={{
                fontSize: 9, padding: '2px 6px', borderRadius: 100,
                background: claridadeColor[d.claridade] + '20', color: claridadeColor[d.claridade],
                fontFamily: THEME.mono, fontWeight: 700,
              }}>{claridadeLabel[d.claridade]} · {d.score}</span>
            </div>
            {d.episodio && (
              <div style={{ fontSize: 11, color: THEME.text2, marginBottom: 4 }}>
                <span style={{ fontFamily: THEME.mono, color: THEME.gold }}>{d.codigo}</span> · {d.episodio}
              </div>
            )}
            <div style={{ fontSize: 11, color: THEME.text3, lineHeight: 1.4 }}>{d.resumo}</div>
            <div style={{ display: 'flex', gap: 6, marginTop: 6 }}>
              {d.ano && <span style={{ fontSize: 9, padding: '2px 6px', background: THEME.bg2, borderRadius: 100, fontFamily: THEME.mono, color: THEME.text2 }}>Ano {d.ano}</span>}
              <span style={{ fontSize: 9, padding: '2px 6px', background: THEME.bg2, borderRadius: 100, fontFamily: THEME.mono, color: THEME.text2 }}>{d.modo}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  return (
    <div style={{ background: THEME.bg, color: THEME.text, height: '100%', overflow: 'auto', paddingBottom: 100 }}>
      <style>{`@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }`}</style>
      {headerTitle}
      {tabBar}
      {tab === 'status' && statusContent}
      {tab === 'pendentes' && pendentesContent}
      {tab === 'logs' && logsContent}
      {tab === 'diag' && diagContent}
    </div>
  );
}

Object.assign(window, { SyncScreen });
