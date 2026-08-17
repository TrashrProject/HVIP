<?php
$getCount = function($query) use ($DB) {
    $result = $DB->Query($query);
    $row = $result ? mysqli_fetch_assoc($result) : array('total' => 0);
    return (int)($row['total'] ?? 0);
};

$totalUsers = $getCount("SELECT COUNT(*) AS total FROM users");
$onlineUsers = $getCount("SELECT COUNT(*) AS total FROM users WHERE online = '1'");
$staffCount = $getCount("SELECT COUNT(*) AS total FROM users WHERE rank >= 3");
$roomCount = $getCount("SELECT COUNT(*) AS total FROM rooms");
$staffRows = $DB->Query("SELECT id, username, rank, online, look FROM users WHERE rank >= 3 ORDER BY rank DESC, username ASC LIMIT 12");
$onlineRows = $DB->Query("SELECT id, username, rank, look FROM users WHERE online = '1' ORDER BY id DESC LIMIT 8");
$newRows = $DB->Query("SELECT id, username, rank, online FROM users ORDER BY id DESC LIMIT 8");

$roleName = function($rank) {
    $rank = (int)$rank;
    if($rank >= 7) return 'Fondateur';
    if($rank === 6) return 'D&eacute;veloppeur';
    if($rank === 5) return 'Administrateur';
    if($rank === 4) return 'Modérateur';
    return 'Assistant';
};
?>
<style>
.staff-admin{max-width:1220px;margin:30px auto 46px;color:#102b45}.staff-admin *{box-sizing:border-box}.staff-hero{background:linear-gradient(120deg,#073c68,#147fb1);border:1px solid #49aada;border-radius:10px;padding:27px 30px;color:#fff;box-shadow:0 8px 22px rgba(5,48,80,.22);display:flex;justify-content:space-between;align-items:center}.staff-hero h1{font-size:28px;margin:0 0 7px;font-weight:800}.staff-hero p{margin:0;color:#dbeefa;font-size:15px}.staff-badge{background:#f5bf23;color:#17334a;padding:11px 16px;border-radius:7px;font-weight:800;text-transform:uppercase;letter-spacing:.4px}.staff-stat-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:14px;margin:18px 0}.staff-stat{background:#f6fbfe;border:1px solid #c1d9e7;border-radius:8px;padding:18px;box-shadow:0 3px 8px rgba(8,61,93,.09)}.staff-stat i{display:inline-flex;width:38px;height:38px;align-items:center;justify-content:center;border-radius:8px;background:#d9effa;color:#147fb1;font-size:18px}.staff-stat b{display:block;font-size:25px;margin:9px 0 2px}.staff-stat span{color:#52718a;font-weight:700;font-size:13px}.staff-columns{display:grid;grid-template-columns:1.3fr 1fr;gap:16px}.staff-panel{background:#f7fbfe;border:1px solid #c1d9e7;border-radius:8px;overflow:hidden;box-shadow:0 3px 8px rgba(8,61,93,.09);margin-bottom:16px}.staff-panel h2{font-size:15px;text-transform:uppercase;letter-spacing:.4px;margin:0;padding:15px 17px;background:#e4f1f8;border-bottom:1px solid #c1d9e7;color:#123a59;font-weight:800}.staff-list{padding:8px 14px}.staff-row{display:flex;align-items:center;gap:12px;padding:11px 4px;border-bottom:1px solid #dbe9f1}.staff-row:last-child{border:0}.staff-avatar{width:36px;height:36px;border-radius:50%;background:#0f6f9e;color:#fff;display:flex;align-items:center;justify-content:center;font-weight:800}.staff-row strong{display:block;font-size:14px}.staff-row small{color:#5d7890;font-weight:600}.staff-state{margin-left:auto;font-size:12px;font-weight:800;padding:4px 8px;border-radius:12px}.staff-state.online{background:#d9f4df;color:#177631}.staff-state.offline{background:#e8edf1;color:#657985}.staff-actions{padding:13px 16px;display:flex;gap:10px;flex-wrap:wrap}.staff-actions a{background:#147fb1;color:#fff!important;text-decoration:none!important;padding:9px 12px;border-radius:5px;font-size:13px;font-weight:800}.staff-actions a:hover{background:#0e638d}@media(max-width:800px){.staff-stat-grid{grid-template-columns:repeat(2,1fr)}.staff-columns{grid-template-columns:1fr}.staff-hero{display:block}.staff-badge{display:inline-block;margin-top:14px}} 
</style>

<main class="staff-admin">
    <section class="staff-hero">
        <div><h1><i class="fas fa-shield-alt"></i> Espace staff</h1><p>Bienvenue <?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?>. Suis l’activité de l’hôtel depuis le CMS.</p></div>
        <span class="staff-badge"><?php echo $roleName($UData['rank']); ?></span>
    </section>

    <section class="staff-stat-grid">
        <article class="staff-stat"><i class="fas fa-users"></i><b><?php echo number_format($totalUsers, 0, ',', ' '); ?></b><span>Citoyens inscrits</span></article>
        <article class="staff-stat"><i class="fas fa-signal"></i><b><?php echo number_format($onlineUsers, 0, ',', ' '); ?></b><span>En ligne</span></article>
        <article class="staff-stat"><i class="fas fa-home"></i><b><?php echo number_format($roomCount, 0, ',', ' '); ?></b><span>Appartements</span></article>
        <article class="staff-stat"><i class="fas fa-user-shield"></i><b><?php echo number_format($staffCount, 0, ',', ' '); ?></b><span>Membres du staff</span></article>
    </section>

    <section class="staff-columns">
        <div>
            <section class="staff-panel"><h2><i class="fas fa-user-shield"></i> Équipe staff</h2><div class="staff-list">
                <?php while($staffRows && ($staff = mysqli_fetch_assoc($staffRows))): ?>
                <div class="staff-row"><div class="staff-avatar"><?php echo strtoupper(substr($staff['username'], 0, 1)); ?></div><div><strong><?php echo htmlspecialchars($staff['username'], ENT_QUOTES, 'UTF-8'); ?></strong><small><?php echo $roleName($staff['rank']); ?></small></div><span class="staff-state <?php echo $staff['online'] ? 'online' : 'offline'; ?>"><?php echo $staff['online'] ? 'EN LIGNE' : 'HORS LIGNE'; ?></span></div>
                <?php endwhile; ?>
            </div></section>
            <section class="staff-panel"><h2><i class="fas fa-bolt"></i> Raccourcis</h2><div class="staff-actions"><a href="<?php echo URL; ?>/staff"><i class="fas fa-users"></i> Équipe</a><a href="<?php echo URL; ?>/online"><i class="fas fa-circle"></i> Joueurs en ligne</a><a href="<?php echo URL; ?>/search_users"><i class="fas fa-search"></i> Rechercher un joueur</a><a href="<?php echo URL; ?>/play"><i class="fas fa-gamepad"></i> Ouvrir le jeu</a></div></section>
        </div>
        <div>
            <section class="staff-panel"><h2><i class="fas fa-circle"></i> Connectés récemment</h2><div class="staff-list">
                <?php while($onlineRows && ($player = mysqli_fetch_assoc($onlineRows))): ?>
                <div class="staff-row"><div class="staff-avatar"><?php echo strtoupper(substr($player['username'], 0, 1)); ?></div><div><strong><?php echo htmlspecialchars($player['username'], ENT_QUOTES, 'UTF-8'); ?></strong><small><?php echo $player['rank'] >= 3 ? $roleName($player['rank']) : 'Joueur'; ?></small></div><span class="staff-state online">EN LIGNE</span></div>
                <?php endwhile; ?>
            </div></section>
            <section class="staff-panel"><h2><i class="fas fa-user-plus"></i> Dernières inscriptions</h2><div class="staff-list">
                <?php while($newRows && ($player = mysqli_fetch_assoc($newRows))): ?>
                <div class="staff-row"><div class="staff-avatar"><?php echo strtoupper(substr($player['username'], 0, 1)); ?></div><div><strong><?php echo htmlspecialchars($player['username'], ENT_QUOTES, 'UTF-8'); ?></strong><small>#<?php echo (int)$player['id']; ?></small></div><span class="staff-state <?php echo $player['online'] ? 'online' : 'offline'; ?>"><?php echo $player['online'] ? 'EN LIGNE' : 'HORS LIGNE'; ?></span></div>
                <?php endwhile; ?>
            </div></section>
        </div>
    </section>
</main>
<?php if((int)$UData['rank'] >= 5): ?>
<style>
/* Admin controls use the same dark glass surfaces as the CMS. */
.staff-admin,.admin-tools{color:#e5f3fc}.staff-hero{border-color:rgba(121,207,246,.28)!important;background:linear-gradient(120deg,rgba(8,54,87,.98),rgba(18,126,179,.84))!important;box-shadow:0 18px 38px rgba(0,0,0,.3)!important}.staff-badge{color:#10293d!important;background:linear-gradient(180deg,#ffd45f,#e5ab27)!important}.staff-stat,.staff-panel,.admin-tool{border-color:rgba(139,206,239,.2)!important;background:rgba(15,37,56,.9)!important;box-shadow:0 13px 27px rgba(0,0,0,.2)!important}.staff-stat b,.staff-stat span,.staff-row strong,.staff-row small,.admin-tool h3{color:#e8f6ff!important}.staff-stat i{color:#72d2fa!important;background:rgba(49,168,220,.15)!important}.staff-panel h2{color:#f3fbff!important;background:rgba(255,255,255,.05)!important;border-bottom-color:rgba(139,206,239,.18)!important}.staff-row{border-bottom-color:rgba(139,206,239,.14)!important}.staff-avatar{background:linear-gradient(180deg,#269fd5,#126eaa)!important}.staff-state.online{background:rgba(49,194,101,.17)!important;color:#92f1af!important}.staff-state.offline{background:rgba(181,204,219,.12)!important;color:#b9d1e1!important}.admin-tools-grid{padding:17px}.admin-tool{padding:20px!important;border-radius:12px!important}.admin-tool input,.admin-tool textarea{color:#edf8ff!important;background:rgba(2,15,26,.58)!important;border-color:rgba(119,201,239,.34)!important;box-shadow:none!important}.admin-tool input::placeholder,.admin-tool textarea::placeholder{color:#86a8bf!important}.admin-tool input:focus,.admin-tool textarea:focus{border-color:#57c7f2!important;box-shadow:0 0 0 3px rgba(87,199,242,.14)!important}.admin-tool label,.admin-tool p{color:#c8dce9!important}.admin-tool button{border:1px solid rgba(127,220,251,.45)!important;border-radius:8px!important;background:linear-gradient(180deg,#299fd3,#1279b5)!important;box-shadow:0 3px 0 rgba(0,0,0,.25)!important}.admin-tool .danger{border-color:rgba(255,163,172,.48)!important;background:linear-gradient(180deg,#df596a,#b92d45)!important}.admin-notice{color:#bdf6ca!important;background:rgba(35,151,78,.2)!important;border-color:rgba(105,232,141,.38)!important}.maintenance-tool{grid-column:span 2;background:linear-gradient(120deg,rgba(15,54,79,.96),rgba(21,83,105,.86))!important}.maintenance-state{display:flex;align-items:center;gap:10px;margin:0 0 13px!important;padding:12px 13px;border:1px solid rgba(144,210,242,.18);border-radius:9px;background:rgba(1,13,24,.35)}.maintenance-state b{padding:4px 9px;border-radius:999px;text-transform:uppercase;font-size:11px}.maintenance-state .active{color:#ffd6a2;background:rgba(221,144,37,.21)}.maintenance-state .inactive{color:#a9f4bd;background:rgba(52,183,92,.19)}.maintenance-actions{display:flex;gap:12px;flex-wrap:wrap;padding-bottom:8px}.maintenance-actions button{min-width:220px;min-height:44px;margin:0!important}.admin-tools{padding-bottom:132px!important}@media(max-width:800px){.maintenance-tool{grid-column:auto}.maintenance-actions button{width:100%}.admin-tools-grid{padding:12px}}
</style>
<style>.admin-tools{max-width:1220px;margin:0 auto 48px}.admin-tools-grid{display:grid;grid-template-columns:repeat(2,1fr);gap:16px}.admin-tool{background:#f7fbfe;border:1px solid #c1d9e7;border-radius:8px;padding:18px}.admin-tool h3{font-size:16px;margin:0 0 12px;color:#123a59}.admin-tool input,.admin-tool textarea{width:100%;margin:5px 0;padding:9px;border:1px solid #a9c7d9;border-radius:5px}.admin-tool button{border:0;border-radius:5px;background:#147fb1;color:#fff;font-weight:800;padding:9px 13px;margin-top:5px}.admin-tool .danger{background:#b83643}.admin-notice{max-width:1220px;margin:14px auto;padding:12px 15px;background:#d9f4df;border:1px solid #91d5a0;color:#176c30;border-radius:6px;font-weight:700}@media(max-width:800px){.admin-tools-grid{grid-template-columns:1fr}}</style>
<style>
.admin-shell{overflow:visible!important}.admin-section-grid{display:grid;grid-template-columns:minmax(280px,.75fr) minmax(0,1.25fr);gap:16px;padding:18px}.admin-category{min-width:0;border:1px solid rgba(136,207,242,.17);border-radius:12px;background:rgba(3,20,34,.3);overflow:hidden}.admin-category-wide{grid-column:span 2}.admin-category-head{display:flex;align-items:center;gap:12px;padding:14px 16px;border-bottom:1px solid rgba(136,207,242,.15);background:rgba(255,255,255,.035)}.admin-category-head>i{display:grid;place-items:center;width:34px;height:34px;border-radius:9px;color:#74d5f7;background:rgba(36,159,213,.16)}.admin-category-head b,.admin-category-head small{display:block}.admin-category-head b{color:#f2faff;font-size:14px}.admin-category-head small{margin-top:2px;color:#94b6ca;font-size:12px}.admin-category>.admin-tool{margin:14px;background:rgba(12,34,52,.82)!important}.admin-category .admin-tools-grid{grid-template-columns:repeat(3,minmax(0,1fr));padding:14px;gap:14px}.admin-category .admin-tools-grid .admin-tool{margin:0;min-width:0}.admin-category .admin-tool h3{margin-bottom:13px!important;font-size:14px!important}.admin-category .admin-tool button{width:100%;margin-top:10px!important}.admin-button-row{display:flex;gap:8px;margin-top:10px}.admin-button-row button{width:auto!important;flex:1;margin:0!important}.maintenance-tool{margin:14px!important}.maintenance-actions{padding:0!important}.maintenance-actions button{width:auto!important;min-width:230px}.admin-flash{position:fixed;z-index:10000;top:22px;right:24px;display:flex;align-items:center;gap:10px;max-width:min(420px,calc(100vw - 32px));padding:14px 17px!important;border-radius:11px!important;box-shadow:0 18px 42px rgba(0,0,0,.38)!important;animation:admin-flash-in .25s ease-out}.admin-flash i{font-size:18px}@keyframes admin-flash-in{from{opacity:0;transform:translateY(-12px)}to{opacity:1;transform:translateY(0)}}@media(max-width:900px){.admin-section-grid{grid-template-columns:1fr}.admin-category-wide{grid-column:auto}.admin-category .admin-tools-grid{grid-template-columns:1fr}.maintenance-actions button{width:100%!important;min-width:0}}
</style>
<?php if(!empty($AdminNotice)): ?><div class="admin-notice admin-flash" role="status"><i class="fas fa-check-circle"></i><span><?php echo htmlspecialchars($AdminNotice, ENT_QUOTES, 'UTF-8'); ?></span></div><?php endif; ?>
<section class="admin-tools">
    <div class="staff-panel admin-shell">
        <h2><i class="fas fa-tools"></i> Centre de gestion</h2>
        <div class="admin-section-grid">
            <section class="admin-category">
                <div class="admin-category-head"><i class="fas fa-shopping-bag"></i><div><b>Catalogue</b><small>Offres et disponibilit&eacute;</small></div></div>
                <form class="admin-tool" method="post"><h3>Modifier une offre</h3><input type="hidden" name="csrf" value="<?php echo htmlspecialchars($_SESSION['cms_admin_csrf'], ENT_QUOTES, 'UTF-8'); ?>"><input type="hidden" name="action" value="catalogue"><input name="offer_id" type="number" min="1" placeholder="ID de l'offre"><input name="price" type="number" min="0" placeholder="Prix en cr&eacute;dits"><label><input name="active" type="checkbox" checked style="width:auto"> Offre active</label><button>Enregistrer l'offre</button></form>
            </section>
            <section class="admin-category admin-category-wide">
                <div class="admin-category-head"><i class="fas fa-user-cog"></i><div><b>Gestion des joueurs</b><small>Badges, apparence et sanctions</small></div></div>
                <div class="admin-tools-grid">
                    <form class="admin-tool" method="post"><h3>Attribuer un badge</h3><input type="hidden" name="csrf" value="<?php echo htmlspecialchars($_SESSION['cms_admin_csrf'], ENT_QUOTES, 'UTF-8'); ?>"><input type="hidden" name="action" value="badge"><input name="username" placeholder="Pseudo du joueur"><input name="badge" placeholder="Code badge, ex. VIP1"><input name="slot" type="number" min="0" max="4" value="0"><button>Attribuer le badge</button></form>
                    <form class="admin-tool" method="post"><h3>Modifier le look</h3><input type="hidden" name="csrf" value="<?php echo htmlspecialchars($_SESSION['cms_admin_csrf'], ENT_QUOTES, 'UTF-8'); ?>"><input type="hidden" name="action" value="look"><input name="username" placeholder="Pseudo du joueur"><textarea name="look" placeholder="Figure Habbo, ex. hd-180-1.ch-..." rows="3"></textarea><button>Mettre &agrave; jour le look</button></form>
                    <form class="admin-tool" method="post"><h3>Bannissement</h3><input type="hidden" name="csrf" value="<?php echo htmlspecialchars($_SESSION['cms_admin_csrf'], ENT_QUOTES, 'UTF-8'); ?>"><input name="username" placeholder="Pseudo du joueur"><input name="reason" placeholder="Motif"><input name="days" type="number" min="1" value="1"><div class="admin-button-row"><button name="action" value="ban" class="danger">Bannir</button><button name="action" value="unban">D&eacute;bannir</button></div></form>
                </div>
            </section>
            <section class="admin-category admin-category-wide">
                <div class="admin-category-head"><i class="fas fa-server"></i><div><b>H&ocirc;tel</b><small>Disponibilit&eacute; du CMS pour les joueurs</small></div></div>
                <form class="admin-tool maintenance-tool" method="post"><h3>Maintenance CMS</h3><input type="hidden" name="csrf" value="<?php echo htmlspecialchars($_SESSION['cms_admin_csrf'], ENT_QUOTES, 'UTF-8'); ?>"><p class="maintenance-state">Etat actuel : <b class="<?php echo Config::$_MANT ? 'active' : 'inactive'; ?>"><?php echo Config::$_MANT ? 'active' : 'desactivee'; ?></b></p><div class="maintenance-actions"><button type="submit" name="action" value="maintenance_on" class="danger">Activer la maintenance</button><button type="submit" name="action" value="maintenance_off">D&eacute;sactiver la maintenance</button></div></form>
            </section>
        </div>
    </div>
</section>
<?php endif; ?>
