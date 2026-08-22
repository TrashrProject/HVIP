<?php
$scalar = function ($sql, $types = '', $params = array(), $key = 'value') use ($DB) {
    try { $row = $DB->PreparedRow($sql, $types, $params); return $row && array_key_exists($key, $row) ? $row[$key] : null; }
    catch (Throwable $e) { return null; }
};

$stats = array();
$totalUsers = $scalar('SELECT COUNT(*) value FROM users');
$onlineUsers = $scalar("SELECT COUNT(*) value FROM users WHERE online='1'");
if ($totalUsers !== null) $stats[] = array('Comptes', $totalUsers, 'fas fa-users', 'users');
if ($onlineUsers !== null) $stats[] = array('En ligne', $onlineUsers, 'fas fa-signal', 'online');
if ($PCC->tableExists('rp_characters')) {
    $v = $scalar('SELECT COUNT(*) value FROM rp_characters');
    if ($v !== null) $stats[] = array('Personnages RP', $v, 'fas fa-id-badge', 'character');
}
$cash = $scalar('SELECT COALESCE(SUM(credits),0) value FROM users');
if ($cash !== null) $stats[] = array('Cash total', $cash, 'fas fa-coins', 'money');
if ($PCC->tableExists('play_stats')) {
    $v = $scalar('SELECT COALESCE(SUM(bank),0) value FROM play_stats');
    if ($v !== null) $stats[] = array('Banque RP', $v, 'fas fa-university', 'money');
}
if ($PCC->tableExists('rp_inventory_items')) {
    $v = $scalar('SELECT COALESCE(SUM(quantity),0) value FROM rp_inventory_items');
    if ($v !== null) $stats[] = array('Objets détenus', $v, 'fas fa-box-open', 'inventory');
}
if ($PCC->tableExists('rp_phones')) {
    $v = $scalar("SELECT COUNT(*) value FROM rp_phones WHERE status='ACTIVE'");
    if ($v !== null) $stats[] = array('Téléphones actifs', $v, 'fas fa-mobile-alt', 'phone');
}
if ($PCC->tableExists('groups')) {
    $v = $scalar("SELECT COUNT(*) value FROM groups WHERE type='1' OR type='2'");
    if ($v !== null) $stats[] = array('Entreprises', $v, 'fas fa-building', 'business');
}
if ($PCC->tableExists('bans')) {
    $v = $scalar("SELECT COUNT(*) value FROM bans WHERE expire>=?", 'i', array(time()));
    if ($v !== null) $stats[] = array('Sanctions actives', $v, 'fas fa-gavel', 'sanctions');
}
$staffOnline = $scalar("SELECT COUNT(*) value FROM users WHERE rank>=3 AND online='1'");
if ($staffOnline !== null) $stats[] = array('Staff connecté', $staffOnline, 'fas fa-user-shield', 'staff');

$activity = array();
if ($PCC->auditReady()) {
    try { $activity = $DB->PreparedAll('SELECT id,staff_username,action,module,target_type,target_id,reason,created_at FROM cms_admin_audit_log ORDER BY id DESC LIMIT 10'); }
    catch (Throwable $e) { $activity = array(); }
}

$onlinePlayers = array();
try {
    $charJoin = $PCC->tableExists('rp_characters') ? ' LEFT JOIN rp_characters c ON c.user_id=u.id' : '';
    $charCols = $PCC->tableExists('rp_characters') ? ',c.first_name,c.last_name,c.citizen_id' : ",NULL first_name,NULL last_name,NULL citizen_id";
    $onlinePlayers = $DB->PreparedAll("SELECT u.id,u.username,u.look,u.rank" . $charCols . " FROM users u" . $charJoin . " WHERE u.online='1' ORDER BY u.id DESC LIMIT 8");
} catch (Throwable $e) { $onlinePlayers = array(); }

$systems = array(
    array('Character / identité', array('rp_characters'), 'Character V2'),
    array('Documents', array('rp_document_types','rp_player_documents'), 'Documents V2'),
    array('Inventaire', array('rp_item_definitions','rp_inventory_items','rp_inventory_profiles'), 'Inventory V2'),
    array('ParadisePhone', array('rp_phones','rp_phone_contacts','play_phone_chats'), 'Phone V1'),
    array('Entreprises', array('groups','group_memberships'), 'Core RP'),
    array('Véhicules', array('play_vehicles_owned'), 'Legacy RP'),
    array('Propriétés', array('play_apartments_owned'), 'Legacy RP'),
    array('Catalogue', array('catalog_items'), 'Habbo core')
);
?>
<section class="pcc-alert info">
    <i class="fas fa-link"></i><div><strong>Core actuel détecté</strong><p>Character, Documents, Inventory et ParadisePhone sont maintenant lus depuis leurs vraies tables. Les mutations qui nécessitent une synchronisation ÉMU restent bloquées sur un joueur connecté plutôt que de provoquer un état incohérent.</p></div>
