<?php
/**
 * ParadiseRP - street RP landing / login / registration.
 */

$registeredUsers = '0';
$onlineUsers = '0';
$staffLoginOnly = isset($_GET['staff-login']);

try {
    if (isset($UserMG)) {
        $registeredUsers = $UserMG->GetStatData('users_registered');
        $onlineUsers = $UserMG->GetStatData('users_online');
    }
} catch (Throwable $e) {
    // La page reste disponible même si les statistiques sont indisponibles.
}
?>
<body class="street-rp-page">

<div class="street-scene" aria-hidden="true">
    <div class="night-glow"></div>
    <span class="cloud cloud-1"></span>
    <span class="cloud cloud-2"></span>
    <span class="cloud cloud-3"></span>
    <span class="cloud cloud-4"></span>

    <div class="far-city"></div>
    <div class="city-fence"></div>

    <div class="street-building left-main">
        <span class="roof-detail"></span>
        <span class="wall-window w1"></span>
        <span class="wall-window w2"></span>
        <span class="shop-awning"></span>
        <span class="shop-door"></span>
        <span class="wall-plant p1"></span>
    </div>

    <div class="street-building left-low">
        <span class="roof-detail"></span>
        <span class="wall-window w1"></span>
        <span class="wall-window w2"></span>
        <span class="garage-door"></span>
        <span class="wall-lamp"></span>
    </div>

    <div class="street-building right-low">
        <span class="roof-detail"></span>
        <span class="wall-window w1"></span>
        <span class="graffiti-mark"></span>
        <span class="street-bench"></span>
    </div>

    <div class="street-building right-main">
        <span class="roof-detail"></span>
        <span class="wall-window w1"></span>
        <span class="wall-window w2"></span>
        <span class="shop-door"></span>
        <span class="wall-plant p2"></span>
    </div>

    <div class="street-tree tree-left"></div>
    <div class="street-tree tree-right"></div>
    <div class="sidewalk"></div>
    <div class="road"></div>

    <div class="street-avatar avatar-left">
        <img src="<?php echo IMG; ?>/male.gif" alt="">
    </div>
    <div class="street-avatar avatar-right">
        <img src="<?php echo IMG; ?>/female.gif" alt="">
    </div>
</div>

