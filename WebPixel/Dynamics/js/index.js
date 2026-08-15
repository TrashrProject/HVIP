/* Velora RP - street landing authentication */
(function ($) {
    'use strict';

    var baseUrl = window.location.pathname;
    if (!baseUrl.endsWith('/')) {
        baseUrl = baseUrl.substring(0, baseUrl.lastIndexOf('/') + 1);
    }

    var endpoint = baseUrl;
    var currentMode = null;
    var pivotLocked = false;
    var cityPulseIndex = 0;
    var cityPulseMessages = [
        'La ville évolue avec les joueurs.',
        'Les métiers et entreprises façonnent l’économie.',
        'Chaque quartier possède sa propre ambiance.',
        'Ta réputation se construit directement en RP.'
    ];

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

    function updateClock() {
        var now = new Date();
        var hours = String(now.getHours()).padStart(2, '0');
        var minutes = String(now.getMinutes()).padStart(2, '0');
        $('#device-clock').text(hours + ':' + minutes);
    }

    function updatePasswordStrength() {
        var password = $('#register-password').val() || '';
        var score = 0;
        var label = 'Sécurité du mot de passe';
        var width = 0;
        var tone = 'weak';

        if (password.length >= 6) score++;
        if (password.length >= 10) score++;
        if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++;
        if (/\d/.test(password)) score++;
        if (/[^A-Za-z0-9]/.test(password)) score++;

        if (password.length) {
            width = Math.max(18, Math.min(100, score * 20));
        }

        if (score <= 1) {
            label = password.length ? 'Mot de passe faible' : 'Sécurité du mot de passe';
            tone = 'weak';
        } else if (score <= 3) {
            label = 'Mot de passe correct';
            tone = 'medium';
        } else {
            label = 'Mot de passe solide';
            tone = 'strong';
        }

        $('#password-strength-fill')
            .removeClass('weak medium strong')
            .addClass(tone)
            .css('width', width + '%');
        $('#password-strength-label').text(label);
    }

    function updatePasswordMatch() {
        var password = $('#register-password').val() || '';
        var confirm = $('#register-password-confirm').val() || '';
        var $label = $('#password-match-label');

        $label.removeClass('match no-match');

        if (!confirm.length) {
            $label.text('');
            return;
        }

        if (password === confirm) {
            $label.addClass('match').text('✓ Les mots de passe correspondent');
        } else {
            $label.addClass('no-match').text('Les mots de passe ne correspondent pas encore');
        }
    }

    function applyAuthMode(mode) {
        hideMessages();

        $('.auth-tab').removeClass('active');
        $('.auth-tab[data-auth-tab="' + mode + '"]').addClass('active');
        $('.auth-tabs').toggleClass('register-active', mode === 'register');

        $('.auth-view').removeClass('active').hide();
        $('[data-auth-view="' + mode + '"]').addClass('active').show();

        $('#auth-title').text(mode === 'register' ? 'Inscription' : 'Connexion');
        $('.device-shell').toggleClass('auth-register', mode === 'register');
    }

    function clearFlipClasses($device) {
        $device.removeClass('flip-90 flip-180 flip-270 flip-reset');
    }

    function resetDeviceTilt($device) {
        if (!$device || !$device.length) return;
        $device.css({
            '--tilt-x': '0deg',
            '--tilt-y': '0deg',
            '--shine-x': '50%',
            '--shine-y': '18%'
        }).removeClass('is-hovering');
    }

    function focusMode(mode) {
        var id = mode === 'register' ? '#register-username' : '#pz-login-uname';
        window.setTimeout(function () {
            var $target = $(id);
            if ($target.length && window.innerWidth > 700) {
                $target.trigger('focus');
            }
        }, 90);
    }

    function openAuth(view, animate) {
        var mode = view === 'register' ? 'register' : 'login';
        var $device = $('.device-shell');
        var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if (currentMode === mode) {
            return;
        }

        if (!currentMode || animate === false || reduceMotion || !$device.length) {
            applyAuthMode(mode);
            currentMode = mode;
            return;
        }

        if (pivotLocked) {
            return;
        }

        pivotLocked = true;
        resetDeviceTilt($device);
        clearFlipClasses($device);

        // Full 360° pivot: the back of the device remains visible at mid-rotation.
        $device.addClass('flip-90');

        window.setTimeout(function () {
            $device.removeClass('flip-90').addClass('flip-180');
        }, 165);

        window.setTimeout(function () {
            $device.removeClass('flip-180').addClass('flip-270');
        }, 335);

        window.setTimeout(function () {
            applyAuthMode(mode);
            currentMode = mode;

            $device.removeClass('flip-270').addClass('flip-reset');

            if ($device[0]) {
                void $device[0].offsetWidth;
            }

            window.requestAnimationFrame(function () {
                $device.removeClass('flip-reset');
            });

            window.setTimeout(function () {
                pivotLocked = false;
                focusMode(mode);
            }, 290);
        }, 505);
    }

    function saveRememberedUsername() {
        try {
            var remember = $('#remember-username').is(':checked');
            var username = $('#pz-login-uname').val().trim();

            if (remember && username) {
                window.localStorage.setItem('veloraRememberUsername', username);
                window.localStorage.setItem('veloraRememberEnabled', '1');
            } else {
                window.localStorage.removeItem('veloraRememberUsername');
                window.localStorage.removeItem('veloraRememberEnabled');
            }
        } catch (e) {
            // localStorage may be blocked; authentication must still work normally.
        }
    }

    function restoreRememberedUsername() {
        try {
            var enabled = window.localStorage.getItem('veloraRememberEnabled') === '1';
            var username = window.localStorage.getItem('veloraRememberUsername') || '';

            if (enabled && username) {
                $('#remember-username').prop('checked', true);
                $('#pz-login-uname').val(username);
            }
        } catch (e) {
            // Ignore storage restrictions.
        }
    }

    function login() {
        var username = $('#pz-login-uname').val().trim();
        var password = $('#pz-login-pass').val();

        $('#e-login-message, #login-message').hide();

        if (!username || !password) {
            showError('#e-login-message', '#e-login-msg', 'Renseigne ton pseudo et ton mot de passe.');
            return;
        }

        saveRememberedUsername();
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

    function initDeviceTilt() {
        var $device = $('.device-shell');
        if (!$device.length || window.innerWidth <= 900) return;

        $device.on('mousemove', function (event) {
            if (pivotLocked || $device.is('.flip-90,.flip-180,.flip-270,.flip-reset')) return;

            var rect = this.getBoundingClientRect();
            var px = Math.max(0, Math.min(1, (event.clientX - rect.left) / rect.width));
            var py = Math.max(0, Math.min(1, (event.clientY - rect.top) / rect.height));
            var tiltY = (px - 0.5) * 3.2;
            var tiltX = (0.5 - py) * 2.4;

            $(this).css({
                '--tilt-x': tiltX.toFixed(2) + 'deg',
                '--tilt-y': tiltY.toFixed(2) + 'deg',
                '--shine-x': (px * 100).toFixed(1) + '%',
                '--shine-y': (py * 100).toFixed(1) + '%'
            }).addClass('is-hovering');
        });

        $device.on('mouseleave', function () {
            resetDeviceTilt($(this));
        });
    }

    function initCapsLockHint() {
        $('#pz-login-pass').on('keydown keyup', function (event) {
            var caps = event.originalEvent && event.originalEvent.getModifierState && event.originalEvent.getModifierState('CapsLock');
            $('#caps-lock-hint').toggleClass('visible', !!caps);
        }).on('blur', function () {
            $('#caps-lock-hint').removeClass('visible');
        });
    }

    function initButtonRipples() {
        $('.auth-primary').on('pointerdown', function (event) {
            var rect = this.getBoundingClientRect();
            var $button = $(this);
            $button.css({
                '--ripple-x': (event.clientX - rect.left) + 'px',
                '--ripple-y': (event.clientY - rect.top) + 'px'
            }).removeClass('ripple');

            if (this.offsetWidth) {
                void this.offsetWidth;
            }

            $button.addClass('ripple');
            window.setTimeout(function () {
                $button.removeClass('ripple');
            }, 480);
        });
    }

    function rotateCityPulse() {
        var $text = $('#city-pulse-text');
        if (!$text.length) return;

        $text.addClass('is-changing');
        window.setTimeout(function () {
            cityPulseIndex = (cityPulseIndex + 1) % cityPulseMessages.length;
            $text.text(cityPulseMessages[cityPulseIndex]).removeClass('is-changing');
        }, 160);
    }

    $(document).ready(function () {
        $('[data-open-auth], [data-auth-tab]').on('click', function () {
            openAuth($(this).data('open-auth') || $(this).data('auth-tab'), true);
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

        $('#register-password').on('input', function () {
            updatePasswordStrength();
            updatePasswordMatch();
        });
        $('#register-password-confirm').on('input', updatePasswordMatch);
        $('#remember-username').on('change', saveRememberedUsername);

        restoreRememberedUsername();
        updateClock();
        window.setInterval(updateClock, 30000);
        updatePasswordStrength();
        updatePasswordMatch();
        initDeviceTilt();
        initCapsLockHint();
        initButtonRipples();

        window.setInterval(rotateCityPulse, 4200);

        openAuth(window.location.hash === '#register' ? 'register' : 'login', false);
    });
})(jQuery);
