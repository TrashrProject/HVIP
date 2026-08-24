<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

?>
<div class="navigation">
    <div class="container d-flex justify-content-center align-items-center">
        <a href="/" class="pixel-wrapper">
            <img class="pixel" src="<?php echo URL; ?>/avatar.php?figure=<?php echo rawurlencode($UData['look']); ?>&amp;size=l&amp;direction=3&amp;head_direction=3&amp;gesture=sml&amp;v=<?php echo rawurlencode($UData['look']); ?>" alt="<?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?>">
        </a>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-user"></i>
                <span class="nav-button-text">Mon compte</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/me" >Accueil</a>
                <a href="<?php echo URL; ?>/profile/<?php echo $UData['username']; ?>">Mon profil</a>
                <a href="<?php echo URL; ?>/account">Param&egrave;tres du compte</a>
                <a href="<?php echo URL; ?>/logout">D&eacute;connexion</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-users"></i>
                <span class="nav-button-text">Communauté</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/staff">&Eacute;quipe staff</a>
                <a href="<?php echo URL; ?>/leaderboards">Classement des citoyens</a>
                <a href="<?php echo URL; ?>/map">Carte de la ville</a>
                <a href="<?php echo URL; ?>/online">Citoyens en ligne</a>
                <a href="<?php echo URL; ?>/search_users">Rechercher un citoyen</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-briefcase"></i>
                <span class="nav-button-text">Entreprises</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/corporations">Voir les entreprises</a>
                <a href="<?php echo URL; ?>/corporations/info">Comment &ccedil;a marche ?</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-briefcase"></i>
                <span class="nav-button-text">Groupes</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/gangs">Voir les gangs</a>
                <a href="<?php echo URL; ?>/gangs/leaderboard">Classements</a>
                <a href="<?php echo URL; ?>/gangs/info">Comment &ccedil;a marche ?</a>
            </div>
        </div>

        <div class="nav-button">
            <a href="<?php echo URL; ?>/store">
                <i class="fas fa-shopping-cart"></i>
                <span class="nav-button-text">Boutique</span>
            </a>
        </div>
        <?php if(isset($UData) && (int)$UData['rank'] >= 3): ?>
        <div class="staff-navbar-action">
            <a href="<?php echo URL; ?>/admin" title="Ouvrir l’administration">
                <span>Administration</span>
            </a>
        </div>
        <?php endif; ?>
    </div>
</div>
</div>
