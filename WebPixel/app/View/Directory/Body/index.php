<?php
/**
 * Velora RP - Landing / login / registration.
 */

$registeredUsers = '0';
$onlineUsers = '0';

try {
    if (isset($UserMG)) {
        $registeredUsers = $UserMG->GetStatData('users_registered');
        $onlineUsers = $UserMG->GetStatData('users_online');
    }
} catch (Throwable $e) {
    // La landing reste accessible même si les statistiques ne sont pas disponibles.
}
?>
<body class="velora-body">
<div class="velora-bg" aria-hidden="true">
    <span class="velora-orb orb-one"></span>
    <span class="velora-orb orb-two"></span>
    <span class="velora-grid"></span>
</div>

<div class="velora-shell">
    <header class="landing-nav">
        <a class="brand" href="<?php echo URL; ?>/" aria-label="Velora RP - Accueil">
            <span class="brand-mark">V</span>
            <span class="brand-copy">
                <strong>VELORA</strong>
                <small>ROLEPLAY</small>
            </span>
        </a>

        <div class="nav-status">
            <span class="status-dot"></span>
            <span><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?> en ligne</span>
        </div>
    </header>

    <main class="landing-main">
        <section class="hero-panel">
            <div class="hero-kicker">
                <span class="kicker-line"></span>
                SAISON 01 · NOUVEAUX ARRIVANTS
            </div>

            <h1>Pars de rien.<br><span>Construis ta vie.</span></h1>

            <p class="hero-lead">
                Velora est une île où l'argent, le pouvoir et la réputation façonnent chaque destin.
                Trouve un métier, monte ton entreprise, protège la ville ou prends le contrôle de ses rues.
            </p>

            <div class="hero-actions">
                <button type="button" class="primary-cta" data-open-auth="register">
                    Créer mon personnage
                    <i class="fas fa-arrow-right"></i>
                </button>
                <button type="button" class="ghost-cta" data-open-auth="login">
                    J'ai déjà un compte
                </button>
            </div>

            <div class="world-stats">
                <div class="world-stat">
                    <strong><?php echo htmlspecialchars($registeredUsers, ENT_QUOTES, 'UTF-8'); ?></strong>
                    <span>citoyens inscrits</span>
                </div>
                <div class="world-stat">
                    <strong>30+</strong>
                    <span>métiers & activités</span>
                </div>
                <div class="world-stat">
                    <strong>3</strong>
                    <span>grands districts</span>
                </div>
            </div>

            <div class="districts">
                <article class="district-card district-southbay">
                    <div class="district-icon"><i class="fas fa-industry"></i></div>
                    <div>
                        <span>SOUTHBAY</span>
                        <p>Quartiers populaires, petits boulots, gangs et marché noir.</p>
                    </div>
                </article>

                <article class="district-card district-downtown">
                    <div class="district-icon"><i class="fas fa-city"></i></div>
                    <div>
                        <span>DOWNTOWN</span>
                        <p>Mairie, commerces, hôpital, police et cœur économique de l'île.</p>
                    </div>
                </article>

                <article class="district-card district-celestia">
                    <div class="district-icon"><i class="fas fa-gem"></i></div>
                    <div>
                        <span>CÉLESTIA</span>
                        <p>Villas, entreprises, politique, luxe et réseaux d'influence.</p>
                    </div>
                </article>
            </div>
        </section>

        <aside class="auth-wrap" id="auth-panel">
            <div class="auth-card">
                <div class="auth-topline">
                    <div>
                        <span class="eyebrow">ACCÈS CITOYEN</span>
                        <h2 id="auth-title">Connexion</h2>
                    </div>
                    <span class="secure-chip"><i class="fas fa-shield-alt"></i> sécurisé</span>
                </div>

                <div class="auth-tabs" role="tablist" aria-label="Connexion ou inscription">
                    <button type="button" class="auth-tab active" data-auth-tab="login">Connexion</button>
                    <button type="button" class="auth-tab" data-auth-tab="register">Inscription</button>
                </div>

                <div id="login-box" class="auth-form active" data-auth-view="login">
                    <div id="e-login-message" class="auth-alert error" style="display:none;">
                        <i class="fas fa-exclamation-circle"></i>
                        <span id="e-login-msg"></span>
                    </div>
                    <div id="login-message" class="auth-alert success" style="display:none;">
                        <i class="fas fa-check-circle"></i>
                        <span id="login-msg"></span>
                    </div>

                    <?php if(isset($_GET['logout']) && $_GET['logout'] === 'success'): ?>
                        <div class="auth-alert success">
                            <i class="fas fa-check-circle"></i>
                            <span>Vous êtes bien déconnecté.</span>
                        </div>
                    <?php endif; ?>

                    <label class="field-label" for="pz-login-uname">Nom de citoyen</label>
                    <div class="field-shell">
                        <i class="fas fa-user"></i>
                        <input id="pz-login-uname" type="text" placeholder="Votre pseudo" required autocomplete="username" maxlength="18">
                    </div>

                    <label class="field-label" for="pz-login-pass">Mot de passe</label>
                    <div class="field-shell">
                        <i class="fas fa-lock"></i>
                        <input id="pz-login-pass" type="password" placeholder="Votre mot de passe" required autocomplete="current-password">
                        <button class="password-toggle" type="button" data-toggle-password="pz-login-pass" aria-label="Afficher ou masquer le mot de passe">
                            <i class="fas fa-eye"></i>
                        </button>
                    </div>

                    <label class="check-line">
                        <input type="checkbox" name="pz_remember" value="1">
                        <span>Rester connecté sur cet appareil</span>
                    </label>

                    <button id="subrmit-login" type="button" class="auth-submit">
                        <span>Entrer à Velora</span>
                        <i class="fas fa-sign-in-alt"></i>
                    </button>

                    <p class="auth-switch-copy">
                        Première arrivée sur l'île ?
                        <button type="button" id="show-register" data-open-auth="register">Créer un compte</button>
                    </p>
                </div>

                <div id="register-box" class="auth-form" data-auth-view="register">
                    <div id="e-register-message" class="auth-alert error" style="display:none;">
                        <i class="fas fa-exclamation-circle"></i>
                        <span id="e-register-msg"></span>
                    </div>
                    <div id="register-message" class="auth-alert success" style="display:none;">
                        <i class="fas fa-check-circle"></i>
                        <span id="register-msg"></span>
                    </div>

                    <div class="register-intro">
                        <strong>Votre histoire commence ici.</strong>
                        <span>Vous recevrez votre identité, un petit logement et votre premier capital en jeu.</span>
                    </div>

                    <label class="field-label" for="register-username">Pseudo RP</label>
                    <div class="field-shell">
                        <i class="fas fa-id-card"></i>
                        <input id="register-username" type="text" placeholder="3 à 18 caractères" required autocomplete="username" maxlength="18">
                    </div>
                    <small class="field-hint">Lettres et chiffres uniquement, sans espace.</small>

                    <label class="field-label" for="email">Adresse e-mail</label>
                    <div class="field-shell">
                        <i class="fas fa-envelope"></i>
                        <input id="email" type="email" placeholder="vous@exemple.fr" required autocomplete="email">
                    </div>

                    <label class="field-label" for="register-password">Mot de passe</label>
                    <div class="field-shell">
                        <i class="fas fa-key"></i>
                        <input id="register-password" type="password" placeholder="6 caractères minimum" autocomplete="new-password" required>
                        <button class="password-toggle" type="button" data-toggle-password="register-password" aria-label="Afficher ou masquer le mot de passe">
                            <i class="fas fa-eye"></i>
                        </button>
                    </div>
                    <div class="password-meter" aria-hidden="true">
                        <span id="password-strength-bar"></span>
                    </div>
                    <small id="password-strength-copy" class="field-hint">Sécurité du mot de passe</small>

                    <label class="field-label" for="register-password-confirm">Confirmation</label>
                    <div class="field-shell">
                        <i class="fas fa-check"></i>
                        <input id="register-password-confirm" type="password" placeholder="Retapez votre mot de passe" autocomplete="new-password" required>
                    </div>

                    <div class="gender-picker" aria-label="Genre du personnage">
                        <label class="gender-option">
                            <input id="genre-m" type="radio" name="gender" value="M" checked>
                            <span><i class="fas fa-mars"></i> Homme</span>
                        </label>
                        <label class="gender-option">
                            <input id="genre-f" type="radio" name="gender" value="F">
                            <span><i class="fas fa-venus"></i> Femme</span>
                        </label>
                    </div>

                    <label class="check-line rules-check">
                        <input id="rp-rules" type="checkbox" value="1">
                        <span>Je m'engage à respecter le RP et les autres joueurs.</span>
                    </label>

                    <button id="subrmit-register" type="button" class="auth-submit register-submit">
                        <span>Créer mon citoyen</span>
                        <i class="fas fa-arrow-right"></i>
                    </button>

                    <p class="auth-switch-copy">
                        Déjà citoyen ?
                        <button type="button" id="show-login" data-open-auth="login">Se connecter</button>
                    </p>
                </div>
            </div>

            <div class="auth-note">
                <i class="fas fa-info-circle"></i>
                <span>Velora RP est un univers fictif. Votre progression dépend de vos choix et de vos interactions.</span>
            </div>
        </aside>
    </main>

    <section class="life-strip" aria-label="Possibilités de jeu">
        <span><i class="fas fa-briefcase"></i> Métiers</span>
        <span><i class="fas fa-building"></i> Entreprises</span>
        <span><i class="fas fa-shield-alt"></i> Police</span>
        <span><i class="fas fa-balance-scale"></i> Justice</span>
        <span><i class="fas fa-car"></i> Véhicules</span>
        <span><i class="fas fa-home"></i> Immobilier</span>
        <span><i class="fas fa-user-secret"></i> Criminalité</span>
        <span><i class="fas fa-vote-yea"></i> Politique</span>
    </section>

    <footer class="landing-footer">
        <span>© <?php echo date('Y'); ?> Velora RP</span>
        <span class="footer-separator">•</span>
        <span>Une île. Une société. Votre histoire.</span>
    </footer>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" crossorigin="anonymous"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" crossorigin="anonymous"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>
</html>
