(function(){
'use strict';
var callButton=document.getElementById('call-911');
var modal=document.getElementById('health-911-modal');
var closeButton=document.getElementById('health-modal-close');
var cancelButton=document.getElementById('health-modal-cancel');
var confirmButton=document.getElementById('health-modal-confirm');
var toast=document.getElementById('health-toast');
var busy=false;

function setModal(open){
    if(!modal)return;
    modal.classList.toggle('show',!!open);
    modal.setAttribute('aria-hidden',open?'false':'true');
}

function showToast(message,error){
    if(!toast)return;
    toast.textContent=message;
    toast.classList.toggle('error',!!error);
    toast.classList.add('show');
    window.clearTimeout(showToast.timer);
    showToast.timer=window.setTimeout(function(){toast.classList.remove('show');},4200);
}

if(callButton)callButton.addEventListener('click',function(){setModal(true);});
if(closeButton)closeButton.addEventListener('click',function(){setModal(false);});
if(cancelButton)cancelButton.addEventListener('click',function(){setModal(false);});
if(modal)modal.addEventListener('click',function(e){if(e.target===modal)setModal(false);});

document.addEventListener('keydown',function(e){if(e.key==='Escape')setModal(false);});

if(confirmButton){
    confirmButton.addEventListener('click',function(){
        if(busy)return;
        busy=true;
        confirmButton.disabled=true;
        var old=confirmButton.innerHTML;
        confirmButton.innerHTML='<i class="fas fa-circle-notch fa-spin"></i> Envoi en cours';

        var body=new URLSearchParams();
        body.set('action','call911');
        fetch(window.VELORA_HEALTH_ENDPOINT||'health',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded; charset=UTF-8'},body:body.toString(),credentials:'same-origin'})
            .then(function(response){return response.json().then(function(data){return {ok:response.ok,data:data};});})
            .then(function(result){
                if(!result.ok||!result.data.ok)throw new Error(result.data&&result.data.message?result.data.message:'Erreur 911');
                setModal(false);
                showToast(result.data.message||'Appel 911 envoyé.',false);
                if(callButton){
                    callButton.classList.add('sent');
                    callButton.innerHTML='<i class="fas fa-check"></i><span><small>DEMANDE TRANSMISE</small><strong>911 PRÉVENU</strong></span>';
                }
            })
            .catch(function(error){showToast(error.message||'Impossible d’envoyer la demande.',true);})
            .finally(function(){busy=false;confirmButton.disabled=false;confirmButton.innerHTML=old;});
    });
}
})();
