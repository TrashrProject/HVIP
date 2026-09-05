<?php
$rooms=$DB->Query("SELECT id,caption,name,roomtype FROM rooms WHERE roomtype='public' OR id IN (174,175) ORDER BY id ASC");
$roomCount=mysqli_num_rows($rooms);
?>
<div class="content"><div class="container">
    <div class="citymap-page-heading">
        <div><small>CARTE DE PARADISE ROLEPLAY</small><h1>Lieux de la ville</h1><p>Cette page représente la <strong>map officielle de la ville</strong>. 🗺️<br>Tu peux y découvrir les différents lieux, quartiers et points importants de la ville afin de mieux te repérer et profiter pleinement de ton expérience Roleplay.</p></div>
        <span class="citymap-heading-icon"><i class="fas fa-map-marked-alt"></i></span>
    </div>
    <div class="citymap-toolbar">
        <label class="citymap-search"><i class="fas fa-search"></i><input id="room-search" type="search" placeholder="Rechercher un lieu par son nom ou son ID..."></label>
        <span id="room-result-count"><?php echo $roomCount; ?> lieu(x) RP</span>
    </div>
    <div class="citymap-list" id="citymap-list">
    <?php while($room=mysqli_fetch_assoc($rooms)):
        $name=trim($room['caption'])!==''?$room['caption']:(trim($room['name'])!==''?$room['name']:'Salle sans nom');
        $searchText=strtolower($room['id'].' '.$name);
    ?>
        <article class="citymap-room citymap-room-simple" data-search="<?php echo htmlspecialchars($searchText,ENT_QUOTES,'UTF-8'); ?>">
            <button class="citymap-room-id" type="button" data-copy-room="<?php echo (int)$room['id']; ?>" title="Copier la commande :taxi <?php echo (int)$room['id']; ?>"><small>ID</small><strong><?php echo (int)$room['id']; ?></strong><i class="far fa-copy"></i></button>
            <div class="citymap-room-main"><div class="citymap-room-title"><h2><?php echo htmlspecialchars($name,ENT_QUOTES,'UTF-8'); ?></h2></div><span class="citymap-copy-hint"><i class="fas fa-taxi"></i> Cliquer sur l’ID pour copier la commande</span></div>
        </article>
    <?php endwhile; ?>
    </div>
    <div class="citymap-empty" id="citymap-empty"><i class="fas fa-map-signs"></i><b>Aucune salle trouvée</b><span>Modifie la recherche ou le filtre.</span></div>
    <div class="citymap-copy-toast" id="citymap-copy-toast">ID copié</div>
</div></div>
<script>
(function(){
var cards=Array.prototype.slice.call(document.querySelectorAll('.citymap-room')),search=document.getElementById('room-search'),count=document.getElementById('room-result-count'),empty=document.getElementById('citymap-empty'),toast=document.getElementById('citymap-copy-toast');
function refresh(){var q=search.value.toLowerCase().trim(),visible=0;cards.forEach(function(card){var show=!q||card.dataset.search.indexOf(q)!==-1;card.style.display=show?'':'none';if(show)visible++;});count.textContent=visible+' lieu(x) RP';empty.style.display=visible?'none':'flex';}
search.addEventListener('input',refresh);
document.querySelectorAll('[data-copy-room]').forEach(function(button){button.addEventListener('click',function(){var id=this.dataset.copyRoom,command=':taxi '+id;if(navigator.clipboard){navigator.clipboard.writeText(command);}else{var input=document.createElement('input');input.value=command;document.body.appendChild(input);input.select();document.execCommand('copy');input.remove();}toast.textContent=command+' copié';toast.classList.add('show');setTimeout(function(){toast.classList.remove('show');},1400);});});
})();
</script>
