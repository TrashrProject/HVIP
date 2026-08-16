(function () {
    'use strict';

    var userToggle = document.getElementById('hub-user-toggle');
    var userDropdown = document.getElementById('hub-user-dropdown');
    var menuToggle = document.getElementById('hub-menu-toggle');
    var nav = document.querySelector('.hub-nav');
    var clock = document.getElementById('hub-clock');

    function updateClock() {
        if (!clock) return;
        var now = new Date();
        var hours = String(now.getHours()).padStart(2, '0');
        var minutes = String(now.getMinutes()).padStart(2, '0');
        clock.textContent = hours + ':' + minutes;
    }

    function closeUserMenu() {
        if (userDropdown) userDropdown.classList.remove('open');
        if (userToggle) userToggle.setAttribute('aria-expanded', 'false');
    }

    if (userToggle && userDropdown) {
        userToggle.addEventListener('click', function (event) {
            event.stopPropagation();
            var open = userDropdown.classList.toggle('open');
            userToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    if (menuToggle && nav) {
        menuToggle.addEventListener('click', function (event) {
            event.stopPropagation();
            nav.classList.toggle('mobile-open');
        });
    }

    document.addEventListener('click', function (event) {
        if (userDropdown && userToggle && !userDropdown.contains(event.target) && !userToggle.contains(event.target)) {
            closeUserMenu();
        }

        if (nav && menuToggle && window.innerWidth <= 820 && !nav.contains(event.target) && !menuToggle.contains(event.target)) {
            nav.classList.remove('mobile-open');
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeUserMenu();
            if (nav) nav.classList.remove('mobile-open');
        }
    });

    document.querySelectorAll('.city-action, .hero-actions a, .quick-panel > a').forEach(function (item) {
        item.addEventListener('pointerenter', function () {
            item.classList.add('is-hovered');
        });
        item.addEventListener('pointerleave', function () {
            item.classList.remove('is-hovered');
        });
    });

    updateClock();
    window.setInterval(updateClock, 30000);
})();
