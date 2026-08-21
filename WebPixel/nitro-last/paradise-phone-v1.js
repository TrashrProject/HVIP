(() => {
  'use strict';
  if (window.ParadisePhoneV1) return;

  const VERSION = '4.0.0-phone-v1';
  const DATA_URL = '../rp-phone-data.php';
  const ACTION_URL = '../rp-phone-action.php';
  const HUD_ID = 'paradise-rp-hud';
  const POLL_MS = 2200;
  const APPS = new Set(['home','messages','contacts','calls','notifications','settings']);

  let hud = null, win = null, body = null, timer = 0, request = null, destroyed = false;
  let app = 'home', conversation = null, conversationMessages = [], lastUnread = 0, lastCallId = null;

  const esc = v => String(v ?? '').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;').replaceAll("'",'&#039;');
  const text = v => v == null ? '' : String(v).trim();
  const nowTime = value => { try { const d = value ? new Date(value) : new Date(); return new Intl.DateTimeFormat('fr-FR',{hour:'2-digit',minute:'2-digit'}).format(d); } catch (_) { return '--:--'; } };
  const phoneState = () => window.ParadiseStore?.getState?.().phone || null;

  function ensureStoreShape() {
    const root = window.ParadiseStore?.getState?.();
    if (!root) return null;
    if (!root.phone) root.phone = { available:false, hasDevice:false, number:null, contacts:[], conversations:[], unreadCount:0, activeCall:null, notifications:[], settings:{silent:false,notifications:true,sounds:true}, lastUpdatedAt:null, lastError:null };
    if (!root.ui.phoneApp) root.ui.phoneApp = 'home';
    if (!Object.prototype.hasOwnProperty.call(root.ui,'phoneConversation')) root.ui.phoneConversation = null;
    return root.phone;
  }

  function emit(name, detail) {
    window.dispatchEvent(new CustomEvent('paradise:phone', { detail:{ event:name, data:detail } }));
    window.dispatchEvent(new CustomEvent(`paradise:${name}`, { detail }));
  }

  function applySnapshot(payload) {
    const target = ensureStoreShape();
    if (!target) return false;
    if (!payload?.ok || !payload.phone) {
      target.lastError = payload?.reason || 'phone_unavailable';
      target.lastUpdatedAt = new Date().toISOString();
      render();
      return false;
    }
    const p = payload.phone;
    const previousUnread = Number(target.unreadCount || 0);
    const previousCall = target.activeCall?.id || null;
    Object.assign(target, {
      available: Boolean(p.available), hasDevice: Boolean(p.has_device), number: p.number || null,
      contacts: Array.isArray(p.contacts) ? p.contacts : [], conversations: Array.isArray(p.conversations) ? p.conversations : [],
      unreadCount: Math.max(0, Number(p.unread_count) || 0), activeCall: p.active_call || null,
      notifications: Array.isArray(p.notifications) ? p.notifications : [], settings: p.settings || target.settings,
      lastUpdatedAt: new Date().toISOString(), lastError: null
    });
    updateBadge();
    if (target.unreadCount > previousUnread && app !== 'messages') toast('Nouveau message', 'Vous avez reçu un nouveau message.');
    if (target.activeCall?.id && target.activeCall.id !== previousCall && target.activeCall.direction === 'incoming' && target.activeCall.status === 'RINGING') toast('Appel entrant', `${target.activeCall.other_name || target.activeCall.other_number} vous appelle.`);
    lastUnread = target.unreadCount; lastCallId = target.activeCall?.id || null;
    emit('phone:update', target); render();
    return true;
  }

  async function refresh() {
    if (destroyed || request) return request;
    request = (async()=>{ try { const r=await fetch(`${DATA_URL}?_=${Date.now()}`,{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}}); const p=await r.json(); return applySnapshot(p); } catch(e){ const s=ensureStoreShape(); if(s)s.lastError=e?.message||'request_failed'; return false; } finally { request=null; } })();
    return request;
  }

  async function action(payload) {
    const r = await fetch(ACTION_URL,{method:'POST',credentials:'same-origin',cache:'no-store',headers:{'Content-Type':'application/json',Accept:'application/json','X-Paradise-Action':'phase4'},body:JSON.stringify(payload||{})});
    let p=null; try{p=await r.json();}catch(_){}
    if(!r.ok||!p?.ok){const e=new Error(p?.message||p?.reason||`HTTP ${r.status}`);e.payload=p;throw e;}
    if(p.phone) applySnapshot({ok:true,phone:p.phone});
    if(p.message) systemFeedback(p.message,'PHONE');
    return p;
  }

  function avatar(look,name='Contact') {
    if (!look || !/^[a-z0-9.\-]+$/i.test(String(look))) return '<span class="pp-avatar-fallback">P</span>';
    return `<img src="../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=m&phone=1" alt="${esc(name)}" draggable="false">`;
  }

  function updateBadge() {
    if(!hud)return; const count=Math.max(0,Number(phoneState()?.unreadCount)||0);
    hud.querySelectorAll('[data-pr-phone-badge]').forEach(n=>{n.textContent=String(count);n.hidden=count<=0;});
  }

  function decorateHudButton() {
    const btn=hud?.querySelector('[data-window-open="phone"]'); if(!btn)return;
    btn.classList.add('pp-hud-button');
    if(!btn.querySelector('[data-pr-phone-badge]')) btn.insertAdjacentHTML('beforeend','<span class="pp-hud-badge" data-pr-phone-badge hidden>0</span>');
  }

  function shell() {
    return `<div class="pp-device" data-pp-device>
      <div class="pp-notch"><span></span></div>
      <div class="pp-screen">
        <header class="pp-status"><strong>${nowTime()}</strong><span>Paradise</span><div><i class="pp-signal"></i><i class="pp-battery"></i></div></header>
        <main class="pp-content" data-pp-content></main>
        <nav class="pp-nav"><button type="button" data-pp-home aria-label="Accueil">⌂</button><button type="button" data-window-close="phone" aria-label="Fermer">—</button></nav>
      </div>
    </div>`;
  }

  const icon = name => `<span class="pp-app-icon" data-icon="${name}"></span>`;
  function homeMarkup(p) {
    if(!p?.available) return `<div class="pp-no-device"><div class="pp-logo">P</div><strong>ParadisePhone</strong><p>${p?.hasDevice ? 'Initialisation du téléphone...' : 'Vous ne possédez pas de téléphone.'}</p><small>Obtenez un téléphone physique puis utilisez-le depuis votre inventaire.</small></div>`;
    return `<section class="pp-home"><div class="pp-hero"><small>PLACID ISLAND</small><strong>${esc(p.number||'---')}</strong><span>ParadisePhone</span></div><div class="pp-app-grid">
      ${appButton('messages','Messages',p.unreadCount)}${appButton('contacts','Contacts')}${appButton('calls','Téléphone')}${appButton('notifications','Notifications',unreadNotifications(p))}${appButton('settings','Paramètres')}
    </div></section>`;
  }
  function appButton(key,label,badge=0){return `<button type="button" class="pp-app" data-pp-app="${key}">${icon(key)}<span>${esc(label)}</span>${badge>0?`<b>${badge}</b>`:''}</button>`;}
  function unreadNotifications(p){return (p?.notifications||[]).filter(n=>!n.read).length;}

  function conversationsMarkup(p){const rows=p?.conversations||[];return appLayout('Messages',`<div class="pp-list">${rows.length?rows.map(c=>`<button type="button" class="pp-row" data-pp-conversation="${c.phone_id}"><span class="pp-avatar">${avatar(c.look,c.name)}</span><span class="pp-row-main"><strong>${esc(c.name||c.number)}</strong><small>${esc(c.last_message||'')}</small></span><span class="pp-row-meta"><time>${nowTime(c.last_at)}</time>${c.unread>0?`<b>${c.unread}</b>`:''}</span></button>`).join(''):'<div class="pp-empty"><strong>Aucune conversation</strong><span>Ajoutez un contact ou envoyez un SMS par numéro.</span></div>'}</div>`);}
  function contactsMarkup(p){const rows=p?.contacts||[];return appLayout('Contacts',`<button class="pp-primary pp-add" type="button" data-pp-add-contact>+ Ajouter</button><div class="pp-list">${rows.length?rows.map(c=>`<div class="pp-row"><span class="pp-avatar">${avatar(c.look,c.name)}</span><span class="pp-row-main"><strong>${esc(c.name)}</strong><small>${esc(c.number)}${c.online?' · En ligne':''}</small></span><span class="pp-row-actions"><button type="button" data-pp-message-number="${esc(c.number)}">Message</button><button type="button" data-pp-call-number="${esc(c.number)}">Appeler</button><button type="button" data-pp-delete-contact="${c.id}" aria-label="Supprimer">×</button></span></div>`).join(''):'<div class="pp-empty"><strong>Aucun contact</strong><span>Ajoutez un numéro que l’on vous a communiqué.</span></div>'}</div><div class="pp-inline-form" data-pp-contact-form hidden><input data-pp-contact-name maxlength="64" placeholder="Nom"><input data-pp-contact-number maxlength="8" placeholder="555-0184"><div><button data-pp-contact-cancel>Annuler</button><button class="pp-primary" data-pp-contact-save>Ajouter</button></div></div>`);}
  function callsMarkup(p){const call=p?.activeCall;if(call)return callMarkup(call);return appLayout('Téléphone',`<div class="pp-dial"><input data-pp-dial-target maxlength="64" placeholder="Contact ou 555-0184"><button class="pp-call-button" type="button" data-pp-start-call>Appeler</button></div><div class="pp-empty"><strong>Appels Paradise</strong><span>Les appels V1 sont des canaux privés RP, sans VoIP.</span></div>`);}
  function callMarkup(call){const incoming=call.direction==='incoming',ringing=call.status==='RINGING',connected=call.status==='CONNECTED';return `<section class="pp-call"><button class="pp-back" data-pp-app="calls">‹</button><div class="pp-call-avatar">${avatar(call.other_look,call.other_name)}</div><h2>${esc(call.other_name||call.other_number||'Correspondant')}</h2><small>${esc(call.other_number||'')}</small><p>${ringing?(incoming?'Appel entrant...':'Appel en cours...'):'Appel connecté'}</p>${connected?`<strong class="pp-call-timer" data-pp-call-timer data-start="${esc(call.answered_at||call.started_at)}">00:00</strong>`:''}<div class="pp-call-actions">${ringing&&incoming?'<button class="is-decline" data-pp-call-action="decline">Refuser</button><button class="is-answer" data-pp-call-action="answer">Décrocher</button>':`<button class="is-decline" data-pp-call-action="hangup">${ringing?'Annuler':'Raccrocher'}</button>`}</div></section>`;}
  function notificationsMarkup(p){return appLayout('Notifications',`<div class="pp-list">${(p?.notifications||[]).map(n=>`<div class="pp-notification ${n.read?'':'is-unread'}"><span>${icon(n.type||'notification')}</span><div><strong>${esc(n.title)}</strong><small>${esc(n.body)}</small><time>${nowTime(n.created_at)}</time></div></div>`).join('')||'<div class="pp-empty"><strong>Aucune notification</strong></div>'}</div>`);}
  function settingsMarkup(p){const s=p?.settings||{};return appLayout('Paramètres',`<div class="pp-settings"><label><span>Mode silencieux<small>Coupe les sons UI du téléphone.</small></span><input type="checkbox" data-pp-setting="silent" ${s.silent?'checked':''}></label><label><span>Notifications<small>Conserve les alertes importantes.</small></span><input type="checkbox" data-pp-setting="notifications" ${s.notifications!==false?'checked':''}></label><label><span>Sons du téléphone<small>Sons courts et discrets.</small></span><input type="checkbox" data-pp-setting="sounds" ${s.sounds!==false?'checked':''}></label></div>`);}
  function appLayout(title,content){return `<section class="pp-app-page"><header><button type="button" class="pp-back" data-pp-home>‹</button><strong>${esc(title)}</strong><span></span></header>${content}</section>`;}

  function conversationMarkup(p){const c=(p?.conversations||[]).find(x=>Number(x.phone_id)===Number(conversation));if(!c)return conversationsMarkup(p);return `<section class="pp-chat"><header><button class="pp-back" data-pp-app="messages">‹</button><span class="pp-avatar">${avatar(c.look,c.name)}</span><div><strong>${esc(c.name||c.number)}</strong><small>${esc(c.number)}</small></div><button data-pp-call-number="${esc(c.number)}">Appeler</button></header><div class="pp-messages" data-pp-messages>${conversationMessages.map(m=>`<div class="pp-bubble ${m.mine?'is-mine':'is-theirs'}"><span>${esc(m.body)}</span><time>${nowTime(m.sent_at)}</time></div>`).join('')}</div><form class="pp-compose" data-pp-compose><textarea maxlength="500" rows="1" placeholder="Écrire un message..." data-pp-message-input></textarea><button type="submit">Envoyer</button></form></section>`;}

  function render(){if(!body)return;const p=phoneState();const content=body.querySelector('[data-pp-content]');if(!content)return;if(!p?.available){content.innerHTML=homeMarkup(p);return;}if(p.activeCall){content.innerHTML=callMarkup(p.activeCall);tickCallTimer();return;}if(app==='messages')content.innerHTML=conversation?conversationMarkup(p):conversationsMarkup(p);else if(app==='contacts')content.innerHTML=contactsMarkup(p);else if(app==='calls')content.innerHTML=callsMarkup(p);else if(app==='notifications')content.innerHTML=notificationsMarkup(p);else if(app==='settings')content.innerHTML=settingsMarkup(p);else content.innerHTML=homeMarkup(p);}

  function openPhone(nextApp=null){const p=phoneState();if(p&&!p.available&&!p.hasDevice){systemFeedback('Vous ne possédez pas de téléphone.','ERROR');window.ParadiseWindowManager?.closeWindow?.('phone');return false;}if(nextApp&&APPS.has(nextApp))app=nextApp;window.ParadiseWindowManager?.openWindow?.('phone');refresh();render();return true;}
  function goHome(){app='home';conversation=null;conversationMessages=[];render();}
  async function openConversation(id){conversation=Number(id);app='messages';try{const p=await action({action:'conversation',other_phone_id:conversation});conversationMessages=p.messages||[];await action({action:'read_conversation',other_phone_id:conversation});}catch(e){toast('Messages',e.message);}render();setTimeout(()=>body?.querySelector('[data-pp-messages]')?.scrollTo({top:999999}),0);}

  function toast(title,message){if(!hud)return;let host=hud.querySelector('.pp-toast-host');if(!host){host=document.createElement('div');host.className='pp-toast-host';hud.appendChild(host);}const n=document.createElement('div');n.className='pp-toast';n.innerHTML=`<strong>${esc(title)}</strong><span>${esc(message)}</span>`;host.appendChild(n);setTimeout(()=>n.remove(),3200);}
  function systemFeedback(message,type='PHONE'){toast(type==='ERROR'?'Erreur':'Téléphone',message);emit('phone:system-message',{type,message});}

  async function onClick(e){const t=e.target.closest('button,[data-pp-conversation]');if(!t||!body?.contains(t))return;
    if(t.matches('[data-pp-home]')){goHome();return;}if(t.dataset.ppApp){app=t.dataset.ppApp;conversation=null;render();return;}if(t.dataset.ppConversation){openConversation(t.dataset.ppConversation);return;}
    if(t.hasAttribute('data-pp-add-contact')){body.querySelector('[data-pp-contact-form]').hidden=false;return;}if(t.hasAttribute('data-pp-contact-cancel')){body.querySelector('[data-pp-contact-form]').hidden=true;return;}
    if(t.hasAttribute('data-pp-contact-save')){const name=text(body.querySelector('[data-pp-contact-name]')?.value),number=text(body.querySelector('[data-pp-contact-number]')?.value);try{await action({action:'add_contact',name,number});render();}catch(err){systemFeedback(err.message,'ERROR');}return;}
    if(t.dataset.ppDeleteContact){if(!confirm('Supprimer ce contact ?'))return;try{await action({action:'delete_contact',contact_id:Number(t.dataset.ppDeleteContact)});render();}catch(err){systemFeedback(err.message,'ERROR');}return;}
    if(t.dataset.ppMessageNumber){app='messages';const c=(phoneState()?.conversations||[]).find(x=>x.number===t.dataset.ppMessageNumber);if(c)openConversation(c.phone_id);else{conversation=null;render();toast('Messages','Utilisez :sms ou démarrez une conversation après un premier message.');}return;}
    if(t.dataset.ppCallNumber){try{await action({action:'call',target:t.dataset.ppCallNumber});app='calls';render();}catch(err){systemFeedback(err.message,'ERROR');}return;}
    if(t.hasAttribute('data-pp-start-call')){const target=text(body.querySelector('[data-pp-dial-target]')?.value);try{await action({action:'call',target});render();}catch(err){systemFeedback(err.message,'ERROR');}return;}
    if(t.dataset.ppCallAction){try{await action({action:t.dataset.ppCallAction});render();}catch(err){systemFeedback(err.message,'ERROR');}return;}
  }

  async function onSubmit(e){const form=e.target.closest('[data-pp-compose]');if(!form||!body?.contains(form))return;e.preventDefault();const input=form.querySelector('[data-pp-message-input]'),bodyText=text(input?.value);const c=(phoneState()?.conversations||[]).find(x=>Number(x.phone_id)===Number(conversation));if(!c||!bodyText)return;try{await action({action:'send_message',target:c.number,body:bodyText});input.value='';await openConversation(conversation);}catch(err){systemFeedback(err.message,'ERROR');}}

  async function onChange(e){const input=e.target.closest('[data-pp-setting]');if(!input||!body?.contains(input))return;const s=phoneState()?.settings||{};const key=input.dataset.ppSetting;const next={silent:Boolean(s.silent),notifications:s.notifications!==false,sounds:s.sounds!==false};next[key]=input.checked;try{await action({action:'settings',...next});}catch(err){systemFeedback(err.message,'ERROR');}}

  function tickCallTimer(){const node=body?.querySelector('[data-pp-call-timer]');if(!node)return;const start=new Date(node.dataset.start||Date.now()).getTime();const sec=Math.max(0,Math.floor((Date.now()-start)/1000));node.textContent=`${String(Math.floor(sec/60)).padStart(2,'0')}:${String(sec%60).padStart(2,'0')}`;setTimeout(()=>{if(body?.contains(node))tickCallTimer();},1000);}

  function rewritePhoneCommand(e){if((e.key!=='Enter'&&e.keyCode!==13)||!(e.target instanceof HTMLInputElement)||e.target.id!=='pr4-chat-input')return;const raw=text(e.target.value);if(!raw.startsWith(':'))return;const [cmd,...parts]=raw.split(/\s+/);const c=cmd.toLowerCase();const handled=[':phone',':number',':numero',':numéro',':contacts',':addcontact',':sms',':call',':answer',':decline',':hangup'];if(!handled.includes(c))return;
    e.preventDefault();e.stopImmediatePropagation();e.target.value='';
    (async()=>{try{if(c===':phone'){const active=window.ParadiseWindowManager?.getActiveWindow?.();active==='phone'?window.ParadiseWindowManager?.closeWindow?.('phone'):openPhone();return;}if(c===':number'||c===':numero'||c===':numéro'){await refresh();const n=phoneState()?.number;if(!n)throw new Error('Vous ne possédez pas de téléphone.');systemFeedback(`Votre numéro est le ${n}.`);return;}if(c===':contacts'){openPhone('contacts');return;}if(c===':addcontact'){if(parts.length<2)throw new Error('Syntaxe : :addcontact <numéro> <nom>');await action({action:'add_contact',number:parts[0],name:parts.slice(1).join(' ')});return;}if(c===':sms'){if(parts.length<2)throw new Error('Syntaxe : :sms <contact/numéro> <message>');await action({action:'send_message',target:parts[0],body:parts.slice(1).join(' ')});return;}if(c===':call'){if(!parts[0])throw new Error('Syntaxe : :call <contact/numéro>');await action({action:'call',target:parts.join(' ')});openPhone('calls');return;}if(c===':answer'||c===':decline'||c===':hangup'){await action({action:c.slice(1)});openPhone('calls');return;}}catch(err){systemFeedback(err.message||'Action impossible.','ERROR');}})();
  }

  function protectPhoneInput(e){if(!e.target.closest?.('.pp-device'))return;e.stopPropagation();if(e.key==='Escape'&&!(e.target instanceof HTMLInputElement)&&!(e.target instanceof HTMLTextAreaElement)){e.preventDefault();window.ParadiseWindowManager?.closeWindow?.('phone');}}

  function mount(){hud=document.getElementById(HUD_ID);win=hud?.querySelector('.pr-window[data-window="phone"]');body=win?.querySelector('.pr-window-body');if(!hud||!win||!body)return false;win.classList.add('pp-window');body.innerHTML=shell();decorateHudButton();body.addEventListener('click',onClick);body.addEventListener('submit',onSubmit);body.addEventListener('change',onChange);hud.addEventListener('keydown',rewritePhoneCommand,true);document.addEventListener('keydown',protectPhoneInput,true);const button=hud.querySelector('[data-window-open="phone"]');button?.addEventListener('click',()=>setTimeout(()=>openPhone(),0),true);return true;}

  function boot(){ensureStoreShape();if(!mount())return;refresh();timer=setInterval(refresh,POLL_MS);window.ParadisePhoneV1=Object.freeze({version:VERSION,open:openPhone,refresh,getState:phoneState,send:(target,body)=>action({action:'send_message',target,body}),call:target=>action({action:'call',target})});console.info('[ParadiseRP] ParadisePhone V1 active',{version:VERSION});}
  function destroy(){destroyed=true;clearInterval(timer);body?.removeEventListener('click',onClick);body?.removeEventListener('submit',onSubmit);body?.removeEventListener('change',onChange);hud?.removeEventListener('keydown',rewritePhoneCommand,true);document.removeEventListener('keydown',protectPhoneInput,true);}
  window.addEventListener('beforeunload',destroy,{once:true});if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',boot,{once:true});else boot();
})();
