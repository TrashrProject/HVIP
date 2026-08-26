<?php
/**
 * ParadiseRP authenticated CMS header.
 */
?>
<!doctype html>
<html lang="fr">
<head>
    <title><?php echo Config::$WName; ?> - <?php echo $PageName; ?></title>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="shortcut icon" href="<?php echo IMG; ?>/favicon.ico?v2" type="image/vnd.microsoft.icon">
    <meta name="csrf-token" content="vDd2f87t7d1JiOyDc3VoJSZKT6tRbszQB1aEtSMv">

    <link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700&amp;display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" integrity="sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWrX2MZw1T" crossorigin="anonymous">
    <!-- CORS-safe Font Awesome mirrors. FA4 stays loaded for legacy templates still using `fa`. -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" referrerpolicy="no-referrer">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" referrerpolicy="no-referrer">

    <!-- Load critical production overrides first. Their !important asset rules
         prevent the legacy stylesheet from ever selecting dead remote hosts. -->
    <link rel="stylesheet" href="<?php echo CSS; ?>/vps-fixes.css?<?php echo time(); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/pixelzone.css?<?php echo time(); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/dynamics.css?<?php echo time(); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/retro-cms.css?<?php echo time(); ?>">

    <?php if($PageName == "Tienda"): ?>
        <script src="https://www.paypal.com/sdk/js?client-id=<?php echo (Config::$SandBox)? Config::$S_PAYPAL_API : Config::$PAYPAL_API; ?>&amp;disable-funding=credit,card"></script>
        <?php $StoreMG->GetStoreCSS(); ?>
    <?php endif; ?>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@10"></script>

    <script>
        // Normalize old avatar-imager URLs without exposing localhost/loopback hosts.
        (function () {
            'use strict';

            var legacyHost = 'https://nitro-imager.kubbo.ch/';
            var officialHost = 'https://www.habbo.es/habbo-imaging/avatarimage';

            function fixAvatar(image) {
                if (!image || !image.src || image.dataset.paradiseAvatarFixed === '1') return;
                if (image.src.indexOf(legacyHost) === -1) return;

                image.dataset.paradiseAvatarFixed = '1';
                var queryPosition = image.src.indexOf('?');
                var query = queryPosition >= 0 ? image.src.substring(queryPosition) : '';
                image.src = officialHost + query;
            }

            document.addEventListener('DOMContentLoaded', function () {
                document.querySelectorAll('img').forEach(fixAvatar);

                if (!window.MutationObserver || !document.body) return;

                new MutationObserver(function (mutations) {
                    mutations.forEach(function (mutation) {
                        mutation.addedNodes.forEach(function (node) {
                            if (node.nodeType !== 1) return;
                            if (node.tagName === 'IMG') fixAvatar(node);
                            if (node.querySelectorAll) node.querySelectorAll('img').forEach(fixAvatar);
                        });
                    });
                }).observe(document.body, { childList: true, subtree: true });
            });
        }());
    </script>
</head>
<body>

<div id="page-wrap">
    <div class="header">
        <div class="container">
            <div class="row" style="margin-bottom: 16px;">
                <div class="col d-flex align-items-center justify-content-center">
                    <div>
                        <a class="logo" href="<?php echo URL; ?>"></a>
                        <form class="header-user-search" action="<?php echo Config::$URL; ?>/search_users" method="get" role="search">
                            <input type="search" name="q" placeholder="Rechercher un citoyen" aria-label="Rechercher un citoyen" maxlength="18" autocomplete="off">
                            <button type="submit" aria-label="Lancer la recherche"><i class="fas fa-search" aria-hidden="true"></i></button>
                        </form>
                        <div class="enter-peak text-center"><a href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener" class="button green enter-apex no-link-styling">ENTRER DANS LE JEU</a></div>
                    </div>
                </div>
            </div>
        </div>
