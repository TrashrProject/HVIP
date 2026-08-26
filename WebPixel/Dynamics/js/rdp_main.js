/* ParadiseRP CMS core helpers.
 * Keep this file dependency-light: it is loaded on every authenticated CMS page.
 */

(function ($) {
    'use strict';

    if (!$) {
        console.error('[ParadiseRP CMS] jQuery is required by the legacy CMS helpers.');
        return;
    }

    console.info('[ParadiseRP CMS] Interface initialisée');

    function safeInputValue($input) {
        if (!$input || !$input.length) return '';
        var value = $input.val();
        return value === undefined || value === null ? '' : String(value);
    }

    // Users lookup
    $(function () {
        var usernameInput = $('input[name="username-lookup"]');
        var results = $('#lookup-results');

        if (!usernameInput.length || !results.length) return;

        usernameInput.on('keyup', function () {
            var username = safeInputValue(usernameInput);
            var searchUrl = results.data('search-url') || '/search_users';

            $.post(searchUrl, { uname: username })
                .done(function (data) {
                    results.html(data);
                })
                .fail(function () {
                    results.empty();
                });
        });

        // Do not call trim() on a possibly undefined jQuery value.
        var initialValue = safeInputValue(usernameInput);
        if (initialValue.replace(/^\s+|\s+$/g, '').length > 0) {
            usernameInput.trigger('keyup');
        }
    });

    // Gangs lookup
    $(function () {
        var gangInput = $('input[name="gang-lookup"]');
        var gangResults = $('#gangs-results');

        if (!gangInput.length || !gangResults.length) return;

        gangInput.on('keyup', function () {
            var gangName = safeInputValue(gangInput);

            $.post('/gangs', { gname: gangName })
                .done(function (data) {
                    gangResults.html(data);
                })
                .fail(function () {
                    gangResults.empty();
                });
        });
    });
}(window.jQuery));
