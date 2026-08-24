<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */
?>
<html>
<head>
    <title><?php echo Config::$WName; ?> - <?php echo $PageName; ?></title>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="shortcut icon" href="<?php echo IMG; ?>/favicon.ico?v2" type="image/vnd.microsoft.icon">
    <meta name="csrf-token" content="vDd2f87t7d1JiOyDc3VoJSZKT6tRbszQB1aEtSMv">
    <link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700&amp;display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" integrity="sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T" crossorigin="anonymous">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css" integrity="sha384-oS3vJWv+0UjzBfQzYUhtDYW+Pj2yciDJxpsK1OYPAYjqT085Qq/1cq5FLXAZQ7Ay" crossorigin="anonymous">
    <link href="https://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css" rel="stylesheet">

    <link rel="stylesheet" href="<?php echo CSS; ?>/pixelzone.css?<?php echo time(); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/dynamics.css?<?php echo time(); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/retro-cms.css?<?php echo time(); ?>">
    <!--<link rel="stylesheet" href="<?php echo CSS; ?>/pixelzone-ha.css?< ?php echo time(); ?>">-->
    <!-- Global site tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-172931792-1"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}
        gtag('js', new Date());

        gtag('config', 'UA-172931792-1');
    </script>

    <?php if($PageName == "Tienda"): ?>
        <script src="https://www.paypal.com/sdk/js?client-id=<?php echo (Config::$SandBox)? Config::$S_PAYPAL_API : Config::$PAYPAL_API; ?>&amp;disable-funding=credit,card"> // Required. Replace SB_CLIENT_ID with your sandbox client ID.
        </script>
    <?php
    $StoreMG->GetStoreCSS();
    endif; ?>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@10"></script>

    <script>
        // Replace only the obsolete Kubbo imager. Official Habbo image URLs
        // must stay untouched because they provide the correct CMS mapping.
        (function () {
            var legacyHost = 'https://nitro-imager.kubbo.ch/';
            var avatarHost = '<?php echo URL; ?>/avatar.php';

            function fixAvatar(image) {
                if (!image || !image.src || image.src.indexOf(legacyHost) === -1) return;
                var query = image.src.substring(image.src.indexOf('?'));
                image.src = avatarHost + query;
            }

            document.addEventListener('DOMContentLoaded', function () {
                document.querySelectorAll('img').forEach(fixAvatar);
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

<!-- Global site tag (gtag.js) - Google Analytics -->
<script async src="https://www.googletagmanager.com/gtag/js?id=UA-55948081-2"></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  gtag('js', new Date());

  gtag('config', 'UA-55948081-2');
</script>

<script type="text/javascript">
    (function(c,l,a,r,i,t,y){
        c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
        t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
        y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
    })(window, document, "clarity", "script", "9mvcxqta4a");
</script>

<!-- Meta Pixel Code -->
<script>
!function(f,b,e,v,n,t,s)
{if(f.fbq)return;n=f.fbq=function(){n.callMethod?
n.callMethod.apply(n,arguments):n.queue.push(arguments)};
if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
n.queue=[];t=b.createElement(e);t.async=!0;
t.src=v;s=b.getElementsByTagName(e)[0];
s.parentNode.insertBefore(t,s)}(window, document,'script',
'https://connect.facebook.net/en_US/fbevents.js');
fbq('init', '594002042232961');
fbq('track', 'PageView');
</script>
<noscript><img height="1" width="1" style="display:none"
src="https://www.facebook.com/tr?id=594002042232961&ev=PageView&noscript=1"
/></noscript>
<!-- End Meta Pixel Code -->
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
                        <!--<div class="online d-flex align-items-center justify-content-center"><div class="online-users"><b>173</b> citizens <a class="no-link-styling" href="https://peakrp.com/online"><u>online!</u></a></div></div>-->
                        <div class="enter-peak text-center"><a href="<?php echo Config::$URL; ?>/play" target="_blank" class="button green enter-apex no-link-styling">ENTRER DANS LE JEU</a></div>
                    </div>
                </div>
            </div>
        </div>
