<?php
$search = trim(isset($_GET['q']) ? $_GET['q'] : '');
$Gangs = $GangMG->GetGangs($search);
function gangInitial($name) { return htmlspecialchars(mb_strtoupper(mb_substr($name, 0, 1, 'UTF-8'), 'UTF-8'), ENT_QUOTES, 'UTF-8'); }
?>
<div class="content"><div class="container">
    <div class="gang-page-heading">
        <div><small>PARADISE ROLEPLAY</small><h1>Gangs</h1><p>Découvre les organisations criminelles actives en ville.</p></div>
        <a class="button blue cms-section-button" href="<?php echo Config::$URL; ?>/gangs/leaderboard"><i class="fas fa-trophy"></i> Classements</a>
    </div>
    <form class="gang-search" method="get" action="<?php echo Config::$URL; ?>/gangs">
        <input class="form-control" name="q" value="<?php echo htmlspecialchars($search, ENT_QUOTES, 'UTF-8'); ?>" placeholder="Rechercher un gang...">
        <button class="button blue cms-section-button" type="submit"><i class="fas fa-search"></i> Rechercher</button>
    </form>

    <?php if (mysqli_num_rows($Gangs)): ?>
        <div class="corporation-index-grid gang-index-grid">
        <?php while ($gang = mysqli_fetch_assoc($Gangs)): ?>
            <div class="corp"><div class="content-box">
                <div class="title"><?php echo htmlspecialchars($gang['name'], ENT_QUOTES, 'UTF-8'); ?><span class="title-small float-right"><small><?php echo (int)$gang['member_count']; ?> membre(s)</small></span></div>
                <div class="box-content gang-card-body">
                    <span class="gang-letter-badge"><?php echo gangInitial($gang['name']); ?></span>
                    <div class="gang-card-info"><b>Chef :</b> <?php echo htmlspecialchars($GangMG->GetGangLeader($gang['id']), ENT_QUOTES, 'UTF-8'); ?><br><span>Capital : <?php echo number_format((int)$gang['bank'], 0, ',', ' '); ?> $</span></div>
                    <a class="button blue cms-section-button card-action" href="<?php echo Config::$URL; ?>/gang/<?php echo (int)$gang['id']; ?>">Voir la fiche <i class="fas fa-chevron-right"></i></a>
                </div>
            </div></div>
        <?php endwhile; ?>
        </div>
    <?php else: ?>
        <div class="content-box"><div class="title">Aucun résultat</div><div class="box-content">Aucun gang ne correspond à ta recherche.</div></div>
    <?php endif; ?>
</div></div>
