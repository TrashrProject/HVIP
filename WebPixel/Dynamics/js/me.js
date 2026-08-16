(function () {
    'use strict';

    var accountToggle = document.getElementById('vx-account-toggle');
    var accountMenu = document.getElementById('vx-account-menu');
    var mobileToggle = document.getElementById('vx-mobile-toggle');
    var nav = document.getElementById('vx-nav');
    var clock = document.getElementById('vx-clock');

    function updateClock() {
        if (!clock) return;
        var now = new Date();
        var hours = String(now.getHours()).padStart(2, '0');
        var minutes = String(now.getMinutes()).padStart(2, '0');
        clock.textContent = hours + ':' + minutes;
    }

    function closeAccount() {
        if (accountMenu) accountMenu.classList.remove('open');
        if (accountToggle) accountToggle.setAttribute('aria-expanded', 'false');
    }

    function closeNav() {
        if (nav) nav.classList.remove('mobile-open');
    }

    if (accountToggle && accountMenu) {
        accountToggle.addEventListener('click', function (event) {
            event.stopPropagation();
            var open = accountMenu.classList.toggle('open');
            accountToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            closeNav();
        });
    }

    if (mobileToggle && nav) {
        mobileToggle.addEventListener('click', function (event) {
            event.stopPropagation();
            nav.classList.toggle('mobile-open');
            closeAccount();
        });
    }

    document.addEventListener('click', function (event) {
        if (accountMenu && accountToggle && !accountMenu.contains(event.target) && !accountToggle.contains(event.target)) {
            closeAccount();
        }

        if (nav && mobileToggle && window.innerWidth <= 900 && !nav.contains(event.target) && !mobileToggle.contains(event.target)) {
            closeNav();
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeAccount();
            closeNav();
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 900) closeNav();
    });

    updateClock();
    window.setInterval(updateClock, 30000);
})();
