const fs = require('fs');

function readJson(path) {
  return JSON.parse(fs.readFileSync(path, 'utf8'));
}

function writeJson(path, value) {
  fs.writeFileSync(path, JSON.stringify(value), 'utf8');
}

function key(v) {
  return String(v ?? '');
}

function mergePalettes(current = [], latest = []) {
  const out = latest.map(p => ({ ...p, colors: Array.isArray(p.colors) ? [...p.colors] : [] }));
  const byId = new Map(out.map((p, i) => [key(p.id), i]));

  for (const oldPalette of current || []) {
    const id = key(oldPalette.id);
    if (!byId.has(id)) {
      out.push(oldPalette);
      byId.set(id, out.length - 1);
      continue;
    }

    const target = out[byId.get(id)];
    const colors = Array.isArray(target.colors) ? target.colors : (target.colors = []);
    const colorIds = new Set(colors.map(c => key(c.id)));

    for (const color of oldPalette.colors || []) {
      if (!colorIds.has(key(color.id))) colors.push(color);
    }
  }

  return out;
}

function unlockSet(set) {
  if (!set || typeof set !== 'object') return set;
  set.club = 0;
  set.selectable = true;
  set.preselectable = true;
  return set;
}

function mergeSetTypes(current = [], latest = []) {
  const out = latest.map(st => ({ ...st, sets: Array.isArray(st.sets) ? st.sets.map(s => unlockSet({ ...s })) : [] }));
  const byType = new Map(out.map((st, i) => [key(st.type), i]));

  for (const oldType of current || []) {
    const type = key(oldType.type);
    if (!byType.has(type)) {
      const cloned = { ...oldType, sets: (oldType.sets || []).map(s => unlockSet({ ...s })) };
      out.push(cloned);
      byType.set(type, out.length - 1);
      continue;
    }

    const target = out[byType.get(type)];
    const sets = Array.isArray(target.sets) ? target.sets : (target.sets = []);
    const setIds = new Set(sets.map(s => key(s.id)));

    for (const set of oldType.sets || []) {
      if (!setIds.has(key(set.id))) sets.push(unlockSet({ ...set }));
    }
  }

  for (const type of out) {
    type.sets = (type.sets || []).map(unlockSet);
  }

  return out;
}

function mergeFigureData(current, latest) {
  const result = { ...latest };
  result.palettes = mergePalettes(current?.palettes || [], latest?.palettes || []);
  result.setTypes = mergeSetTypes(current?.setTypes || [], latest?.setTypes || []);
  return result;
}

function mergeFigureMap(current, latest) {
  const result = { ...latest };
  const latestLibraries = Array.isArray(latest?.libraries) ? [...latest.libraries] : [];
  const ids = new Set(latestLibraries.map(x => key(x.id)));

  for (const lib of current?.libraries || []) {
    if (!ids.has(key(lib.id))) latestLibraries.push(lib);
  }

  result.libraries = latestLibraries;
  return result;
}

function countSets(data) {
  return (data?.setTypes || []).reduce((n, st) => n + (st.sets?.length || 0), 0);
}

const [currentFdPath, currentFmPath, latestFdPath, latestFmPath, outFdPath, outFmPath] = process.argv.slice(2);
if (![currentFdPath, currentFmPath, latestFdPath, latestFmPath, outFdPath, outFmPath].every(Boolean)) {
  console.error('Usage: node merge-wardrobe.js currentFD currentFM latestFD latestFM outFD outFM');
  process.exit(2);
}

const currentFD = readJson(currentFdPath);
const currentFM = readJson(currentFmPath);
const latestFD = readJson(latestFdPath);
const latestFM = readJson(latestFmPath);

const mergedFD = mergeFigureData(currentFD, latestFD);
const mergedFM = mergeFigureMap(currentFM, latestFM);

writeJson(outFdPath, mergedFD);
writeJson(outFmPath, mergedFM);

console.log(`FigureData: ancien=${countSets(currentFD)} officiel=${countSets(latestFD)} fusionne=${countSets(mergedFD)}`);
console.log(`FigureMap: ancien=${currentFM.libraries?.length || 0} officiel=${latestFM.libraries?.length || 0} fusionne=${mergedFM.libraries?.length || 0}`);
console.log('Tous les sets fusionnes ont ete rendus selectionnables dans Mi ropa.');
