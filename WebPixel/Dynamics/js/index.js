/* Velora RP - street landing authentication */
(function ($) {
    'use strict';

    var baseUrl = window.location.pathname;
    if (!baseUrl.endsWith('/')) {
        baseUrl = baseUrl.substring(0, baseUrl.lastIndexOf('/') + 1);
    }

    var endpoint = baseUrl;

    function parseResponse(data) {
        if (typeof data === 'object') return data;
        try {
            return JSON.parse(data);
        } catch (e) {
            console.error('[Velora RP] Réponse invalide :', data);
            return { result: false, msg: 'Le serveur a renvoyé une réponse invalide.' };
        }
    }

    function setLoading(selector, loading) {
        var $button = $(selector);

        if (loading) {
            if (!$button.data('original-html')) {
                $button.data('original-html', $button.html());
            }
            $button.addClass('loading').prop('disabled', true);
            $button.html('<span>Patiente...</span><i class="fas fa-circle-notch fa-spin"></i>');
            return;
        }

        $button.removeClass('loading').prop('disabled', false);
        if ($button.data('original-html')) {
            $button.html($button.data('original-html'));
        }
    }

    function hideMessages() {
        $('#e-login-message, #login-message, #e-register-message, #register-message').hide();
    }

    function showError(box, target, message) {
        $(target).text(message);
        $(box).stop(true, true).fadeIn(120);
    }

    function openAuth(view) {
        var mode = view === 'register' ? 'register' : 'login';

        hideMessages();
        $('.auth-tab').removeClass('active');
        $('.auth-tab[data-auth-tab="' + mode + '"]').addClass('active');

        $('.auth-view').removeClass('active').hide();
        $('[data-auth-view="' + mode + '"]').addClass('active').fadeIn(120);
        $('#auth-title').text(mode === 'register' ? 'Inscription' : 'Connexion');
    }

    function login() {
        var username = $('#pz-login-uname').val().trim();
        var password = $('#pz-login-pass').val();

        $('#e-login-message, #login-message').hide();

        if (!username || !password) {
            showError('#e-login-message', '#e-login-msg', 'Renseigne ton pseudo et ton mot de passe.');
            return;
        }

        setLoading('#subrmit-login', true);

        $.ajax({
            url: endpoint,
            method: 'POST',
            data: {
                login_username: username,
                login_password: password
            }
        }).done(function (data) {
            var response = parseResponse(data);

            if (response.result === true) {
                $('#login-msg').text(response.msg || 'Connexion réussie.');
                $('#login-message').fadeIn(120);
                setTimeout(function () {
                    window.location.href = baseUrl + 'me.php';
                }, 350);
            } else {
                showError('#e-login-message', '#e-login-msg', response.msg || 'Pseudo ou mot de passe incorrect.');
            }
        }).fail(function (xhr) {
            console.error('[Velora RP] Erreur connexion', xhr.status, xhr.responseText);
            showError('#e-login-message', '#e-login-msg', 'Impossible de contacter le serveur. Erreur HTTP ' + xhr.status + '.');
        }).always(function () {
            setLoading('#subrmit-login', false);
        });
    }

    function register() {
        var username = $('#register-username').val().trim();
        var email = $('#email').val().trim();
        var password = $('#register-password').val();
        var confirm = $('#register-password-confirm').val();
        var rulesAccepted = $('#rp-rules').is(':checked');

        $('#e-register-message, #register-message').hide();

        if (!username || !email || !password || !confirm) {
            showError('#e-register-message', '#e-register-msg', 'Tous les champs sont obligatoires.');
            return;
        }

        if (!/^[A-Za-z0-9]{3,18}$/.test(username)) {
            showError('#e-register-message', '#e-register-msg', 'Le pseudo doit contenir 3 à 18 lettres ou chiffres, sans espace.');
            return;
        }

        if (!/^\S+@\S+\.\S+$/.test(email)) {
            showError('#e-register-message', '#e-register-msg', 'Entre une adresse e-mail valide.');
            return;
        }

        if (password.length < 6) {
            showError('#e-register-message', '#e-register-msg', 'Le mot de passe doit contenir au moins 6 caractères.');
            return;
        }

        if (password !== confirm) {
            showError('#e-register-message', '#e-register-msg', 'Les deux mots de passe ne correspondent pas.');
            return;
        }

        if (!rulesAccepted) {
            showError('#e-register-message', '#e-register-msg', 'Tu dois accepter les règles RP avant de créer ton compte.');
            return;
        }

        setLoading('#subrmit-register', true);

        $.ajax({
            url: endpoint,
            method: 'POST',
            data: {
                reg_username: username,
                reg_password: password,
                reg_mail: email
            }
        }).done(function (data) {
            var response = parseResponse(data);

            if (response.result === true) {
                $('#register-msg').text(response.msg || 'Ton personnage a été créé.');
                $('#register-message').fadeIn(120);
                setTimeout(function () {
                    window.location.href = baseUrl + 'me.php?newuser=true';
                }, 650);
            } else {
                showError('#e-register-message', '#e-register-msg', response.msg || 'Impossible de créer le compte.');
            }
        }).fail(function (xhr) {
            console.error('[Velora RP] Erreur inscription', xhr.status, xhr.responseText);
            showError('#e-register-message', '#e-register-msg', 'Impossible de contacter le serveur. Erreur HTTP ' + xhr.status + '.');
        }).always(function () {
            setLoading('#subrmit-register', false);
        });
    }

    $(document).ready(function () {
        $('[data-open-auth], [data-auth-tab]').on('click', function () {
            openAuth($(this).data('open-auth') || $(this).data('auth-tab'));
        });

        $('#subrmit-login').on('click', login);
        $('#subrmit-register').on('click', register);

        $('#pz-login-uname, #pz-login-pass').on('keypress', function (event) {
            if (event.which === 13) login();
        });

        $('#register-username, #email, #register-password, #register-password-confirm').on('keypress', function (event) {
            if (event.which === 13) register();
        });

        $('[data-toggle-password]').on('click', function () {
            var id = $(this).data('toggle-password');
            var $input = $('#' + id);
            var $icon = $(this).find('i');
            if (!$input.length) return;

            var show = $input.attr('type') === 'password';
            $input.attr('type', show ? 'text' : 'password');
            $icon.toggleClass('fa-eye', !show).toggleClass('fa-eye-slash', show);
        });

        openAuth(window.location.hash === '#register' ? 'register' : 'login');
    });
})(jQuery);
