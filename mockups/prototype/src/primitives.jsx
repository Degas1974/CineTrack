// primitives.jsx — shared visual primitives for both variants.
// THEME tokens (dark cinematic: black, gold, red).

const THEME = {
  // surfaces
  bg: '#0a0908',          // near-black, warm
  bg1: '#13110f',         // card surface
  bg2: '#1c1916',         // raised
  bg3: '#26211c',         // hover/active
  border: 'rgba(255,235,200,0.08)',
  border2: 'rgba(255,235,200,0.14)',
  // text
  text: '#f5ebd6',        // warm off-white
  text2: 'rgba(245,235,214,0.72)',
  text3: 'rgba(245,235,214,0.45)',
  text4: 'rgba(245,235,214,0.28)',
  // brand
  gold: '#e8b94a',        // primary accent
  goldDim: '#a8862e',
  red: '#c64133',         // secondary accent
  redDim: '#8a2a20',
  // semantic
  success: '#5fa86a',
  warning: '#e0a020',
  danger: '#c64133',
  // ratings
  imdb: '#f5c518',
  tomato: '#fa320a',
  popcorn: '#faa81a',
  // type
  serif: '"Fraunces", Georgia, serif',
  sans: '"Inter", system-ui, sans-serif',
  mono: '"JetBrains Mono", ui-monospace, monospace',
};

