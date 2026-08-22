<?php
$h = function ($value) { return htmlspecialchars((string)$value, ENT_QUOTES, 'UTF-8'); };
$number = function ($value) { return number_format((int)$value, 0, ',', ' '); };
$money = function ($value) { return number_format((int)$value, 0, ',', ' ') . ' cr'; };
$getScalar = function ($sql) use ($DB) {
    try {
        $result = $DB->Query($sql);
        $row = mysqli_fetch_row($result);
        return $row ? $row[0] : null;
    } catch (Throwable $e) {
        return null;
    }
};
$fetchAll = function ($result) {
    $rows = array();
    if ($result instanceof mysqli_result) while ($row = mysqli_fetch_assoc($result)) $rows[] = $row;
    return $rows;
};
$auditReady = false;
try { $auditReady = $PCC->auditReady(); } catch (Throwable $e) { $auditReady = false; }

$pageTitles = array(
    'dashboard' => array('Vue d’ensemble', 'La situation du serveur en un coup d’œil.'),
    'players' => array('Joueurs', 'Rechercher, filtrer et ouvrir une fiche joueur.'),
    'player' => array('Fiche joueur', 'Compte, économie, badges, sanctions et historique administratif.'),
    'businesses' => array('Entreprises', 'Lecture des corporations réellement présentes dans la base.'),
    'sanctions' => array('Sanctions', 'Bannissements actifs et actions disciplinaires autorisées.'),
    'logs' => array('Journal d’audit', 'Traçabilité des actions sensibles du Control Center.'),
    'staff' => array('Équipe staff', 'Comptes staff et état de connexion.'),
    'permissions' => array('Permissions', 'Correspondance actuelle entre ranks Habbo et capacités admin.'),
    'settings' => array('Configuration', 'Réglages CMS exposés sans afficher de secrets.'),
    'tools' => array('Outils existants', 'Fonctions historiques conservées, sécurisées et unifiées.')
);
$currentTitle = $pageTitles[$AdminPage];
$staffAvatar = $PCC->avatarUrl(isset($UData['look']) ? $UData['look'] : '', 's');
$onlineNow = $getScalar("SELECT COUNT(*) FROM users WHERE online='1'");
?>
<div class="pcc-app" data-pcc-app>
    <aside class="pcc-sidebar" data-sidebar>
        <div class="pcc-brand">
            <div class="pcc-brand-mark">P</div>
            <div class="pcc-brand-copy"><strong>PARADISE</strong><span>CONTROL CENTER</span></div>
            <button class="pcc-icon-button pcc-sidebar-toggle" type="button" data-sidebar-toggle aria-label="Réduire la navigation"><i class="fas fa-bars"></i></button>
        </div>

        <nav class="pcc-nav" aria-label="Navigation administration">
            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Vue d’ensemble</span>
                <a class="pcc-nav-item <?php echo $AdminPage === 'dashboard' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=dashboard"><i class="fas fa-th-large"></i><span>Dashboard</span></a>
            </div>
            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Gestion</span>
                <?php if($PCC->can('admin.players.view')): ?><a class="pcc-nav-item <?php echo in_array($AdminPage, array('players','player'), true) ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=players"><i class="fas fa-users"></i><span>Joueurs</span></a><?php endif; ?>
                <?php if($PCC->can('admin.sanctions.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'sanctions' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=sanctions"><i class="fas fa-gavel"></i><span>Sanctions</span></a><?php endif; ?>
            </div>
            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Roleplay</span>
                <?php if($PCC->can('admin.businesses.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'businesses' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=businesses"><i class="fas fa-building"></i><span>Entreprises</span><em>lecture</em></a><?php endif; ?>
                <span class="pcc-nav-item is-disabled" title="Backend administratif fiable non détecté"><i class="fas fa-id-card"></i><span>Documents</span><em>à connecter</em></span>
                <span class="pcc-nav-item is-disabled" title="Backend administratif fiable non détecté"><i class="fas fa-box-open"></i><span>Inventaire</span><em>à connecter</em></span>
                <span class="pcc-nav-item is-disabled" title="Backend administratif fiable non détecté"><i class="fas fa-mobile-alt"></i><span>Téléphone</span><em>à connecter</em></span>
            </div>
            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Administration</span>
                <?php if($PCC->can('admin.staff.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'staff' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=staff"><i class="fas fa-user-shield"></i><span>Staff</span></a><?php endif; ?>
                <?php if($PCC->can('admin.logs.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'logs' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=logs"><i class="fas fa-stream"></i><span>Logs</span></a><?php endif; ?>
                <?php if($PCC->can('admin.permissions.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'permissions' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=permissions"><i class="fas fa-key"></i><span>Permissions</span></a><?php endif; ?>
                <?php if($PCC->can('admin.settings.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'settings' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=settings"><i class="fas fa-sliders-h"></i><span>Configuration</span></a><?php endif; ?>
                <?php if($PCC->can('admin.tools.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'tools' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=tools"><i class="fas fa-toolbox"></i><span>Outils existants</span></a><?php endif; ?>
            </div>
        </nav>

        <div class="pcc-sidebar-foot">
            <div class="pcc-runtime"><span class="pcc-runtime-dot neutral"></span><div><strong>ÉMU</strong><small>état non exposé au CMS</small></div></div>
            <div class="pcc-version">CMS <?php echo $h(Config::$V); ?></div>
        </div>
    </aside>

    <section class="pcc-workspace">
        <header class="pcc-topbar">
            <div class="pcc-global-search" data-global-search>
                <i class="fas fa-search"></i>
                <input type="search" placeholder="Rechercher un joueur ou un ID…" autocomplete="off" aria-label="Recherche globale" data-global-search-input>
                <kbd>⌘ K</kbd>
                <div class="pcc-search-results" data-global-search-results hidden></div>
            </div>
            <div class="pcc-topbar-state">
                <span class="pcc-status-pill"><i class="fas fa-user-friends"></i><?php echo $onlineNow === null ? '—' : $number($onlineNow); ?> en ligne</span>
                <span class="pcc-status-pill neutral"><i class="fas fa-server"></i>ÉMU non sondé</span>
            </div>
            <div class="pcc-account">
                <img src="<?php echo $h($staffAvatar); ?>" alt="" onerror="this.style.display='none'">
                <div><strong><?php echo $h($UData['username']); ?></strong><span><?php echo $h($PCC->roleName($UData['rank'])); ?></span></div>
                <a class="pcc-icon-button" href="<?php echo URL; ?>/me" title="Retour au CMS"><i class="fas fa-external-link-alt"></i></a>
                <form method="post" class="pcc-inline-form">
                    <input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>">
                    <input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>">
                    <input type="hidden" name="action" value="logout">
                    <button class="pcc-icon-button" type="submit" title="Déconnexion"><i class="fas fa-sign-out-alt"></i></button>
                </form>
            </div>
        </header>

        <main class="pcc-main">
            <div class="pcc-page-head">
                <div><span class="pcc-eyebrow">Paradise Control Center</span><h1><?php echo $h($currentTitle[0]); ?></h1><p><?php echo $h($currentTitle[1]); ?></p></div>
                <div class="pcc-page-head-actions"><button class="pcc-button ghost" type="button" data-refresh><i class="fas fa-sync-alt"></i> Actualiser</button></div>
            </div>

            <?php if($AdminFlash): ?>
            <div class="pcc-toast is-<?php echo $h($AdminFlash['type']); ?>" data-toast><i class="fas fa-info-circle"></i><span><?php echo $h($AdminFlash['message']); ?></span><button type="button" data-toast-close aria-label="Fermer">×</button></div>
            <?php endif; ?>

            <?php if($AdminPage === 'dashboard'):
                $stats = array();
                $totalUsers = $getScalar('SELECT COUNT(*) FROM users');
                if($totalUsers !== null) $stats[] = array('users','Comptes totaux',$totalUsers,'fas fa-users');
                if($onlineNow !== null) $stats[] = array('online','Joueurs en ligne',$onlineNow,'fas fa-signal');
                $creditTotal = $getScalar('SELECT COALESCE(SUM(credits),0) FROM users');
                if($creditTotal !== null) $stats[] = array('credits','Crédits comptes',$creditTotal,'fas fa-coins');
                if($PCC->tableExists('play_stats')) {
                    $bankTotal = $getScalar('SELECT COALESCE(SUM(bank),0) FROM play_stats');
                    if($bankTotal !== null) $stats[] = array('bank','Banque RP',$bankTotal,'fas fa-university');
                }
                if($PCC->tableExists('groups')) {
                    $businessTotal = $getScalar("SELECT COUNT(*) FROM groups WHERE type='1' OR type='2'");
                    if($businessTotal !== null) $stats[] = array('business','Entreprises détectées',$businessTotal,'fas fa-building');
                }
                $roomTotal = $getScalar('SELECT COUNT(*) FROM rooms');
                if($roomTotal !== null) $stats[] = array('rooms','Appartements',$roomTotal,'fas fa-home');
                $activity = array();
                if($auditReady) {
                    try { $activity = $fetchAll($DB->Query('SELECT id,staff_username,action,target_type,target_id,reason,created_at FROM cms_admin_audit_log ORDER BY id DESC LIMIT 8')); } catch(Throwable $e) {}
                }
                $onlinePlayers = array();
                try { $onlinePlayers = $fetchAll($DB->Query("SELECT id,username,rank,look FROM users WHERE online='1' ORDER BY id DESC LIMIT 7")); } catch(Throwable $e) {}
            ?>
                <?php if(!$auditReady): ?><section class="pcc-alert warning"><i class="fas fa-exclamation-triangle"></i><div><strong>Journal d’audit non installé</strong><p>La migration V2 doit être appliquée avant toute action sensible. Les écritures admin sont bloquées volontairement tant que la table d’audit n’existe pas.</p></div></section><?php endif; ?>
                <section class="pcc-alert info"><i class="fas fa-link"></i><div><strong>Synchronisation ÉMU protégée</strong><p>Aucun pont admin → ÉMU fiable n’a été détecté dans le CMS audité. Les ajustements économiques d’un joueur connecté sont donc bloqués plutôt que d’écrire directement en SQL et désynchroniser sa session.</p></div></section>

                <section class="pcc-kpi-grid">
                    <?php foreach($stats as $stat): ?><article class="pcc-kpi"><div class="pcc-kpi-icon"><i class="<?php echo $h($stat[3]); ?>"></i></div><div><span><?php echo $h($stat[1]); ?></span><strong><?php echo $number($stat[2]); ?></strong></div></article><?php endforeach; ?>
                </section>

                <section class="pcc-dashboard-grid">
                    <article class="pcc-panel pcc-panel-wide">
                        <div class="pcc-panel-head"><div><h2>Activité récente</h2><p>Actions réellement enregistrées par le Control Center.</p></div><?php if($PCC->can('admin.logs.view')): ?><a href="<?php echo URL; ?>/admin.php?page=logs" class="pcc-text-link">Voir tous les logs</a><?php endif; ?></div>
                        <?php if(!$auditReady): ?><div class="pcc-empty"><i class="fas fa-clipboard-list"></i><strong>Aucune source d’audit disponible</strong><span>Applique la migration V2 pour commencer la traçabilité.</span></div>
                        <?php elseif(!$activity): ?><div class="pcc-empty"><i class="fas fa-check-circle"></i><strong>Aucune action récente</strong><span>Le journal est prêt. Les prochaines actions sensibles apparaîtront ici.</span></div>
                        <?php else: ?><div class="pcc-activity-list"><?php foreach($activity as $log): ?><div class="pcc-activity"><span class="pcc-activity-icon"><i class="fas fa-history"></i></span><div><strong><?php echo $h($log['staff_username']); ?> · <?php echo $h($log['action']); ?></strong><p><?php echo $h($log['target_type']); ?><?php echo $log['target_id'] !== null ? ' #' . $h($log['target_id']) : ''; ?> — <?php echo $h($log['reason']); ?></p></div><time><?php echo $h($PCC->formatDate($log['created_at'])); ?></time></div><?php endforeach; ?></div><?php endif; ?>
                    </article>
                    <article class="pcc-panel">
                        <div class="pcc-panel-head"><div><h2>Connectés maintenant</h2><p>Source : <code>users.online</code>.</p></div></div>
                        <?php if(!$onlinePlayers): ?><div class="pcc-empty compact"><strong>Aucun joueur en ligne</strong><span>La liste se remplira avec les données réelles.</span></div><?php else: ?><div class="pcc-mini-users"><?php foreach($onlinePlayers as $player): ?><a href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$player['id']; ?>"><img src="<?php echo $h($PCC->avatarUrl($player['look'],'s')); ?>" alt=""><span><strong><?php echo $h($player['username']); ?></strong><small><?php echo $h($PCC->roleName($player['rank'])); ?></small></span><em>en ligne</em></a><?php endforeach; ?></div><?php endif; ?>
                    </article>
                </section>

                <section class="pcc-panel">
                    <div class="pcc-panel-head"><div><h2>Cartographie des systèmes</h2><p>Ce que le CMS V2 peut réellement exploiter après audit.</p></div></div>
                    <div class="pcc-system-grid">
                        <div class="pcc-system ready"><i class="fas fa-users"></i><div><strong>Joueurs</strong><span>users + play_stats</span></div><b>Connecté</b></div>
                        <div class="pcc-system ready"><i class="fas fa-gavel"></i><div><strong>Sanctions</strong><span>bans</span></div><b>Connecté</b></div>
                        <div class="pcc-system ready"><i class="fas fa-building"></i><div><strong>Entreprises</strong><span>groups + group_memberships</span></div><b>Lecture</b></div>
                        <div class="pcc-system pending"><i class="fas fa-id-badge"></i><div><strong>Character / Citizen ID</strong><span>Aucun backend administratif fiable détecté</span></div><b>Non exposé</b></div>
                        <div class="pcc-system pending"><i class="fas fa-box-open"></i><div><strong>Inventaire</strong><span>Aucun backend administratif fiable détecté</span></div><b>Non exposé</b></div>
                        <div class="pcc-system pending"><i class="fas fa-mobile-alt"></i><div><strong>ParadisePhone</strong><span>Aucun backend administratif fiable détecté</span></div><b>Non exposé</b></div>
                    </div>
                </section>

            <?php elseif($AdminPage === 'players'):
                $q = trim((string)($_GET['q'] ?? ''));
                $filter = in_array($_GET['filter'] ?? 'all', array('all','online','staff','citizens'), true) ? $_GET['filter'] : 'all';
                $perPage = in_array((int)($_GET['per_page'] ?? 25), array(25,50,100), true) ? (int)$_GET['per_page'] : 25;
                $page = max(1, (int)($_GET['p'] ?? 1));
                $where = array('1=1'); $params = array(); $types = '';
                if($q !== '') { $where[] = '(u.username LIKE ? OR u.id = ?)'; $params[] = '%' . mb_substr($q,0,64) . '%'; $params[] = ctype_digit($q) ? (int)$q : -1; $types .= 'si'; }
                if($filter === 'online') $where[] = "u.online='1'";
                elseif($filter === 'staff') $where[] = 'u.rank >= 3';
                elseif($filter === 'citizens') $where[] = 'u.rank < 3';
                $whereSql = implode(' AND ', $where);
                $countSql = 'SELECT COUNT(*) total FROM users u WHERE ' . $whereSql;
                $countResult = $types ? $DB->PreparedResult($countSql,$types,$params) : $DB->Query($countSql);
                $total = (int)(mysqli_fetch_assoc($countResult)['total'] ?? 0);
                $pages = max(1,(int)ceil($total/$perPage)); if($page>$pages)$page=$pages; $offset=($page-1)*$perPage;
                $sql = 'SELECT u.id,u.username,u.rank,u.online,u.look,u.credits,p.bank FROM users u LEFT JOIN play_stats p ON p.id=u.id WHERE ' . $whereSql . ' ORDER BY u.online DESC,u.id DESC LIMIT ' . $perPage . ' OFFSET ' . $offset;
                $playersResult = $types ? $DB->PreparedResult($sql,$types,$params) : $DB->Query($sql);
                $players = $fetchAll($playersResult);
            ?>
                <section class="pcc-panel">
                    <form class="pcc-filterbar" method="get">
                        <input type="hidden" name="page" value="players">
                        <label class="pcc-search-field"><i class="fas fa-search"></i><input name="q" value="<?php echo $h($q); ?>" placeholder="Pseudo ou ID joueur" autocomplete="off"></label>
                        <select name="filter" aria-label="Filtre"><option value="all" <?php echo $filter==='all'?'selected':''; ?>>Tous</option><option value="online" <?php echo $filter==='online'?'selected':''; ?>>En ligne</option><option value="staff" <?php echo $filter==='staff'?'selected':''; ?>>Staff</option><option value="citizens" <?php echo $filter==='citizens'?'selected':''; ?>>Citoyens</option></select>
                        <select name="per_page" aria-label="Lignes par page"><option <?php echo $perPage===25?'selected':''; ?>>25</option><option <?php echo $perPage===50?'selected':''; ?>>50</option><option <?php echo $perPage===100?'selected':''; ?>>100</option></select>
                        <button class="pcc-button secondary" type="submit">Filtrer</button>
                        <span class="pcc-filter-count"><?php echo $number($total); ?> résultat(s)</span>
                    </form>
                    <div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Joueur</th><th>ID</th><th>Rôle</th><th>Crédits</th><th>Banque</th><th>Statut</th><th class="right">Action</th></tr></thead><tbody>
                        <?php if(!$players): ?><tr><td colspan="7"><div class="pcc-empty compact"><strong>Aucun joueur trouvé</strong><span>Modifiez la recherche ou les filtres.</span></div></td></tr><?php endif; ?>
                        <?php foreach($players as $player): ?><tr><td><div class="pcc-user-cell"><img src="<?php echo $h($PCC->avatarUrl($player['look'],'s')); ?>" alt=""><strong><?php echo $h($player['username']); ?></strong></div></td><td>#<?php echo (int)$player['id']; ?></td><td><span class="pcc-badge role"><?php echo $h($PCC->roleName($player['rank'])); ?></span></td><td><?php echo $money($player['credits']); ?></td><td><?php echo $player['bank'] === null ? '—' : $money($player['bank']); ?></td><td><span class="pcc-badge <?php echo $player['online'] ? 'success':'muted'; ?>"><?php echo $player['online'] ? 'En ligne':'Hors ligne'; ?></span></td><td class="right"><a class="pcc-button ghost small" href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$player['id']; ?>">Ouvrir <i class="fas fa-chevron-right"></i></a></td></tr><?php endforeach; ?>
                    </tbody></table></div>
                    <div class="pcc-pagination"><span>Page <?php echo $page; ?> / <?php echo $pages; ?></span><div><?php if($page>1): ?><a href="?page=players&q=<?php echo rawurlencode($q); ?>&filter=<?php echo rawurlencode($filter); ?>&per_page=<?php echo $perPage; ?>&p=<?php echo $page-1; ?>">Précédent</a><?php endif; ?><?php for($i=max(1,$page-2);$i<=min($pages,$page+2);$i++): ?><a class="<?php echo $i===$page?'active':''; ?>" href="?page=players&q=<?php echo rawurlencode($q); ?>&filter=<?php echo rawurlencode($filter); ?>&per_page=<?php echo $perPage; ?>&p=<?php echo $i; ?>"><?php echo $i; ?></a><?php endfor; ?><?php if($page<$pages): ?><a href="?page=players&q=<?php echo rawurlencode($q); ?>&filter=<?php echo rawurlencode($filter); ?>&per_page=<?php echo $perPage; ?>&p=<?php echo $page+1; ?>">Suivant</a><?php endif; ?></div></div>
                </section>

            <?php elseif($AdminPage === 'player'):
                $playerId = max(0,(int)($_GET['id'] ?? 0));
                $playerResult = $DB->PreparedResult('SELECT u.id,u.username,u.rank,u.online,u.look,u.credits,u.account_created,p.bank FROM users u LEFT JOIN play_stats p ON p.id=u.id WHERE u.id=? LIMIT 1','i',array($playerId));
                $player = mysqli_fetch_assoc($playerResult);
                if(!$player): ?>
                    <section class="pcc-alert danger"><i class="fas fa-user-slash"></i><div><strong>Joueur introuvable</strong><p>L’ID demandé n’existe pas. <a href="<?php echo URL; ?>/admin.php?page=players">Retour aux joueurs</a>.</p></div></section>
                <?php else:
                    $badges = array(); if($PCC->tableExists('user_badges')) { try{$badges=$fetchAll($DB->PreparedResult('SELECT badge_id,badge_slot FROM user_badges WHERE user_id=? ORDER BY badge_slot ASC','i',array($playerId)));}catch(Throwable $e){} }
                    $bans = array(); if($PCC->tableExists('bans')) { try{$bans=$fetchAll($DB->PreparedResult("SELECT reason,expire,added_by,added_date FROM bans WHERE bantype='user' AND value=? ORDER BY added_date DESC LIMIT 10",'s',array($player['username'])));}catch(Throwable $e){} }
                    $playerLogs = array(); if($auditReady && $PCC->can('admin.logs.view')) { try{$playerLogs=$fetchAll($DB->PreparedResult("SELECT action,staff_username,reason,before_data,after_data,created_at FROM cms_admin_audit_log WHERE target_type='player' AND target_id=? ORDER BY id DESC LIMIT 12",'s',array((string)$playerId)));}catch(Throwable $e){} }
                ?>
                    <section class="pcc-player-hero">
                        <div class="pcc-player-avatar"><img src="<?php echo $h($PCC->avatarUrl($player['look'],'l')); ?>" alt="Avatar de <?php echo $h($player['username']); ?>"></div>
                        <div class="pcc-player-heading"><div class="pcc-player-name"><h2><?php echo $h($player['username']); ?></h2><span class="pcc-badge <?php echo $player['online']?'success':'muted'; ?>"><?php echo $player['online']?'En ligne':'Hors ligne'; ?></span></div><p>ID compte <strong>#<?php echo (int)$player['id']; ?></strong> · <?php echo $h($PCC->roleName($player['rank'])); ?></p><code><?php echo $h($player['look']); ?></code></div>
                        <div class="pcc-player-actions"><a class="pcc-button secondary" href="<?php echo URL; ?>/admin.php?page=players"><i class="fas fa-arrow-left"></i> Joueurs</a><?php if($PCC->can('admin.sanctions.manage')): ?><button class="pcc-button danger" type="button" data-open-section="sanction-card"><i class="fas fa-gavel"></i> Sanctionner</button><?php endif; ?></div>
                    </section>

                    <nav class="pcc-tabs" aria-label="Sections fiche joueur"><a href="#overview">Vue générale</a><a href="#economy">Économie</a><a href="#badges">Badges</a><a href="#sanctions">Sanctions</a><?php if($PCC->can('admin.logs.view')): ?><a href="#player-logs">Logs</a><?php endif; ?></nav>

                    <section class="pcc-two-col" id="overview">
                        <article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Compte Habbo</h2><p>Données vérifiées dans <code>users</code>.</p></div></div><dl class="pcc-detail-list"><div><dt>Username</dt><dd><?php echo $h($player['username']); ?></dd></div><div><dt>ID</dt><dd>#<?php echo (int)$player['id']; ?></dd></div><div><dt>Rank</dt><dd><?php echo (int)$player['rank']; ?> · <?php echo $h($PCC->roleName($player['rank'])); ?></dd></div><div><dt>Création du compte</dt><dd><?php echo $h($PCC->formatDate($player['account_created'])); ?></dd></div><div><dt>Statut</dt><dd><span class="pcc-badge <?php echo $player['online']?'success':'muted'; ?>"><?php echo $player['online']?'En ligne':'Hors ligne'; ?></span></dd></div></dl></article>
                        <article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Systèmes RP non exposés</h2><p>Le Control Center n’invente aucune donnée.</p></div></div><div class="pcc-availability"><div><i class="fas fa-id-badge"></i><span><strong>Character / Citizen ID</strong><small>Backend fiable non détecté pendant l’audit.</small></span></div><div><i class="fas fa-id-card"></i><span><strong>Documents</strong><small>Non exposé au CMS V2.</small></span></div><div><i class="fas fa-box-open"></i><span><strong>Inventaire</strong><small>Non exposé au CMS V2.</small></span></div><div><i class="fas fa-mobile-alt"></i><span><strong>Téléphone</strong><small>Non exposé au CMS V2.</small></span></div></div></article>
                    </section>

                    <section class="pcc-panel" id="economy">
                        <div class="pcc-panel-head"><div><h2>Économie</h2><p>Valeurs réelles de <code>users.credits</code> et <code>play_stats.bank</code>.</p></div></div>
                        <div class="pcc-balance-grid"><div><span>Crédits</span><strong><?php echo $money($player['credits']); ?></strong><small>users.credits</small></div><div><span>Banque</span><strong><?php echo $player['bank']===null?'—':$money($player['bank']); ?></strong><small>play_stats.bank</small></div></div>
                        <?php if($PCC->can('admin.economy.adjust')): ?>
                        <form class="pcc-form pcc-form-inline-grid" method="post" data-confirm-form data-confirm-title="Confirmer l’ajustement économique" data-confirm-message="Cette action modifie la source SQL et sera inscrite dans le journal d’audit.">
                            <input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="economy_adjust"><input type="hidden" name="user_id" value="<?php echo (int)$playerId; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo (int)$playerId; ?>">
                            <label><span>Solde</span><select name="wallet"><option value="credits">Crédits</option><option value="bank">Banque</option></select></label>
                            <label><span>Opération</span><select name="operation"><option value="add">Ajouter</option><option value="remove">Retirer</option><option value="set">Définir</option></select></label>
                            <label><span>Montant</span><input name="amount" type="number" min="0" max="2000000000" required placeholder="500"></label>
                            <label class="wide"><span>Raison obligatoire</span><input name="reason" required minlength="3" maxlength="500" placeholder="Correction administrative, compensation bug…"></label>
                            <div class="pcc-form-action wide"><button class="pcc-button primary" type="submit" <?php echo (!$auditReady || $player['online'])?'disabled':''; ?>><i class="fas fa-exchange-alt"></i> Ajuster le solde</button><?php if($player['online']): ?><small class="pcc-form-note warning">Joueur connecté : action bloquée tant que le pont ÉMU temps réel n’est pas disponible.</small><?php elseif(!$auditReady): ?><small class="pcc-form-note warning">Migration d’audit requise.</small><?php endif; ?></div>
                        </form>
                        <?php elseif($PCC->can('admin.economy.view')): ?><div class="pcc-inline-note">Votre rôle peut consulter l’économie mais pas la modifier.</div><?php endif; ?>
                    </section>

                    <section class="pcc-two-col">
                        <article class="pcc-panel" id="badges"><div class="pcc-panel-head"><div><h2>Badges</h2><p>Source : <code>user_badges</code>.</p></div></div><?php if(!$badges): ?><div class="pcc-empty compact"><strong>Aucun badge détecté</strong></div><?php else: ?><div class="pcc-badge-list"><?php foreach($badges as $badge): ?><div><span class="pcc-badge role"><?php echo $h($badge['badge_id']); ?></span><small>slot <?php echo (int)$badge['badge_slot']; ?></small></div><?php endforeach; ?></div><?php endif; ?>
                            <?php if($PCC->can('admin.players.badges.edit')): ?><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Attribuer ce badge ?" data-confirm-message="L’ancien badge du même slot sera remplacé et l’action sera auditée."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="badge"><input type="hidden" name="user_id" value="<?php echo (int)$playerId; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo (int)$playerId; ?>"><label><span>Code badge</span><input name="badge" pattern="[A-Za-z0-9_]+" maxlength="100" required></label><label><span>Slot</span><input name="slot" type="number" min="0" max="4" value="0" required></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button primary" <?php echo !$auditReady?'disabled':''; ?>>Attribuer</button></form><?php endif; ?>
                        </article>
                        <article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Apparence</h2><p>Modification contrôlée de <code>users.look</code>.</p></div></div><div class="pcc-look-preview"><img src="<?php echo $h($PCC->avatarUrl($player['look'],'m')); ?>" alt=""><code><?php echo $h($player['look']); ?></code></div><?php if($PCC->can('admin.players.appearance.edit')): ?><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Modifier l’apparence ?" data-confirm-message="Le look actuel sera conservé dans le diff du journal d’audit."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="look"><input type="hidden" name="user_id" value="<?php echo (int)$playerId; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo (int)$playerId; ?>"><label><span>Figure Habbo</span><textarea name="look" rows="3" maxlength="700" required><?php echo $h($player['look']); ?></textarea></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button primary" <?php echo !$auditReady?'disabled':''; ?>>Mettre à jour</button></form><?php endif; ?></article>
                    </section>

                    <section class="pcc-panel" id="sanctions">
                        <div class="pcc-panel-head"><div><h2>Sanctions</h2><p>Bannissements connus dans <code>bans</code>.</p></div></div>
                        <?php if(!$bans): ?><div class="pcc-empty compact"><strong>Aucun bannissement enregistré pour ce joueur</strong></div><?php else: ?><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Motif</th><th>Staff</th><th>Ajout</th><th>Expiration</th><th>État</th></tr></thead><tbody><?php foreach($bans as $ban): ?><tr><td><?php echo $h($ban['reason']); ?></td><td><?php echo $h($ban['added_by']); ?></td><td><?php echo $h($PCC->formatDate($ban['added_date'])); ?></td><td><?php echo $h($PCC->formatDate($ban['expire'])); ?></td><td><span class="pcc-badge <?php echo (int)$ban['expire'] >= time() ? 'danger':'muted'; ?>"><?php echo (int)$ban['expire'] >= time() ? 'Actif':'Expiré'; ?></span></td></tr><?php endforeach; ?></tbody></table></div><?php endif; ?>
                        <?php if($PCC->can('admin.sanctions.manage')): ?><div class="pcc-action-cards" id="sanction-card"><form class="pcc-form compact" method="post" data-confirm-form data-confirm-danger data-confirm-title="Bannir <?php echo $h($player['username']); ?> ?" data-confirm-message="Le bannissement remplacera tout ban utilisateur actif et sera journalisé."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="ban"><input type="hidden" name="user_id" value="<?php echo (int)$playerId; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo (int)$playerId; ?>"><h3>Bannir</h3><label><span>Durée (jours)</span><input name="days" type="number" min="1" max="3650" value="1" required></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button danger" <?php echo !$auditReady?'disabled':''; ?>>Bannir</button></form><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Débannir <?php echo $h($player['username']); ?> ?" data-confirm-message="Une raison est obligatoire et l’action sera journalisée."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="unban"><input type="hidden" name="user_id" value="<?php echo (int)$playerId; ?>"><input type="hidden" name="return_page" value="player"><input type="hidden" name="return_id" value="<?php echo (int)$playerId; ?>"><h3>Débannir</h3><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required placeholder="Fin de sanction, erreur…"></label><button class="pcc-button secondary" <?php echo !$auditReady?'disabled':''; ?>>Débannir</button></form></div><?php endif; ?>
                    </section>

                    <?php if($PCC->can('admin.logs.view')): ?><section class="pcc-panel" id="player-logs"><div class="pcc-panel-head"><div><h2>Historique administratif</h2><p>Actions V2 ciblant ce joueur.</p></div></div><?php if(!$auditReady || !$playerLogs): ?><div class="pcc-empty compact"><strong>Aucun log V2 pour ce joueur</strong></div><?php else: ?><div class="pcc-audit-list"><?php foreach($playerLogs as $log): ?><details><summary><span><strong><?php echo $h($log['action']); ?></strong><small><?php echo $h($log['staff_username']); ?> · <?php echo $h($PCC->formatDate($log['created_at'])); ?></small></span><em><?php echo $h($log['reason']); ?></em></summary><div class="pcc-diff"><div><b>Avant</b><pre><?php echo $h($log['before_data'] ?: '—'); ?></pre></div><div><b>Après</b><pre><?php echo $h($log['after_data'] ?: '—'); ?></pre></div></div></details><?php endforeach; ?></div><?php endif; ?></section><?php endif; ?>
                <?php endif; ?>

            <?php elseif($AdminPage === 'businesses'):
                $businesses = array();
                if($PCC->tableExists('groups') && $PCC->tableExists('group_memberships')) {
                    try { $businesses=$fetchAll($DB->Query("SELECT g.id,g.name,g.badge,g.type,COUNT(gm.user_id) employees FROM groups g LEFT JOIN group_memberships gm ON gm.group_id=g.id WHERE g.type='1' OR g.type='2' GROUP BY g.id,g.name,g.badge,g.type ORDER BY g.type,g.name")); } catch(Throwable $e) {}
                }
            ?>
                <section class="pcc-alert info"><i class="fas fa-info-circle"></i><div><strong>Module volontairement en lecture seule</strong><p>Les tables <code>groups</code> et <code>group_memberships</code> existent déjà, mais aucun service d’administration métier unifié n’a été détecté. Le V2 affiche donc les données sans créer un nouveau backend parallèle.</p></div></section>
                <section class="pcc-panel"><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>ID</th><th>Entreprise / Corporation</th><th>Type</th><th>Employés</th><th>Source</th></tr></thead><tbody><?php if(!$businesses): ?><tr><td colspan="5"><div class="pcc-empty compact"><strong>Aucune entreprise détectée</strong></div></td></tr><?php endif; ?><?php foreach($businesses as $business): ?><tr><td>#<?php echo (int)$business['id']; ?></td><td><strong><?php echo $h($business['name']); ?></strong></td><td><span class="pcc-badge role">Type <?php echo (int)$business['type']; ?></span></td><td><?php echo $number($business['employees']); ?></td><td><code>groups</code></td></tr><?php endforeach; ?></tbody></table></div></section>

            <?php elseif($AdminPage === 'sanctions'):
                $sanctions = array(); try{$sanctions=$fetchAll($DB->Query("SELECT b.value username,b.reason,b.expire,b.added_by,b.added_date,u.id,u.look,u.online FROM bans b LEFT JOIN users u ON u.username=b.value WHERE b.bantype='user' ORDER BY b.added_date DESC LIMIT 100"));}catch(Throwable $e){}
            ?>
                <section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Bannissements</h2><p>100 entrées les plus récentes de la table <code>bans</code>.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Joueur</th><th>Motif</th><th>Staff</th><th>Ajout</th><th>Expiration</th><th>État</th></tr></thead><tbody><?php if(!$sanctions): ?><tr><td colspan="6"><div class="pcc-empty compact"><strong>Aucune sanction trouvée</strong></div></td></tr><?php endif; ?><?php foreach($sanctions as $ban): ?><tr><td><?php if($ban['id']!==null): ?><a class="pcc-user-link" href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$ban['id']; ?>"><img src="<?php echo $h($PCC->avatarUrl($ban['look'],'s')); ?>" alt=""><strong><?php echo $h($ban['username']); ?></strong></a><?php else: ?><strong><?php echo $h($ban['username']); ?></strong><?php endif; ?></td><td><?php echo $h($ban['reason']); ?></td><td><?php echo $h($ban['added_by']); ?></td><td><?php echo $h($PCC->formatDate($ban['added_date'])); ?></td><td><?php echo $h($PCC->formatDate($ban['expire'])); ?></td><td><span class="pcc-badge <?php echo (int)$ban['expire']>=time()?'danger':'muted'; ?>"><?php echo (int)$ban['expire']>=time()?'Actif':'Expiré'; ?></span></td></tr><?php endforeach; ?></tbody></table></div></section>

            <?php elseif($AdminPage === 'logs'):
                $logQ=trim((string)($_GET['q']??'')); $logPage=max(1,(int)($_GET['p']??1)); $logPer=50; $logTotal=0; $logs=array();
                if($auditReady){$where='1=1';$params=array();$types='';if($logQ!==''){$where.=' AND (staff_username LIKE ? OR action LIKE ? OR target_id LIKE ? OR reason LIKE ?)';$needle='%'.mb_substr($logQ,0,80).'%';$params=array($needle,$needle,$needle,$needle);$types='ssss';}$countSql='SELECT COUNT(*) total FROM cms_admin_audit_log WHERE '.$where;$cr=$types?$DB->PreparedResult($countSql,$types,$params):$DB->Query($countSql);$logTotal=(int)(mysqli_fetch_assoc($cr)['total']??0);$logPages=max(1,(int)ceil($logTotal/$logPer));if($logPage>$logPages)$logPage=$logPages;$off=($logPage-1)*$logPer;$sql='SELECT id,staff_username,action,target_type,target_id,before_data,after_data,reason,created_at FROM cms_admin_audit_log WHERE '.$where.' ORDER BY id DESC LIMIT '.$logPer.' OFFSET '.$off;$logs=$fetchAll($types?$DB->PreparedResult($sql,$types,$params):$DB->Query($sql));}else{$logPages=1;}
            ?>
                <?php if(!$auditReady): ?><section class="pcc-alert warning"><i class="fas fa-database"></i><div><strong>Migration d’audit requise</strong><p>Applique <code>20260822_001_paradise_control_center_audit.sql</code>. Tant que la table n’existe pas, les écritures sensibles sont bloquées.</p></div></section><?php else: ?>
                <section class="pcc-panel"><form class="pcc-filterbar" method="get"><input type="hidden" name="page" value="logs"><label class="pcc-search-field"><i class="fas fa-search"></i><input name="q" value="<?php echo $h($logQ); ?>" placeholder="Staff, action, cible, raison…"></label><button class="pcc-button secondary">Rechercher</button><span class="pcc-filter-count"><?php echo $number($logTotal); ?> log(s)</span></form><div class="pcc-audit-list table-mode"><?php if(!$logs): ?><div class="pcc-empty compact"><strong>Aucun log correspondant</strong></div><?php endif; ?><?php foreach($logs as $log): ?><details><summary><span><strong><?php echo $h($log['action']); ?></strong><small><?php echo $h($log['staff_username']); ?> · <?php echo $h($log['target_type']); ?> <?php echo $log['target_id']!==null?'#'.$h($log['target_id']):''; ?></small></span><em><?php echo $h($PCC->formatDate($log['created_at'])); ?></em></summary><p class="pcc-log-reason"><?php echo $h($log['reason']); ?></p><div class="pcc-diff"><div><b>Avant</b><pre><?php echo $h($log['before_data']?:'—'); ?></pre></div><div><b>Après</b><pre><?php echo $h($log['after_data']?:'—'); ?></pre></div></div></details><?php endforeach; ?></div><div class="pcc-pagination"><span>Page <?php echo $logPage; ?> / <?php echo $logPages; ?></span><div><?php if($logPage>1): ?><a href="?page=logs&q=<?php echo rawurlencode($logQ); ?>&p=<?php echo $logPage-1; ?>">Précédent</a><?php endif; ?><?php if($logPage<$logPages): ?><a href="?page=logs&q=<?php echo rawurlencode($logQ); ?>&p=<?php echo $logPage+1; ?>">Suivant</a><?php endif; ?></div></div></section>
                <?php endif; ?>

            <?php elseif($AdminPage === 'staff'):
                $staffRows=array();try{$staffRows=$fetchAll($DB->Query("SELECT id,username,rank,online,look FROM users WHERE rank>=3 ORDER BY rank DESC,username ASC"));}catch(Throwable $e){}
            ?>
                <section class="pcc-panel"><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Staff</th><th>ID</th><th>Rank</th><th>Rôle CMS</th><th>Statut</th></tr></thead><tbody><?php foreach($staffRows as $staff): ?><tr><td><a class="pcc-user-link" href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$staff['id']; ?>"><img src="<?php echo $h($PCC->avatarUrl($staff['look'],'s')); ?>" alt=""><strong><?php echo $h($staff['username']); ?></strong></a></td><td>#<?php echo (int)$staff['id']; ?></td><td><?php echo (int)$staff['rank']; ?></td><td><span class="pcc-badge role"><?php echo $h($PCC->roleName($staff['rank'])); ?></span></td><td><span class="pcc-badge <?php echo $staff['online']?'success':'muted'; ?>"><?php echo $staff['online']?'En ligne':'Hors ligne'; ?></span></td></tr><?php endforeach; ?></tbody></table></div></section>
                <section class="pcc-alert info"><i class="fas fa-shield-alt"></i><div><strong>Modification des ranks volontairement absente</strong><p>Le V2 conserve les ranks Habbo comme source d’identité staff. La modification de rôle n’est pas exposée tant qu’un workflow anti-lockout et une politique de délégation n’ont pas été validés.</p></div></section>

            <?php elseif($AdminPage === 'permissions'):
                $caps=$PCC->capabilityMap();
            ?>
                <section class="pcc-panel"><div class="pcc-panel-head"><div><h2>RBAC de compatibilité</h2><p>Les permissions sont centralisées côté serveur et mappées sur les ranks existants au lieu de disperser des <code>rank &gt;= X</code> dans chaque action.</p></div></div><div class="pcc-table-wrap"><table class="pcc-table"><thead><tr><th>Capacité</th><th>Rank minimum</th><th>Rôle minimum</th><th>Votre accès</th></tr></thead><tbody><?php foreach($caps as $cap=>$minRank): ?><tr><td><code><?php echo $h($cap); ?></code></td><td><?php echo (int)$minRank; ?></td><td><?php echo $h($PCC->roleName($minRank)); ?></td><td><span class="pcc-badge <?php echo $PCC->can($cap)?'success':'muted'; ?>"><?php echo $PCC->can($cap)?'Autorisé':'Refusé'; ?></span></td></tr><?php endforeach; ?></tbody></table></div></section>

            <?php elseif($AdminPage === 'settings'): ?>
                <section class="pcc-two-col"><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>CMS</h2><p>Réglages réellement exposés.</p></div></div><dl class="pcc-detail-list"><div><dt>Version</dt><dd><?php echo $h(Config::$V); ?></dd></div><div><dt>URL publique</dt><dd><?php echo $h(Config::$URL); ?></dd></div><div><dt>Maintenance</dt><dd><span class="pcc-badge <?php echo Config::$_MANT?'warning':'success'; ?>"><?php echo Config::$_MANT?'Active':'Désactivée'; ?></span></dd></div><div><dt>Secrets</dt><dd>Masqués / non exposés</dd></div></dl></article><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>État technique</h2><p>Pas de faux indicateur serveur.</p></div></div><dl class="pcc-detail-list"><div><dt>Base SQL</dt><dd><span class="pcc-badge success">Connectée</span></dd></div><div><dt>ÉMU</dt><dd><span class="pcc-badge muted">Non sondé</span></dd></div><div><dt>Audit V2</dt><dd><span class="pcc-badge <?php echo $auditReady?'success':'warning'; ?>"><?php echo $auditReady?'Prêt':'Migration requise'; ?></span></dd></div></dl></article></section>
                <?php if($PCC->can('admin.settings.maintenance')): ?><section class="pcc-panel"><div class="pcc-panel-head"><div><h2>Maintenance CMS</h2><p>Action critique : confirmation et raison obligatoires.</p></div></div><form class="pcc-form pcc-maintenance-form" method="post" data-confirm-form data-confirm-danger data-confirm-title="Changer l’état de maintenance ?" data-confirm-message="L’accès public au CMS sera modifié. L’action sera journalisée."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="return_page" value="settings"><label><span>Raison obligatoire</span><input name="reason" minlength="3" maxlength="500" required placeholder="Maintenance planifiée, intervention technique…"></label><div class="pcc-form-action"><button class="pcc-button danger" name="action" value="maintenance_on" <?php echo (!$auditReady||Config::$_MANT)?'disabled':''; ?>>Activer la maintenance</button><button class="pcc-button secondary" name="action" value="maintenance_off" <?php echo (!$auditReady||!Config::$_MANT)?'disabled':''; ?>>Désactiver la maintenance</button></div></form></section><?php endif; ?>

            <?php elseif($AdminPage === 'tools'): ?>
                <section class="pcc-alert info"><i class="fas fa-wrench"></i><div><strong>Fonctions historiques conservées</strong><p>Catalogue, badges et look ne sont plus des formulaires dispersés : ils passent maintenant par permissions serveur, CSRF, nonce anti-rejeu et audit.</p></div></section>
                <section class="pcc-tool-grid">
                    <?php if($PCC->can('admin.catalogue.edit')): ?><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Catalogue</h2><p>Modifier une offre existante.</p></div></div><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Modifier cette offre ?" data-confirm-message="Le prix et l’état actuel seront conservés dans le journal d’audit."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="catalogue"><input type="hidden" name="return_page" value="tools"><label><span>ID de l’offre</span><input name="offer_id" type="number" min="1" required></label><label><span>Prix en crédits</span><input name="price" type="number" min="0" max="999999" required></label><label class="pcc-check"><input name="active" type="checkbox" value="1" checked><span>Offre active</span></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button primary" <?php echo !$auditReady?'disabled':''; ?>>Enregistrer</button></form></article><?php endif; ?>
                    <?php if($PCC->can('admin.players.badges.edit')): ?><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Badge joueur</h2><p>Attribution par ID validé.</p></div></div><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Attribuer ce badge ?" data-confirm-message="Le slot existant sera remplacé et l’action sera auditée."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="badge"><input type="hidden" name="return_page" value="tools"><label class="pcc-player-picker"><span>Joueur</span><input type="search" placeholder="Rechercher un joueur…" data-player-picker-input autocomplete="off"><input type="hidden" name="user_id" data-player-picker-id required><div class="pcc-picker-results" data-player-picker-results hidden></div></label><label><span>Code badge</span><input name="badge" pattern="[A-Za-z0-9_]+" maxlength="100" required></label><label><span>Slot</span><input name="slot" type="number" min="0" max="4" value="0" required></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button primary" <?php echo !$auditReady?'disabled':''; ?>>Attribuer</button></form></article><?php endif; ?>
                    <?php if($PCC->can('admin.players.appearance.edit')): ?><article class="pcc-panel"><div class="pcc-panel-head"><div><h2>Apparence joueur</h2><p>Correction du look avec diff avant/après.</p></div></div><form class="pcc-form compact" method="post" data-confirm-form data-confirm-title="Modifier ce look ?" data-confirm-message="Le look précédent sera conservé dans l’audit."><input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>"><input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>"><input type="hidden" name="action" value="look"><input type="hidden" name="return_page" value="tools"><label class="pcc-player-picker"><span>Joueur</span><input type="search" placeholder="Rechercher un joueur…" data-player-picker-input autocomplete="off"><input type="hidden" name="user_id" data-player-picker-id required><div class="pcc-picker-results" data-player-picker-results hidden></div></label><label><span>Figure Habbo</span><textarea name="look" rows="4" maxlength="700" required></textarea></label><label><span>Raison</span><input name="reason" minlength="3" maxlength="500" required></label><button class="pcc-button primary" <?php echo !$auditReady?'disabled':''; ?>>Mettre à jour</button></form></article><?php endif; ?>
                </section>
            <?php endif; ?>
        </main>
    </section>
</div>

<div class="pcc-modal-backdrop" data-confirm-modal hidden>
    <div class="pcc-modal" role="dialog" aria-modal="true" aria-labelledby="pcc-confirm-title">
        <div class="pcc-modal-icon"><i class="fas fa-exclamation-triangle"></i></div>
        <div><span class="pcc-eyebrow">Confirmation requise</span><h2 id="pcc-confirm-title" data-confirm-modal-title>Confirmer l’action</h2><p data-confirm-modal-message>Cette action sera enregistrée.</p></div>
        <div class="pcc-modal-actions"><button class="pcc-button ghost" type="button" data-confirm-cancel>Annuler</button><button class="pcc-button danger" type="button" data-confirm-accept>Confirmer</button></div>
    </div>
</div>
