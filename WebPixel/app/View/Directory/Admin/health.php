<?php
$dbOk=false;$dbLatency=null;$start=microtime(true);try{$r=$DB->PreparedRow('SELECT 1 ok');$dbOk=$r&&(int)$r['ok']===1;$dbLatency=round((microtime(true)-$start)*1000,1);}catch(Throwable $e){$dbOk=false;}
$status=null;if($PCC->tableExists('server_status')){try{$status=$DB->PreparedRow('SELECT * FROM server_status LIMIT 1');}catch(Throwable $e){$status=null;}}
$usersDb=null;try{$x=$DB->PreparedRow("SELECT COUNT(*) c FROM users WHERE online='1'");$usersDb=$x?(int)$x['c']:null;}catch(Throwable $e){}
$signals=array(
 array('CMS PHP','Disponible','PHP '.PHP_VERSION,true,'Le Control Center répond dans ce processus PHP.'),
 array('MariaDB',$dbOk?'Disponible':'Erreur',$dbLatency!==null?$dbLatency.' ms':'—',$dbOk,'SELECT 1 sur la connexion CMS.'),
 array('ÉMU / server_status',$status?(((int)($status['status']??0)===1)?'Signal actif':'Signal inactif'):'Non exposé',$status?'server_status':'—',$status&&((int)($status['status']??0)===1),'C’est un signal DB existant, pas un faux ping réseau.'),
 array('Proxy','Non sondé','—',null,'Aucun endpoint de santé proxy sécurisé n’a été identifié dans le CMS.'),
 array('WebSocket / MUS','Non sondé','—',null,'Le WebSocket client existe, mais ce n’est pas un endpoint d’administration authentifié.')
);
?>
<section class="pcc-alert info"><i class="fas fa-stethoscope"></i><div><strong>Pas de faux “Online”</strong><p>Cette page distingue les signaux observables d’un vrai health check. <code>server_status</code> est affiché comme signal DB uniquement ; Proxy et MUS restent “non sondés” tant qu’un endpoint sûr n’existe pas.</p></div></section>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Santé des composants</h2><p>Contrôles exécutés à l’ouverture de la page.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Composant</th><th>État</th><th>Mesure</th><th>Source / interprétation</th></tr></thead><tbody><?php foreach($signals as $s):?><tr><td><strong><?php echo $h($s[0]);?></strong></td><td><span class="pcc-badge <?php echo $s[3]===true?'success':($s[3]===false?'danger':'warning');?>"><?php echo $h($s[1]);?></span></td><td><code><?php echo $h($s[2]);?></code></td><td><?php echo $h($s[4]);?></td></tr><?php endforeach;?></tbody></table></div></section>
<section class="pcc-three-col">
<article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Joueurs online</h2><p>Source users.online.</p></div></div><div class="pcc-empty compact"><strong><?php echo $usersDb===null?'—':$number($usersDb);?></strong><span>Compte SQL actuel.</span></div></article>
<article class="pcc-panel"><div class="pcc-panel-head"><div><h2>server_status</h2><p>Valeurs connues si présentes.</p></div></div><dl class="pcc-detail-list"><div><dt>Status</dt><dd><?php echo $status&&isset($status['status'])?$h($status['status']):'—';?></dd></div><div><dt>Users online</dt><dd><?php echo $status&&isset($status['users_online'])?$number($status['users_online']):'—';?></dd></div><div><dt>Rooms chargées</dt><dd><?php echo $status&&isset($status['loaded_rooms'])?$number($status['loaded_rooms']):'—';?></dd></div></dl></article>
<article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Audit admin</h2><p>Précondition des écritures.</p></div></div><div class="pcc-empty compact"><span class="pcc-badge <?php echo $PCC->auditReady()?'success':'danger';?>"><?php echo $PCC->auditReady()?'PRÊT':'MIGRATION REQUISE';?></span><span><?php echo $PCC->auditReady()?'Les actions sensibles peuvent être tracées.':'Les mutations sensibles restent volontairement bloquées.';?></span></div></article>
</section>
