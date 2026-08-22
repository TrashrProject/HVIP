<?php
$userId = max(0, (int)($_GET['id'] ?? 0));
$user = $PCC->requireUser($userId);
$tab = strtolower((string)($_GET['tab'] ?? 'overview'));
$tabs = array('overview','character','economy','inventory','documents','phone','sanctions','logs');
if (!in_array($tab, $tabs, true)) $tab = 'overview';
$con = $DB->Con();

$bank = 0;
$playStats = null;
if ($PCC->tableExists('play_stats')) {
    try { $playStats = $DB->PreparedRow('SELECT * FROM play_stats WHERE id=? LIMIT 1', 'i', array($userId)); }
    catch (Throwable $e) { $playStats = null; }
    if ($playStats && isset($playStats['bank'])) $bank = (int)$playStats['bank'];
}

$character = array('exists'=>false,'first_name'=>null,'last_name'=>null,'full_name'=>null,'birth_date'=>null,'gender'=>null,'nationality'=>null,'citizen_id'=>null,'biography'=>null,'reputation'=>0,'created_at'=>null,'updated_at'=>null);
if (function_exists('pr_character_snapshot')) {
    try { $character = pr_character_snapshot($con, $userId); } catch(Throwable $e) {}
}
$documents = array();
if (function_exists('pr_character_documents')) {
    try { $documents = pr_character_documents($con, $userId); } catch(Throwable $e) { $documents = array(); }
}
$inventory = array('items'=>array(),'physical_items'=>0,'documents'=>0,'slots_used'=>0,'max_slots'=>0,'weight'=>0,'capacity'=>0);
if (function_exists('pr_inventory_snapshot')) {
    try { $inventory = pr_inventory_snapshot($con, $userId); } catch(Throwable $e) {}
}

$phone = null;
$phoneStats = array('contacts'=>0,'unread'=>0,'messages'=>0,'calls'=>0,'notifications'=>0,'last_activity'=>null);
if ($PCC->tableExists('rp_phones')) {
    try { $phone = $DB->PreparedRow('SELECT * FROM rp_phones WHERE user_id=? LIMIT 1', 'i', array($userId)); } catch(Throwable $e) { $phone=null; }
    if ($phone) {
        $phoneId = (int)$phone['id'];
        if ($PCC->tableExists('rp_phone_contacts')) {
            $r=$DB->PreparedRow('SELECT COUNT(*) c FROM rp_phone_contacts WHERE phone_id=?','i',array($phoneId)); $phoneStats['contacts']=$r?(int)$r['c']:0;
        }
        if ($PCC->tableExists('play_phone_chats')) {
            $hasReadAt = $PCC->columnExists('play_phone_chats','read_at');
            if ($hasReadAt) { $r=$DB->PreparedRow('SELECT COUNT(*) c FROM play_phone_chats WHERE type=1 AND receptor_id=? AND read_at IS NULL','i',array($userId)); $phoneStats['unread']=$r?(int)$r['c']:0; }
            $r=$DB->PreparedRow('SELECT COUNT(*) c,MAX(timestamp) last_activity FROM play_phone_chats WHERE type=1 AND (emisor_id=? OR receptor_id=?)','ii',array($userId,$userId));
            if($r){$phoneStats['messages']=(int)$r['c'];$phoneStats['last_activity']=$r['last_activity'];}
        }
        if ($PCC->tableExists('rp_phone_calls')) {
            $r=$DB->PreparedRow('SELECT COUNT(*) c,MAX(COALESCE(ended_at,started_at)) last_activity FROM rp_phone_calls WHERE caller_phone_id=? OR receiver_phone_id=?','ii',array($phoneId,$phoneId));
            if($r){$phoneStats['calls']=(int)$r['c']; if(!$phoneStats['last_activity'] && $r['last_activity'])$phoneStats['last_activity']=$r['last_activity'];}
        }
        if ($PCC->tableExists('rp_phone_notifications')) {
            $r=$DB->PreparedRow('SELECT COUNT(*) c FROM rp_phone_notifications WHERE phone_id=?','i',array($phoneId)); $phoneStats['notifications']=$r?(int)$r['c']:0;
        }
    }
}