</section>

<section class="pcc-kpi-grid pcc-kpi-grid-auto">
    <?php foreach($stats as $stat): ?>
        <article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="<?php echo $h($stat[2]); ?>"></i></div><div><span><?php echo $h($stat[0]); ?></span><strong><?php echo $number($stat[1]); ?></strong></div></article>
    <?php endforeach; ?>
</section>

<section class="pcc-dashboard-grid">
    <article class="pcc-panel pcc-panel-wide">
        <div class="pcc-panel-head"><div><h2>Activité récente</h2><p>Actions administratives réellement auditées.</p></div><?php if($PCC->can('logs.view')): ?><a class="pcc-text-link" href="<?php echo URL; ?>/admin.php?page=logs">Voir tous les logs</a><?php endif; ?></div>
        <?php if(!$PCC->auditReady()): ?>
            <div class="pcc-empty"><i class="fas fa-clipboard-list"></i><strong>Audit en attente de migration</strong><span>Les écritures sensibles sont verrouillées tant que la table n’existe pas.</span></div>
        <?php elseif(!$activity): ?>
            <div class="pcc-empty"><i class="fas fa-check-circle"></i><strong>Aucune action récente</strong><span>Le journal est prêt et attend les prochaines actions.</span></div>
        <?php else: ?>
            <div class="pcc-activity-list">
                <?php foreach($activity as $log): ?>
                    <div class="pcc-activity"><span class="pcc-activity-icon"><i class="fas fa-history"></i></span><div><strong><?php echo $h($log['staff_username']); ?> · <?php echo $h($log['action']); ?></strong><p><?php echo $h($log['module']); ?> · <?php echo $h($log['target_type']); ?><?php echo $log['target_id'] !== null ? ' #' . $h($log['target_id']) : ''; ?> — <?php echo $h($log['reason']); ?></p></div><time><?php echo $h($PCC->formatDate($log['created_at'])); ?></time></div>
                <?php endforeach; ?>
            </div>
        <?php endif; ?>
    </article>

    <article class="pcc-panel">
        <div class="pcc-panel-head"><div><h2>Connectés maintenant</h2><p>Source réelle : <code>users.online</code>.</p></div></div>
        <?php if(!$onlinePlayers): ?><div class="pcc-empty compact"><strong>Aucun joueur en ligne</strong><span>La liste se met à jour depuis la base.</span></div>
        <?php else: ?><div class="pcc-mini-users">
            <?php foreach($onlinePlayers as $player): $rpName = trim((string)$player['first_name'].' '.(string)$player['last_name']); ?>
                <a href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$player['id']; ?>"><img src="<?php echo $h($PCC->avatarUrl($player['look'],'s')); ?>" alt=""><span><strong><?php echo $h($player['username']); ?></strong><small><?php echo $rpName !== '' ? $h($rpName) : $h($PCC->roleName($player['rank'])); ?></small></span><em>en ligne</em></a>
            <?php endforeach; ?>
        </div><?php endif; ?>
    </article>
</section>

<section class="pcc-panel">
    <div class="pcc-panel-head"><div><h2>Cartographie des systèmes</h2><p>Disponibilité déterminée par les tables réellement présentes dans la base.</p></div><a class="pcc-text-link" href="<?php echo URL; ?>/admin.php?page=health">Santé système</a></div>
    <div class="pcc-system-grid">
        <?php foreach($systems as $system): $ready = true; foreach($system[1] as $table) if(!$PCC->tableExists($table)) $ready = false; ?>
            <div class="pcc-system"><div class="pcc-system-top"><strong><?php echo $h($system[0]); ?></strong><span class="pcc-badge <?php echo $ready ? 'success' : 'warning'; ?>"><?php echo $ready ? 'CONNECTÉ' : 'BACKEND ABSENT'; ?></span></div><span><?php echo $h($system[2]); ?></span><code><?php echo $h(implode(' · ', $system[1])); ?></code></div>
        <?php endforeach; ?>
    </div>
</section>
