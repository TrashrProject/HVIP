<?php
/**
 * Velora RP - Habbo inspired landing / login / registration.
 */

$registeredUsers = '0';
$onlineUsers = '0';

try {
    if (isset($UserMG)) {
        $registeredUsers = $UserMG->GetStatData('users_registered');
        $onlineUsers = $UserMG->GetStatData('users_online');
    }
} catch (Throwable $e) {
    // La page reste accessible si les statistiques sont momentanément indisponibles.
}
?>
<body class="habbo-rp-body">
<div class="sky-scene" aria-hidden="true">
    <div class="sun"></div>
    <div class="cloud cloud-a"></div>
    <div class="cloud cloud-b"></div>
    <div class="cloud cloud-c"></div>
    <div class="city city-back"></div>
    <div class="city city-front"></div>
</div>

<div class="site-wrap">
    <header class="topbar pixel-panel">
        <a class="rp-logo" href="<?php echo URL; ?>/" aria-label="Velora RP">
            <span class="logo-main">VELORA</span>
            <span class="logo-rp">RP</span>
        </a>

        <nav class="top-links" aria-label="Navigation principale">
            <a href="#rp-world"><i class="fas fa-map-marked-alt"></i> Le monde</a>
            <a href="#rp-jobs"><i class="fas fa-briefcase"></i> Métiers</a>
            <a href="#auth-panel" data-open-auth="register"><i class="fas fa-user-plus"></i> S'inscrire</a>
        </nav>

        <div class="hotel-status">
            <span class="online-lamp"></span>
            <div>
                <strong><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?> en ligne</strong>
                <small>Hôtel ouvert</small>
            </div>
        </div>
    </header>

    <main class="home-grid">
        <section class="world-column" id="rp-world">
            <div class="welcome-window pixel-window">
                <div class="window-titlebar">
                    <span><i class="fas fa-city"></i> BIENVENUE À VELORA</span>
                    <div class="window-controls"><b></b><b></b><b></b></div>
                </div>

                <div class="welcome-content">
                    <div class="welcome-copy">
                        <span class="rp-badge">HABBO ROLEPLAY • SAISON 01</span>
                        <h1>Une ville.<br><em>Mille vies possibles.</em></h1>
                        <p>
                            Commence comme simple citoyen, trouve un travail, gagne ton argent,
                            achète ton appartement, monte ton entreprise… ou choisis une vie beaucoup moins légale.
                        </p>

                        <div class="hero-actions">
                            <button type="button" class="pixel-btn orange" data-open-auth="register">
                                <i class="fas fa-user-plus"></i> Créer mon personnage
                            </button>
                            <button type="button" class="pixel-btn blue" data-open-auth="login">
                                <i class="fas fa-key"></i> Me connecter
                            </button>
                        </div>

                        <div class="quick-stats">
                            <div><strong><?php echo htmlspecialchars($registeredUsers, ENT_QUOTES, 'UTF-8'); ?></strong><span>citoyens</span></div>
                            <div><strong>30+</strong><span>métiers</span></div>
                            <div><strong>3</strong><span>districts</span></div>
                            <div><strong>∞</strong><span>histoires</span></div>
                        </div>
                    </div>

                    <div class="habbo-stage" aria-label="Illustration de citoyens">
                        <div class="speech-bubble">Alors… tu deviens qui ?</div>
                        <div class="avatar-floor"></div>
                        <img class="habbo-avatar avatar-male" src="<?php echo IMG; ?>/male.gif" alt="Avatar homme">
                        <img class="habbo-avatar avatar-female" src="<?php echo IMG; ?>/female.gif" alt="Avatar femme">
                        <div class="stage-object sofa"></div>
                        <div class="stage-object plant"></div>
                    </div>
                </div>
            </div>

            <div class="rp-section-title" id="rp-jobs">
                <span class="section-icon"><i class="fas fa-gamepad"></i></span>
                <div>
                    <small>TA VIE, TES CHOIX</small>
                    <h2>Que veux-tu devenir ?</h2>
                </div>
            </div>

            <div class="life-cards">
                <article class="life-card police-card">
                    <div class="life-icon"><i class="fas fa-shield-alt"></i></div>
                    <div><strong>Police & Justice</strong><p>Patrouilles, enquêtes, arrestations, avocats et procès RP.</p></div>
                </article>
                <article class="life-card jobs-card">
                    <div class="life-icon"><i class="fas fa-hard-hat"></i></div>
                    <div><strong>Métiers</strong><p>Taxi, mécano, pêche, livraison, mine, hôpital et bien plus.</p></div>
                </article>
                <article class="life-card business-card">
                    <div class="life-icon"><i class="fas fa-store"></i></div>
                    <div><strong>Business</strong><p>Crée ton commerce, recrute des joueurs et construis ton empire.</p></div>
                </article>
                <article class="life-card crime-card">
                    <div class="life-icon"><i class="fas fa-user-secret"></i></div>
                    <div><strong>Crime & Gangs</strong><p>Marché noir, territoires, braquages et réputation criminelle.</p></div>
                </article>
                <article class="life-card home-card">
                    <div class="life-icon"><i class="fas fa-home"></i></div>
                    <div><strong>Immobilier</strong><p>Loue un studio puis évolue jusqu'aux villas de Célestia.</p></div>
                </article>
                <article class="life-card politics-card">
                    <div class="life-icon"><i class="fas fa-landmark"></i></div>
                    <div><strong>Politique</strong><p>Élections, mairie, taxes et décisions qui changent la ville.</p></div>
                </article>
            </div>

            <div class="district-window pixel-window">
                <div class="window-titlebar orange-title">
                    <span><i class="fas fa-map"></i> LES DISTRICTS</span>
                    <div class="window-controls"><b></b><b></b><b></b></div>
                </div>
                <div class="district-row">
                    <article class="district-tile southbay">
                        <span class="district-number">01</span>
                        <div><strong>Southbay</strong><small>Populaire • gangs • marché noir</small></div>
                    </article>
                    <article class="district-tile downtown">
                        <span class="district-number">02</span>
                        <div><strong>Downtown</strong><small>Commerces • mairie • services</small></div>
                    </article>
                    <article class="district-tile celestia">
                        <span class="district-number">03</span>
                        <div><strong>Célestia</strong><small>Luxe • villas • influence</small></div>
                    </article>
                </div>
            </div>
        </section>

        <aside class="auth-column" id="auth-panel">
            <div class="auth-window pixel-window">
                <div class="window-titlebar auth-titlebar">
                    <span><i class="fas fa-hotel"></i> ESPACE CITOYEN</span>
                    <div class="window-controls"><b></b><b></b><b></b></div>
                </div>

                <div class="auth-inner">
                    <div class="auth-mascot-row">
                        <div class="mini-avatar-frame">
                            <img src="<?php echo IMG; ?>/male.gif" alt="Avatar Velora">
                        </div>
                        <div>
                            <span class="eyebrow">BIENVENUE À L'HÔTEL</span>
                            <h2 id="auth-title">Connexion</h2>
                            <p>Entre en ville et reprends ton histoire.</p>
                        </div>
                    </div>

                    <div class="auth-tabs" role="tablist" aria-label="Connexion ou inscription">
                        <button type="button" class="auth-tab active" data-auth-tab="login"><i class="fas fa-key"></i> Connexion</button>
                        <button type="button" class="auth-tab" data-auth-tab="register"><i class="fas fa-user-plus"></i> Inscription</button>
                    </div>

                    <div id="login-box" class="auth-form active" data-auth-view="login">
                        <div id="e-login-message" class="auth-alert error" style="display:none;"><i class="fas fa-exclamation-triangle"></i><span id="e-login-msg"></span></div>
                        <div id="login-message" class="auth-alert success" style="display:none;"><i class="fas fa-check-circle"></i><span id="login-msg"></span></div>

                        <?php if(isset($_GET['logout']) && $_GET['logout'] === 'success'): ?>
                            <div class="auth-alert success"><i class="fas fa-check-circle"></i><span>Tu es bien déconnecté.</span></div>
                        <?php endif; ?>

                        <label class="field-label" for="pz-login-uname">Pseudo</label>
                        <div class="field-shell">
                            <i class="fas fa-user"></i>
                            <input id="pz-login-uname" type="text" placeholder="Ton pseudo RP" required autocomplete="username" maxlength="18">
                        </div>

                        <label class="field-label" for="pz-login-pass">Mot de passe</label>
                        <div class="field-shell">
                            <i class="fas fa-lock"></i>
                            <input id="pz-login-pass" type="password" placeholder="Ton mot de passe" required autocomplete="current-password">
                            <button class="password-toggle" type="button" data-toggle-password="pz-login-pass" aria-label="Afficher ou masquer le mot de passe"><i class="fas fa-eye"></i></button>
                        </div>

                        <label class="check-line"><input type="checkbox" name="pz_remember" value="1"><span>Rester connecté</span></label>

                        <button id="subrmit-login" type="button" class="auth-submit pixel-btn green wide"><span>Entrer dans l'hôtel</span><i class="fas fa-arrow-right"></i></button>

                        <p class="auth-switch-copy">Pas encore citoyen ? <button type="button" id="show-register" data-open-auth="register">Inscris-toi gratuitement</button></p>
                    </div>

                    <div id="register-box" class="auth-form" data-auth-view="register">
                        <div id="e-register-message" class="auth-alert error" style="display:none;"><i class="fas fa-exclamation-triangle"></i><span id="e-register-msg"></span></div>
                        <div id="register-message" class="auth-alert success" style="display:none;"><i class="fas fa-check-circle"></i><span id="register-msg"></span></div>

                        <div class="register-ticket">
                            <i class="fas fa-ticket-alt"></i>
                            <div><strong>Nouveau citoyen</strong><span>Crée ton identité et démarre ta vie à Velora.</span></div>
                        </div>

                        <label class="field-label" for="register-username">Pseudo RP</label>
                        <div class="field-shell"><i class="fas fa-id-card"></i><input id="register-username" type="text" placeholder="3 à 18 caractères" required autocomplete="username" maxlength="18"></div>
                        <small class="field-hint">Lettres et chiffres, sans espace.</small>

                        <label class="field-label" for="email">Adresse e-mail</label>
                        <div class="field-shell"><i class="fas fa-envelope"></i><input id="email" type="email" placeholder="ton@email.fr" required autocomplete="email"></div>

                        <label class="field-label" for="register-password">Mot de passe</label>
                        <div class="field-shell">
                            <i class="fas fa-key"></i>
                            <input id="register-password" type="password" placeholder="6 caractères minimum" autocomplete="new-password" required>
                            <button class="password-toggle" type="button" data-toggle-password="register-password" aria-label="Afficher ou masquer le mot de passe"><i class="fas fa-eye"></i></button>
                        </div>
                        <div class="password-meter" aria-hidden="true"><span id="password-strength-bar"></span></div>
                        <small id="password-strength-copy" class="field-hint">Sécurité du mot de passe</small>

                        <label class="field-label" for="register-password-confirm">Confirmation</label>
                        <div class="field-shell"><i class="fas fa-check"></i><input id="register-password-confirm" type="password" placeholder="Retape ton mot de passe" autocomplete="new-password" required></div>

                        <div class="gender-picker" aria-label="Genre du personnage">
                            <label class="gender-option">
                                <input id="genre-m" type="radio" name="gender" value="M" checked>
                                <span><img src="<?php echo IMG; ?>/male.gif" alt=""> Homme</span>
                            </label>
                            <label class="gender-option">
                                <input id="genre-f" type="radio" name="gender" value="F">
                                <span><img src="<?php echo IMG; ?>/female.gif" alt=""> Femme</span>
                            </label>
                        </div>

                        <label class="check-line rules-check"><input id="rp-rules" type="checkbox" value="1"><span>J'accepte les règles RP et le respect des autres joueurs.</span></label>

                        <button id="subrmit-register" type="button" class="auth-submit pixel-btn orange wide"><span>Créer mon personnage</span><i class="fas fa-user-plus"></i></button>

                        <p class="auth-switch-copy">Déjà inscrit ? <button type="button" id="show-login" data-open-auth="login">Retour connexion</button></p>
                    </div>
                </div>
            </div>

            <div class="side-note pixel-panel">
                <i class="fas fa-star"></i>
                <div><strong>Objectif : vivre ton propre RP</strong><span>Aucun chemin imposé. Tes choix créent ton histoire.</span></div>
            </div>
        </aside>
    </main>

    <footer class="rp-footer pixel-panel">
        <div><strong>VELORA RP</strong><span>Habbo Roleplay francophone</span></div>
        <p>© <?php echo date('Y'); ?> — Une ville, des métiers, des histoires.</p>
    </footer>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" crossorigin="anonymous"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" crossorigin="anonymous"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>
</html>
