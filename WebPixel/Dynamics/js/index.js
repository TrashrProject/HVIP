/* Localhost-safe login / registration for WebPixel */

console.log("[Init] -> Inicializando sistema de login");

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
            console.error('[RDP] Réponse PHP invalide:', data);
            return { result: false, msg: 'Le serveur a renvoyé une réponse invalide.' };
        }
    }

    function showError(box, msgBox, message) {
        $(msgBox).text(message);
        $(box).stop(true, true).show('fast');
    }

    $('#show-register').on('click', function () {
        $('#login-box').hide('fast');
        $('#register-box').show('fast');
    });

    $('#show-login').on('click', function () {
        $('#register-box').hide('fast');
        $('#login-box').show('fast');
    });

    function Login() {
        var username = $('#pz-login-uname').val().trim();
        var password = $('#pz-login-pass').val();

        $('#e-login-message').hide();

        if (!username || !password) {
            showError('#e-login-message', '#e-login-msg', '¡Debes llenar todos los campos!');
            return;
        }

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
                $('#login-message').show('fast');
                window.location.href = baseUrl + 'me.php';
            } else {
                showError('#e-login-message', '#e-login-msg', res.msg || 'Connexion impossible.');
            }
        }).fail(function (xhr) {
            console.error('[RDP] Login HTTP error', xhr.status, xhr.responseText);
            showError('#e-login-message', '#e-login-msg', 'Erreur HTTP ' + xhr.status + ' pendant la connexion.');
        });
    }

    function Register() {
        var username = $('#register-username').val().trim();
        var password = $('#register-password').val();
        var confirm = $('#register-password-confirm').val();
        var email = $('#email').val().trim();

        $('#e-register-message').hide();

        if (!username || !password || !confirm || !email) {
            showError('#e-register-message', '#e-register-msg', '¡Debes llenar todos los campos!');
            return;
        }

        if (password !== confirm) {
            showError('#e-register-message', '#e-register-msg', 'Tus contraseñas no coinciden.');
            return;
        }

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
                $('#register-msg').text(res.msg || 'Compte créé.');
                $('#register-message').show('fast');
                setTimeout(function () {
                    window.location.href = baseUrl + 'me.php?newuser=true';
                }, 700);
            } else {
                showError('#e-register-message', '#e-register-msg', res.msg || 'Inscription impossible.');
            }
        }).fail(function (xhr) {
            console.error('[RDP] Register HTTP error', xhr.status, xhr.responseText);
            showError('#e-register-message', '#e-register-msg', 'Erreur HTTP ' + xhr.status + ' pendant l’inscription.');
        });
    }

    $(document).ready(function () {
        $('#subrmit-login').on('click', Login);
        $('#subrmit-register').on('click', Register);

        $('#pz-login-uname, #pz-login-pass').on('keypress', function (e) {
            if (e.which === 13) Login();
        });

        $('#register-username, #register-password, #register-password-confirm, #email').on('keypress', function (e) {
            if (e.which === 13) Register();
        });

        $('#fb-login').hide();
    });
})(jQuery);
