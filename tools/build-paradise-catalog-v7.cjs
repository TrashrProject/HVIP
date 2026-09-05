/*
 * ParadiseRP catalogue React rebuild.
 *
 * The deployed Nitro client is a customized production bundle whose matching
 * TypeScript project is not present in this repository or its Git history.
 * Rebuilding the public Nitro 2.1.1 tree would remove ParadiseRP-specific
 * features embedded in the current 2.2.0 bundle. This deterministic patcher
 * therefore replaces only the catalogue React components in the known bundle.
 *
 * It deliberately does not alter packets, the renderer, furniture data,
 * catalogue offers, ids, currencies, or any WavePlus source.
 */

'use strict';

const crypto = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');

const repositoryRoot = path.resolve(__dirname, '..');
const inputPath = path.join(repositoryRoot, 'WebPixel', 'nitro', 'assets', 'index-988G3uA2.js');
const outputPath = path.join(repositoryRoot, 'WebPixel', 'nitro', 'assets', 'index-paradise-catalog-v7.js');
const expectedSha256 = '34f7941abee35bb1fd4d64e9b3fa6ab49c50f61441330ded97168c6f27abf0a8';
const buildMarker = '20260906-react-v7';

function sha256(value) {
    return crypto.createHash('sha256').update(value).digest('hex');
}

function findFunctionEnd(source, openingBrace) {
    let depth = 0;
    let quote = null;
    let escaped = false;

    for(let index = openingBrace; index < source.length; index++) {
        const character = source[index];

        if(quote) {
            if(escaped) {
                escaped = false;
                continue;
            }

            if(character === '\\') {
                escaped = true;
                continue;
            }

            if(character === quote) quote = null;
            continue;
        }

        if(character === '"' || character === "'" || character === '`') {
            quote = character;
            continue;
        }

        if(character === '{') depth++;
        if(character === '}') {
            depth--;
            if(depth === 0) return index;
        }
    }

    throw new Error(`Unterminated function body at offset ${ openingBrace }.`);
}

function replaceArrowFunction(source, marker, body) {
    const first = source.indexOf(marker);
    const second = source.indexOf(marker, first + marker.length);

    if(first < 0) throw new Error(`Component marker not found: ${ marker }`);
    if(second >= 0) throw new Error(`Component marker is ambiguous: ${ marker }`);

    const openingBrace = source.indexOf('{', first + marker.length);
    if(openingBrace < 0) throw new Error(`Function body not found: ${ marker }`);

    const end = findFunctionEnd(source, openingBrace);
    return source.slice(0, first) + marker + body.trim() + source.slice(end + 1);
}

