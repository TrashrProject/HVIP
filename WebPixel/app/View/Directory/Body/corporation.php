<?php
$roster=array();
$rosterRows=$Corp->GetCorpRoster();
while($row=mysqli_fetch_assoc($rosterRows)) {
    $rank=(int)$row['rank'];
    if(!isset($roster[$rank])) $roster[$rank]=array('name'=>$row['rank_name'],'pay'=>(int)$row['pay'],'members'=>array());
    if($row['user_id']!==null && $row['username']!==null) $roster[$rank]['members'][]=$row;
}
$corpName=htmlspecialchars($Corp->CorpData['name'],ENT_QUOTES,'UTF-8');
?>
<div class="content"><div class="container">
    <div class="profession-detail-heading"><a class="cms-back-link" href="<?php echo Config::$URL; ?>/corporations"><i class="fas fa-arrow-left"></i> Retour aux métiers</a></div>
    <div class="row profession-detail-layout">
        <aside class="col-3"><div class="content-box profession-summary">
            <div class="title text-center"><?php echo $corpName; ?></div>
            <div class="box-content text-center">
                <div class="corporation-badge-stage"><span class="corporation-letter-badge large"><?php echo htmlspecialchars(mb_strtoupper(mb_substr($Corp->CorpData['name'],0,1,'UTF-8'),'UTF-8'),ENT_QUOTES,'UTF-8'); ?></span></div>
                <ul class="list-group list-group-flush">
                    <li class="list-group-item d-flex justify-content-between align-items-center"><span>Employés</span><b><?php echo $Corp->GetCorpEmployeesCount(); ?></b></li>
                    <li class="list-group-item d-flex justify-content-between align-items-center"><span>Valeur totale</span><b><?php echo number_format((int)$Corp->CorpData['bank'],0,',',' '); ?> $</b></li>
                </ul>
            </div>
        </div></aside>
        <main class="col">
        <?php foreach($roster as $rank=>$rankData): ?>
            <section class="content-box corporation-rank-box">
                <div class="title"><?php echo htmlspecialchars($rankData['name'],ENT_QUOTES,'UTF-8'); ?><span class="title-small float-right"><small>Salaire : <?php echo number_format($rankData['pay'],0,',',' '); ?> $</small></span></div>
                <div class="box-content">
                <?php if(count($rankData['members'])): ?><div class="corporation-employee-grid">
                    <?php foreach($rankData['members'] as $employee): ?>
                    <a href="<?php echo Config::$URL; ?>/profile/<?php echo (int)$employee['user_id']; ?>" class="employee d-flex justify-content-center align-items-center no-link-styling">
                        <div class="employee-pixel <?php echo $employee['online']=='1'?'':'grayscale'; ?>"><img src="<?php echo URL; ?>/avatar.php?figure=<?php echo rawurlencode($employee['look']); ?>&amp;head_direction=3&amp;direction=3&amp;gesture=sml" alt=""></div>
                        <div class="profession-employee-name"><b><?php echo htmlspecialchars($employee['username'],ENT_QUOTES,'UTF-8'); ?></b><small><?php echo htmlspecialchars($rankData['name'],ENT_QUOTES,'UTF-8'); ?></small></div>
                        <span class="corp-online-dot <?php echo $employee['online']=='1'?'on':''; ?>"></span>
                    </a>
                    <?php endforeach; ?>
                </div><?php else: ?><div class="profession-empty"><i class="fas fa-user-clock"></i> Aucun citoyen n’occupe ce poste.</div><?php endif; ?>
                </div>
            </section>
        <?php endforeach; ?>
        </main>
    </div>
</div></div>
