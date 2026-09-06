'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const repositoryRoot = path.resolve(__dirname, '..');
const read = relativePath => fs.readFileSync(path.join(repositoryRoot, relativePath), 'utf8');

const html = read('WebPixel/nitro/index.html');
const bundle = read('WebPixel/nitro/assets/index-paradise-catalog-v7.js');
const css = read('WebPixel/nitro/paradise-catalog-react-v7.css');

assert.match(html, /assets\/index-paradise-catalog-v7\.js\?v=20260906-react-v9-variant-icons/);
assert.match(html, /paradise-catalog-react-v7\.css\?v=20260906-react-v9-variant-icons/);
assert.doesNotMatch(html, /paradise-catalog-zero\.(?:js|css)/);

for(const marker of [
    '20260906-react-v9-variant-icons',
    'paradise-catalog-v7',
    'pc7-workspace',
    'pc7-standard-layout',
    'pc7-product-card',
    'width:96',
    'height:82',
    'pc7-details-pane',
    'Aucun mobilier trouvé'
]) assert.ok(bundle.includes(marker), `Missing generated catalogue marker: ${ marker }`);

assert.ok(bundle.includes('p.furnitureData.fullName||p.furnitureData.className'), 'Variant furniture fullName support is missing.');
assert.ok(bundle.includes('20260906-card-icons-v4'), 'Variant icon cache marker is missing.');

// Regression guards for native catalogue and ParadiseRP behavior that must
// survive the component replacement.
for(const nativeBehavior of [
    'new iP(',                 // native purchase packet
    'new J0(',                 // native gift packet
    'requestOfferToMover',     // placement/mover flow
    'roomPreviewer',           // furniture renderer preview
    'default_3x3_color_grouping',
    'room_bundle',
    'marketplace_own_items',
    'club_gifts',
    'badge_display',
    'roleplay-inventory',      // ParadiseRP custom client feature
    'corporation-center',      // ParadiseRP custom client feature
    'teleportDissolve'         // customized renderer behavior
]) assert.ok(bundle.includes(nativeBehavior), `Native behavior missing from bundle: ${ nativeBehavior }`);

for(const selector of [
    '.nitro-catalog.paradise-catalog-v7',
    '.pc7-workspace',
    '.pc7-products-grid',
    '.pc7-product-card.layout-grid-item',
    '.pc7-details-pane',
    'aspect-ratio: 96 / 82',
    '@media (max-width: 790px)',
    '@media (prefers-reduced-motion: reduce)'
]) assert.ok(css.includes(selector), `Missing catalogue stylesheet selector: ${ selector }`);

const openingBraces = (css.match(/{/g) || []).length;
const closingBraces = (css.match(/}/g) || []).length;
assert.equal(openingBraces, closingBraces, 'Unbalanced CSS braces.');

console.log('ParadiseRP catalogue v7 checks passed.');
console.log('Native purchase, gift, preview, placement and special-layout markers are present.');
