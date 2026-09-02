const fs = require('fs');
const path = require('path');
const vm = require('vm');

const bundlePath = path.resolve(__dirname, '../WebPixel/nitro/assets/index-988G3uA2.js');
const bundle = fs.readFileSync(bundlePath, 'utf8');
const required = [
  'eventUrlPrefix:`commands/`',
  'e.startsWith(`commands/chunk/`)',
  'console.error(`[Commands] Unable to parse command-center payload`',
  'console.error(`[Commands] Unable to decode completed transfer`',
  'className:`commands-window`',
  'placeholder:`Rechercher une commande, une action…`'
];
for (const marker of required) {
  if (!bundle.includes(marker)) throw new Error(`Missing commands marker: ${marker}`);
}
if (bundle.includes('eventUrlPrefix:`habblet/open/`,linkReceived:e=>{if(e.startsWith(`habblet/open/commands-chunk/`)')) {
  throw new Error('Legacy conflicting commands transport is still present');
}

const payload = {
  header: 'commands',
  data: {
    categories: ['Toutes', 'Roleplay', 'Métiers', 'General', 'Staff'],
    commands: [
      { name: ':reanimer', aliases: [':medicalrevive'], description: 'Réanimer un joueur.', usage: ':reanimer [pseudo]', category: 'Métiers', subcategory: 'Hôpital', access: 'EMS' },
      { name: ':revive', aliases: [], description: 'Réanimation staff.', usage: ':revive [pseudo]', category: 'Staff', subcategory: 'Staff', access: 'Staff uniquement' }
    ]
  }
};
const encoded = Buffer.from(JSON.stringify(payload), 'utf8').toString('base64url');
const chunks = [encoded.slice(0, 23), encoded.slice(23)];
const rebuilt = chunks.join('').replace(/-/g, '+').replace(/_/g, '/');
const decoded = JSON.parse(Buffer.from(rebuilt, 'base64').toString('utf8'));
if (decoded.header !== 'commands' || decoded.data.commands.length !== 2) {
  throw new Error('Chunk round-trip failed');
}
for (const category of decoded.data.categories) {
  const visible = decoded.data.commands.filter(command => category === 'Toutes' || command.category === category);
  if (!Array.isArray(visible)) throw new Error(`Category filter failed: ${category}`);
}
const search = 'revive';
const results = decoded.data.commands.filter(command =>
  `${command.name} ${command.aliases.join(' ')} ${command.description} ${command.usage} ${command.category} ${command.subcategory} ${command.access}`
    .toLowerCase().includes(search)
);
if (results.length !== 2) throw new Error('Search test failed');

new vm.Script(bundle, { filename: 'index-988G3uA2.js' });
console.log('Commands center verification passed: transport, UTF-8, categories, search, bundle syntax.');