<div class="landing-layer">
    <header class="minimal-topbar">
        <a class="mini-brand" href="<?php echo URL; ?>/" aria-label="ParadiseRP">
            <img class="mini-logo" src="<?php echo IMG; ?>/logos/hv_logo_p.png" alt="<?php echo htmlspecialchars(Config::$WName, ENT_QUOTES, 'UTF-8'); ?>">
            <span><strong>PARADISE</strong><small>ROLEPLAY</small></span>
        </a>

        <div class="server-pill">
            <span class="server-dot"></span>
            <strong><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?></strong>
            <span>en ligne</span>
        </div>
    </header>

    <main class="login-stage" id="auth-panel">
        <section class="device-shell">
            <span class="device-top-speaker"></span>
            <span class="device-side side-a"></span>
            <span class="device-side side-b"></span>
            <span class="device-side side-c"></span>
            <span class="device-notch"></span>
            <span class="device-home"></span>

            <div class="device-screen">
                <div class="brand-zone">
                    <div class="brand-emblem">
                        <span class="emblem-city"></span>
                        <span class="emblem-label">RP</span>
                    </div>

                    <div class="brand-title">PARADISERP</div>
                    <div class="brand-subtitle">HABBO ROLEPLAY</div>

                    <div class="online-badge">
                        <span class="online-light"></span>
                        <strong><?php echo htmlspecialchars($onlineUsers, ENT_QUOTES, 'UTF-8'); ?> CITIZENS ONLINE</strong>
                    </div>

                    <div class="brand-stats">
                        <div><strong><?php echo htmlspecialchars($registeredUsers, ENT_QUOTES, 'UTF-8'); ?></strong><span>INSCRITS</span></div>
                        <div><strong>30+</strong><span>MÉTIERS</span></div>
                        <div><strong>3</strong><span>QUARTIERS</span></div>
                    </div>

                    <p class="brand-pitch">
                        Fais ta vie. Trouve un métier. Monte ton business. Rejoins la police ou impose ton nom dans la rue.
                    </p>
                </div>

                <div class="auth-zone">
                    <div class="auth-head">
                        <div>
                            <span class="auth-eyebrow"><?php echo $staffLoginOnly ? 'ESPACE STAFF' : 'ESPACE CITOYEN'; ?></span>
                            <h1 id="auth-title"><?php echo $staffLoginOnly ? 'Connexion staff' : 'Connexion'; ?></h1>
                        </div>
                        <span class="secure-state"><i class="fas fa-shield-alt"></i> sécurisé</span>
                    </div>

                    <?php if(!$staffLoginOnly): ?>
                    <div class="auth-tabs" role="tablist" aria-label="Connexion ou inscription">
                        <button type="button" class="auth-tab active" data-auth-tab="login">Connexion</button>
                        <button type="button" class="auth-tab" data-auth-tab="register">Inscription</button>
                    </div>
                    <?php else: ?>
                    <div class="auth-alert success"><i class="fas fa-shield-alt"></i><span>La connexion pendant la maintenance est r&eacute;serv&eacute;e aux membres du staff.</span></div>
                    <?php endif; ?>
                    <?php if($staffLoginOnly): ?><style>#register-box,[data-open-auth="register"]{display:none!important}</style><?php endif; ?>

                    <div id="login-box" class="auth-view active" data-auth-view="login">
                        <div id="e-login-message" class="auth-alert error" style="display:none;">
                            <i class="fas fa-exclamation-circle"></i><span id="e-login-msg"></span>
                        </div>
                        <div id="login-message" class="auth-alert success" style="display:none;">
                            <i class="fas fa-check-circle"></i><span id="login-msg"></span>
                        </div>

                        <?php if(isset($_GET['logout']) && $_GET['logout'] === 'success'): ?>
                            <div class="auth-alert success"><i class="fas fa-check-circle"></i><span>Tu es bien déconnecté.</span></div>
                        <?php endif; ?>

                        <label class="field-label" for="pz-login-uname">Pseudo</label>
                        <div class="field-wrap login-identity-field">
                            <div id="login-avatar-preview" class="login-avatar-preview" aria-live="polite" aria-label="Aperçu du personnage">
                                <i class="fas fa-user"></i>
                            </div>
                            <input id="pz-login-uname" type="text" placeholder="Ton pseudo" autocomplete="username" maxlength="18">
                        </div>

                        <label class="field-label" for="pz-login-pass">Mot de passe</label>
                        <div class="field-wrap">
                            <i class="fas fa-lock"></i>
                            <input id="pz-login-pass" type="password" placeholder="Ton mot de passe" autocomplete="current-password">
                            <button type="button" class="password-toggle" data-toggle-password="pz-login-pass" aria-label="Afficher ou masquer le mot de passe"><i class="fas fa-eye"></i></button>
                        </div>

                        <button id="subrmit-login" type="button" class="auth-primary">
                            <span>Se connecter</span>
                        </button>

                        <button type="button" class="auth-secondary" data-open-auth="register">
                            <span>Créer un compte</span>
                        </button>
                    </div>

                    <div id="register-box" class="auth-view" data-auth-view="register">
                        <div id="e-register-message" class="auth-alert error" style="display:none;">
                            <i class="fas fa-exclamation-circle"></i><span id="e-register-msg"></span>
                        </div>
                        <div id="register-message" class="auth-alert success" style="display:none;">
                            <i class="fas fa-check-circle"></i><span id="register-msg"></span>
                        </div>

                        <label class="field-label" for="register-username">Pseudo RP</label>
                        <div class="field-wrap">
                            <i class="far fa-id-card"></i>
                            <input id="register-username" type="text" placeholder="3 à 18 caractères" autocomplete="username" maxlength="18">
                        </div>

                        <label class="field-label" for="email">Adresse e-mail</label>
                        <div class="field-wrap">
                            <i class="far fa-envelope"></i>
                            <input id="email" type="email" placeholder="ton@email.fr" autocomplete="email">
                        </div>

                        <div class="register-columns">
                            <div>
                                <label class="field-label" for="register-password">Mot de passe</label>
                                <div class="field-wrap">
                                    <i class="fas fa-key"></i>
                                    <input id="register-password" type="password" placeholder="6 caractères minimum" autocomplete="new-password">
                                    <button type="button" class="password-toggle" data-toggle-password="register-password" aria-label="Afficher ou masquer"><i class="fas fa-eye"></i></button>
                                </div>
                            </div>
                            <div>
                                <label class="field-label" for="register-password-confirm">Confirmation</label>
                                <div class="field-wrap">
                                    <i class="fas fa-check"></i>
                                    <input id="register-password-confirm" type="password" placeholder="Retape ton mot de passe" autocomplete="new-password">
                                </div>
                            </div>
                        </div>

                        <label class="rules-line">
                            <input id="rp-rules" type="checkbox" value="1">
                            <span>J'accepte les règles RP et le respect des autres joueurs.</span>
                        </label>

                        <button id="subrmit-register" type="button" class="auth-primary register-primary">
                            <span>Créer mon personnage</span>
                        </button>

                        <button type="button" class="auth-secondary" data-open-auth="login">
                            <span>J'ai déjà un compte</span>
                        </button>
                    </div>
                </div>
            </div>
        </section>
    </main>

    <footer class="street-footer">
        <span>© <?php echo date('Y'); ?> <?php echo strtoupper(Config::$WName); ?></span>
        <span class="footer-divider">•</span>
        <span>PROJET ROLEPLAY COMMUNAUTAIRE</span>
    </footer>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>
</html>
