(function () {
    'use strict';

    var sidebar = document.getElementById('rp-sidebar');
    var sidebarOverlay = document.getElementById('rp-sidebar-overlay');
    var menuButton = document.getElementById('rp-menu-button');
    var userToggle = document.getElementById('rp-user-toggle');
    var userDropdown = document.getElementById('rp-user-dropdown');
    var phoneClock = document.getElementById('rp-phone-clock');
    var eventPanel = document.querySelector('.rp-event-panel[data-event-end]');

    function setSidebar(open) {
        if (!sidebar) return;
        sidebar.classList.toggle('mobile-open', !!open);
        if (sidebarOverlay) sidebarOverlay.classList.toggle('show', !!open);
        document.body.classList.toggle('rp-sidebar-open', !!open);
    }

    function closeUserMenu() {
        if (userDropdown) userDropdown.classList.remove('open');
        if (userToggle) userToggle.setAttribute('aria-expanded', 'false');
    }

    if (menuButton) {
        menuButton.addEventListener('click', function () {
            var isOpen = sidebar && sidebar.classList.contains('mobile-open');
            setSidebar(!isOpen);
            closeUserMenu();
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            setSidebar(false);
        });
    }

    if (userToggle && userDropdown) {
        userToggle.addEventListener('click', function (event) {
            event.stopPropagation();
            var open = userDropdown.classList.toggle('open');
            userToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    document.addEventListener('click', function (event) {
        if (userDropdown && userToggle && !userDropdown.contains(event.target) && !userToggle.contains(event.target)) {
            closeUserMenu();
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            setSidebar(false);
            closeUserMenu();
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 1060) setSidebar(false);
    });

    function pad(value) {
        return String(Math.max(0, value)).padStart(2, '0');
    }

    function updateClock() {
        if (!phoneClock) return;
        var now = new Date();
        phoneClock.textContent = pad(now.getHours()) + ':' + pad(now.getMinutes());
    }

    function updateEventCountdown() {
        if (!eventPanel) return;

        var end = Number(eventPanel.getAttribute('data-event-end'));
        if (!Number.isFinite(end)) return;

        var remaining = Math.max(0, end - Date.now());
        var totalSeconds = Math.floor(remaining / 1000);
        var days = Math.floor(totalSeconds / 86400);
        var hours = Math.floor((totalSeconds % 86400) / 3600);
        var minutes = Math.floor((totalSeconds % 3600) / 60);
        var seconds = totalSeconds % 60;

        var dayNode = document.getElementById('rp-days');
        var hourNode = document.getElementById('rp-hours');
        var minuteNode = document.getElementById('rp-minutes');
        var secondNode = document.getElementById('rp-seconds');

        if (dayNode) dayNode.textContent = pad(days);
        if (hourNode) hourNode.textContent = pad(hours);
        if (minuteNode) minuteNode.textContent = pad(minutes);
        if (secondNode) secondNode.textContent = pad(seconds);
    }

    function installHealthApp() {
        var grid = document.querySelector('.phone-app-grid');
        if (!grid || grid.querySelector('[data-health-app]')) return;

        var link = document.createElement('a');
        link.href = 'health';
        link.setAttribute('data-health-app', '1');
        link.innerHTML = '<span class="app-icon" style="background:linear-gradient(180deg,#f06472,#cf3e4c);color:#fff"><i class="fas fa-heartbeat"></i></span><small>Santé</small>';
        grid.appendChild(link);
    }

    document.querySelectorAll('.inventory-slot, .side-document').forEach(function (item) {
        item.addEventListener('click', function () {
            item.classList.remove('clicked');
            void item.offsetWidth;
            item.classList.add('clicked');
            window.setTimeout(function () {
                item.classList.remove('clicked');
            }, 260);
        });
    });

    installHealthApp();
    updateClock();
    updateEventCountdown();
    window.setInterval(updateClock, 30000);
    window.setInterval(updateEventCountdown, 1000);
})();