$bans = array();
if ($PCC->tableExists('bans')) {
    try { $bans = $DB->PreparedAll("SELECT * FROM bans WHERE bantype='user' AND value=? ORDER BY expire DESC", 's', array((string)$user['username'])); } catch(Throwable $e) { $bans=array(); }
}
$activeBan = null;
foreach($bans as $ban) if((int)($ban['expire']??0)>=time()) { $activeBan=$ban; break; }

$badges = array();
if ($PCC->tableExists('user_badges')) {
    try { $badges = $DB->PreparedAll('SELECT badge_id,badge_slot FROM user_badges WHERE user_id=? ORDER BY badge_slot,badge_id','i',array($userId)); } catch(Throwable $e) {}
}
$audit = array();
if ($PCC->auditReady()) {
    try { $audit = $DB->PreparedAll("SELECT * FROM cms_admin_audit_log WHERE target_type='user' AND target_id=? ORDER BY id DESC LIMIT 40", 's', array((string)$userId)); } catch(Throwable $e) {}
}

$rpName = !empty($character['exists']) ? trim((string)$character['first_name'].' '.(string)$character['last_name']) : '';
$isOnline = (int)$user['online'] === 1;
$tabUrl = function($name) use ($userId) { return URL . '/admin.php?page=player&id=' . $userId . '&tab=' . rawurlencode($name); };
$csrf = $PCC->csrfToken();
$actionHidden = function($action,$returnTab='overview') use ($h,$PCC,$csrf,$userId) {
    return '<input type="hidden" name="csrf" value="'.$h($csrf).'"><input type="hidden" name="action_nonce" value="'.$h($PCC->issueNonce()).'"><input type="hidden" name="action" value="'.$h($action).'"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="'.$userId.'"><input type="hidden" name="return_tab" value="'.$h($returnTab).'"><input type="hidden" name="user_id" value="'.$userId.'">';
};
?>
<section class="pcc-player-hero pcc-panel">
    <div class="pcc-player-avatar"><img src="<?php echo $h($PCC->avatarUrl($user['look'],'l')); ?>" alt=""></div>
    <div class="pcc-player-heading">
        <div class="pcc-player-titleline"><h2><?php echo $h($user['username']); ?></h2><span class="pcc-badge <?php echo $isOnline?'success':''; ?>"><?php echo $isOnline?'EN LIGNE':'HORS LIGNE'; ?></span></div>
        <p><?php echo $rpName!==''?$h($rpName):'Aucun personnage RP'; ?> · <?php echo $h($PCC->roleName($user['rank'])); ?></p>
        <div class="pcc-player-identifiers"><code>#<?php echo (int)$user['id']; ?></code><?php if(!empty($character['citizen_id'])):?><code><?php echo $h($character['citizen_id']); ?></code><?php endif; ?><?php if($phone):?><code><?php echo $h($phone['phone_number']); ?></code><?php endif; ?></div>
    </div>
    <div class="pcc-player-actions">
        <?php if($activeBan): ?><span class="pcc-badge danger">SANCTION ACTIVE</span><?php endif; ?>
        <a class="pcc-button secondary" href="<?php echo URL; ?>/profile/<?php echo (int)$userId; ?>" target="_blank"><i class="fas fa-external-link-alt"></i> Profil CMS</a>
    </div>
</section>

<?php if($isOnline): ?>
<section class="pcc-alert warning"><i class="fas fa-plug"></i><div><strong>Joueur connecté : protections ÉMU actives</strong><p>Les modifications qui changent Character, économie, documents ou apparence sont verrouillées tant qu’un bridge admin → ÉMU transactionnel n’est pas disponible. Les consultations restent temps réel depuis la base.</p></div></section>
<?php endif; ?>

<nav class="pcc-subtabs">
<?php foreach(array('overview'=>'Aperçu','character'=>'Personnage','economy'=>'Économie','inventory'=>'Inventaire','documents'=>'Documents','phone'=>'Téléphone','sanctions'=>'Sanctions','logs'=>'Logs') as $key=>$label): ?><a class="<?php echo $tab===$key?'is-active':''; ?>" href="<?php echo $h($tabUrl($key)); ?>"><?php echo $h($label); ?></a><?php endforeach; ?>
</nav>