// ─────────────────────────────────────────────────────────────
// Phone Frame — custom dark frame using AndroidStatusBar/NavBar
// ─────────────────────────────────────────────────────────────
function PhoneFrame({ children, w = 360, h = 740 }) {
  return (
    <div style={{
      width: w, height: h, borderRadius: 36, overflow: 'hidden',
      background: THEME.bg,
      border: `8px solid #2a2622`,
      boxShadow: '0 30px 80px rgba(0,0,0,0.45), 0 0 0 1px rgba(255,255,255,0.04) inset',
      display: 'flex', flexDirection: 'column', position: 'relative',
      fontFamily: THEME.sans, color: THEME.text,
    }}>
      <div style={{ background: THEME.bg, color: THEME.text }}>
        <AndroidStatusBar dark />
      </div>
      <div style={{ flex: 1, overflow: 'hidden', position: 'relative', background: THEME.bg }}>
        {children}
      </div>
      <div style={{ background: THEME.bg }}>
        <AndroidNavBar dark />
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────
// Bottom nav — used in both variants
// ─────────────────────────────────────────────────────────────
function BottomNav({ active = 'home', onNav, variant = 'a' }) {
  const items = [
    { id: 'home', icon: 'home', label: 'Início' },
    { id: 'search', icon: 'search', label: 'Buscar' },
    { id: 'stats', icon: 'chart', label: 'Stats' },
    { id: 'sync', icon: 'sync', label: 'Sync' },
  ];
  if (variant === 'b') {
    // bold: floating pill nav with gold for active
    return (
      <div style={{
        position: 'absolute', left: 12, right: 12, bottom: 12,
        background: 'rgba(20,17,14,0.85)', backdropFilter: 'blur(24px)',
        border: `1px solid ${THEME.border2}`,
        borderRadius: 100, padding: 6,
        display: 'flex', justifyContent: 'space-around', alignItems: 'center',
        boxShadow: '0 8px 24px rgba(0,0,0,0.4)',
      }}>
        {items.map(it => {
          const isActive = active === it.id;
          return (
            <button key={it.id} onClick={() => onNav?.(it.id)}
              style={{
                background: isActive ? THEME.gold : 'transparent',
                color: isActive ? '#1a1410' : THEME.text2,
                border: 'none', borderRadius: 100,
                padding: isActive ? '8px 14px' : '8px 10px',
                display: 'flex', alignItems: 'center', gap: 6,
                fontSize: 12, fontWeight: 600, fontFamily: THEME.sans,
                cursor: 'pointer', transition: 'all .2s',
              }}>
              <Icon name={it.icon} size={18} strokeWidth={isActive ? 2.5 : 2} />
              {isActive && <span>{it.label}</span>}
            </button>
          );
        })}
      </div>
    );
  }
  // conservative: classic bottom nav
  return (
    <div style={{
      position: 'absolute', left: 0, right: 0, bottom: 0,
      background: 'rgba(10,9,8,0.92)', backdropFilter: 'blur(20px)',
      borderTop: `1px solid ${THEME.border}`,
      display: 'flex', justifyContent: 'space-around', alignItems: 'center',
      padding: '8px 0 6px',
    }}>
      {items.map(it => {
        const isActive = active === it.id;
        return (
          <button key={it.id} onClick={() => onNav?.(it.id)}
            style={{
              background: 'transparent', border: 'none',
              color: isActive ? THEME.gold : THEME.text3,
              display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4,
              padding: '4px 12px', cursor: 'pointer', fontFamily: THEME.sans,
            }}>
            <Icon name={it.icon} size={20} strokeWidth={isActive ? 2.4 : 2} />
            <span style={{ fontSize: 10, fontWeight: 600, letterSpacing: 0.2 }}>{it.label}</span>
          </button>
        );
      })}
    </div>
  );
}

// ─────────────────────────────────────────────────────────────
// MediaCard — poster card. Two visual modes via "variant" + density.
// ─────────────────────────────────────────────────────────────
function MediaCard({ media, w = 110, showProgress = false, onClick, density = 'comfortable', variant = 'a' }) {
  const compact = density === 'compact';
  const h = w * 1.5;
  return (
    <button onClick={onClick} style={{
      flexShrink: 0, width: w, padding: 0, background: 'transparent', border: 'none', cursor: 'pointer',
      textAlign: 'left', color: THEME.text, fontFamily: THEME.sans,
    }}>
      <div style={{
        width: w, height: h, borderRadius: variant === 'b' ? 4 : 8, overflow: 'hidden',
        background: THEME.bg2, position: 'relative',
        boxShadow: variant === 'b' ? '0 4px 16px rgba(0,0,0,0.5)' : '0 2px 8px rgba(0,0,0,0.4)',
      }}>
        <img src={media.poster} alt={media.title}
          style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
        {showProgress && media.progress && (
          <div style={{
            position: 'absolute', left: 0, right: 0, bottom: 0,
            background: 'linear-gradient(180deg, transparent, rgba(0,0,0,0.85))',
            padding: '20px 8px 6px',
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 4 }}>
              <span style={{ fontSize: 10, fontWeight: 600, color: THEME.gold, fontFamily: THEME.mono }}>
                EP {media.progress.watched}/{media.progress.total}
              </span>
              <span style={{ fontSize: 10, color: THEME.text2, fontFamily: THEME.mono }}>
                {Math.round(media.progress.watched / media.progress.total * 100)}%
              </span>
            </div>
            <div style={{ height: 3, background: 'rgba(255,255,255,0.15)', borderRadius: 2, overflow: 'hidden' }}>
              <div style={{
                height: '100%', width: `${media.progress.watched / media.progress.total * 100}%`,
                background: THEME.gold,
              }} />
            </div>
          </div>
        )}
        {media.user && !showProgress && (
          <div style={{
            position: 'absolute', top: 6, right: 6,
            background: 'rgba(0,0,0,0.7)', color: THEME.gold,
            padding: '2px 6px', borderRadius: 4,
            fontSize: 10, fontWeight: 700, fontFamily: THEME.mono,
            display: 'flex', alignItems: 'center', gap: 2,
          }}>
            <Icon name="star" size={9} color={THEME.gold} /> {media.user}
          </div>
        )}
      </div>
      {!compact && (
        <>
          <div style={{
            marginTop: 8, fontSize: 12, fontWeight: 600, color: THEME.text,
            overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
          }}>{media.title}</div>
          <div style={{ marginTop: 2, fontSize: 10, color: THEME.text3, fontFamily: THEME.mono, display: 'flex', alignItems: 'center', gap: 6 }}>
            <span>{media.year}</span>
            {media.imdb && <><span>·</span><span style={{color: THEME.imdb}}>★ {media.imdb.toFixed(1)}</span></>}
          </div>
        </>
      )}
    </button>
  );
}

// ─────────────────────────────────────────────────────────────
// Section header
// ─────────────────────────────────────────────────────────────
function SectionHeader({ title, em, link, variant = 'a', icon }) {
  if (variant === 'b') {
    return (
      <div style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'baseline',
        padding: '20px 16px 12px',
      }}>
        <h2 style={{
          margin: 0, fontFamily: THEME.serif, fontWeight: 500,
          fontSize: 22, color: THEME.text, letterSpacing: -0.4,
        }}>
          {title}{em && <em style={{ color: THEME.gold, fontStyle: 'italic', fontWeight: 400 }}> {em}</em>}
        </h2>
        {link && <span style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.mono, textTransform: 'uppercase', letterSpacing: 1 }}>{link}</span>}
      </div>
    );
  }
  return (
    <div style={{
      display: 'flex', justifyContent: 'space-between', alignItems: 'center',
      padding: '18px 16px 8px',
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        {icon && <Icon name={icon} size={14} color={THEME.gold} />}
        <h2 style={{
          margin: 0, fontFamily: THEME.sans, fontWeight: 700,
          fontSize: 14, color: THEME.text, letterSpacing: 0.5, textTransform: 'uppercase',
        }}>{title}{em && <span style={{ color: THEME.gold, marginLeft: 4 }}>{em}</span>}</h2>
      </div>
      {link && <span style={{ fontSize: 11, color: THEME.text3, fontFamily: THEME.sans }}>{link}</span>}
    </div>
  );
}

Object.assign(window, { THEME, PhoneFrame, BottomNav, MediaCard, SectionHeader });
