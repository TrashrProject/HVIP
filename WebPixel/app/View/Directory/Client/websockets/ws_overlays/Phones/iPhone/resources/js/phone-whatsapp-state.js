(function(){
  'use strict';
  if(window.__rdpWhatsStateV1) return;
  window.__rdpWhatsStateV1 = true;

  var KEY = 'rdp.whatsapp.lastChat';

  function getSocket(){
    if(window.__rdpPhoneSocket && window.__rdpPhoneSocket.readyState === 1) return window.__rdpPhoneSocket;
    try{ if(window.rdp_app && rdp_app.webSocket && rdp_app.webSocket.readyState === 1) return rdp_app.webSocket; }catch(e){}
    return null;
  }

  function getUserId(){
    if(window.__rdpPhoneUserId) return window.__rdpPhoneUserId;
    try{ if(window.rdp_app && rdp_app.UserID) return rdp_app.UserID; }catch(e){}
    return null;
  }

  function sendPhone(extra){
    var socket = getSocket(), uid = getUserId();
    if(!socket || !uid) return false;
    try{
      socket.send(JSON.stringify({UserId:parseInt(uid,10)||uid,EventName:'event_phone',Bypass:false,ExtraData:extra,JSON:false}));
      return true;
    }catch(e){ return false; }
  }

  function saveChat(name){
    name = String(name || '').trim();
    if(!name || name === 'Contact') return;
    try{ localStorage.setItem(KEY,name); }catch(e){}
  }

  function clearChat(){
    try{ localStorage.removeItem(KEY); }catch(e){}
  }

  function currentSaved(){
    try{ return String(localStorage.getItem(KEY) || '').trim(); }catch(e){ return ''; }
  }

  function showChatShell(){
    var title=document.querySelector('#app_WhatsApp .Whats_Title');
    var menu=document.getElementById('What_Menu');
    var chats=document.getElementById('WS_WhatsApp');
    var contacts=document.getElementById('WS_WhatsApp_Contacts');
    var chat=document.getElementById('WS_WhatsApp_Chatting');
    if(title) title.style.display='none';
    if(menu) menu.style.display='none';
    if(chats) chats.style.display='none';
    if(contacts) contacts.style.display='none';
    if(chat) chat.style.display='';
  }

  function restoreChat(){
    var name = currentSaved();
    if(!name) return false;
    if(!sendPhone('open_whatsapp,')) return false;
    setTimeout(function(){
      if(sendPhone('open_whatschats,' + name)) showChatShell();
    },120);
    return true;
  }

  document.addEventListener('click',function(e){
    var row=e.target.closest && e.target.closest('#WS_WhatsApp .app_msg_content[data-whatsname],#WS_WhatsApp_Contacts .app_msg_content[data-whatsname]');
    if(row) saveChat(row.getAttribute('data-whatsname'));

    var back=e.target.closest && e.target.closest('#app_WhatsApp .whats-back');
    if(back) clearChat();
  },true);

  function boot(){
    var tries=0;
    var timer=setInterval(function(){
      tries++;
      if(restoreChat() || tries>40) clearInterval(timer);
    },250);
  }

  if(document.readyState==='loading') document.addEventListener('DOMContentLoaded',boot);
  else boot();
})();
