(function(){
  'use strict';
  function mount(){
    var phone=document.getElementById('phone_content');
    var stack=document.getElementById('RdpPhoneToastStack');
    if(phone&&stack&&stack.parentNode!==phone){
      phone.appendChild(stack);
    }
  }
  var obs=new MutationObserver(function(){ mount(); });
  if(document.readyState==='loading'){
    document.addEventListener('DOMContentLoaded',function(){
      mount();
      obs.observe(document.body,{childList:true,subtree:true});
    });
  }else{
    mount();
    if(document.body) obs.observe(document.body,{childList:true,subtree:true});
  }
})();