<?php if($tab==='overview'): ?>
<section class="pcc-kpi-grid pcc-kpi-grid-auto">
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-wallet"></i></div><div><span>Cash</span><strong><?php echo $number($user['credits']); ?></strong></div></article>
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-university"></i></div><div><span>Banque</span><strong><?php echo $number($bank); ?></strong></div></article>
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-box-open"></i></div><div><span>Objets physiques</span><strong><?php echo $number($inventory['physical_items']??0); ?></strong></div></article>
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-id-card"></i></div><div><span>Documents</span><strong><?php echo $number(count($documents)); ?></strong></div></article>
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-mobile-alt"></i></div><div><span>Téléphone</span><strong class="pcc-kpi-text"><?php echo $phone?$h($phone['phone_number']):'Aucun'; ?></strong></div></article>
    <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="fas fa-certificate"></i></div><div><span>Badges</span><strong><?php echo $number(count($badges)); ?></strong></div></article>
</section>
<section class="pcc-two-col">
    <article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Compte & identité</h2><p>Compte Habbo + Character V2.</p></div></div><dl class="pcc-detail-list">
        <div><dt>Username</dt><dd><?php echo $h($user['username']); ?></dd></div>
        <div><dt>Rank</dt><dd><?php echo $h($PCC->roleName($user['rank'])); ?> (<?php echo (int)$user['rank']; ?>)</dd></div>
        <div><dt>Création compte</dt><dd><?php echo $h($PCC->formatDate($user['account_created'])); ?></dd></div>
        <div><dt>Nom RP</dt><dd><?php echo $rpName!==''?$h($rpName):'—'; ?></dd></div>
        <div><dt>Citizen ID</dt><dd><code><?php echo !empty($character['citizen_id'])?$h($character['citizen_id']):'—'; ?></code></dd></div>
        <div><dt>Nationalité</dt><dd><?php echo !empty($character['nationality'])?$h($character['nationality']):'—'; ?></dd></div>
        <div><dt>Réputation</dt><dd><?php echo isset($character['reputation'])?(int)$character['reputation']:'—'; ?></dd></div>
    </dl></article>
    <article class="pcc-panel"><div class="pcc-panel-head"><div><h2>État des systèmes</h2><p>Données disponibles pour ce joueur.</p></div></div><div class="pcc-system-list">
        <?php foreach(array(
            array('Personnage RP',!empty($character['exists']),'Character V2'),array('Documents',count($documents)>0,'Documents V2'),array('Inventaire',($PCC->tableExists('rp_inventory_items')),'Inventory V2'),array('Téléphone',(bool)$phone,'ParadisePhone V1'),array('Audit',$PCC->auditReady(),'PCC V3')
        ) as $s): ?><div><span><strong><?php echo $h($s[0]); ?></strong><small><?php echo $h($s[2]); ?></small></span><em class="pcc-badge <?php echo $s[1]?'success':'warning'; ?>"><?php echo $s[1]?'DISPONIBLE':'ABSENT'; ?></em></div><?php endforeach; ?>
    </div></article>
</section>
<?php if($badges): ?><section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Badges actuels</h2><p>Source : user_badges.</p></div></div><div class="pcc-badge-list"><?php foreach($badges as $badge): ?><span class="pcc-badge"><?php echo $h($badge['badge_id']); ?> · slot <?php echo (int)$badge['badge_slot']; ?></span><?php endforeach; ?></div></section><?php endif; ?>

