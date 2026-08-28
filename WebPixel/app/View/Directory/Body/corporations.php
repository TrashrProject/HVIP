<div class="content"><div class="container">
    <div class="profession-page-heading">
        <div><small>PARADISE ROLEPLAY</small><h1>Metiers</h1><p>Retrouve les entreprises et services publics de la ville.</p></div>
        <span class="profession-heading-icon"><i class="fas fa-briefcase"></i></span>
    </div>
    <div class="corporation-index-grid profession-index-grid">
    <?php
    $Jobs = $DB->Query("SELECT j.id,j.name,j.display_name,j.description,
        COUNT(DISTINCT ur.user_id) AS employee_count,
        COUNT(DISTINCT jr.id) AS rank_count
        FROM jobs j
        LEFT JOIN users_roleplay ur ON ur.job_id=j.id
        LEFT JOIN job_ranks jr ON jr.job_id=j.id AND jr.active=1
        WHERE j.active=1
        GROUP BY j.id,j.name,j.display_name,j.description
        ORDER BY CASE WHEN j.name='unemployed' THEN 1 ELSE 0 END,j.display_name ASC");
    if(mysqli_num_rows($Jobs)): while($Job=mysqli_fetch_assoc($Jobs)):
        $name = $Job['display_name'] ?: $Job['name'];
    ?>
        <div class="corp profession-card"><div class="content-box">
            <div class="title"><?php echo htmlspecialchars($name,ENT_QUOTES,'UTF-8'); ?><span class="title-small float-right"><small><?php echo (int)$Job['employee_count']; ?> employe(s)</small></span></div>
            <div class="box-content profession-card-body">
                <span class="corporation-letter-badge"><?php echo htmlspecialchars(mb_strtoupper(mb_substr($name,0,1,'UTF-8'),'UTF-8'),ENT_QUOTES,'UTF-8'); ?></span>
                <div class="profession-card-copy">
                    <b><?php echo htmlspecialchars($name,ENT_QUOTES,'UTF-8'); ?></b>
                    <span><?php echo htmlspecialchars($Job['description'] ?: 'Metier disponible en ville',ENT_QUOTES,'UTF-8'); ?></span>
                    <small><?php echo (int)$Job['rank_count']; ?> grade(s) disponible(s)</small>
                </div>
            </div>
        </div></div>
    <?php endwhile; else: ?>
        <div class="content-box"><div class="title">Aucun metier</div><div class="box-content">Aucun metier n'est disponible actuellement.</div></div>
    <?php endif; ?>
    </div>
</div></div>
