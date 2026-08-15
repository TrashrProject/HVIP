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
            <img class="pixel" src="https://nitro-imager.kubbo.ch/?figure=<?php echo $UData['look']; ?>&size=l&head_direction=3&gesture=sml&&action=wav">
        </a>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-user"></i>
                <span class="nav-button-text"><?php echo $UData['username']; ?></span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/me" >Inicio</a>
                <a href="<?php echo URL; ?>/profile/<?php echo $UData['username']; ?>">Mi perfil</a>
                <a href="<?php echo URL; ?>/account">Ajustes de cuenta</a>
                <a href="<?php echo URL; ?>/logout">Cerrar sesi&oacute;n</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-users"></i>
                <span class="nav-button-text">Comunidad</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/staff">Equipo Staff</a>
                <a href="<?php echo URL; ?>/leaderboards">Clasificaci&oacute;n de usuarios</a>
                <a href="<?php echo URL; ?>/map">Mapa de ciudad</a>
                <a href="<?php echo URL; ?>/online">Usuarios online</a>
                <a href="<?php echo URL; ?>/search_users">Buscar usuarios</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-briefcase"></i>
                <span class="nav-button-text">Corporaciones</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/corporations">Ver Corporaciones</a>
                <a href="<?php echo URL; ?>/corporations/info">¿C&oacute;mo funcionan?</a>
            </div>
        </div>
        <div class="nav-button dropdown">
            <a href="#">
                <i class="fas fa-briefcase"></i>
                <span class="nav-button-text">Bandas</span>
                <i class="fas fa-caret-down"></i>
            </a>
            <div class="dropdown-content">
                <a href="<?php echo URL; ?>/gangs">Ver Bandas</a>
                <a href="<?php echo URL; ?>/gangs/leaderboard">Clasificaciones</a>
                <a href="<?php echo URL; ?>/gangs/info">¿C&oacute;mo funcionan?</a>
            </div>
        </div>

        <div class="nav-button">
            <a href="<?php echo URL; ?>/store">
                <i class="fas fa-shopping-cart"></i>
                <span class="nav-button-text">Tienda</span>
            </a>
        </div>
    </div>
</div>
</div>