<?php elseif($tab==='character'): ?>
<section class="pcc-two-col">
<article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Identité RP</h2><p>Citizen ID est volontairement non éditable.</p></div></div>
<?php if(empty($character['exists'])): ?><div class="pcc-empty"><i class="fas fa-id-badge"></i><strong>Aucun personnage RP</strong><span>La création reste gérée par Character V2 côté client/serveur ; le Control Center ne crée pas une identité concurrente.</span></div>
<?php else: ?><dl class="pcc-detail-list"><div><dt>Citizen ID</dt><dd><code><?php echo $h($character['citizen_id']); ?></code></dd></div><div><dt>Prénom</dt><dd><?php echo $h($character['first_name']); ?></dd></div><div><dt>Nom</dt><dd><?php echo $h($character['last_name']); ?></dd></div><div><dt>Naissance</dt><dd><?php echo $h($character['birth_date']); ?></dd></div><div><dt>Genre RP</dt><dd><?php echo $h($character['gender']??'—'); ?></dd></div><div><dt>Nationalité</dt><dd><?php echo $h($character['nationality']); ?></dd></div><div><dt>Biographie</dt><dd><?php echo $h($character['biography']??'—'); ?></dd></div></dl><?php endif; ?></article>
<article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Modifier le personnage</h2><p>Validation Character V2 + audit.</p></div></div>
<?php if(empty($character['exists']) || !$PCC->can('character.edit')): ?><div class="pcc-empty compact"><strong>Modification indisponible</strong><span><?php echo empty($character['exists'])?'Aucun Character V2 existant.':'Permission character.edit requise.'; ?></span></div>
<?php else: ?><form method="post" class="pcc-form" data-confirm-form data-confirm-title="Modifier l’identité RP ?" data-confirm-message="Les valeurs avant/après seront enregistrées dans l’audit."><?php echo $actionHidden('character_update','character'); ?>
<div class="pcc-form-grid"><label><span>Prénom RP</span><input name="first_name" value="<?php echo $h($character['first_name']); ?>" required maxlength="32"></label><label><span>Nom RP</span><input name="last_name" value="<?php echo $h($character['last_name']); ?>" required maxlength="32"></label><label><span>Date naissance</span><input type="date" name="birth_date" value="<?php echo $h($character['birth_date']); ?>" required></label><label><span>Genre RP</span><input name="gender" value="<?php echo $h($character['gender']??''); ?>" maxlength="24"></label><label class="wide"><span>Nationalité</span><input name="nationality" value="<?php echo $h($character['nationality']); ?>" required maxlength="48"></label><label class="wide"><span>Biographie</span><textarea name="biography" rows="4" maxlength="400"><?php echo $h($character['biography']??''); ?></textarea></label><label class="wide"><span>Raison administrative</span><input name="reason" required minlength="3" maxlength="500" placeholder="Ex. correction demandée par le joueur"></label></div>
<button class="pcc-button primary" <?php echo $isOnline||!$PCC->auditReady()?'disabled':''; ?>>Enregistrer les modifications</button><?php if($isOnline): ?><p class="pcc-form-note warning">Déconnecte le joueur avant modification pour éviter de désynchroniser CharacterService.</p><?php endif; ?></form><?php endif; ?></article>
</section>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Apparence Habbo</h2><p>Preview avant / après, raison obligatoire.</p></div></div><div class="pcc-appearance-editor"><div class="pcc-look-preview"><span>Actuel</span><img src="<?php echo $h($PCC->avatarUrl($user['look'],'l')); ?>" alt=""></div><?php if($PCC->can('appearance.edit')):?><form method="post" class="pcc-form" data-confirm-form data-confirm-title="Modifier l’apparence ?" data-confirm-message="La figure Habbo actuelle et la nouvelle seront auditées."><?php echo $actionHidden('appearance_update','character'); ?><label><span>Figure Habbo</span><textarea name="look" rows="4" maxlength="700" data-look-input required><?php echo $h($user['look']); ?></textarea></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><div class="pcc-look-preview small"><span>Prévisualisation</span><img src="<?php echo $h($PCC->avatarUrl($user['look'],'l')); ?>" data-look-preview alt=""></div><button class="pcc-button primary" <?php echo $isOnline||!$PCC->auditReady()?'disabled':''; ?>>Appliquer</button></form><?php endif; ?></div></section>

