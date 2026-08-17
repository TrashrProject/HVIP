const fs = require('fs');
const path = require('path');

const root = process.argv[2] || 'C:\\HVIP';
const bundle = path.join(root, 'WebPixel', 'nitro-last', 'assets', 'index-9f9954ad.js');
const indexFile = path.join(root, 'WebPixel', 'nitro-last', 'index.html');
if (!fs.existsSync(bundle)) throw new Error(`Bundle introuvable: ${bundle}`);

let src = fs.readFileSync(bundle, 'utf8');
const marker = 'PARADISE_RP_WARDROBE_V1';
if (!src.includes(marker)) {
  const componentAnchor = 'aT="hr-100.hd-180-7.ch-215-66.lg-270-79.sh-305-62.ha-1002-70.wa-2007"';
  if (!src.includes(componentAnchor)) throw new Error('Ancre AvatarEditor introuvable dans le bundle.');

  const component = `rpT=a=>{const{figureData:e=null,loadAvatarInEditor:r=null}=a,[n,o]=i.exports.useState([]),[s,c]=i.exports.useState("all"),[u,m]=i.exports.useState(!0),[p,g]=i.exports.useState("");i.exports.useEffect(()=>{let h=!1;return fetch("./rp-outfits.json?v="+Date.now(),{cache:"no-store"}).then(f=>f.ok?f.json():Promise.reject(new Error("HTTP "+f.status))).then(f=>{h||(o(Array.isArray(f.outfits)?f.outfits:[]),m(!1))}).catch(f=>{h||(g("Impossible de charger les tenues RP"),m(!1))}),()=>{h=!0}},[]);const h=i.exports.useMemo(()=>{const f=new Map;for(const _ of n)f.has(_.category)||f.set(_.category,{id:_.category,label:_.categoryLabel||_.category,icon:_.icon||"★"});return Array.from(f.values())},[n]),f=i.exports.useMemo(()=>s==="all"?n:n.filter(_=>_.category===s),[n,s]);return l(T,{className:"paradise-rp-wardrobe",gap:2,children:[l(w,{className:"pr-rp-head",justifyContent:"between",alignItems:"center",children:[l(T,{gap:0,children:[t(S,{bold:!0,children:"Tenues RP"}),t(S,{small:!0,variant:"muted",children:"Uniformes et looks métiers ParadiseRP"})]}),t(S,{small:!0,bold:!0,className:"pr-rp-count",children:f.length+" looks"})]}),l(w,{className:"pr-rp-filters",gap:1,children:[t(M,{size:"sm",variant:s==="all"?"primary":"secondary",onClick:()=>c("all"),children:"Tous"}),h.map(_=>t(M,{size:"sm",variant:s===_.id?"primary":"secondary",onClick:()=>c(_.id),children:(_.icon?_.icon+" ":"")+_.label},_.id))]}),u&&t(T,{center:!0,grow:!0,children:t(zi,{})}),p&&t(T,{center:!0,grow:!0,className:"text-danger",children:p}),!u&&!p&&t(Le,{columnCount:4,columnMinWidth:105,columnMinHeight:170,className:"pr-rp-grid",children:f.map(_=>l(be,{className:"pr-rp-card",center:!0,column:!0,children:[l(L,{className:"pr-rp-avatar",position:"relative",children:[t(ot,{figure:_.figure,gender:_.gender||e.gender,direction:2}),t(L,{className:"avatar-shadow"})]}),t(S,{bold:!0,small:!0,center:!0,truncate:!0,className:"pr-rp-name",children:_.name}),_.source&&t(S,{small:!0,center:!0,truncate:!0,variant:"muted",className:"pr-rp-source",children:_.source}),t(M,{size:"sm",variant:"success",fullWidth:!0,onClick:()=>r(_.figure,_.gender||e.gender,!1),children:"Essayer"})]},_.id))})]})};/*${marker}*/,`;

  src = src.replace(componentAnchor, component + componentAnchor);

  const stateAnchor = '[C,E]=i.exports.useState([]),[b,N]=i.exports.useState(!1),[v,y]=i.exports.useState(null)';
  if (!src.includes(stateAnchor)) throw new Error('Ancre état AvatarEditor introuvable.');
  src = src.replace(stateAnchor, '[C,E]=i.exports.useState([]),[b,N]=i.exports.useState(!1),[rpTab,setRpTab]=i.exports.useState(!1),[v,y]=i.exports.useState(null)');

  const categoryClick = 'onClick:Qe=>B(le),children:d(`avatareditor.category.${le}`)';
  if (!src.includes(categoryClick)) throw new Error('Ancre clic catégorie introuvable.');
  src = src.replace(categoryClick, 'onClick:Qe=>(setRpTab(!1),B(le)),children:d(`avatareditor.category.${le}`)');

  const tabsAnchor = 't(Wr,{isActive:b,onClick:le=>N(!0),children:d("avatareditor.category.wardrobe")})]';
  if (!src.includes(tabsAnchor)) throw new Error('Ancre onglet Armario introuvable.');
  src = src.replace(tabsAnchor, 't(Wr,{isActive:b&&!rpTab,onClick:le=>(setRpTab(!1),N(!0)),children:d("avatareditor.category.wardrobe")}),t(Wr,{isActive:rpTab,onClick:le=>(N(!1),setRpTab(!0)),className:"pr-rp-tab",children:"Tenues RP"})]');

  const bodyAnchor = 'p&&!b&&t(sT,{model:p,gender:s.gender,setGender:Ee}),b&&t(oT,{figureData:s,savedFigures:C,setSavedFigures:E,loadAvatarInEditor:X})';
  if (!src.includes(bodyAnchor)) throw new Error('Ancre contenu AvatarEditor introuvable.');
  src = src.replace(bodyAnchor, 'p&&!b&&!rpTab&&t(sT,{model:p,gender:s.gender,setGender:Ee}),b&&!rpTab&&t(oT,{figureData:s,savedFigures:C,setSavedFigures:E,loadAvatarInEditor:X}),rpTab&&t(rpT,{figureData:s,loadAvatarInEditor:X})');

  fs.copyFileSync(bundle, bundle + '.before-rp.bak');
  fs.writeFileSync(bundle, src, 'utf8');
  console.log('Bundle Nitro patché: onglet Tenues RP ajouté.');
} else {
  console.log('Bundle déjà patché pour Tenues RP.');
}

if (fs.existsSync(indexFile)) {
  let html = fs.readFileSync(indexFile, 'utf8');
  if (!html.includes('rp-wardrobe.css')) {
    html = html.replace('</head>', '    <link rel="stylesheet" href="./rp-wardrobe.css?v=1">\n  </head>');
    fs.writeFileSync(indexFile, html, 'utf8');
    console.log('rp-wardrobe.css ajouté à index.html.');
  }
}
