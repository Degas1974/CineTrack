window.cineTrackIcons = {
  _cache: {},
  async get(name) {
    if (this._cache[name] !== undefined) return this._cache[name];
    try {
      const r = await fetch(`icons/duotone/${name}.svg`);
      if (!r.ok) { this._cache[name] = ''; return ''; }
      const text = await r.text();
      const cleaned = text.replace(/<!--[\s\S]*?-->/g, '');
      this._cache[name] = cleaned;
      return cleaned;
    } catch {
      this._cache[name] = '';
      return '';
    }
  }
};