<?php elseif($tab==='economy'): ?>
<section class="pcc-balance-grid"><article><span>Cash actuel</span><strong><?php echo $number($user['credits']); ?></strong><small>users.credits</small></article><article><span>Banque actuelle</span><strong><?php echo $number($bank); ?></strong><small>play_stats.bank</small></article><article><span>État joueur</span><strong><?php echo $isOnline?'Connecté':'Hors ligne'; ?></strong><small><?php echo $isOnline?'mutation verrouillée':'mutation sûre SQL + audit'; ?></small></article></section>
<?php if($PCC->can('economy.adjust')): ?><section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Ajustement économique</h2><p>Ajouter, retirer ou définir un solde avec transaction SQL et audit.</p></div></div><form method="post" class="pcc-form pcc-form-horizontal" data-confirm-form data-confirm-danger data-confirm-title="Confirmer l’ajustement économique ?" data-confirm-message="Cette action modifie un solde réel et sera auditée."><?php echo $actionHidden('economy_adjust','economy'); ?><label><span>Compte</span><select name="wallet"><option value="cash">Cash</option><option value="bank">Banque</option></select></label><label><span>Action</span><select name="operation"><option value="add">Ajouter</option><option value="remove">Retirer</option><option value="set">Définir</option></select></label><label><span>Montant</span><input name="amount" type="number" min="0" max="2000000000" required></label><label class="wide"><span>Raison obligatoire</span><input name="reason" minlength="3" maxlength="500" required placeholder="Ex. compensation bug inventaire"></label><button class="pcc-button primary" <?php echo $isOnline||!$PCC->auditReady()?'disabled':''; ?>>Appliquer l’ajustement</button></form></section><?php endif; ?>
<section class="pcc-alert info"><i class="fas fa-lock"></i><div><strong>Protection session active</strong><p>Pour un joueur connecté, le Control Center refuse l’écriture directe. Le prochain niveau d’intégration devra exposer une commande admin ÉMU qui réutilise l’économie en mémoire, puis retourne un résultat auditable.</p></div></section>

<?php elseif($tab==='inventory'): ?>
<section class="pcc-balance-grid"><article><span>Poids</span><strong><?php echo $h(number_format((float)($inventory['weight']??0),3,',',' ')); ?> kg</strong><small>capacité <?php echo $h(number_format((float)($inventory['capacity']??0),3,',',' ')); ?> kg</small></article><article><span>Slots</span><strong><?php echo (int)($inventory['slots_used']??0); ?> / <?php echo (int)($inventory['max_slots']??0); ?></strong><small>Inventory V2</small></article><article><span>Objets physiques</span><strong><?php echo (int)($inventory['physical_items']??0); ?></strong><small>documents affichés séparément</small></article></section>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Inventaire réel</h2><p>Projection de rp_inventory_items + rp_item_definitions.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table pcc-table-dense"><thead><tr><th>Objet</th><th>Code</th><th>Catégorie</th><th>Quantité</th><th>Poids unité</th><th>Total</th><th>Slot</th><th>État</th></tr></thead><tbody><?php $physical=array_filter($inventory['items']??array(),fn($x)=>($x['source']??'')==='inventory'); if(!$physical):?><tr><td colspan="8"><div class="pcc-empty compact"><strong>Inventaire vide</strong><span>Aucun objet physique.</span></div></td></tr><?php endif; ?><?php foreach($physical as $item):?><tr><td><strong><?php echo $h($item['name']); ?></strong><small class="pcc-subline"><?php echo $h($item['description']); ?></small></td><td><code><?php echo $h($item['code']); ?></code></td><td><?php echo $h($item['category']); ?></td><td><?php echo (int)$item['quantity']; ?></td><td><?php echo $h(number_format((float)$item['weight'],3,',',' ')); ?> kg</td><td><?php echo $h(number_format((float)$item['total_weight'],3,',',' ')); ?> kg</td><td><?php echo $item['slot']===null?'—':(int)$item['slot']; ?></td><td><span class="pcc-badge <?php echo !empty($item['usable'])?'success':''; ?>"><?php echo !empty($item['usable'])?'UTILISABLE':'STOCK'; ?></span></td></tr><?php endforeach; ?></tbody></table></div></section>
<section class="pcc-alert info"><i class="fas fa-project-diagram"></i><div><strong>Lecture connectée, mutation protégée</strong><p>InventoryService possède déjà capacité, stacking, transfert et journal métier. Je n’ajoute pas un INSERT SQL concurrent dans le CMS. Les boutons Ajouter/Retirer seront ouverts quand le bridge admin ÉMU appellera cette logique commune.</p></div></section>

