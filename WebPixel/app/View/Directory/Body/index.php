<?php
/**
 * Velora RP - modern Habbo RP landing / login / registration.
 */

$registeredUsers = '0';
$onlineUsers = '0';

try {
    if (isset($UserMG)) {
        $registeredUsers = $UserMG->GetStatData('users_registered');
        $onlineUsers = $UserMG->GetStatData('users_online');
    }
} catch (Throwable $e) {
    // La page reste accessible même si les statistiques sont indisponibles.
}
?>
<body class="velora-home">
<div class="ambient-bg" aria-hidden="true">
    <div class="ambient-glow glow-one"></div>
    <div class="ambient-glow glow-two"></div>
    <div class="ambient-grid"></div>
</div>

<div class="page-shell">
    <header class="main-header">
        <a class="brand" href="<?php echo URL; ?>/" aria-label="Velora RP">
            <span class="brand-symbol">V</span>
            <span class="brand-text"><strong>VELORA</strong><small>ROLEPLAY</small></span>
        </a>

        <nav class="main-nav" aria-label="Navigation principale">
            <a href="#universe">Univers</a>
            <a href="#life">Vie RP</a>
            <a href="#districts">Quartiers</a>
        </nav>

        <div class="header-actions">
            <div class="live-status"><span></span><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?> en ligne</div>
            <button type="button" class="header-login" data-open-auth="login">Connexion</button>
        </div>
    </header>

    <main>
        <section class="hero" id="universe">
            <div class="hero-copy">
                <div class="hero-label"><span></span> HABBO ROLEPLAY • SAISON 01</div>
                <h1>Ta vie virtuelle.<br><em>À ta façon.</em></h1>
                <p>
                    Velora est une ville RP où chaque joueur construit sa propre histoire.
                    Travaille, gagne de l'argent, achète ton logement, crée ton business,
                    rejoins la police ou impose ton nom dans la rue.
                </p>

                <div class="hero-buttons">
                    <button type="button" class="btn-main" data-open-auth="register">Créer mon personnage <i class="fas fa-arrow-right"></i></button>
                    <button type="button" class="btn-secondary" data-open-auth="login">J'ai déjà un compte</button>
                </div>

                <div class="hero-stats">
                    <div><strong><?php echo htmlspecialchars($registeredUsers, ENT_QUOTES, 'UTF-8'); ?></strong><span>joueurs inscrits</span></div>
                    <div><strong>30+</strong><span>métiers & activités</span></div>
                    <div><strong>3</strong><span>quartiers majeurs</span></div>
                </div>
            </div>

            <div class="hero-visual">
                <div class="city-card">
                    <div class="city-card-top">
                        <span class="city-pill"><i class="fas fa-map-marker-alt"></i> Downtown Velora</span>
                        <span class="city-time">RP ONLINE</span>
                    </div>

                    <div class="character-scene">
                        <div class="scene-panel police"><i class="fas fa-shield-alt"></i><span>Police</span></div>
                        <div class="scene-panel bank"><i class="fas fa-university"></i><span>Banque</span></div>
                        <div class="scene-panel business"><i class="fas fa-store"></i><span>Business</span></div>

                        <div class="character-floor"></div>
                        <div class="character-card character-one">
                            <img src="<?php echo IMG; ?>/male.gif" alt="Citoyen Velora">
                            <span class="character-name">Citoyen</span>
                        </div>
                        <div class="character-card character-two">
                            <img src="<?php echo IMG; ?>/female.gif" alt="Citoyenne Velora">
                            <span class="character-name">Citoyenne</span>
                        </div>
                    </div>

                    <div class="city-card-footer">
                        <div><i class="fas fa-briefcase"></i><span><strong>Choisis ton métier</strong><small>Progresse à ton rythme</small></span></div>
                        <div><i class="fas fa-wallet"></i><span><strong>Construis ta richesse</strong><small>Cash, banque, biens</small></span></div>
                    </div>
                </div>
            </div>
        </section>

        <section class="rp-life" id="life">
            <div class="section-head">
                <div><span class="section-kicker">UNE VRAIE VIE RP</span><h2>Tout est connecté.</h2></div>
                <p>Pas de niveaux inutiles : tes actions, ton métier, ton argent et ta réputation construisent ton personnage.</p>
            </div>

            <div class="feature-grid">
                <article class="feature-card accent-blue"><div class="feature-icon"><i class="fas fa-briefcase"></i></div><strong>Métiers</strong><p>Taxi, mécanicien, médecin, livreur, pêcheur, avocat, policier et bien plus.</p><span>Travailler & évoluer</span></article>
                <article class="feature-card accent-gold"><div class="feature-icon"><i class="fas fa-store"></i></div><strong>Entreprises</strong><p>Crée ton commerce, recrute des joueurs et bâtis une vraie économie locale.</p><span>Créer ton empire</span></article>
                <article class="feature-card accent-red"><div class="feature-icon"><i class="fas fa-user-secret"></i></div><strong>Crime & gangs</strong><p>Territoires, marché noir, braquages et réputation. Chaque choix a ses risques.</p><span>Prendre le contrôle</span></article>
                <article class="feature-card accent-green"><div class="feature-icon"><i class="fas fa-home"></i></div><strong>Vie quotidienne</strong><p>Appartement, véhicules, nourriture, banque, téléphone, loisirs et rencontres RP.</p><span>Construire ta vie</span></article>
            </div>
        </section>

        <section class="district-section" id="districts">
            <div class="section-head compact">
                <div><span class="section-kicker">VELORA CITY</span><h2>Trois quartiers. Trois ambiances.</h2></div>
            </div>

            <div class="district-grid">
                <article class="district-card">
                    <div class="district-top"><span>01</span><i class="fas fa-industry"></i></div>
                    <h3>Southbay</h3><p>Quartier populaire, garages, petits boulots, gangs et marché noir.</p>
                    <div class="district-tags"><span>Populaire</span><span>Crime</span></div>
                </article>
                <article class="district-card featured">
                    <div class="district-top"><span>02</span><i class="fas fa-city"></i></div>
                    <h3>Downtown</h3><p>Le cœur de Velora : mairie, banque, hôpital, commerces et services publics.</p>
                    <div class="district-tags"><span>Centre-ville</span><span>Business</span></div>
                </article>
                <article class="district-card">
                    <div class="district-top"><span>03</span><i class="fas fa-gem"></i></div>
                    <h3>Célestia</h3><p>Villas, luxe, clubs privés, grands patrons et réseaux d'influence.</p>
                    <div class="district-tags"><span>Luxe</span><span>Influence</span></div>
                </article>
            </div>
        </section>

        <section class="auth-section" id="auth-panel">
            <div class="auth-side-copy">
                <span class="section-kicker">REJOINS VELORA</span>
                <h2>Ton histoire commence ici.</h2>
                <p>Crée ton personnage gratuitement et entre dans une ville où les autres joueurs font réellement partie de ton quotidien.</p>
                <div class="starter-pack">
                    <div><i class="fas fa-id-card"></i><span><strong>Identité citoyenne</strong><small>Ton personnage RP</small></span></div>
                    <div><i class="fas fa-home"></i><span><strong>Logement de départ</strong><small>Ton premier chez-toi</small></span></div>
                    <div><i class="fas fa-coins"></i><span><strong>Capital de départ</strong><small>Pour commencer ta vie</small></span></div>
                </div>
            </div>

            <div class="auth-card">
                <div class="auth-header">
                    <div><span>ESPACE JOUEUR</span><h2 id="auth-title">Connexion</h2></div>
                    <div class="auth-avatar"><img src="<?php echo IMG; ?>/male.gif" alt="Avatar"></div>
                </div>

                <div class="auth-tabs" role="tablist" aria-label="Connexion ou inscription">
                    <button type="button" class="auth-tab active" data-auth-tab="login">Connexion</button>
                    <button type="button" class="auth-tab" data-auth-tab="register">Inscription</button>
                </div>

                <div id="login-box" class="auth-form active" data-auth-view="login">
                    <div id="e-login-message" class="auth-alert error" style="display:none;"><i class="fas fa-exclamation-circle"></i><span id="e-login-msg"></span></div>
                    <div id="login-message" class="auth-alert success" style="display:none;"><i class="fas fa-check-circle"></i><span id="login-msg"></span></div>

                    <?php if(isset($_GET['logout']) && $_GET['logout'] === 'success'): ?>
                        <div class="auth-alert success"><i class="fas fa-check-circle"></i><span>Tu es bien déconnecté.</span></div>
                    <?php endif; ?>

                    <label class="field-label" for="pz-login-uname">Pseudo</label>
                    <div class="field-shell"><i class="fas fa-user"></i><input id="pz-login-uname" type="text" placeholder="Ton pseudo" required autocomplete="username" maxlength="18"></div>

                    <label class="field-label" for="pz-login-pass">Mot de passe</label>
                    <div class="field-shell"><i class="fas fa-lock"></i><input id="pz-login-pass" type="password" placeholder="Ton mot de passe" required autocomplete="current-password"><button class="password-toggle" type="button" data-toggle-password="pz-login-pass" aria-label="Afficher ou masquer le mot de passe"><i class="fas fa-eye"></i></button></div>

                    <label class="check-line"><input type="checkbox" name="pz_remember" value="1"><span>Rester connecté</span></label>
                    <button id="subrmit-login" type="button" class="auth-submit"><span>Se connecter</span><i class="fas fa-arrow-right"></i></button>
                    <p class="auth-switch-copy">Nouveau sur Velora ? <button type="button" id="show-register" data-open-auth="register">Créer un compte</button></p>
                </div>

                <div id="register-box" class="auth-form" data-auth-view="register">
                    <div id="e-register-message" class="auth-alert error" style="display:none;"><i class="fas fa-exclamation-circle"></i><span id="e-register-msg"></span></div>
                    <div id="register-message" class="auth-alert success" style="display:none;"><i class="fas fa-check-circle"></i><span id="register-msg"></span></div>

                    <label class="field-label" for="register-username">Pseudo RP</label>
                    <div class="field-shell"><i class="fas fa-id-card"></i><input id="register-username" type="text" placeholder="3 à 18 caractères" required autocomplete="username" maxlength="18"></div>
                    <small class="field-hint">Lettres et chiffres, sans espace.</small>

                    <label class="field-label" for="email">Adresse e-mail</label>
                    <div class="field-shell"><i class="fas fa-envelope"></i><input id="email" type="email" placeholder="ton@email.fr" required autocomplete="email"></div>

                    <label class="field-label" for="register-password">Mot de passe</label>
                    <div class="field-shell"><i class="fas fa-key"></i><input id="register-password" type="password" placeholder="6 caractères minimum" autocomplete="new-password" required><button class="password-toggle" type="button" data-toggle-password="register-password" aria-label="Afficher ou masquer le mot de passe"><i class="fas fa-eye"></i></button></div>
                    <div class="password-meter" aria-hidden="true"><span id="password-strength-bar"></span></div>
                    <small id="password-strength-copy" class="field-hint">Sécurité du mot de passe</small>

                    <label class="field-label" for="register-password-confirm">Confirmation</label>
                    <div class="field-shell"><i class="fas fa-check"></i><input id="register-password-confirm" type="password" placeholder="Retape ton mot de passe" autocomplete="new-password" required></div>

                    <div class="gender-picker" aria-label="Genre du personnage">
                        <label class="gender-option"><input id="genre-m" type="radio" name="gender" value="M" checked><span><img src="<?php echo IMG; ?>/male.gif" alt=""> Homme</span></label>
                        <label class="gender-option"><input id="genre-f" type="radio" name="gender" value="F"><span><img src="<?php echo IMG; ?>/female.gif" alt=""> Femme</span></label>
                    </div>

                    <label class="check-line rules-check"><input id="rp-rules" type="checkbox" value="1"><span>J'accepte les règles RP et le respect des autres joueurs.</span></label>
                    <button id="subrmit-register" type="button" class="auth-submit accent"><span>Créer mon personnage</span><i class="fas fa-user-plus"></i></button>
                    <p class="auth-switch-copy">Déjà inscrit ? <button type="button" id="show-login" data-open-auth="login">Se connecter</button></p>
                </div>
            </div>
        </section>
    </main>

    <footer class="footer">
        <a class="brand footer-brand" href="<?php echo URL; ?>/"><span class="brand-symbol">V</span><span class="brand-text"><strong>VELORA</strong><small>ROLEPLAY</small></span></a>
        <p>Une ville virtuelle, des choix réels en RP.</p>
        <span>© <?php echo date('Y'); ?> Velora RP</span>
    </footer>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" crossorigin="anonymous"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" crossorigin="anonymous"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>
</html>
