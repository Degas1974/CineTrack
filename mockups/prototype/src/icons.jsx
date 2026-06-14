// icons.jsx — minimal inline SVG icon set used across screens.
// All icons use stroke or fill = currentColor.

const Icon = ({ name, size = 16, color, style, strokeWidth = 2 }) => {
  const props = {
    width: size, height: size, viewBox: '0 0 24 24',
    fill: 'none', stroke: 'currentColor', strokeWidth,
    strokeLinecap: 'round', strokeLinejoin: 'round',
    style: { color: color || 'currentColor', flexShrink: 0, ...style },
  };
  const fillProps = {
    width: size, height: size, viewBox: '0 0 24 24',
    fill: 'currentColor',
    style: { color: color || 'currentColor', flexShrink: 0, ...style },
  };
  switch (name) {
    case 'home': return <svg {...props}><path d="M3 12L12 3l9 9" /><path d="M5 10v10h14V10" /></svg>;
    case 'search': return <svg {...props}><circle cx="11" cy="11" r="7" /><path d="M21 21l-4.3-4.3" /></svg>;
    case 'chart': return <svg {...props}><path d="M3 3v18h18" /><rect x="7" y="12" width="3" height="6" /><rect x="12" y="8" width="3" height="10" /><rect x="17" y="4" width="3" height="14" /></svg>;
    case 'sync': return <svg {...props}><path d="M21 12a9 9 0 0 1-15 6.7L3 16" /><path d="M3 12a9 9 0 0 1 15-6.7L21 8" /><path d="M21 3v5h-5" /><path d="M3 21v-5h5" /></svg>;
    case 'star': return <svg {...fillProps}><path d="M12 2l3 6.9 7.5.6-5.7 5 1.7 7.4L12 18l-6.5 3.9 1.7-7.4-5.7-5 7.5-.6z"/></svg>;
    case 'star-line': return <svg {...props}><path d="M12 2l3 6.9 7.5.6-5.7 5 1.7 7.4L12 18l-6.5 3.9 1.7-7.4-5.7-5 7.5-.6z"/></svg>;
    case 'heart': return <svg {...fillProps}><path d="M12 21s-7-4.5-9.5-9A5.5 5.5 0 0 1 12 6a5.5 5.5 0 0 1 9.5 6c-2.5 4.5-9.5 9-9.5 9z"/></svg>;
    case 'play': return <svg {...fillProps}><path d="M6 4l14 8-14 8z"/></svg>;
    case 'check': return <svg {...props}><path d="M5 13l4 4L19 7" /></svg>;
    case 'check-circle': return <svg {...props}><circle cx="12" cy="12" r="9" /><path d="M8 12l3 3 5-6" /></svg>;
    case 'x': return <svg {...props}><path d="M6 6l12 12M18 6L6 18" /></svg>;
    case 'x-circle': return <svg {...props}><circle cx="12" cy="12" r="9" /><path d="M9 9l6 6M15 9l-6 6" /></svg>;
    case 'ban': return <svg {...props}><circle cx="12" cy="12" r="9" /><path d="M5.6 5.6l12.8 12.8" /></svg>;
    case 'chevron-left': return <svg {...props}><path d="M15 6l-6 6 6 6" /></svg>;
    case 'chevron-right': return <svg {...props}><path d="M9 6l6 6-6 6" /></svg>;
    case 'chevron-down': return <svg {...props}><path d="M6 9l6 6 6-6" /></svg>;
    case 'film': return <svg {...props}><rect x="3" y="3" width="18" height="18" rx="2" /><path d="M3 8h18M3 16h18M8 3v18M16 3v18" /></svg>;
    case 'tv': return <svg {...props}><rect x="2" y="5" width="20" height="14" rx="2" /><path d="M8 21h8M12 17v4" /></svg>;
    case 'clock': return <svg {...props}><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></svg>;
    case 'calendar': return <svg {...props}><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M3 9h18M8 3v4M16 3v4" /></svg>;
    case 'trophy': return <svg {...props}><path d="M8 3h8v5a4 4 0 0 1-8 0V3z" /><path d="M16 5h3v2a3 3 0 0 1-3 3M8 5H5v2a3 3 0 0 0 3 3" /><path d="M10 13h4M9 21h6M12 17v4"/></svg>;
    case 'flame': return <svg {...props}><path d="M12 2c1 4 5 5 5 10a5 5 0 0 1-10 0c0-2 1-3 2-4-1 3 1 5 3 5"/></svg>;
    case 'popcorn': return <svg {...props}><path d="M5 9h14l-2 12H7L5 9z"/><path d="M7 9a3 3 0 1 1 1.5-5.7A3 3 0 0 1 14 3a3 3 0 0 1 5 4 3 3 0 0 1-2 2"/></svg>;
    case 'tomato': return <svg {...props}><circle cx="12" cy="13" r="8"/><path d="M8 6c1-1 3-2 4-2s3 1 4 2M12 4V2"/></svg>;
    case 'imdb': return <svg viewBox="0 0 64 32" width={size*2} height={size} fill="currentColor" style={{flexShrink:0,...style}}><rect width="64" height="32" rx="4"/></svg>;
    case 'filter': return <svg {...props}><path d="M3 5h18M6 12h12M10 19h4" /></svg>;
    case 'arrow-up': return <svg {...props}><path d="M12 19V5M5 12l7-7 7 7" /></svg>;
    case 'arrow-right': return <svg {...props}><path d="M5 12h14M13 5l7 7-7 7" /></svg>;
    case 'list': return <svg {...props}><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01" /></svg>;
    case 'info': return <svg {...props}><circle cx="12" cy="12" r="9"/><path d="M12 8h.01M11 12h1v4h1"/></svg>;
    case 'warn': return <svg {...props}><path d="M12 2L2 21h20L12 2z"/><path d="M12 9v5M12 18h.01"/></svg>;
    case 'plus': return <svg {...props}><path d="M12 5v14M5 12h14"/></svg>;
    case 'eye': return <svg {...props}><path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>;
    case 'sparkle': return <svg {...props}><path d="M12 3l1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5z"/><path d="M19 16l.7 2 2 .7-2 .7L19 22l-.7-2-2-.7 2-.7z"/></svg>;
    case 'edit': return <svg {...props}><path d="M11 4H4v16h16v-7"/><path d="M18 2l4 4-11 11H7v-4z"/></svg>;
    case 'database': return <svg {...props}><ellipse cx="12" cy="5" rx="9" ry="3"/><path d="M3 5v14a9 3 0 0 0 18 0V5M3 12a9 3 0 0 0 18 0"/></svg>;
    case 'menu': return <svg {...props}><path d="M3 6h18M3 12h18M3 18h18"/></svg>;
    default: return <svg {...props}><circle cx="12" cy="12" r="3"/></svg>;
  }
};

Object.assign(window, { Icon });