<?php elseif($tab==='documents'): ?>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Documents du joueur</h2><p>Source : rp_player_documents + rp_document_types.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Document</th><th>Numéro</th><th>Statut</th><th>Émission</th><th>Expiration</th><th>Actions</th></tr></thead><tbody><?php if(!$documents):?><tr><td colspan="6"><div class="pcc-empty compact"><strong>Aucun document</strong><span>Aucun document V2 délivré à ce joueur.</span></div></td></tr><?php endif; ?><?php foreach($documents as $doc):?><tr><td><strong><?php echo $h($doc['name']); ?></strong><small class="pcc-subline"><code><?php echo $h($doc['type']); ?></code></small></td><td><code><?php echo $h($doc['number']); ?></code></td><td><span class="pcc-badge <?php echo strtoupper($doc['status'])==='VALID'?'success':(strtoupper($doc['status'])==='REVOKED'?'danger':'warning'); ?>"><?php echo $h($doc['status']); ?></span></td><td><?php echo $h($PCC->formatDate($doc['issued_at'])); ?></td><td><?php echo $doc['expires_at']?$h($PCC->formatDate($doc['expires_at'])):'—'; ?></td><td><?php if($PCC->can('documents.manage')):?><div class="pcc-row-actions"><form method="post" data-confirm-form data-confirm-title="Modifier ce document ?" data-confirm-message="Document <?php echo $h($doc['number']); ?> : action auditée."><input type="hidden" name="csrf" value="<?php echo $h($csrf); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="document_status"><input type="hidden" name="document_id" value="<?php echo (int)$doc['id']; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo $userId; ?>"><input type="hidden" name="return_tab" value="documents"><select name="status_action"><option value="suspend">Suspendre</option><option value="reactivate">Réactiver</option><option value="revoke">Révoquer</option></select><input name="reason" placeholder="Raison" minlength="3" maxlength="500" required><button class="pcc-button small danger" <?php echo $isOnline||!$PCC->auditReady()?'disabled':''; ?>>Valider</button></form></div><?php endif; ?></td></tr><?php endforeach; ?></tbody></table></div></section>
<?php if($isOnline): ?><section class="pcc-alert warning"><i class="fas fa-sync"></i><div><strong>Cache DocumentService détecté</strong><p>Le joueur doit être hors ligne pour suspendre/révoquer depuis le CMS tant qu’aucune invalidation de cache n’est exposée par un endpoint admin ÉMU.</p></div></section><?php endif; ?>

<?php elseif($tab==='phone'): ?>
<?php if(!$phone): ?><section class="pcc-panel"><div class="pcc-empty"><i class="fas fa-mobile-alt"></i><strong>Aucun ParadisePhone</strong><span>Aucune ligne rp_phones pour ce joueur. La consultation admin ne crée jamais automatiquement de téléphone.</span></div></section>
<?php else: ?><section class="pcc-balance-grid"><article><span>Numéro</span><strong class="pcc-kpi-text"><?php echo $h($phone['phone_number']); ?></strong><small><?php echo $h($phone['status']); ?></small></article><article><span>Contacts</span><strong><?php echo (int)$phoneStats['contacts']; ?></strong><small>rp_phone_contacts</small></article><article><span>SMS</span><strong><?php echo (int)$phoneStats['messages']; ?></strong><small><?php echo (int)$phoneStats['unread']; ?> non lu(s)</small></article><article><span>Appels</span><strong><?php echo (int)$phoneStats['calls']; ?></strong><small>rp_phone_calls</small></article><article><span>Notifications</span><strong><?php echo (int)$phoneStats['notifications']; ?></strong><small>historique</small></article><article><span>Dernière activité</span><strong class="pcc-kpi-text"><?php echo $phoneStats['last_activity']?$h($PCC->formatDate($phoneStats['last_activity'])):'—'; ?></strong></article></section>
<section class="pcc-two-col"><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Appareil</h2><p>ParadisePhone V1.</p></div></div><dl class="pcc-detail-list"><div><dt>Numéro</dt><dd><code><?php echo $h($phone['phone_number']); ?></code></dd></div><div><dt>Device ID</dt><dd><code><?php echo $h($phone['device_identifier']); ?></code></dd></div><div><dt>Statut</dt><dd><?php echo $h($phone['status']); ?></dd></div><div><dt>Silencieux</dt><dd><?php echo !empty($phone['silent_mode'])?'Oui':'Non'; ?></dd></div><div><dt>Notifications</dt><dd><?php echo !empty($phone['notifications_enabled'])?'Activées':'Désactivées'; ?></dd></div><div><dt>Sons</dt><dd><?php echo !empty($phone['sounds_enabled'])?'Activés':'Désactivés'; ?></dd></div></dl></article><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Confidentialité</h2><p>Le Control Center n’expose pas le contenu privé par défaut.</p></div></div><div class="pcc-empty compact"><i class="fas fa-user-lock"></i><strong>SMS privés protégés</strong><span>Les statistiques sont visibles. La lecture du contenu exige la permission très élevée phone.messages.read et une consultation auditée ; cette vue n’affiche aucun message.</span></div></article></section><?php endif; ?>

