const fs=require('fs'),path=require('path');
const [figureMapPath,figureDir]=process.argv.slice(2);
if(!figureMapPath||!figureDir){console.error('Usage: node scan-missing-clothes.js FigureMap.json figureDir');process.exit(2)}
try{
  const fm=JSON.parse(fs.readFileSync(figureMapPath,'utf8'));
  const libs=Array.isArray(fm.libraries)?fm.libraries:[];
  const missing=[];
  for(const lib of libs){
    const id=String(lib?.id||'');
    if(!id) continue;
    const file=id.split('*')[0]+'.nitro';
    if(!fs.existsSync(path.join(figureDir,file))) missing.push({id,file,parts:Array.isArray(lib.parts)?lib.parts.length:0});
  }
  const unique=[...new Map(missing.map(x=>[x.file.toLowerCase(),x])).values()].sort((a,b)=>a.file.localeCompare(b.file));
  console.log(`FigureMap libraries: ${libs.length}`);
  console.log(`Assets .nitro manquants: ${unique.length}`);
  for(const x of unique.slice(0,150)) console.log(`MISSING ${x.file} (${x.parts} parts)`);
  if(unique.length>150) console.log(`... ${unique.length-150} autres manquants`);
  const report=path.join(path.dirname(figureMapPath),'MissingFigureAssets.txt');
  fs.writeFileSync(report,unique.map(x=>x.file).join('\r\n'),'utf8');
  console.log(`Rapport: ${report}`);
  if(unique.length) console.log('Scan termine avec des assets manquants a completer, mais la structure est valide.');
  else console.log('Scan termine: aucun asset figure manquant.');
  process.exitCode=0;
}catch(error){
  console.error(`Erreur reelle du scan: ${error && error.message ? error.message : error}`);
  process.exitCode=2;
}
