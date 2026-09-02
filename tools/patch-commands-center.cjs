const fs = require('fs');
const path = require('path');

const target = path.resolve(__dirname, '../WebPixel/nitro/assets/index-988G3uA2.js');
let source = fs.readFileSync(target, 'utf8');
const start = source.indexOf('Qhe=()=>');
const end = source.indexOf(';var $he=', start);
if (start < 0 || end < 0) throw new Error('Commands component was not found in the Nitro bundle');

let component = source.slice(start, end);
const replaceOnce = (before, after, label) => {
  const count = component.split(before).length - 1;
  if (count !== 1) throw new Error(`${label}: expected one match, found ${count}`);
  component = component.replace(before, after);
};

replaceOnce(
  'if(n.header!==`commands`||!n.data)return;r(n.data.commands||[])',
  'if(n.header!==`commands`||!n.data)throw Error(`Invalid commands payload`);if(!Array.isArray(n.data.commands))throw Error(`Commands list is not an array`);r(n.data.commands)',
  'payload validation'
);
replaceOnce(
  '}catch{}};(0,V.useEffect)',
  '}catch(e){console.error(`[Commands] Unable to parse command-center payload`,e)}};(0,V.useEffect)',
  'payload error logging'
);
replaceOnce(
  'eventUrlPrefix:`habblet/open/`,linkReceived:e=>{if(e.startsWith(`habblet/open/commands-chunk/`)){let t=e.split(`/`);if(t.length<7)return;let n=t[3],r=parseInt(t[4],10),i=parseInt(t[5],10);',
  'eventUrlPrefix:`commands/`,linkReceived:e=>{if(e.startsWith(`commands/chunk/`)){let t=e.split(`/`);if(t.length<6){console.error(`[Commands] Invalid chunk envelope`,e);return}let n=t[2],r=parseInt(t[3],10),i=parseInt(t[4],10);',
  'dedicated event transport'
);
replaceOnce(
  'a.chunks[r]=t.slice(6).join(`/`)',
  'a.chunks[r]=t.slice(5).join(`/`)',
  'chunk payload index'
);
replaceOnce(
  '}catch{}finally{f.current.delete(n)}})();return}e.startsWith(`habblet/open/`)&&p(e.substring(13).replaceAll(`&#47;`,`/`))',
  '}catch(e){console.error(`[Commands] Unable to decode completed transfer`,e)}finally{f.current.delete(n)}})();return}console.error(`[Commands] Unsupported event`,e)',
  'decode error logging'
);

source = source.slice(0, start) + component + source.slice(end);
fs.writeFileSync(target, source, 'utf8');
console.log('Patched native :commands transport and diagnostics.');
