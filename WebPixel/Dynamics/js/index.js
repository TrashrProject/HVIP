/* Velora RP - modern login / registration interactions */

console.log('[Velora RP] Interface d’authentification chargée.');

(function ($) {
    'use strict';

    var baseUrl = window.location.pathname;

    if (!baseUrl.endsWith('/')) {
        baseUrl = baseUrl.substring(0, baseUrl.lastIndexOf('/') + 1);
    }

    var endpoint = baseUrl;

    function parseResponse(data) {
        if (typeof data === 'object') {
            return data;
        }

        try {
            return JSON.parse(data);
        } catch (e) {
            console.error('[Velora RP] Réponse PHP invalide :', data);
            return {
                result: false,
                msg: 'Le serveur a renvoyé une réponse invalide.'
            };
        }
    }

    function showError(box, msgBox, message) {
        $(msgBox).text(message);
        $(box).stop(true, true).fadeIn(140);
    }

    function hideAuthMessages() {
        $('#e-login-message, #login-message, #e-register-message, #register-message').hide();
    }

    function setButtonLoading(selector, loading, idleText) {
        var $button = $(selector);

        if (loading) {
            if (!$button.data('idle-html')) {
                $button.data('idle-html', $button.html());
            }

            $button.addClass('loading').prop('disabled', true);
            $button.html('<span>Patientez...</span><i class="fas fa-circle-notch fa-spin"></i>');
        } else {
            $button.removeClass('loading').prop('disabled', false);

            if ($button.data('idle-html')) {
                $button.html($button.data('idle-html'));
            } else if (idleText) {
                $button.find('span').text(idleText);
            }
        }
    }

    function openAuth(view) {
        var normalized = view === 'register' ? 'register' : 'login';

        hideAuthMessages();

        $('.auth-tab').removeClass('active');
        $('.auth-tab[data-auth-tab="' + normalized + '"]').addClass('active');

        $('.auth-form').removeClass('active').hide();
        $('[data-auth-view="' + normalized + '"]').addClass('active').fadeIn(150);

        $('#auth-title').text(normalized === 'register' ? 'Inscription' : 'Connexion');

        if (window.innerWidth <= 900) {
            var panel = document.getElementById('auth-panel');
            if (panel) {
                panel.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        }
    }

    function passwordScore(password) {
        var score = 0;

        if (password.length >= 6) score++;
        if (password.length >= 10) score++;
        if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++;
        if (/\d/.test(password)) score++;
        if (/[^A-Za-z0-9]/.test(password)) score++;

        return score;
    }

    function updatePasswordStrength() {
        var password = $('#register-password').val() || '';
        var score = passwordScore(password);
        var width = 0;
        var label = 'Sécurité du mot de passe';
        var color = '#ff6b7a';

        if (password.length > 0) {
            width = Math.max(20, score * 20);
        }

        if (score <= 1) {
            label = password.length ? 'Mot de passe faible' : 'Sécurité du mot de passe';
            color = '#ff6b7a';
        } else if (score <= 3) {
            label = 'Mot de passe correct';
            color = '#f2c66d';
        } else {
            label = 'Mot de passe solide';
            color = '#5fe0a2';
        }

        $('#password-strength-bar').css({
            width: width + '%',
            background: color
        });

        $('#password-strength-copy').text(label);
    }

    function Login() {
        var username = $('#pz-login-uname').val().trim();
        var password = $('#pz-login-pass').val();

        $('#e-login-message, #login-message').hide();

        if (!username || !password) {
            showError(
                '#e-login-message',
                '#e-login-msg',
                'Remplis ton pseudo et ton mot de passe.'
            );
            return;
        }

        setButtonLoading('#subrmit-login', true);

        $.ajax({
            url: endpoint,
            method: 'POST',
            data: {
                login_username: username,
                login_password: password
            }
        }).done(function (data) {
            var res = parseResponse(data);

            if (res.result === true) {
                $('#login-msg').text(res.msg || 'Connexion réussie.');
                $('#login-message').fadeIn(140);

                setTimeout(function () {
                    window.location.href = baseUrl + 'me.php';
                }, 350);
            } else {
                showError(
                    '#e-login-message',
                    '#e-login-msg',
                    res.msg || 'Connexion impossible.'
                );
            }
        }).fail(function (xhr) {
            console.error('[Velora RP] Login HTTP error', xhr.status, xhr.responseText);

            showError(
                '#e-login-message',
                '#e-login-msg',
                'Erreur HTTP ' + xhr.status + ' pendant la connexion.'
            );
        }).always(function () {
            setButtonLoading('#subrmit-login', false);
        });
    }

    function Register() {
        var username = $('#register-username').val().trim();
        var password = $('#register-password').val();
        var confirm = $('#register-password-confirm').val();
        var email = $('#email').val().trim();
        var acceptedRules = $('#rp-rules').is(':checked');

        $('#e-register-message, #register-message').hide();

        if (!username || !password || !confirm || !email) {
            showError(
                '#e-register-message',
                '#e-register-msg',
                'Tous les champs sont obligatoires.'
            );
            return;
        }

        if (!/^[A-Za-z0-9]{3,18}$/.test(username)) {
            showError(
                '#e-register-message',
                '#e-register-msg',
                'Le pseudo doit contenir 3 à 18 lettres ou chiffres, sans espace.'
            );
            return;
        }

        if (password.length < 6) {
            showError(
                '#e-register-message',
                '#e-register-msg',
                'Le mot de passe doit contenir au moins 6 caractères.'
            );
            return;
        }

        if (password !== confirm) {
            showError(
                '#e-register-message',
                '#e-register-msg',
                'Les deux mots de passe ne correspondent pas.'
            );
            return;
        }

        if (!acceptedRules) {
            showError(
                '#e-register-message',
                '#e-register-msg',
                'Tu dois t’engager à respecter le roleplay avant de créer ton compte.'
            );
            return;
        }

        setButtonLoading('#subrmit-register', true);

        $.ajax({
            url: endpoint,
            method: 'POST',
            data: {
                reg_username: username,
                reg_password: password,
                reg_mail: email
            }
        }).done(function (data) {
            var res = parseResponse(data);

            if (res.result === true) {
                $('#register-msg').text(res.msg || 'Compte créé. Bienvenue à Velora.');
                $('#register-message').fadeIn(140);

                setTimeout(function () {
                    window.location.href = baseUrl + 'me.php?newuser=true';
                }, 700);
            } else {
                showError(
                    '#e-register-message',
                    '#e-register-msg',
                    res.msg || 'Inscription impossible.'
                );
            }
        }).fail(function (xhr) {
            console.error('[Velora RP] Register HTTP error', xhr.status, xhr.responseText);

            showError(
                '#e-register-message',
                '#e-register-msg',
                'Erreur HTTP ' + xhr.status + ' pendant l’inscription.'
            );
        }).always(function () {
            setButtonLoading('#subrmit-register', false);
        });
    }

    $(document).ready(function () {
        $('[data-open-auth], [data-auth-tab]').on('click', function () {
            var view = $(this).data('open-auth') || $(this).data('auth-tab');
            openAuth(view);
        });

        $('#subrmit-login').on('click', Login);
        $('#subrmit-register').on('click', Register);

        $('#pz-login-uname, #pz-login-pass').on('keypress', function (e) {
            if (e.which === 13) Login();
        });

        $('#register-username, #register-password, #register-password-confirm, #email').on('keypress', function (e) {
            if (e.which === 13) Register();
        });

        $('[data-toggle-password]').on('click', function () {
            var targetId = $(this).data('toggle-password');
            var $input = $('#' + targetId);
            var $icon = $(this).find('i');

            if (!$input.length) return;

            var makeVisible = $input.attr('type') === 'password';
            $input.attr('type', makeVisible ? 'text' : 'password');
            $icon.toggleClass('fa-eye', !makeVisible).toggleClass('fa-eye-slash', makeVisible);
        });

        $('#register-password').on('input', updatePasswordStrength);

        if (window.location.hash === '#register') {
            openAuth('register');
        } else {
            openAuth('login');
        }
    });
})(jQuery);
