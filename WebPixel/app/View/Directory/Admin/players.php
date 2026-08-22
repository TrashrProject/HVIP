<?php
$q = trim((string)($_GET['q'] ?? ''));
$filter = strtolower((string)($_GET['filter'] ?? 'all'));
if (!in_array($filter, array('all','online','staff','citizens'), true)) $filter = 'all';
$per = (int)($_GET['per'] ?? 25);
if (!in_array($per, array(25,50,100), true)) $per = 25;
$page = max(1, (int)($_GET['p'] ?? 1));
$sort = strtolower((string)($_GET['sort'] ?? 'id'));
$direction = strtolower((string)($_GET['dir'] ?? 'desc')) === 'asc' ? 'ASC' : 'DESC';
$sortMap = array('id'=>'u.id','username'=>'u.username','rank'=>'u.rank','cash'=>'u.credits','bank'=>'bank_balance','created'=>'u.account_created');
if (!isset($sortMap[$sort])) $sort = 'id';

$hasCharacter = $PCC->tableExists('rp_characters');
$hasPhone = $PCC->tableExists('rp_phones');
$hasBank = $PCC->tableExists('play_stats');
$hasLastOnline = $PCC->columnExists('users', 'last_online');

$joins = '';
$cols = 'u.id,u.username,u.look,u.rank,u.online,u.credits,u.account_created';
if ($hasLastOnline) $cols .= ',u.last_online'; else $cols .= ',NULL last_online';
if ($hasCharacter) { $joins .= ' LEFT JOIN rp_characters c ON c.user_id=u.id'; $cols .= ',c.first_name,c.last_name,c.citizen_id'; }
else $cols .= ',NULL first_name,NULL last_name,NULL citizen_id';
if ($hasPhone) { $joins .= ' LEFT JOIN rp_phones ph ON ph.user_id=u.id'; $cols .= ',ph.phone_number,ph.status phone_status'; }
else $cols .= ',NULL phone_number,NULL phone_status';
if ($hasBank) { $joins .= ' LEFT JOIN play_stats ps ON ps.id=u.id'; $cols .= ',COALESCE(ps.bank,0) bank_balance'; }
else $cols .= ',0 bank_balance';

$where = array('1=1');
$types = '';
$params = array();
if ($filter === 'online') $where[] = "u.online='1'";
elseif ($filter === 'staff') $where[] = 'u.rank>=3';
elseif ($filter === 'citizens') $where[] = 'u.rank<3';
if ($q !== '') {
    $like = '%' . mb_substr($q,0,64) . '%';
    $search = array('u.username LIKE ?','CAST(u.id AS CHAR)=?');
    $types .= 'ss'; $params[] = $like; $params[] = $q;
    if ($hasCharacter) { $search[] = 'c.citizen_id LIKE ?'; $search[] = "CONCAT(COALESCE(c.first_name,''),' ',COALESCE(c.last_name,'')) LIKE ?"; $types .= 'ss'; $params[] = $like; $params[] = $like; }
    if ($hasPhone) { $search[] = 'ph.phone_number LIKE ?'; $types .= 's'; $params[] = $like; }
    $where[] = '(' . implode(' OR ', $search) . ')';
}

$countRow = $DB->PreparedRow('SELECT COUNT(DISTINCT u.id) total FROM users u' . $joins . ' WHERE ' . implode(' AND ', $where), $types, $params);
$total = $countRow ? (int)$countRow['total'] : 0;
$pages = max(1, (int)ceil($total / $per));
if ($page > $pages) $page = $pages;
$offset = ($page - 1) * $per;
$orderColumn = $sortMap[$sort];
$rows = $DB->PreparedAll('SELECT ' . $cols . ' FROM users u' . $joins . ' WHERE ' . implode(' AND ', $where) . ' GROUP BY u.id ORDER BY ' . $orderColumn . ' ' . $direction . ' LIMIT ' . (int)$per . ' OFFSET ' . (int)$offset, $types, $params);

