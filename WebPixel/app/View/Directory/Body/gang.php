<?php
function gangValue($data, $key) { return isset($data[$key]) ? (int)$data[$key] : 0; }
$gangName = htmlspecialchars($Gang->Data['name'], ENT_QUOTES, 'UTF-8');
$initial = htmlspecialchars(mb_strtoupper(mb_substr($Gang->Data['name'], 0, 1, 'UTF-8'), 'UTF-8'), ENT_QUOTES, 'UTF-8');
?>
<div class="content"><div class="container"><div class="row">
    <div class="col-3">
        <div class="content-box"><div class="title text-center"><?php echo $gangName; ?></div><div class="box-content text-center">
            <div class="gang-badge-stage"><span class="gang-letter-badge large"><?php echo $initial; ?></span></div>
            <ul class="list-group list-group-flush gang-info-list">
                <li class="list-group-item d-flex justify-content-between"><span>Chef</span><b><?php echo htmlspecialchars($Gang->GangOwner['username'], ENT_QUOTES, 'UTF-8'); ?></b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Membres</span><b><?php echo (int)$Gang->MemberCount; ?></b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Capital</span><b><?php echo number_format(gangValue($Gang->Data,'bank'),0,',',' '); ?> $</b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Territoires</span><b><?php echo (int)$Gang->TurfCount; ?></b></li>
            </ul>
        </div></div>
        <div class="content-box"><div class="title">Statistiques</div><div class="box-content">
            <ul class="list-group list-group-flush gang-info-list">
                <li class="list-group-item d-flex justify-content-between"><span>Éliminations</span><b><?php echo number_format(gangValue($Gang->Data,'gang_kills')); ?></b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Décès</span><b><?php echo number_format(gangValue($Gang->Data,'gang_deaths')); ?></b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Territoires capturés</span><b><?php echo number_format(gangValue($Gang->Data,'gang_turfs_taken')); ?></b></li>
                <li class="list-group-item d-flex justify-content-between"><span>Territoires défendus</span><b><?php echo number_format(gangValue($Gang->Data,'gang_turfs_defend')); ?></b></li>
            </ul>
        </div></div>
    </div>
    <div class="col-9">
        <?php $ranks = $Gang->GetRankList(); if (mysqli_num_rows($ranks)): while ($rank = mysqli_fetch_assoc($ranks)): $members = $Gang->GetMembersForRank($rank['rank']); ?>
            <div class="content-box corporation-rank-box"><div class="title"><?php echo htmlspecialchars($rank['name'],ENT_QUOTES,'UTF-8'); ?><span class="title-small float-right"><small>Niveau <?php echo (int)$rank['rank']; ?></small></span></div><div class="box-content">
                <?php if (mysqli_num_rows($members)): ?><div class="gang-member-grid">
                    <?php while ($member=mysqli_fetch_assoc($members)): ?><a class="gang-member d-flex justify-content-center align-items-center no-link-styling" href="<?php echo Config::$URL; ?>/profile/<?php echo (int)$member['id']; ?>">
                        <div class="gang-member-pixel <?php echo $member['online']?'':'grayscale'; ?>"><img src="<?php echo URL; ?>/avatar.php?figure=<?php echo rawurlencode($member['look']); ?>&amp;head_direction=3&amp;direction=3&amp;gesture=sml"></div>
                        <b><?php echo htmlspecialchars($member['username'],ENT_QUOTES,'UTF-8'); ?></b><span class="corp-online-dot <?php echo $member['online']?'on':''; ?>"></span>
                    </a><?php endwhile; ?>
                </div><?php else: ?><div class="gang-empty-rank">Aucun citoyen n’occupe ce rang.</div><?php endif; ?>
            </div></div>
        <?php endwhile; else: ?>
            <div class="content-box"><div class="title">Membres</div><div class="box-content">Aucun rang n’est configuré pour ce gang.</div></div>
        <?php endif; ?>
    </div>
</div></div></div>
