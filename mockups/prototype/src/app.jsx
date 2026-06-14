// app.jsx — main app with design canvas, two phone variants per screen, tweaks panel.

const { useState } = React;

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "density": "comfortable"
}/*EDITMODE-END*/;

function Phone({ variant, density, initialScreen = 'home' }) {
  const [screen, setScreen] = useState(initialScreen);
  const [mediaId, setMediaId] = useState(1);
  const [prevScreen, setPrevScreen] = useState('home');

  const open = (id) => { setPrevScreen(screen); setMediaId(id); setScreen('detail'); };
  const back = () => setScreen(prevScreen);
  const nav = (s) => { setPrevScreen(screen); setScreen(s); };

  return (
    <PhoneFrame>
      {screen === 'home'   && <HomeScreen   variant={variant} density={density} onOpen={open} />}
      {screen === 'search' && <SearchScreen variant={variant} density={density} onOpen={open} />}
      {screen === 'detail' && <DetailScreen mediaId={mediaId} variant={variant} density={density} onBack={back} />}
      {screen === 'stats'  && <StatsScreen  variant={variant} density={density} />}
      {screen === 'sync'   && <SyncScreen   variant={variant} density={density} />}
      {screen !== 'detail' && <BottomNav active={screen} onNav={nav} variant={variant} />}
    </PhoneFrame>
  );
}

function App() {
  const { tweaks, setTweak } = useTweaks(TWEAK_DEFAULTS);
  const density = tweaks.density;

  const screens = [
    { id: 'home', title: 'Home' },
    { id: 'search', title: 'Search' },
    { id: 'detail', title: 'Detail' },
    { id: 'stats', title: 'Stats' },
    { id: 'sync', title: 'Sync' },
  ];

  return (
    <>
      <DesignCanvas>
        <DCSection id="intro" title="CineTrack" subtitle="Redesign cinematográfico — preto, dourado, vermelho · 2 variações por tela (A conservador · B ousado)">
          <DCArtboard id="readme" label="README" width={420} height={740}>
            <div style={{ width: '100%', height: '100%', background: '#0a0908', color: '#f5ebd6', padding: 28, fontFamily: '"Inter",system-ui', boxSizing: 'border-box', overflow: 'auto' }}>
              <div style={{ fontFamily: '"Fraunces", serif', fontSize: 32, fontWeight: 600, letterSpacing: -1 }}>
                CineTrack<em style={{ color: '#e8b94a', fontStyle: 'italic', fontWeight: 400 }}>.</em>
              </div>
              <div style={{ fontFamily: '"JetBrains Mono", monospace', fontSize: 10, color: '#e8b94a', letterSpacing: 3, marginTop: 4 }}>REDESIGN · ABR 2026</div>
              <hr style={{ border: 0, borderTop: '1px solid rgba(245,235,214,0.14)', margin: '20px 0' }} />
              <p style={{ fontSize: 13, lineHeight: 1.6, color: 'rgba(245,235,214,0.72)' }}>
                Redesign do app a partir dos seus <code style={{ background: '#1c1916', padding: '1px 6px', borderRadius: 3, fontFamily: '"JetBrains Mono", monospace', fontSize: 11 }}>.razor</code> originais. Identidade dark cinematográfica, foco em usuário casual, com pôsteres reais.
              </p>
              <div style={{ fontSize: 11, color: '#e8b94a', fontFamily: '"Inter"', fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginTop: 18, marginBottom: 8 }}>O que melhorou</div>
              <ul style={{ fontSize: 12, lineHeight: 1.7, color: 'rgba(245,235,214,0.72)', paddingLeft: 18, margin: 0 }}>
                <li><b>Sync</b> dividido em 4 abas (Status · Pendências · Logs · Diagnóstico)</li>
                <li><b>Stats</b> redesenhado estilo Wrapped — tempo total em destaque, gêneros, atividade</li>
                <li><b>Home</b> com hero "continue assistindo" diferenciado de rails normais</li>
                <li><b>Detail</b> com poster integrado ao backdrop + status inline</li>
                <li><b>Search</b> com filtros enxutos e estados visuais por status</li>
              </ul>
              <div style={{ fontSize: 11, color: '#e8b94a', fontFamily: '"Inter"', fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginTop: 18, marginBottom: 8 }}>Como navegar</div>
              <ul style={{ fontSize: 12, lineHeight: 1.7, color: 'rgba(245,235,214,0.72)', paddingLeft: 18, margin: 0 }}>
                <li>Toque qualquer card pra abrir o Detail</li>
                <li>Bottom nav alterna entre Home / Search / Stats / Sync</li>
                <li>Tweaks (canto inferior) — densidade compacto/confortável</li>
                <li>Fullscreen em qualquer artboard pelo ícone ⤢ no hover</li>
              </ul>
              <div style={{ fontSize: 11, color: '#e8b94a', fontFamily: '"Inter"', fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', marginTop: 18, marginBottom: 8 }}>Sistema</div>
              <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', fontSize: 10, fontFamily: '"JetBrains Mono"' }}>
                {['#0a0908 BG','#e8b94a GOLD','#c64133 RED','Fraunces SERIF','Inter SANS','JetBrains MONO'].map(t=>(
                  <span key={t} style={{ padding: '4px 8px', background: '#13110f', border: '1px solid rgba(245,235,214,0.14)', borderRadius: 4, color: 'rgba(245,235,214,0.72)' }}>{t}</span>
                ))}
              </div>
            </div>
          </DCArtboard>
        </DCSection>

        <DCSection id="variantA" title="A · Conservador" subtitle="Estrutura familiar, type sans, alta densidade — direto ao ponto.">
          {screens.map(s => (
            <DCArtboard key={s.id} id={`a-${s.id}`} label={s.title} width={376} height={780}>
              <Phone variant="a" density={density} initialScreen={s.id} />
            </DCArtboard>
          ))}
        </DCSection>

        <DCSection id="variantB" title="B · Ousado" subtitle="Editorial cinematográfico — Fraunces serif, full-bleed, hero grande, espaço generoso.">
          {screens.map(s => (
            <DCArtboard key={s.id} id={`b-${s.id}`} label={s.title} width={376} height={780}>
              <Phone variant="b" density={density} initialScreen={s.id} />
            </DCArtboard>
          ))}
        </DCSection>
      </DesignCanvas>

      <TweaksPanel title="Tweaks">
        <TweakSection title="Densidade">
          <TweakRadio
            value={tweaks.density}
            options={[{ value: 'comfortable', label: 'Confortável' }, { value: 'compact', label: 'Compacto' }]}
            onChange={(v) => setTweak('density', v)}
          />
        </TweakSection>
        <div style={{ fontSize: 11, color: '#666', padding: '4px 0', lineHeight: 1.5 }}>
          Compacto reduz padding e esconde labels nos cards.
        </div>
      </TweaksPanel>
    </>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);