<?php elseif($tab==='sanctions'): ?>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Sanctions</h2><p>Source legacy réelle : bans.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Type</th><th>Raison</th><th>Staff</th><th>Ajout</th><th>Fin</th><th>Statut</th></tr></thead><tbody><?php if(!$bans):?><tr><td colspan="6"><div class="pcc-empty compact"><strong>Aucune sanction</strong></div></td></tr><?php endif; ?><?php foreach($bans as $ban):$active=(int)($ban['expire']??0)>=time();?><tr><td><?php echo $h($ban['bantype']??'user'); ?></td><td><?php echo $h($ban['reason']??'—'); ?></td><td><?php echo $h($ban['added_by']??'—'); ?></td><td><?php echo !empty($ban['added_date'])?$h($PCC->formatDate($ban['added_date'])):'—'; ?></td><td><?php echo !empty($ban['expire'])?$h($PCC->formatDate($ban['expire'])):'—'; ?></td><td><span class="pcc-badge <?php echo $active?'danger':''; ?>"><?php echo $active?'ACTIVE':'EXPIRÉE'; ?></span></td></tr><?php endforeach; ?></tbody></table></div></section>
<?php if($PCC->can('sanctions.issue')): ?><section class="pcc-two-col"><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Bannir</h2><p>Durée bornée + raison + audit.</p></div></div><form method="post" class="pcc-form" data-confirm-form data-confirm-danger data-confirm-title="Bannir <?php echo $h($user['username']); ?> ?" data-confirm-message="Cette sanction prendra effet lors des contrôles d’authentification."><?php echo $actionHidden('ban','sanctions'); ?><label><span>Durée en jours</span><input type="number" name="days" min="1" max="3650" value="1" required></label><label><span>Raison</span><textarea name="reason" rows="3" minlength="3" maxlength="500" required></textarea></label><button class="pcc-button danger" <?php echo !$PCC->auditReady()?'disabled':''; ?>>Créer la sanction</button></form></article><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Lever la sanction</h2><p>Supprime le ban utilisateur actif, avec audit.</p></div></div><?php if(!$activeBan):?><div class="pcc-empty compact"><strong>Aucun ban actif</strong></div><?php else:?><form method="post" class="pcc-form" data-confirm-form data-confirm-title="Lever la sanction ?" data-confirm-message="Le bannissement actif de <?php echo $h($user['username']); ?> sera retiré."><?php echo $actionHidden('unban','sanctions'); ?><label><span>Raison</span><textarea name="reason" rows="3" minlength="3" maxlength="500" required></textarea></label><button class="pcc-button secondary" <?php echo !$PCC->auditReady()?'disabled':''; ?>>Débannir</button></form><?php endif; ?></article></section><?php endif; ?>

<?php elseif($tab==='logs'): ?>
<section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Historique administratif</h2><p>Actions du Control Center ciblant ce joueur.</p></div></div><?php if(!$PCC->auditReady()):?><div class="pcc-empty"><strong>Audit non installé</strong></div><?php elseif(!$audit):?><div class="pcc-empty compact"><strong>Aucune action auditée sur ce joueur</strong></div><?php else:?><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Date</th><th>Staff</th><th>Module</th><th>Action</th><th>Raison</th><th>Diff</th></tr></thead><tbody><?php foreach($audit as $log):?><tr><td><?php echo $h($PCC->formatDate($log['created_at'])); ?></td><td><?php echo $h($log['staff_username']); ?></td><td><?php echo $h($log['module']); ?></td><td><code><?php echo $h($log['action']); ?></code></td><td><?php echo $h($log['reason']); ?></td><td><details class="pcc-diff-details"><summary>Voir</summary><div class="pcc-diff"><pre><?php echo $h($log['before_data']??'—'); ?></pre><pre><?php echo $h($log['after_data']??'—'); ?></pre></div></details></td></tr><?php endforeach; ?></tbody></table></div><?php endif; ?></section>
<?php endif; ?>
