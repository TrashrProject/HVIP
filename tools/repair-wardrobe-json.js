const fs = require('fs');

function readJson(path) {
  const raw = fs.readFileSync(path, 'utf8').replace(/^\uFEFF/, '');
  return JSON.parse(raw);
}

function writeJson(path, value) {
  fs.writeFileSync(path, JSON.stringify(value), 'utf8');
}

function asArray(value) {
  if (Array.isArray(value)) return value;
  if (value == null) return [];
  if (typeof value === 'object') return Object.values(value);
  return [];
}

function normalizeFigureData(fd) {
  fd.palettes = asArray(fd.palettes).map(p => ({
    ...p,
    colors: asArray(p.colors)
  }));

  fd.setTypes = asArray(fd.setTypes).map(st => ({
    ...st,
    sets: asArray(st.sets).map(set => ({
      ...set,
      club: 0,
      selectable: true,
      preselectable: true,
      parts: asArray(set.parts),
      hiddenLayers: asArray(set.hiddenLayers)
    }))
  }));

  return fd;
}

function normalizeFigureMap(fm) {
  fm.libraries = asArray(fm.libraries)
    .filter(lib => lib && lib.id)
    .map(lib => ({
      ...lib,
      parts: asArray(lib.parts)
    }));

  return fm;
}

const [figureDataPath, figureMapPath] = process.argv.slice(2);
if (!figureDataPath || !figureMapPath) {
  console.error('Usage: node repair-wardrobe-json.js FigureData.json FigureMap.json');
  process.exit(2);
}

const fd = normalizeFigureData(readJson(figureDataPath));
const fm = normalizeFigureMap(readJson(figureMapPath));

writeJson(figureDataPath, fd);
writeJson(figureMapPath, fm);

let setCount = 0;
let partCount = 0;
for (const st of fd.setTypes) {
  setCount += st.sets.length;
  for (const set of st.sets) partCount += set.parts.length;
}

let libraryPartCount = 0;
let emptyLibraries = 0;
for (const lib of fm.libraries) {
  libraryPartCount += lib.parts.length;
  if (!lib.parts.length) emptyLibraries++;
}

console.log(`FigureData OK: ${fd.setTypes.length} types, ${setCount} sets, ${partCount} parts`);
console.log(`FigureMap OK: ${fm.libraries.length} libraries, ${libraryPartCount} parts, ${emptyLibraries} libraries vides`);
console.log('Structure compatible Nitro réécrite avec parts[] partout.');
