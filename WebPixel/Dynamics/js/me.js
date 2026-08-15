(function () {
    'use strict';

    var sidebar = document.querySelector('.me-sidebar');
    var overlay = document.querySelector('.sidebar-overlay');
    var toggle = document.getElementById('me-menu-toggle');
    var clock = document.getElementById('me-clock');

    function closeSidebar() {
        if (sidebar) sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('show');
    }

    function toggleSidebar() {
        if (!sidebar || !overlay) return;
        var open = sidebar.classList.toggle('open');
        overlay.classList.toggle('show', open);
    }

    function updateClock() {
        if (!clock) return;
        var now = new Date();
        var hours = String(now.getHours()).padStart(2, '0');
        var minutes = String(now.getMinutes()).padStart(2, '0');
        clock.textContent = hours + ':' + minutes;
    }

    if (toggle) toggle.addEventListener('click', toggleSidebar);
    if (overlay) overlay.addEventListener('click', closeSidebar);

    document.querySelectorAll('.me-sidebar a').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth <= 760) closeSidebar();
        });
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') closeSidebar();
    });

    updateClock();
    window.setInterval(updateClock, 30000);
})();
