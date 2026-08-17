const fs = require('fs');
const path = require('path');

const root = process.argv[2] || 'C:\\HVIP';
const bundle = path.join(root, 'WebPixel', 'nitro-last', 'assets', 'index-9f9954ad.js');
const backup = bundle + '.before-rp.bak';
const indexFile = path.join(root, 'WebPixel', 'nitro-last', 'index.html');

console.log('ParadiseRP - Tenues RP / mode securise');
console.log('Le patch direct du bundle Nitro minifie est desactive.');

// Si un ancien patch RP est detecte et qu un backup existe, on restaure automatiquement.
if (fs.existsSync(bundle)) {
  const src = fs.readFileSync(bundle, 'utf8');
  if (src.includes('PARADISE_RP_WARDROBE_V1')) {
    if (!fs.existsSync(backup)) {
      throw new Error('Bundle RP patche detecte mais backup .before-rp.bak introuvable. Utilise git restore sur le bundle Nitro.');
    }
    fs.copyFileSync(backup, bundle);
    console.log('Ancien patch Tenues RP retire: bundle Nitro restaure depuis le backup.');
  }
}

// Retire uniquement la feuille RP de index.html si l ancien installateur l avait injectee.
if (fs.existsSync(indexFile)) {
  let html = fs.readFileSync(indexFile, 'utf8');
  const cleaned = html.replace(/\s*<link rel="stylesheet" href="\.\/rp-wardrobe\.css\?v=1">\s*/g, '\n');
  if (cleaned !== html) {
    fs.writeFileSync(indexFile, cleaned, 'utf8');
    console.log('Reference rp-wardrobe.css retiree de index.html.');
  }
}

console.log('Aucune injection dans le bundle Nitro n est effectuee.');
console.log('Les presets RP peuvent rester generes, mais l onglet sera recode proprement ensuite depuis les sources/client.');
