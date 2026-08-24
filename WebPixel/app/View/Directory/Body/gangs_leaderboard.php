<?php
$metrics = array(
    'bank'=>array('Plus riches','Capital',' $'),
    'gang_kills'=>array('Plus d’éliminations','Éliminations',''),
    'gang_deaths'=>array('Plus de décès','Décès',''),
    'gang_cop_kills'=>array('Forces de l’ordre vaincues','Éliminations',''),
    'gang_turfs_taken'=>array('Territoires capturés','Captures',''),
    'gang_turfs_defend'=>array('Territoires défendus','Défenses',''),
    'gang_farm_cocaine'=>array('Production de cocaïne','Grammes',' g'),
    'gang_farm_weed'=>array('Production de cannabis','Grammes',' g'),
    'gang_farm_medicines'=>array('Médicaments','Unités',''),
    'gang_fab_guns'=>array('Armes fabriquées','Armes',''),
    'gang_heists'=>array('Braquages','Braquages','')
);
?>
<div class="content"><div class="container">
    <div class="gang-page-heading"><div><small>PARADISE ROLEPLAY</small><h1>Classements des gangs</h1><p>Les statistiques enregistrées par le nouvel émulateur.</p></div><a class="button blue cms-section-button" href="<?php echo Config::$URL; ?>/gangs"><i class="fas fa-users"></i> Tous les gangs</a></div>
    <div class="leaderboard-grid gang-leaderboard-grid">
    <?php foreach ($metrics as $column=>$meta): $rows=$GangMG->GetLeaderBoard('groups',$column); ?>
        <div class="content-box blue leaderboards mb-0"><div class="title"><?php echo $meta[0]; ?></div><div class="box-content">
        <?php if ($rows && mysqli_num_rows($rows)): while($gang=mysqli_fetch_assoc($rows)): ?>
            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo (int)$gang['id']; ?>">
                <span class="gang-letter-badge small"><?php echo htmlspecialchars(mb_strtoupper(mb_substr($gang['name'],0,1,'UTF-8'),'UTF-8'),ENT_QUOTES,'UTF-8'); ?></span>
                <div class="leaderboard-user-info mr-auto"><div class="leaderboard-user-name font-weight-bold"><?php echo htmlspecialchars($gang['name'],ENT_QUOTES,'UTF-8'); ?></div><div class="leaderboard-user-stat"><?php echo $meta[1]; ?> : <?php echo number_format((int)$gang[$column],0,',',' '); ?><?php echo $meta[2]; ?></div></div>
            </a>
        <?php endwhile; else: ?>Aucun gang dans cette catégorie.<?php endif; ?>
        </div></div>
    <?php endforeach; ?>
    </div>
</div></div>
