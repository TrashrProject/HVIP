const fs = require('fs');
const path = require('path');

const root = process.argv[2] || 'C:\\HVIP';
const assetRoot = path.join(root, 'swf_pz', 'V5-0-2');
const figureDataPath = path.join(assetRoot, 'gamedata', 'json', 'FigureData.json');
const figureMapPath = path.join(assetRoot, 'gamedata', 'json', 'FigureMap.json');
const outPath = path.join(root, 'WebPixel', 'nitro-last', 'rp-outfits.json');

const MALE_BASE = 'hr-100.hd-180-7.ch-215-66.lg-270-79.sh-305-62.ha-1002-70.wa-2007';
const FEMALE_BASE = 'hr-515-33.hd-600-1.ch-635-70.lg-716-66-62.sh-735-68';

const ROLE_RULES = [
  { id: 'police', label: 'Police & SWAT', icon: '🛡️', re: /(police|policia|cop\b|swat|security|sheriff|gendar|fbi|detective|tactical|riot|guard|agent[_ -]?police)/i },
  { id: 'medical', label: 'Médical', icon: '⚕️', re: /(medic|doctor|nurse|hospital|ambulance|paramedic|surgeon|health|medico|enferm)/i },
  { id: 'fire', label: 'Pompiers', icon: '🚒', re: /(firefighter|fireman|firewoman|bombero|pompier|fire[_ -]?dept)/i },
  { id: 'justice', label: 'Justice & Gouvernement', icon: '⚖️', re: /(lawyer|judge|justice|mayor|president|government|formal|business|office|suit|tie)/i },
  { id: 'worker', label: 'Métiers', icon: '🧰', re: /(mechanic|worker|construction|builder|engineer|repair|chef|cook|waiter|barista|delivery|driver|taxi|pilot|farmer)/i },
  { id: 'military', label: 'Militaire', icon: '🎖️', re: /(military|militar|army|soldier|combat|marine|navy|airforce|commando)/i },
  { id: 'criminal', label: 'Underground', icon: '💀', re: /(gang|biker|prison|inmate|criminal|thug|balaclava|bandit|mafia|narco|cartel|robber|hood)/i }
];

function readJson(file) {
  return JSON.parse(fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, ''));
}
function arr(v) {
  if (Array.isArray(v)) return v;
  if (!v) return [];
  if (typeof v === 'object') return Object.values(v);
  return [];
}
function parseFigure(str) {
  const map = new Map();
  String(str || '').split('.').filter(Boolean).forEach(token => {
    const p = token.split('-');
    if (p.length >= 2) map.set(p[0], token);
  });
  return map;
}
function figureString(map) {
  return [...map.values()].join('.');
}
function cleanName(id) {
  return String(id || '')
    .replace(/^(figure_|shirt_|jacket_|trousers_|shoes_|hat_|hair_|acc_[a-z]+_)/i, '')
    .replace(/^[mfu]_/i, '')
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, c => c.toUpperCase())
    .trim();
}

if (!fs.existsSync(figureDataPath) || !fs.existsSync(figureMapPath)) {
  console.error('FigureData/FigureMap introuvable.');
  process.exit(2);
}

const fd = readJson(figureDataPath);
const fm = readJson(figureMapPath);
const palettes = new Map();
for (const p of arr(fd.palettes)) {
  const colors = arr(p.colors);
  const usable = colors.find(c => c && (c.selectable === true || c.selectable === 1 || c.selectable === '1')) || colors[0];
  palettes.set(String(p.id), usable && usable.id != null ? String(usable.id) : '1');
}

const partToLibrary = new Map();
for (const lib of arr(fm.libraries)) {
  if (!lib || !lib.id) continue;
  for (const part of arr(lib.parts)) {
    const type = String(part.type || '').toLowerCase();
    const id = String(part.id ?? '');
    if (type && id) partToLibrary.set(`${type}:${id}`, String(lib.id));
  }
}

const detected = [];
for (const st of arr(fd.setTypes)) {
  const type = String(st.type || st.name || '').toLowerCase();
  if (!type) continue;
  const defaultColor = palettes.get(String(st.paletteId ?? st.palette ?? '')) || '1';
  for (const set of arr(st.sets)) {
    if (set == null || set.id == null) continue;
    const libs = [...new Set(arr(set.parts).map(part => {
      const ptype = String(part.type || type).toLowerCase();
      return partToLibrary.get(`${ptype}:${String(part.id ?? '')}`);
    }).filter(Boolean))];
    if (!libs.length) continue;
    const haystack = libs.join(' ');
    const rule = ROLE_RULES.find(r => r.re.test(haystack));
    if (!rule) continue;
    const genderRaw = String(set.gender || 'U').toUpperCase();
    const genders = genderRaw === 'U' ? ['M', 'F'] : [genderRaw.startsWith('F') ? 'F' : 'M'];
    detected.push({
      role: rule.id,
      roleLabel: rule.label,
      icon: rule.icon,
      type,
      setId: String(set.id),
      color: defaultColor,
      genders,
      library: libs[0],
      libraries: libs
    });
  }
}

const clothingPriority = ['ha','he','ea','fa','ca','wa','cc','cp','ch','lg','sh'];
const result = [];
let uid = 1;
for (const rule of ROLE_RULES) {
  for (const gender of ['M', 'F']) {
    const pool = detected.filter(x => x.role === rule.id && x.genders.includes(gender));
    if (!pool.length) continue;
    const byType = new Map();
    for (const item of pool) {
      if (!byType.has(item.type)) byType.set(item.type, []);
      byType.get(item.type).push(item);
    }
    for (const list of byType.values()) list.sort((a,b) => a.library.localeCompare(b.library));
    const richest = [...byType.values()].reduce((m, x) => Math.max(m, x.length), 0);
    const count = Math.min(Math.max(richest, 3), 18);
    for (let i = 0; i < count; i++) {
      const figure = parseFigure(gender === 'F' ? FEMALE_BASE : MALE_BASE);
      const sources = [];
      for (const type of clothingPriority) {
        const list = byType.get(type);
        if (!list || !list.length) continue;
        const item = list[i % list.length];
        figure.set(type, `${type}-${item.setId}-${item.color}`);
        sources.push(item.library);
      }
      // Ajoute les types RP inconnus sans toucher au visage/corps.
      for (const [type, list] of byType) {
        if (clothingPriority.includes(type) || ['hd','hr','hrb','ey','fc','bd'].includes(type)) continue;
        const item = list[i % list.length];
        figure.set(type, `${type}-${item.setId}-${item.color}`);
        sources.push(item.library);
      }
      result.push({
        id: `rp-${uid++}`,
        category: rule.id,
        categoryLabel: rule.label,
        icon: rule.icon,
        name: `${rule.label} ${i + 1}`,
        gender,
        figure: figureString(figure),
        source: sources.slice(0, 8).map(cleanName).filter(Boolean).join(' · ')
      });
    }
  }
}

const payload = {
  generatedAt: new Date().toISOString(),
  total: result.length,
  categories: ROLE_RULES.map(r => ({ id: r.id, label: r.label, icon: r.icon, count: result.filter(x => x.category === r.id).length })).filter(x => x.count),
  outfits: result
};

fs.mkdirSync(path.dirname(outPath), { recursive: true });
fs.writeFileSync(outPath, JSON.stringify(payload, null, 2), 'utf8');
console.log(`Tenues/pièces RP détectées: ${detected.length}`);
console.log(`Presets RP générés: ${result.length}`);
for (const c of payload.categories) console.log(`${c.icon} ${c.label}: ${c.count}`);
console.log(`Fichier: ${outPath}`);
