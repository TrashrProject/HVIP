<?php
$boards = array(
    'wealth' => array('Fortunes','fas fa-coins','fortune totale',' $'),
    'online_time' => array('Temps en ligne','fas fa-clock','temps de jeu','time'),
    'room_visits' => array('Explorateurs','fas fa-door-open','appartements visités',''),
    'achievement_score' => array('Score de succès','fas fa-trophy','points de succès',' pts'),
    'respect' => array('Citoyens respectés','fas fa-heart','respects reçus',''),
    'experience' => array('Expérience RP','fas fa-star','points d’expérience',' XP'),
    'strength' => array('Force','fas fa-dumbbell','points de force',''),
    'knowledge' => array('Connaissances','fas fa-brain','points de connaissance',''),
    'damage_dealt' => array('Dégâts infligés','fas fa-crosshairs','points de dégâts',''),
    'knockouts' => array('Combattants','fas fa-fist-raised','adversaires neutralisés',''),
    'robberies' => array('Braquages','fas fa-mask','braquages réussis',''),
    'escapes' => array('Évasions','fas fa-running','évasions réussies','')
);
function leaderboardValue($value,$format) {
    $value=(int)$value;
    if($format==='time') {
        $hours=floor($value/3600); $minutes=floor(($value%3600)/60);
        return $hours.' h '.str_pad($minutes,2,'0',STR_PAD_LEFT).' min';
    }
    return number_format($value,0,',',' ').$format;
}
?>
<div class="content"><div class="container">
    <div class="ranking-page-heading">
        <div><small>PARADISE ROLEPLAY</small><h1>Classements des citoyens</h1><p>Données synchronisées avec les statistiques actuelles du jeu.</p></div>
        <span class="ranking-heading-icon"><i class="fas fa-trophy"></i></span>
    </div>
    <div class="leaderboard-grid ranking-grid">
    <?php foreach($boards as $metric=>$meta): $rows=$UserMG->GetGameLeaderboard($metric); ?>
        <section class="content-box blue leaderboards ranking-board mb-0">
            <div class="title"><i class="<?php echo $meta[1]; ?>"></i><?php echo $meta[0]; ?></div>
            <div class="box-content">
            <?php if($rows && mysqli_num_rows($rows)): $position=0; while($row=mysqli_fetch_assoc($rows)): $position++; ?>
                <a class="leaderboard-user ranking-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo (int)$row['id']; ?>">
                    <span class="ranking-position pos-<?php echo $position; ?>"><?php echo $position; ?></span>
                    <div class="leaderboard-pixel <?php echo $row['online']=='1'?'':'grayscale'; ?>"><img src="<?php echo URL; ?>/avatar.php?figure=<?php echo rawurlencode($row['look']); ?>&amp;head_direction=3&amp;direction=3&amp;gesture=sml"></div>
                    <div class="leaderboard-user-info mr-auto"><div class="leaderboard-user-name font-weight-bold"><?php echo htmlspecialchars($row['username'],ENT_QUOTES,'UTF-8'); ?><?php if($row['online']=='1'): ?><span class="ranking-online">En ligne</span><?php endif; ?></div><div class="leaderboard-user-stat"><?php echo htmlspecialchars(leaderboardValue($row['value'],$meta[3]),ENT_QUOTES,'UTF-8'); ?> · <?php echo $meta[2]; ?></div></div>
                </a>
            <?php endwhile; else: ?><div class="ranking-empty">Aucune statistique disponible.</div><?php endif; ?>
            </div>
        </section>
    <?php endforeach; ?>
    </div>
</div></div>
