(function () {
    'use strict';

    var sidebar = document.getElementById('rp-sidebar');
    var sidebarToggle = document.getElementById('rp-mobile-sidebar');
    var sidebarOverlay = document.getElementById('sidebar-mobile-overlay');

    function setSidebar(open) {
        if (!sidebar || !sidebarOverlay) return;
        sidebar.classList.toggle('mobile-open', open);
        sidebarOverlay.classList.toggle('show', open);
        document.body.classList.toggle('sidebar-open', open);
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            setSidebar(!sidebar.classList.contains('mobile-open'));
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            setSidebar(false);
        });
    }

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') setSidebar(false);
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 1180) setSidebar(false);
    });

    function pad(value) {
        return String(value).padStart(2, '0');
    }

    function getNextDrawDate() {
        var now = new Date();
        var target = new Date(now);
        var daysUntilSunday = (7 - now.getDay()) % 7;

        if (daysUntilSunday === 0 && now.getHours() >= 20) {
            daysUntilSunday = 7;
        }

        target.setDate(now.getDate() + daysUntilSunday);
        target.setHours(20, 0, 0, 0);
        return target;
    }

    var drawDate = getNextDrawDate();

    function updateCountdown() {
        var now = new Date();
        var diff = drawDate.getTime() - now.getTime();

        if (diff <= 0) {
            drawDate = getNextDrawDate();
            diff = drawDate.getTime() - now.getTime();
        }

        var totalSeconds = Math.max(0, Math.floor(diff / 1000));
        var days = Math.floor(totalSeconds / 86400);
        var hours = Math.floor((totalSeconds % 86400) / 3600);
        var minutes = Math.floor((totalSeconds % 3600) / 60);
        var seconds = totalSeconds % 60;

        var daysNode = document.getElementById('count-days');
        var hoursNode = document.getElementById('count-hours');
        var minutesNode = document.getElementById('count-minutes');
        var secondsNode = document.getElementById('count-seconds');

        if (daysNode) daysNode.textContent = pad(days);
        if (hoursNode) hoursNode.textContent = pad(hours);
        if (minutesNode) minutesNode.textContent = pad(minutes);
        if (secondsNode) secondsNode.textContent = pad(seconds);
    }

    document.querySelectorAll('.inventory-tile').forEach(function (tile) {
        tile.addEventListener('click', function () {
            tile.classList.remove('tile-pop');
            void tile.offsetWidth;
            tile.classList.add('tile-pop');
            window.setTimeout(function () {
                tile.classList.remove('tile-pop');
            }, 230);
        });
    });

    updateCountdown();
    window.setInterval(updateCountdown, 1000);
})();
