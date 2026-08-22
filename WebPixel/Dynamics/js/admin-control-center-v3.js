(function(){
'use strict';
var app=document.querySelector('[data-pcc-app]');
var sidebarKey='paradise-control-center-v3.sidebar';
if(app&&localStorage.getItem(sidebarKey)==='collapsed') app.classList.add('is-sidebar-collapsed');

document.querySelectorAll('[data-sidebar-toggle]').forEach(function(button){
  button.addEventListener('click',function(){
    if(!app)return;
    app.classList.toggle('is-sidebar-collapsed');
    localStorage.setItem(sidebarKey,app.classList.contains('is-sidebar-collapsed')?'collapsed':'open');
  });
});

document.querySelectorAll('[data-refresh]').forEach(function(button){
  button.addEventListener('click',function(){location.reload();});
});

function esc(value){return String(value==null?'':value).replace(/[&<>"']/g,function(char){return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char];});}
function debounce(fn,wait){var timer;return function(){var args=arguments,ctx=this;clearTimeout(timer);timer=setTimeout(function(){fn.apply(ctx,args);},wait);};}

var toast=document.querySelector('[data-toast]');
if(toast){
  var removeToast=function(){if(toast&&toast.parentNode)toast.remove();};
  var toastClose=toast.querySelector('[data-toast-close]');
  if(toastClose)toastClose.addEventListener('click',removeToast);
  setTimeout(removeToast,6500);
}

function fetchSearch(q){
  if(!window.PCC||!window.PCC.searchUrl)return Promise.resolve({players:[],businesses:[]});
  return fetch(window.PCC.searchUrl+encodeURIComponent(q),{credentials:'same-origin',headers:{Accept:'application/json'}})
    .then(function(response){if(!response.ok)throw new Error('search');return response.json();})
    .then(function(data){return data.groups||{players:[],businesses:[]};})
    .catch(function(){return {players:[],businesses:[]};});
}

var globalSearch=document.querySelector('[data-global-search]');
if(globalSearch){
  var searchInput=globalSearch.querySelector('[data-global-search-input]');
  var searchResults=globalSearch.querySelector('[data-global-search-results]');
  var runSearch=debounce(function(){
    var q=searchInput.value.trim();
    if(q.length<2){searchResults.hidden=true;searchResults.innerHTML='';return;}
    searchResults.hidden=false;
    searchResults.innerHTML='<div class="pcc-search-empty">Recherche…</div>';
    fetchSearch(q).then(function(groups){
      var html='';
      var players=groups.players||[];
      var businesses=groups.businesses||[];
      if(players.length){
        html+='<div class="pcc-search-group-title">Joueurs</div>';
        html+=players.map(function(player){
          var detail=[];
          if(player.rp_name)detail.push(player.rp_name);
          if(player.citizen_id)detail.push(player.citizen_id);
          if(player.phone_number)detail.push(player.phone_number);
          if(!detail.length)detail.push('#'+Number(player.id)+' · '+esc(player.role));
          return '<a href="'+esc(player.url)+'"><img src="'+esc(player.avatar)+'" alt=""><span><strong>'+esc(player.username)+'</strong><small>'+detail.map(esc).join(' · ')+'</small></span><em>'+(player.online?'en ligne':'')+'</em></a>';
        }).join('');
      }
      if(businesses.length){
        html+='<div class="pcc-search-group-title">Entreprises</div>';
        html+=businesses.map(function(business){
          return '<a href="'+esc(business.url)+'"><span><strong>'+esc(business.name)+'</strong><small>Entreprise #'+Number(business.id)+'</small></span></a>';
        }).join('');
      }
      if(!html)html='<div class="pcc-search-empty">Aucun résultat</div>';
      searchResults.innerHTML=html;
    });
  },240);
  searchInput.addEventListener('input',runSearch);
  searchInput.addEventListener('focus',runSearch);
  document.addEventListener('click',function(event){if(!globalSearch.contains(event.target))searchResults.hidden=true;});
  document.addEventListener('keydown',function(event){
    if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();searchInput.focus();searchInput.select();}
    if(event.key==='Escape')searchResults.hidden=true;
  });
}

var modal=document.querySelector('[data-confirm-modal]');
var pendingForm=null;
if(modal){
  var modalTitle=modal.querySelector('[data-confirm-modal-title]');
  var modalMessage=modal.querySelector('[data-confirm-modal-message]');
  var modalAccept=modal.querySelector('[data-confirm-accept]');
  var modalCancel=modal.querySelector('[data-confirm-cancel]');

  document.querySelectorAll('[data-confirm-form]').forEach(function(form){
    form.addEventListener('submit',function(event){
      if(form.dataset.confirmed==='1')return;
      event.preventDefault();
      if(!form.reportValidity())return;
      pendingForm=form;
      if(modalTitle)modalTitle.textContent=form.getAttribute('data-confirm-title')||'Confirmer l’action';
      if(modalMessage)modalMessage.textContent=form.getAttribute('data-confirm-message')||'Cette action modifiera des données réelles et sera auditée.';
      if(modalAccept){
        modalAccept.classList.toggle('danger',form.hasAttribute('data-confirm-danger'));
        modalAccept.classList.toggle('primary',!form.hasAttribute('data-confirm-danger'));
      }
      modal.hidden=false;
    });
  });

  function hideModal(){modal.hidden=true;pendingForm=null;}
  if(modalCancel)modalCancel.addEventListener('click',hideModal);
  if(modalAccept)modalAccept.addEventListener('click',function(){
    if(!pendingForm)return;
    var form=pendingForm;
    form.dataset.confirmed='1';
    modal.hidden=true;
    form.querySelectorAll('button[type="submit"],button:not([type])').forEach(function(button){button.disabled=true;});
    form.submit();
    pendingForm=null;
  });
  modal.addEventListener('click',function(event){if(event.target===modal)hideModal();});
  document.addEventListener('keydown',function(event){if(event.key==='Escape'&&!modal.hidden)hideModal();});
}

// Any non-confirmed POST also receives a submit guard.
document.querySelectorAll('form[method="post"]:not([data-confirm-form])').forEach(function(form){
  form.addEventListener('submit',function(){
    if(form.dataset.submitting==='1')return false;
    form.dataset.submitting='1';
    setTimeout(function(){form.querySelectorAll('button[type="submit"],button:not([type])').forEach(function(button){button.disabled=true;});},0);
  });
});

// Habbo figure preview on the player sheet. No server mutation occurs here.
document.querySelectorAll('[data-look-input]').forEach(function(input){
  var form=input.closest('form');
  if(!form)return;
  var preview=form.querySelector('[data-look-preview]');
  if(!preview)return;
  var update=debounce(function(){
    var figure=input.value.trim();
    if(!/^[A-Za-z0-9.\-]+$/.test(figure))return;
    preview.src='https://www.habbo.es/habbo-imaging/avatarimage?figure='+encodeURIComponent(figure)+'&size=l&direction=2&head_direction=3&gesture=sml';
  },180);
  input.addEventListener('input',update);
});
})();
