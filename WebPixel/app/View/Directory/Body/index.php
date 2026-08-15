<?php
/**
 * Velora RP - landing, login and registration.
 */

$registeredUsers = '0';
$onlineUsers = '0';

try {
    if (isset($UserMG)) {
        $registeredUsers = $UserMG->GetStatData('users_registered');
        $onlineUsers = $UserMG->GetStatData('users_online');
    }
} catch (Throwable $e) {
    // La page reste disponible même si les statistiques sont indisponibles.
}
?>
<body class="velora-landing">
<div class="page-noise" aria-hidden="true"></div>

<div class="site-shell">
    <header class="site-header">
        <a class="brand" href="<?php echo URL; ?>/" aria-label="Velora RP">
            <span class="brand-dot"></span>
            <span class="brand-name">VELORA</span>
            <span class="brand-rp">RP</span>
        </a>

        <nav class="nav-links" aria-label="Navigation principale">
            <a href="#experience">Expérience</a>
            <a href="#careers">Métiers</a>
            <a href="#districts">Ville</a>
        </nav>

        <div class="header-right">
            <div class="online-chip">
                <span class="online-dot"></span>
                <strong><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?></strong>
                <span>en ligne</span>
            </div>
            <button type="button" class="small-login" data-open-auth="login">Connexion</button>
        </div>
    </header>

    <main>
        <section class="hero-section" id="experience">
            <div class="hero-copy">
                <span class="eyebrow">UNIVERS SOCIAL ROLEPLAY</span>
                <h1>Une ville vivante.<br><span>Une histoire à toi.</span></h1>
                <p class="hero-lead">
                    Velora mélange le charme d'un univers social à la profondeur d'un vrai RP.
                    Travaille, rencontre du monde, gagne de l'argent, achète ton logement, crée ton business
                    ou choisis une vie plus risquée.
                </p>

                <div class="hero-actions">
                    <button type="button" class="primary-btn" data-open-auth="register">
                        Créer mon personnage
                        <i class="fas fa-arrow-right"></i>
                    </button>
                    <a class="text-link" href="#careers">Découvrir la ville <i class="fas fa-chevron-down"></i></a>
                </div>

                <div class="hero-stats">
                    <div><strong><?php echo htmlspecialchars($registeredUsers, ENT_QUOTES, 'UTF-8'); ?></strong><span>citoyens inscrits</span></div>
                    <div><strong>30+</strong><span>métiers & activités</span></div>
                    <div><strong>3</strong><span>quartiers principaux</span></div>
                </div>
            </div>

            <div class="hero-scene" aria-label="Aperçu de l'univers Velora">
                <div class="scene-card scene-police">
                    <span class="scene-icon blue"><i class="fas fa-shield-alt"></i></span>
                    <div><strong>Police</strong><small>Protéger ou enquêter</small></div>
                </div>
                <div class="scene-card scene-business">
                    <span class="scene-icon gold"><i class="fas fa-store"></i></span>
                    <div><strong>Business</strong><small>Créer et recruter</small></div>
                </div>
                <div class="scene-card scene-home">
                    <span class="scene-icon green"><i class="fas fa-home"></i></span>
                    <div><strong>Chez toi</strong><small>Studio, appart, villa</small></div>
                </div>

                <div class="scene-city-name">
                    <span class="live-pill"><i class="fas fa-circle"></i> LIVE</span>
                    <div><strong>DOWNTOWN</strong><small>VELORA CITY</small></div>
                </div>

                <div class="avatar-stage">
                    <div class="avatar-shadow shadow-one"></div>
                    <div class="avatar-shadow shadow-two"></div>
                    <img class="avatar avatar-one" src="<?php echo IMG; ?>/male.gif" alt="Avatar homme">
                    <img class="avatar avatar-two" src="<?php echo IMG; ?>/female.gif" alt="Avatar femme">
                </div>

                <div class="scene-floor"></div>
                <div class="scene-building building-a"></div>
                <div class="scene-building building-b"></div>
                <div class="scene-building building-c"></div>
            </div>
        </section>

        <section class="careers-section" id="careers">
            <div class="section-heading">
                <div>
                    <span class="eyebrow">CHOISIS TA VOIE</span>
                    <h2>Tu peux devenir qui tu veux.</h2>
                </div>
                <p>Pas de classe imposée. Ton métier, tes fréquentations et tes choix construisent ton personnage.</p>
            </div>

            <div class="career-grid">
                <article class="career-card">
                    <span class="career-number">01</span>
                    <div class="career-icon blue"><i class="fas fa-shield-alt"></i></div>
                    <h3>Service public</h3>
                    <p>Police, hôpital, justice, mairie. Fais carrière et gagne en responsabilités.</p>
                    <div class="career-tags"><span>Police</span><span>Médecin</span><span>Avocat</span></div>
                </article>

                <article class="career-card">
                    <span class="career-number">02</span>
                    <div class="career-icon orange"><i class="fas fa-tools"></i></div>
                    <h3>Métiers civils</h3>
                    <p>Taxi, mécanique, livraison, pêche, mine et de nombreuses activités quotidiennes.</p>
                    <div class="career-tags"><span>Taxi</span><span>Mécano</span><span>Livreur</span></div>
                </article>

                <article class="career-card">
                    <span class="career-number">03</span>
                    <div class="career-icon gold"><i class="fas fa-briefcase"></i></div>
                    <h3>Entrepreneur</h3>
                    <p>Monte ton commerce, recrute des joueurs et transforme ton activité en empire.</p>
                    <div class="career-tags"><span>Restaurant</span><span>Garage</span><span>Club</span></div>
                </article>

                <article class="career-card dark-career">
                    <span class="career-number">04</span>
                    <div class="career-icon red"><i class="fas fa-user-secret"></i></div>
                    <h3>Vie criminelle</h3>
                    <p>Gangs, territoires, marché noir et braquages. Plus de gains, mais plus de risques.</p>
                    <div class="career-tags"><span>Gang</span><span>Crime</span><span>Réseau</span></div>
                </article>
            </div>
        </section>

        <section class="districts-section" id="districts">
            <div class="section-heading compact">
                <div>
                    <span class="eyebrow">VELORA CITY</span>
                    <h2>Trois quartiers, trois façons de vivre.</h2>
                </div>
            </div>

            <div class="districts-grid">
                <article class="district-card southbay-card">
                    <div class="district-art">
                        <span class="district-index">01</span>
                        <i class="fas fa-industry"></i>
                    </div>
                    <div class="district-copy">
                        <span class="district-type">QUARTIER POPULAIRE</span>
                        <h3>Southbay</h3>
                        <p>Garages, petits boulots, logements accessibles et une activité de rue permanente.</p>
                    </div>
                </article>

                <article class="district-card downtown-card">
                    <div class="district-art">
                        <span class="district-index">02</span>
                        <i class="fas fa-city"></i>
                    </div>
                    <div class="district-copy">
                        <span class="district-type">CENTRE DE LA VILLE</span>
                        <h3>Downtown</h3>
                        <p>Mairie, banque, hôpital, commerces et entreprises. Le cœur vivant de Velora.</p>
                    </div>
                </article>

                <article class="district-card celestia-card">
                    <div class="district-art">
                        <span class="district-index">03</span>
                        <i class="fas fa-gem"></i>
                    </div>
                    <div class="district-copy">
                        <span class="district-type">QUARTIER PREMIUM</span>
                        <h3>Célestia</h3>
                        <p>Villas, luxe, grands patrons et influence politique. L'objectif des plus ambitieux.</p>
                    </div>
                </article>
            </div>
        </section>

        <section class="join-section" id="auth-panel">
            <div class="join-copy">
                <span class="eyebrow">PREMIÈRE ARRIVÉE</span>
                <h2>Commence avec presque rien.<br><span>Construis tout le reste.</span></h2>
                <p>
                    Ton personnage reçoit une identité, un logement de départ et de quoi commencer sa vie.
                    Ensuite, tout dépend de toi.
                </p>

                <div class="starter-list">
                    <div><span><i class="fas fa-id-card"></i></span><div><strong>Identité RP</strong><small>Ton pseudo devient ton personnage.</small></div></div>
                    <div><span><i class="fas fa-home"></i></span><div><strong>Premier logement</strong><small>Un point de départ avant de viser plus haut.</small></div></div>
                    <div><span><i class="fas fa-coins"></i></span><div><strong>Économie réelle</strong><small>Ton argent sert réellement à progresser.</small></div></div>
                </div>
            </div>

            <div class="auth-card">
                <div class="auth-heading">
                    <div>
                        <span class="auth-kicker">ESPACE CITOYEN</span>
                        <h2 id="auth-title">Connexion</h2>
                    </div>
                    <div class="auth-avatar-wrap">
                        <img src="<?php echo IMG; ?>/male.gif" alt="Avatar Velora">
                    </div>
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

                    <label for="pz-login-uname">Pseudo</label>
                    <div class="input-wrap"><i class="fas fa-user"></i><input id="pz-login-uname" type="text" placeholder="Ton pseudo" autocomplete="username" maxlength="18"></div>

                    <label for="pz-login-pass">Mot de passe</label>
                    <div class="input-wrap"><i class="fas fa-lock"></i><input id="pz-login-pass" type="password" placeholder="Ton mot de passe" autocomplete="current-password"><button type="button" class="show-password" data-toggle-password="pz-login-pass" aria-label="Afficher ou masquer"><i class="fas fa-eye"></i></button></div>

                    <label class="remember-row"><input type="checkbox" name="pz_remember" value="1"><span>Rester connecté</span></label>

                    <button id="subrmit-login" type="button" class="auth-submit"><span>Entrer à Velora</span><i class="fas fa-arrow-right"></i></button>
                    <p class="switch-copy">Pas encore de personnage ? <button type="button" id="show-register" data-open-auth="register">Créer un compte</button></p>
                </div>

                <div id="register-box" class="auth-form" data-auth-view="register">
                    <div id="e-register-message" class="auth-alert error" style="display:none;"><i class="fas fa-exclamation-circle"></i><span id="e-register-msg"></span></div>
                    <div id="register-message" class="auth-alert success" style="display:none;"><i class="fas fa-check-circle"></i><span id="register-msg"></span></div>

                    <label for="register-username">Pseudo RP</label>
                    <div class="input-wrap"><i class="fas fa-id-card"></i><input id="register-username" type="text" placeholder="3 à 18 caractères" autocomplete="username" maxlength="18"></div>
                    <small class="input-hint">Lettres et chiffres uniquement, sans espace.</small>

                    <label for="email">Adresse e-mail</label>
                    <div class="input-wrap"><i class="fas fa-envelope"></i><input id="email" type="email" placeholder="ton@email.fr" autocomplete="email"></div>

                    <label for="register-password">Mot de passe</label>
                    <div class="input-wrap"><i class="fas fa-key"></i><input id="register-password" type="password" placeholder="6 caractères minimum" autocomplete="new-password"><button type="button" class="show-password" data-toggle-password="register-password" aria-label="Afficher ou masquer"><i class="fas fa-eye"></i></button></div>

                    <label for="register-password-confirm">Confirmer</label>
                    <div class="input-wrap"><i class="fas fa-check"></i><input id="register-password-confirm" type="password" placeholder="Retape ton mot de passe" autocomplete="new-password"></div>

                    <div class="gender-row">
                        <label class="gender-choice"><input id="genre-m" type="radio" name="gender" value="M" checked><span><img src="<?php echo IMG; ?>/male.gif" alt=""> Homme</span></label>
                        <label class="gender-choice"><input id="genre-f" type="radio" name="gender" value="F"><span><img src="<?php echo IMG; ?>/female.gif" alt=""> Femme</span></label>
                    </div>

                    <label class="rules-row"><input id="rp-rules" type="checkbox" value="1"><span>J'accepte les règles de roleplay et le respect des autres joueurs.</span></label>

                    <button id="subrmit-register" type="button" class="auth-submit register-submit"><span>Créer mon personnage</span><i class="fas fa-user-plus"></i></button>
                    <p class="switch-copy">Déjà inscrit ? <button type="button" id="show-login" data-open-auth="login">Se connecter</button></p>
                </div>
            </div>
        </section>
    </main>

    <footer class="site-footer">
        <div class="footer-brand"><span class="brand-dot"></span><strong>VELORA RP</strong></div>
        <p>Une ville sociale. Une vraie vie RP. Ton histoire.</p>
        <span>© <?php echo date('Y'); ?> Velora RP</span>
    </footer>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>
</html>