const components = {
    // Search is rendered once in the new header. The sidebar only owns the
    // server-provided category tree and never duplicates business state.
    'zae=e=>': `{
        let { node:t=null }=e,
            { searchResult:n=null }=g2();

        return (0,K.jsxs)(K.Fragment,{children:[
            (0,K.jsx)(\`div\`,{className:\`pc7-sidebar-title\`,children:n?\`Résultats\`:\`Catégories\`}),
            (0,K.jsx)(X,{
                fullHeight:!0,
                className:\`nitro-catalog-navigation-grid-container pc7-navigation\`,
                overflow:\`hidden\`,
                children:(0,K.jsxs)(xQ,{
                    id:\`nitro-catalog-main-navigation\`,
                    gap:1,
                    columnCount:1,
                    children:[
                        n&&n.filteredNodes.length>0&&n.filteredNodes.map((e,t)=>(0,K.jsx)(a6,{node:e},t)),
                        !n&&(0,K.jsx)(i6,{node:t})
                    ]
                })
            })
        ]})
    }`,

    // A real React product card driven by the native CatalogOffer instance.
    // No duplicated ids, prices or currency data are introduced here.
    's6=e=>': `{
        let { offer:t=null,selectOffer:n=null,itemActive:r=!1,...i }=e,
            [a,o]=(0,V.useState)(!1),
            [s,c]=(0,V.useState)(!1),
            { requestOfferToMover:l=null }=g2(),
            { isVisible:u=!1 }=G2(),
            d=(0,V.useMemo)(()=>t.pricingModel===mJ.PRICING_MODEL_BUNDLE?null:t.product.getIconUrl(t),[t]),
            f=e=>{
                switch(e.type){
                    case Qp.MOUSE_DOWN:n(t),o(!0);return;
                    case Qp.MOUSE_UP:o(!1);return;
                    case Qp.ROLL_OUT:if(!a||!r||!u)return;l(t);return
                }
            },
            p=t.product;

        if(!p)return null;

        let m=t.localizationName||t.offerName||\`Mobilier\`,
            h=!!p.uniqueLimitedItemSeriesSize,
            g=h&&!p.uniqueLimitedItemsLeft,
            _=[];

        t.priceInCredits>0&&_.push((0,K.jsxs)(\`span\`,{className:\`pc7-price-part\`,children:[
            (0,K.jsx)(\`strong\`,{children:t.priceInCredits}),
            (0,K.jsx)(Y$,{type:-1})
        ]},\`credits\`));

        t.priceInActivityPoints>0&&_.push((0,K.jsxs)(\`span\`,{className:\`pc7-price-part\`,children:[
            (0,K.jsx)(\`strong\`,{children:t.priceInActivityPoints}),
            (0,K.jsx)(Y$,{type:t.activityPointType})
        ]},\`points\`));

        !_.length&&_.push((0,K.jsx)(\`span\`,{className:\`pc7-price-free\`,children:\`Gratuit\`},\`free\`));

        return (0,K.jsx)(n1,{
            itemCount:t.pricingModel===mJ.PRICING_MODEL_MULTI?p.productCount:1,
            itemActive:r,
            disabled:g,
            classNames:[\`pc7-product-card\`,g?\`pc7-product-soldout\`:\`\`].filter(Boolean),
            title:m,
            "aria-label":m,
            role:\`button\`,
            tabIndex:0,
            onMouseDown:f,
            onMouseUp:f,
            onMouseOut:f,
            onKeyDown:e=>{
                (e.key===\`Enter\`||e.key===\` \`)&&(e.preventDefault(),n(t))
            },
            ...i,
            children:(0,K.jsxs)(K.Fragment,{children:[
                (0,K.jsxs)(\`div\`,{className:\`pc7-product-visual\`,children:[
                    p.productType===fJ.ROBOT&&(0,K.jsx)(G$,{figure:p.extraParam,headOnly:!0,direction:3}),
                    p.productType!==fJ.ROBOT&&d&&!s&&(0,K.jsx)(\`img\`,{
                        src:d,
                        alt:m,
                        loading:\`lazy\`,
                        draggable:!1,
                        onError:()=>c(!0)
                    }),
                    p.productType!==fJ.ROBOT&&(!d||s)&&(0,K.jsx)(\`span\`,{className:\`pc7-product-fallback\`,"aria-hidden":\`true\`,children:\`◇\`}),
                    h&&(0,K.jsx)(\`span\`,{className:\`pc7-product-limited\`,children:g?\`Épuisé\`:\`LIMITÉ\`})
                ]}),
                (0,K.jsxs)(\`div\`,{className:\`pc7-product-meta\`,children:[
                    (0,K.jsx)(\`span\`,{className:\`pc7-product-name\`,children:m}),
                    (0,K.jsx)(\`span\`,{className:\`pc7-product-price\`,children:_})
                ]})
            ]})
        })
    }`,

    // Incremental rendering keeps very large catalogue pages responsive while
    // retaining every offer. More cards are mounted as the native grid scrolls.
    'c6=e=>': `{
        let { columnCount:t=5,children:n=null,...r }=e,
            { currentOffer:i=null,setCurrentOffer:a=null,currentPage:o=null,setPurchaseOptions:s=null }=g2(),
            [c,l]=(0,V.useState)(180),
            u=(0,V.useRef)();

        (0,V.useEffect)(()=>{
            u&&u.current&&(u.current.scrollTop=0);
            l(180)
        },[o]);

        if(!o)return null;

        let d=e=>{
                e.activate();
                !e.isLazy&&(a(e),e.product&&e.product.productType===fJ.WALL&&s(t=>{
                    let n={...t};
                    return n.extraData=e.product.extraParam||null,n
                }))
            },
            f=o.offers||[],
            p=f.slice(0,c),
            m=e=>{
                let t=e.currentTarget;
                t&&t.scrollHeight-t.scrollTop-t.clientHeight<260&&l(e=>Math.min(e+120,f.length))
            };

        return (0,K.jsxs)(xQ,{
            innerRef:u,
            columnCount:t,
            onScroll:m,
            ...r,
            children:[
                !f.length&&(0,K.jsxs)(\`div\`,{className:\`pc7-grid-empty\`,children:[
                    (0,K.jsx)(\`span\`,{className:\`pc7-empty-icon\`,"aria-hidden":\`true\`,children:\`◇\`}),
                    (0,K.jsx)(\`strong\`,{children:\`Aucun mobilier trouvé\`}),
                    (0,K.jsx)(\`span\`,{children:\`Essayez une autre catégorie ou modifiez votre recherche.\`})
                ]}),
                p.map((e,t)=>(0,K.jsx)(s6,{itemActive:i&&i.offerId===e.offerId,offer:e,selectOffer:d},e.offerId||t)),
                c<f.length&&(0,K.jsxs)(\`div\`,{className:\`pc7-grid-progress\`,children:[
                    (0,K.jsx)(i1,{}),
                    (0,K.jsxs)(\`span\`,{children:[c,\` / \`,f.length,\` objets chargés\`]})
                ]}),
                n
            ]
        })
    }`,

    // The normal furniture layout becomes a products + details split. The
    // server-specific layouts remain routed through the original layout router.
    'Gae=e=>': `{
        let { page:t=null }=e,
            { currentOffer:n=null }=g2();

        return (0,K.jsxs)(\`div\`,{className:\`pc7-standard-layout\`,children:[
            (0,K.jsx)(\`section\`,{className:\`pc7-products-pane\`,"aria-label":\`Mobilier\`,children:
                (0,K.jsx)(c6,{
                    columnCount:4,
                    columnMinWidth:132,
                    columnMinHeight:168,
                    classNames:[\`pc7-products-grid\`]
                })
            }),
            (0,K.jsxs)(\`aside\`,{className:\`pc7-details-pane\`,"aria-label":\`Fiche produit\`,children:[
                !n&&(0,K.jsxs)(\`div\`,{className:\`pc7-empty-details\`,children:[
                    (0,K.jsx)(\`span\`,{className:\`pc7-empty-icon\`,"aria-hidden":\`true\`,children:\`◇\`}),
                    (0,K.jsx)(\`strong\`,{children:\`Choisissez un mobilier\`}),
                    (0,K.jsx)(\`p\`,{children:\`Sélectionnez une carte pour afficher sa preview, son prix et les options d’achat.\`})
                ]}),
                n&&(0,K.jsxs)(K.Fragment,{children:[
                    (0,K.jsxs)(\`div\`,{className:\`pc7-detail-preview\`,children:[
                        n.product.productType!==fJ.BADGE&&(0,K.jsxs)(K.Fragment,{children:[
                            (0,K.jsx)(p6,{}),
                            (0,K.jsx)(m6,{className:\`pc7-addon-badge\`})
                        ]}),
                        n.product.productType===fJ.BADGE&&(0,K.jsx)(m6,{className:\`scale-2\`})
                    ]}),
                    (0,K.jsxs)(\`div\`,{className:\`pc7-detail-content\`,children:[
                        (0,K.jsx)(l6,{fullWidth:!0}),
                        (0,K.jsx)(\`h2\`,{className:\`pc7-detail-name\`,children:n.localizationName}),
                        (0,K.jsxs)(\`div\`,{className:\`pc7-detail-price-row\`,children:[
                            (0,K.jsx)(\`span\`,{children:\`Prix\`}),
                            (0,K.jsx)(f6,{justifyContent:\`end\`,alignItems:\`end\`})
                        ]}),
                        (0,K.jsxs)(\`div\`,{className:\`pc7-detail-quantity-row\`,children:[
                            (0,K.jsx)(\`span\`,{children:\`Quantité\`}),
                            (0,K.jsx)(h6,{})
                        ]}),
                        (0,K.jsx)(\`div\`,{className:\`pc7-purchase-actions\`,children:(0,K.jsx)(u6,{})})
                    ]})
                ]})
            ]})
        ]})
    }`,

    // New top-level shell. Existing visibility state, links, root nodes,
    // current page selection, purchase dialogs and gift flow are all reused.
    'boe=e=>': `{
        let {
                isVisible:t=!1,setIsVisible:n=null,rootNode:r=null,currentPage:i=null,
                navigationHidden:a=!1,setNavigationHidden:o=null,activeNodes:s=[],isBusy:m=!1,
                searchResult:c=null,setSearchResult:l=null,openPageByName:u=null,
                openPageByOfferId:d=null,activateNode:f=null
            }=g2(),
            { getCurrencyAmount:p=null }=n4();

        (0,V.useEffect)(()=>{
            let e={
                linkReceived:e=>{
                    let t=e.split(\`/\`);
                    if(t.length<2)return;
                    switch(t[1]){
                        case \`show\`:n(!0);return;
                        case \`hide\`:n(!1);return;
                        case \`toggle\`:n(e=>!e);return;
                        case \`open\`:
                            if(t.length>2){
                                if(t.length===4&&t[2]===\`offerId\`){d(parseInt(t[3]));return}
                                u(t[2]);return
                            }
                            n(!0);return
                    }
                },
                eventUrlPrefix:\`catalog/\`
            };
            return wq(e),()=>Lq(e)
        },[n,d,u]);

        return (0,K.jsxs)(K.Fragment,{children:[
            t&&(0,K.jsxs)(W$,{
                uniqueKey:\`catalog\`,
                className:\`nitro-catalog paradise-catalog-v7\`,
                style:{width:\`1120px\`,height:\`720px\`,maxWidth:\`calc(100vw - 24px)\`,maxHeight:\`calc(100vh - 24px)\`},
                children:[
                    (0,K.jsx)(I$,{
                        headerText:\`ParadiseRP · CATALOGUE\`,
                        classNames:[\`pc7-window-header\`],
                        onCloseClick:e=>n(!1)
                    }),
                    (0,K.jsxs)(\`div\`,{className:\`pc7-toolbar\`,children:[
                        (0,K.jsxs)(\`div\`,{className:\`pc7-brand\`,children:[
                            (0,K.jsx)(\`strong\`,{children:\`ParadiseRP\`}),
                            (0,K.jsx)(\`span\`,{children:\`Tout pour construire votre univers.\`})
                        ]}),
                        (0,K.jsx)(\`div\`,{className:\`pc7-global-search\`,children:(0,K.jsx)(Rae,{})}),
                        (0,K.jsxs)(\`div\`,{className:\`pc7-balance\`,title:\`Solde de crédits\`,children:[
                            (0,K.jsx)(Y$,{type:-1}),
                            (0,K.jsx)(\`strong\`,{children:Number(p?p(-1):0).toLocaleString(\`fr-FR\`)}),
                            (0,K.jsx)(\`span\`,{children:\`crédits\`})
                        ]})
                    ]}),
                    (0,K.jsx)(R1,{
                        classNames:[\`pc7-root-tabs\`],
                        justifyContent:\`start\`,
                        children:r&&r.children.length>0&&r.children.map(e=>e.isVisible?(0,K.jsx)(L1,{
                            isActive:e.isActive,
                            classNames:[\`pc7-root-tab\`],
                            onClick:t=>{c&&l(null),f(e)},
                            children:(0,K.jsxs)(J,{gap:Nq(\`catalog.tab.icons\`)?1:0,alignItems:\`center\`,children:[
                                Nq(\`catalog.tab.icons\`)&&(0,K.jsx)(t6,{icon:e.iconId}),
                                e.localization
                            ]})
                        },e.pageId):null)
                    }),
                    (0,K.jsx)(N$,{
                        classNames:[\`pc7-content-area\`],
                        grow:!0,
                        overflow:\`hidden\`,
                        children:(0,K.jsxs)(\`div\`,{
                            className:\`pc7-workspace \`+(a?\`pc7-navigation-hidden\`:\`\`),
                            children:[
                                !a&&(0,K.jsx)(\`aside\`,{className:\`pc7-sidebar\`,children:s&&s.length>0&&(0,K.jsx)(zae,{node:s[0]})}),
                                (0,K.jsxs)(\`main\`,{className:\`pc7-main\`,children:[
                                    voe(i,()=>o(!0)),
                                    m&&(0,K.jsxs)(\`div\`,{className:\`pc7-main-loading\`,children:[
                                        (0,K.jsx)(i1,{}),
                                        (0,K.jsx)(\`span\`,{children:\`Chargement du catalogue…\`})
                                    ]})
                                ]})
                            ]
                        })
                    })
                ]
            }),
            (0,K.jsx)(Fae,{}),
            (0,K.jsx)(yoe,{})
        ]})
    }`
};

let source = fs.readFileSync(inputPath, 'utf8');
const actualSha256 = sha256(source);

if(actualSha256 !== expectedSha256) {
    throw new Error(
        `Refusing to patch an unknown Nitro bundle. Expected ${ expectedSha256 }, received ${ actualSha256 }.`
    );
}

for(const [marker, body] of Object.entries(components)) {
    source = replaceArrowFunction(source, marker, body);
}

// The upstream production artifact contains a few lines ending in spaces.
// Normalizing line endings keeps the generated Git artifact reviewable and
// does not alter JavaScript or embedded shader semantics.
source = source.replace(/[ \t]+$/gm, '');
source += `\n;globalThis.__PARADISE_CATALOG_BUILD__=${ JSON.stringify(buildMarker) };\n`;
fs.writeFileSync(outputPath, source, 'utf8');

console.log(`ParadiseRP catalogue bundle generated: ${ path.relative(repositoryRoot, outputPath) }`);
console.log(`Build marker: ${ buildMarker }`);
console.log(`SHA-256: ${ sha256(source) }`);