$link = function(array $changes = array()) use ($q,$filter,$per,$page,$sort,$direction) {
    $base = array('page'=>'players','q'=>$q,'filter'=>$filter,'per'=>$per,'p'=>$page,'sort'=>$sort,'dir'=>strtolower($direction));
    foreach($changes as $k=>$v) $base[$k]=$v;
    return URL . '/admin.php?' . http_build_query($base);
};
$sortLink = function($key) use ($link,$sort,$direction) {
    $next = ($sort === $key && $direction === 'ASC') ? 'desc' : 'asc';
    return $link(array('sort'=>$key,'dir'=>$next,'p'=>1));
};
?>
<section class="pcc-panel">
    <form class="pcc-filterbar" method="get">
        <input type="hidden" name="page" value="players">
        <div class="pcc-search-field"><i class="fas fa-search"></i><input type="search" name="q" value="<?php echo $h($q); ?>" placeholder="Username, nom RP, Citizen ID, téléphone…"></div>
        <div class="pcc-tabs">
            <?php foreach(array('all'=>'Tous','online'=>'En ligne','staff'=>'Staff','citizens'=>'Citoyens') as $key=>$label): ?><a class="<?php echo $filter===$key?'is-active':''; ?>" href="<?php echo $h($link(array('filter'=>$key,'p'=>1))); ?>"><?php echo $h($label); ?></a><?php endforeach; ?>
        </div>
        <select name="per" class="pcc-select-compact" onchange="this.form.submit()"><option value="25" <?php echo $per===25?'selected':''; ?>>25 / page</option><option value="50" <?php echo $per===50?'selected':''; ?>>50 / page</option><option value="100" <?php echo $per===100?'selected':''; ?>>100 / page</option></select>
        <button class="pcc-button secondary" type="submit">Rechercher</button>
        <span class="pcc-filter-count"><?php echo $number($total); ?> résultat(s)</span>
    </form>

    <div class="pcc-table-wrap">
        <table class="pcc-table pcc-table-dense">
            <thead><tr>
                <th>Avatar</th>
                <th><a href="<?php echo $h($sortLink('username')); ?>">Username</a></th>
                <th>Identité RP</th>
                <th>Citizen ID</th>
                <th><a href="<?php echo $h($sortLink('rank')); ?>">Rôle</a></th>
                <th><a href="<?php echo $h($sortLink('cash')); ?>">Cash</a></th>
                <th><a href="<?php echo $h($sortLink('bank')); ?>">Banque</a></th>
                <th>Téléphone</th>
                <th>Statut</th>
                <th>Dernière connexion</th>
                <th></th>
            </tr></thead>
            <tbody>
            <?php if(!$rows): ?><tr><td colspan="11"><div class="pcc-empty compact"><strong>Aucun joueur trouvé</strong><span>Modifie les filtres ou la recherche.</span></div></td></tr><?php endif; ?>
            <?php foreach($rows as $player): $rpName=trim((string)$player['first_name'].' '.(string)$player['last_name']); ?>
                <tr>
                    <td><img class="pcc-table-avatar" src="<?php echo $h($PCC->avatarUrl($player['look'],'s')); ?>" alt=""></td>
                    <td><a class="pcc-user-link" href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$player['id']; ?>"><?php echo $h($player['username']); ?></a><small class="pcc-subline">#<?php echo (int)$player['id']; ?></small></td>
                    <td><?php echo $rpName !== '' ? $h($rpName) : '<span class="pcc-muted">Non créé</span>'; ?></td>
                    <td><code><?php echo $player['citizen_id'] ? $h($player['citizen_id']) : '—'; ?></code></td>
                    <td><span class="pcc-badge"><?php echo $h($PCC->roleName($player['rank'])); ?></span></td>
                    <td><?php echo $number($player['credits']); ?></td>
                    <td><?php echo $number($player['bank_balance']); ?></td>
                    <td><?php echo $player['phone_number'] ? $h($player['phone_number']) : '—'; ?></td>
                    <td><span class="pcc-badge <?php echo (int)$player['online']===1?'success':''; ?>"><?php echo (int)$player['online']===1?'EN LIGNE':'HORS LIGNE'; ?></span></td>
                    <td><?php echo $player['last_online'] ? $h($PCC->formatDate($player['last_online'])) : '—'; ?></td>
                    <td><a class="pcc-button small secondary" href="<?php echo URL; ?>/admin.php?page=player&id=<?php echo (int)$player['id']; ?>">Ouvrir</a></td>
                </tr>
            <?php endforeach; ?>
            </tbody>
        </table>
    </div>

    <?php if($pages>1): ?><div class="pcc-pagination">
        <?php if($page>1): ?><a href="<?php echo $h($link(array('p'=>$page-1))); ?>">‹</a><?php endif; ?>
        <?php for($i=max(1,$page-2);$i<=min($pages,$page+2);$i++): ?><a class="<?php echo $i===$page?'is-active':''; ?>" href="<?php echo $h($link(array('p'=>$i))); ?>"><?php echo $i; ?></a><?php endfor; ?>
        <?php if($page<$pages): ?><a href="<?php echo $h($link(array('p'=>$page+1))); ?>">›</a><?php endif; ?>
    </div><?php endif; ?>
</section>
