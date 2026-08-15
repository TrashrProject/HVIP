<?php

	/*if(EnumToBool(User($_SESSION['ID'], 'facebook_change_name'))){
		include 'change_name.tpl';
		die();
	}

	CheckVIPStatus($USERID);

	$MySSO = GenerateAuthToken($USERID); 
	if(CheckExist('play_stats', 'id', $USERID) <= 0){
		MYSQL_::Bits("INSERT INTO play_stats (id) VALUES ('".$USERID."');");
	}*/
if($UData['username'] == null || empty($UData['username'])){
	echo "<script>alert('Debe iniciar sesión nuevamente con tu nuevo nombre de usuario.');window.location.href = '/logout';</script>";
  	exit;
}

$Ban = $USession->BanValidation("ip", AppFunctions::GetIP());
$Ban_ = $USession->BanValidation("user", $UData['username']);
if($Ban['banned'] || $Ban_['banned']):
    header("Location: /logout");
    exit;
endif;

// Check Polls
if($UData["rank"] < 3 AND $UPData["level"] > 1):
	$POLLS = $DB->Query("SELECT * FROM rdp_polls WHERE enabled = '1'");
	if(mysqli_num_rows($POLLS) > 0):
		while($mPoll = mysqli_fetch_assoc($POLLS)):
			$QUESTS = $DB->Query("SELECT * FROM rdp_polls_quests WHERE poll_id = " . $mPoll["id"] . " AND id NOT IN (SELECT quest_id FROM rdp_polls_answers WHERE quest_id = id  AND poll_id = " . $mPoll["id"] . " AND user_id = " . $UData["id"] . ")");
			if(mysqli_num_rows($QUESTS)  > 0):
				echo "<script>window.location = '/poll.php';</script>";
				exit;
			endif;
		endwhile;
	endif;
endif;

$ClientAUTH = $UserMG->GenerateAUTH($UData['id']);
$UserMG->GenerateMachineId($UData['id']);
$UserMG->CheckVIPStatus($UData['id']);

/*
if($UserMG->GiveVIP($UData['id'], 1604275200, $UData['rank'], true)):
    header("Location: /FreeVIP.html");
    exit;
endif;*/


?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en" xmlns:og="http://opengraphprotocol.org/schema/" xmlns:fb="http://www.facebook.com/2008/fbml">
	<head>
		<title><?php echo Config::$WName; ?> ~ Client</title>
		<meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no" />
        <meta name="description" content="Diversión sin limites" />
		<link rel="shortcut icon" href="<?php echo DY; ?>/img/favicon.ico" type="image/vnd.microsoft.icon" />

		<!-- Google Fonts -->
		<link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700,400italic,700italic|Ubuntu+Medium" rel="stylesheet" type="text/css">
		<link href="https://fonts.googleapis.com/css?family=Squada+One" rel="stylesheet">

		<!-- CSS -->
		<link href="<?php echo DY; ?>/css/client/animate.css" rel="stylesheet">
		<link href="<?php echo DY; ?>/css/client/adsense.css?ergerghae" rel="stylesheet">
		<link href="<?php echo WS_DY;?>/ws_resources/css/client.css?p=<?php echo time(); ?>" rel="stylesheet">
		<link href="<?php echo WS_DY;?>/ws_resources/css/balloon.min.css?p=<?php echo time(); ?>" rel="stylesheet">
		<link rel="stylesheet" type="text/css" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css">
		
		<!-- EMOJI RESOURCES -->
		<link href="https://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css" rel="stylesheet">
		<link href="<?php echo WS_DY;?>/ws_overlays/Phones/iPhone/resources/emojis/lib/css/emoji.css?v=<?php echo time(); ?>" rel="stylesheet">
		<!-- END EMOJI RESOURCES -->

		<!-- Custom CSS -->
		
		<!-- JS Client Vars -->
		<script type="text/javascript"> 
	    	var ChangeLogs = false;
	        var andSoItBegins = (new Date()).getTime();
	        var ad_keywords = "";
	        document.habboLoggedIn = true;
	       	var habboId = "<?php echo $UData['id']; ?>";
	        var habboName = "<?php echo $UData['username']; ?>";
	        var habboFigure = "<?php echo $UData['look']; ?>";
            var WS_DY = "<?php echo WS_DY;?>";
	        var habboStaticFileSite = "";
	        var habboImagerUrl = "https://images.kubbo.city/habbo-imaging/";
	        var habboPartner = "";
	        
	        var habboSWFUrl = "<?php echo Config::$SWF; ?>";
	        var habboSWF = "Habbo60l2";
	        window.name = "habboMain";
	        if (typeof HabboClient != "undefined") { HabboClient.windowName = "uberClientWnd"; }

	        // To Flash Player
	    	var callback = function(e) {
				if(!e.success) {
					document.getElementById("content").style.display = "block";
				}
				else {
					document.getElementById("content").style.display = "none";
				}
			};
		
			var habboReqSite = "<?php echo Config::$URL; ?>";
			var habboDefaultClientPopupUrl = "<?php echo Config::$URL; ?>/play";
			var habboSWFUrl = "<?php echo Config::$SWF; ?>";
			
	        var flashvars = {
	            "client.allow.cross.domain" : "0", 
	            "client.notify.cross.domain" : "1", 
	            "connection.info.host" : "<?php echo Config::$TCP; ?>",
	            "connection.socket.host" : "<?php echo Config::$WS_SERVER; ?>",
	            "connection.info.port" : "<?php echo Config::$TCP_PORT; ?>",
	            "connection.socket.port" : "<?php echo Config::$WS_PORT; ?>",
	            "site.url" : habboReqSite, 
	            "url.prefix" : habboReqSite, 
	            "client.reload.url" : habboDefaultClientPopupUrl, 
	            "client.fatal.error.url" : habboDefaultClientPopupUrl, 
	            "client.connection.failed.url" : habboDefaultClientPopupUrl, 
	            "logout.url" : habboReqSite + "/logout?client=true",
	            "logout.disconnect.url" : habboDefaultClientPopupUrl, 
	            "external.variables.txt" : habboSWFUrl + "/gamedata/external_variables.txt?p=<?php echo time(); ?>",
	            "external.texts.txt" : habboSWFUrl + "/gamedata/external_flash_texts.txt?p=<?php echo time(); ?>",
	            "productdata.load.url" : habboSWFUrl + "/gamedata/productdata.txt?p=<?php echo time(); ?>",
	            "furnidata.load.url" : habboSWFUrl + "/gamedata/furnidata.xml?p=<?php echo time(); ?>",  
	            "sso.ticket": "<?php echo $ClientAUTH; ?>",
	            "processlog.enabled" : "1", 
	            "account_id" : "<?php echo $UData['id'] ?>",
	            "client.starting" : "¡Por favor, espera! <?php echo Config::$WName; ?> se está cargando",
	            "flash.client.url" : habboSWFUrl + "/gordon/PRODUCTION-201602082203-712976078_2/", 
	            "user.hash" : "<?php hash('sha256', $UData['id']); ?>",
	            "has.identity" : "1", 
	            "flash.client.origin" : "popup", 
	            "nux.lobbies.enabled" : "false", 
	            "country_code" : "MX" 
	        };

	         var params = {
	            "base" : habboSWFUrl + "/gordon/PRODUCTION-201602082203-712976078_2/",
	            "allowScriptAccess" : "always",
	            "menu" : "true"
	        };

	        swfobject.embedSWF(habboSWFUrl + "/gordon/PRODUCTION-201602082203-712976078_2/"+ habboSWF +".swf?<?php echo time(); ?>", "flash-container", "100%", "100%", "10.1.0", "http://cdn.uber.meth0d.org/expressInstall.swf", flashvars, params, null, callback);
        </script>

        <!-- JS (Internet) -->
   		<script src="https://code.jquery.com/jquery-1.11.3.min.js"></script>

		<script src="<?php echo WS_DY;?>/ws_resources/js/adsense.js?htujrtse"></script>

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
		<!-- Global site tag (gtag.js) - Google Ads: 10944022158 -->
		<script async src="https://www.googletagmanager.com/gtag/js?id=AW-10950622224"></script>
		<script>
		window.dataLayer = window.dataLayer || [];
		function gtag(){dataLayer.push(arguments);}
		gtag('js', new Date());

		gtag('config', 'AW-10950622224');
		</script>
		<!-- Event snippet for Sign-up conversion page
		In your html page, add the snippet and call gtag_report_conversion when someone clicks on the chosen link or button. -->
		<script>
		function gtag_report_conversion(url) {
		var callback = function () {
			if (typeof(url) != 'undefined') {
			window.location = url;
			}
		};
		gtag('event', 'conversion', {
			'send_to': 'AW-10950622224/eG3dCNbO3NMDEJD41OUo',
			'event_callback': callback
		});
		return false;
		}
		</script>



    </head>

    <body>
    	<!-- EMOJI JS -->
		<script src="<?php echo WS_DY;?>/ws_overlays/Phones/iPhone/resources/emojis/lib/js/config.js"></script>
		<script src="<?php echo WS_DY;?>/ws_overlays/Phones/iPhone/resources/emojis/lib/js/util.js"></script>
		<script src="<?php echo WS_DY;?>/ws_overlays/Phones/iPhone/resources/emojis/lib/js/jquery.emojiarea.js"></script>
		<script src="<?php echo WS_DY;?>/ws_overlays/Phones/iPhone/resources/emojis/lib/js/emoji-picker.js"></script>
		<!-- END EMOJI JS -->

		<!-- Google ADS 
		<?php if ($UData['rank'] < 6 && $UData['username'] != "Jeihden" && $UData['username'] != "Tester"): ?>
		<div id="AdBox2">
		    <div id="avoid"></div>
		    <div id="exitb"></div>
		    <div id="AdBox" class="item" style="position: absolute;">

		    </div>
		</div>
		<?php endif; ?>-->

		<iframe src="https://nitro.habbovip.us/index.html?sso=<?php echo $ClientAUTH; ?>" class="Nitro" allow="camera none; microphone *">
		</iframe>


		<!-- Loader V1 - ->
		<div id="container_loader">
		  <div id="nico">
		  	<img src="https://rsgames.hs.llnwd.net/o10/ZTM1MTNmMmVjYjRhNWViNWQ3ZmZhOGUyMDI2OGJlYzY=/images/shadows/char_nico.png" />
		  </div>
		  <div id="roman">
		  	<img src="https://rsgames.hs.llnwd.net/o10/ZTM1MTNmMmVjYjRhNWViNWQ3ZmZhOGUyMDI2OGJlYzY=/images/shadows/char_roman.png"/>
		  </div>
		  <div id="dmitriy">
		  	<img src="https://rsgames.hs.llnwd.net/o10/ZTM1MTNmMmVjYjRhNWViNWQ3ZmZhOGUyMDI2OGJlYzY=/images/shadows/char_dmitri.png"/>
		  </div>
		</div> -->

		<!-- Publi -->
		<div id="AdBox2" style="display:none">
			<div id="avoid" class="avoid_class"></div>
			<button id="exitb" class="exit_class" onclick="gtag_report_conversion();"></button>
			<div id="AdBox" class="item" style="position: relative;display:inline-block;"></div>
		</div>
		<div id="client-banner" class="advertisement" style="display:none; margin: 14px 38%;z-index: 999999999999999999999;" data-close="30" data-open="2" >
		<div class="head"> <span>Anuncios patrocinados. &nbsp; Se cerrara en <strong>n</strong> Segundos.</span></div>
			<div class="body" style=""></div>
		</div>

        <div id="app"><?php require 'ws_overlays.tpl'; ?></div>

		<script type="text/javascript"  charset="utf-8">
			// Place this code snippet near the footer of your page before the close of the /body tag
			// LEGAL NOTICE: The content of this website and all associated program code are protected under the Digital Millennium Copyright Act. Intentionally circumventing this code may constitute a violation of the DMCA.
			
			eval(function(p,a,c,k,e,d){e=function(c){return(c<a?'':e(parseInt(c/a)))+((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))};if(!''.replace(/^/,String)){while(c--){d[e(c)]=k[c]||e(c)}k=[function(e){return d[e]}];e=function(){return'\\w+'};c=1};while(c--){if(k[c]){p=p.replace(new RegExp('\\b'+e(c)+'\\b','g'),k[c])}}return p}(';q M=\'\',29=\'24\';1P(q i=0;i<12;i++)M+=29.U(C.I(C.J()*29.E));q 2p=11,2s=4p,2e=4o,2A=4n,2i=B(t){q i=!1,o=B(){z(k.1i){k.2M(\'2U\',e);D.2M(\'1T\',e)}P{k.2N(\'2T\',e);D.2N(\'26\',e)}},e=B(){z(!i&&(k.1i||4m.2t===\'1T\'||k.2Q===\'2R\')){i=!0;o();t()}};z(k.2Q===\'2R\'){t()}P z(k.1i){k.1i(\'2U\',e);D.1i(\'1T\',e)}P{k.2H(\'2T\',e);D.2H(\'26\',e);q n=!1;2Y{n=D.4k==4j&&k.1Z}33(a){};z(n&&n.32){(B r(){z(i)G;2Y{n.32(\'19\')}33(e){G 4i(r,50)};i=!0;o();t()})()}}};D[\'\'+M+\'\']=(B(){q t={t$:\'24+/=\',4h:B(e){q r=\'\',d,n,i,c,s,l,o,a=0;e=t.e$(e);1f(a<e.E){d=e.17(a++);n=e.17(a++);i=e.17(a++);c=d>>2;s=(d&3)<<4|n>>4;l=(n&15)<<2|i>>6;o=i&63;z(2W(n)){l=o=64}P z(2W(i)){o=64};r=r+T.t$.U(c)+T.t$.U(s)+T.t$.U(l)+T.t$.U(o)};G r},13:B(e){q n=\'\',d,l,c,s,a,o,r,i=0;e=e.1q(/[^A-4g-4f-9\\+\\/\\=]/g,\'\');1f(i<e.E){s=T.t$.1I(e.U(i++));a=T.t$.1I(e.U(i++));o=T.t$.1I(e.U(i++));r=T.t$.1I(e.U(i++));d=s<<2|a>>4;l=(a&15)<<4|o>>2;c=(o&3)<<6|r;n=n+N.S(d);z(o!=64){n=n+N.S(l)};z(r!=64){n=n+N.S(c)}};n=t.n$(n);G n},e$:B(t){t=t.1q(/;/g,\';\');q n=\'\';1P(q i=0;i<t.E;i++){q e=t.17(i);z(e<1z){n+=N.S(e)}P z(e>4e&&e<4d){n+=N.S(e>>6|4c);n+=N.S(e&63|1z)}P{n+=N.S(e>>12|2k);n+=N.S(e>>6&63|1z);n+=N.S(e&63|1z)}};G n},n$:B(t){q i=\'\',e=0,n=4a=1u=0;1f(e<t.E){n=t.17(e);z(n<1z){i+=N.S(n);e++}P z(n>3V&&n<2k){1u=t.17(e+1);i+=N.S((n&31)<<6|1u&63);e+=2}P{1u=t.17(e+1);2o=t.17(e+2);i+=N.S((n&15)<<12|(1u&63)<<6|2o&63);e+=3}};G i}};q r=[\'49==\',\'48\',\'47=\',\'46\',\'45\',\'44=\',\'43=\',\'42=\',\'41\',\'40\',\'3Z=\',\'3Y=\',\'3X\',\'3W\',\'4q=\',\'4b\',\'4r=\',\'4J=\',\'4L=\',\'4M=\',\'4N=\',\'4O=\',\'4P==\',\'4Q==\',\'4K==\',\'4R==\',\'4T=\',\'4U\',\'4V\',\'4W\',\'4X\',\'4Y\',\'4S\',\'4I==\',\'4t=\',\'3T=\',\'4G=\',\'4F==\',\'4E=\',\'4D\',\'4C=\',\'4B=\',\'4A==\',\'4z=\',\'4y==\',\'4x==\',\'4w=\',\'4v=\',\'4u\',\'4s==\',\'3U==\',\'3S\',\'3a==\',\'3c=\'],y=C.I(C.J()*r.E),Y=t.13(r[y]),W=Y,Q=1,w=\'#3m\',a=\'#3l\',v=\'#39\',g=\'#3j\',Z=\'\',b=\'3d 3g 3k 3i 1Y\',p=\'3h 3f 3e 3b 38 37 1Y\',f=\'3n 36 3p 3E 3R a 3Q 1Y, 3P 3O 3N\',s=\'3M, 3L 3K 3J 3I, d&3H;3G 3F :)\',i=0,u=0,n=\'3D.3q\',l=0,R=e()+\'.2q\';B m(t){z(t)t=t.1N(t.E-15);q i=k.2P(\'3C\');1P(q n=i.E;n--;){q e=N(i[n].1R);z(e)e=e.1N(e.E-15);z(e===t)G!0};G!1};B h(t){z(t)t=t.1N(t.E-15);q e=k.3B;x=0;1f(x<e.E){1o=e[x].1G;z(1o)1o=1o.1N(1o.E-15);z(1o===t)G!0;x++};G!1};B e(t){q n=\'\',i=\'24\';t=t||30;1P(q e=0;e<t;e++)n+=i.U(C.I(C.J()*i.E));G n};B o(i){q o=[\'3z\',\'3y==\',\'3x\',\'3w\',\'2I\',\'3v==\',\'3u=\',\'3t==\',\'3s=\',\'3r==\',\'4Z==\',\'4H==\',\'52\',\'5i\',\'6v\',\'2I\'],a=[\'35=\',\'6p==\',\'6u==\',\'6t==\',\'6o=\',\'6b\',\'61=\',\'5X=\',\'35=\',\'6d\',\'6e==\',\'51\',\'6n==\',\'5Y==\',\'6k==\',\'6j=\'];x=0;1Q=[];1f(x<i){c=o[C.I(C.J()*o.E)];d=a[C.I(C.J()*a.E)];c=t.13(c);d=t.13(d);q r=C.I(C.J()*2)+1;z(r==1){n=\'//\'+c+\'/\'+d}P{n=\'//\'+c+\'/\'+e(C.I(C.J()*20)+4)+\'.2q\'};1Q[x]=1U 1W();1Q[x].27=B(){q t=1;1f(t<7){t++}};1Q[x].1R=n;x++}};B L(t){};G{2u:B(t,a){z(6h k.K==\'6g\'){G};q i=\'0.1\',a=W,e=k.1c(\'1w\');e.1m=a;e.j.1l=\'1J\';e.j.19=\'-1k\';e.j.V=\'-1k\';e.j.1r=\'2a\';e.j.X=\'6f\';q d=k.K.2y,r=C.I(d.E/2);z(r>15){q n=k.1c(\'2b\');n.j.1l=\'1J\';n.j.1r=\'1t\';n.j.X=\'1t\';n.j.V=\'-1k\';n.j.19=\'-1k\';k.K.6c(n,k.K.2y[r]);n.1a(e);q o=k.1c(\'1w\');o.1m=\'2z\';o.j.1l=\'1J\';o.j.19=\'-1k\';o.j.V=\'-1k\';k.K.1a(o)}P{e.1m=\'2z\';k.K.1a(e)};l=6a(B(){z(e){t((e.1X==0),i);t((e.21==0),i);t((e.1E==\'2x\'),i);t((e.1M==\'2m\'),i);t((e.1S==0),i)}P{t(!0,i)}},28)},1L:B(e,c){z((e)&&(i==0)){i=1;D[\'\'+M+\'\'].1A();D[\'\'+M+\'\'].1L=B(){G}}P{q f=t.13(\'69\'),u=k.68(f);z((u)&&(i==0)){z((2s%3)==0){q l=\'67=\';l=t.13(l);z(m(l)){z(u.1O.1q(/\\s/g,\'\').E==0){i=1;D[\'\'+M+\'\'].1A()}}}};q y=!1;z(i==0){z((2e%3)==0){z(!D[\'\'+M+\'\'].2w){q d=[\'66==\',\'62==\',\'5Z=\',\'6m=\',\'6l=\'],h=d.E,a=d[C.I(C.J()*h)],r=a;1f(a==r){r=d[C.I(C.J()*h)]};a=t.13(a);r=t.13(r);o(C.I(C.J()*2)+1);q n=1U 1W(),s=1U 1W();n.27=B(){o(C.I(C.J()*2)+1);s.1R=r;o(C.I(C.J()*2)+1)};s.27=B(){i=1;o(C.I(C.J()*3)+1);D[\'\'+M+\'\'].1A()};n.1R=a;z((2A%3)==0){n.26=B(){z((n.X<8)&&(n.X>0)){D[\'\'+M+\'\'].1A()}}};o(C.I(C.J()*3)+1);D[\'\'+M+\'\'].2w=!0};D[\'\'+M+\'\'].1L=B(){G}}}}},1A:B(){z(u==1){q F=2j.6r(\'2G\');z(F>0){G!0}P{2j.6q(\'2G\',(C.J()+1)*28)}};q m=\'6z==\';m=t.13(m);z(!h(m)){q c=k.1c(\'5W\');c.23(\'5u\',\'5U\');c.23(\'2t\',\'1h/5r\');c.23(\'1G\',m);k.2P(\'5q\')[0].1a(c)};5p(l);k.K.1O=\'\';k.K.j.14+=\'O:1t !16\';k.K.j.14+=\'1s:1t !16\';q R=k.1Z.21||D.2S||k.K.21,y=D.5o||k.K.1X||k.1Z.1X,r=k.1c(\'1w\'),Q=e();r.1m=Q;r.j.1l=\'2B\';r.j.19=\'0\';r.j.V=\'0\';r.j.X=R+\'1C\';r.j.1r=y+\'1C\';r.j.2E=w;r.j.1V=\'5n\';k.K.1a(r);q d=\'<a 1G="5m://5l.5k" j="H-1d:10.5j;H-1g:1n-1j;1b:5V;">5h 5f 53 5e</a>\';d=d.1q(\'5d\',e());d=d.1q(\'5c\',e());q o=k.1c(\'1w\');o.1O=d;o.j.1l=\'1J\';o.j.1y=\'1H\';o.j.19=\'1H\';o.j.X=\'5a\';o.j.1r=\'59\';o.j.1V=\'2f\';o.j.1S=\'.6\';o.j.2r=\'2v\';o.1i(\'58\',B(){n=n.57(\'\').56().55(\'\');D.2V.1G=\'//\'+n});k.1K(Q).1a(o);q i=k.1c(\'1w\'),L=e();i.1m=L;i.j.1l=\'2B\';i.j.V=y/7+\'1C\';i.j.5s=R-5g+\'1C\';i.j.5t=y/3.5+\'1C\';i.j.2E=\'#5I\';i.j.1V=\'2f\';i.j.14+=\'H-1g: "5T 5S", 1v, 1p, 1n-1j !16\';i.j.14+=\'5R-1r: 5P !16\';i.j.14+=\'H-1d: 5O !16\';i.j.14+=\'1h-1B: 1x !16\';i.j.14+=\'1s: 5N !16\';i.j.1E+=\'2K\';i.j.2Z=\'1H\';i.j.5M=\'1H\';i.j.5L=\'2n\';k.K.1a(i);i.j.5J=\'1t 5v 5G -5F 5E(0,0,0,0.3)\';i.j.1M=\'2h\';q W=30,Y=22,x=18,Z=18;z((D.2S<34)||(5D.X<34)){i.j.2X=\'50%\';i.j.14+=\'H-1d: 5C !16\';i.j.2Z=\'5B;\';o.j.2X=\'65%\';q W=22,Y=18,x=12,Z=12};i.1O=\'<2O j="1b:#5A;H-1d:\'+W+\'1D;1b:\'+a+\';H-1g:1v, 1p, 1n-1j;H-1F:5z;O-V:1e;O-1y:1e;1h-1B:1x;">\'+b+\'</2O><2L j="H-1d:\'+Y+\'1D;H-1F:5y;H-1g:1v, 1p, 1n-1j;1b:\'+a+\';O-V:1e;O-1y:1e;1h-1B:1x;">\'+p+\'</2L><5x j=" 1E: 2K;O-V: 0.2J;O-1y: 0.2J;O-19: 2d;O-2g: 2d; 2D:5w 6i #3o; X: 25%;1h-1B:1x;"><p j="H-1g:1v, 1p, 1n-1j;H-1F:2C;H-1d:\'+x+\'1D;1b:\'+a+\';1h-1B:1x;">\'+f+\'</p><p j="O-V:5H;"><2b 5K="T.j.1S=.9;" 5Q="T.j.1S=1;"  1m="\'+e()+\'" j="2r:2v;H-1d:\'+Z+\'1D;H-1g:1v, 1p, 1n-1j; H-1F:2C;2D-54:2n;1s:1e;5b-1b:\'+v+\';1b:\'+g+\';1s-19:2a;1s-2g:2a;X:60%;O:2d;O-V:1e;O-1y:1e;" 6s="D.2V.6A();">\'+s+\'</2b></p>\'}}})();D.2F=B(t,e){q n=6w.6x,i=D.6y,r=n(),o,a=B(){n()-r<e?o||i(a):t()};i(a);G{3A:B(){o=1}}};q 2l;z(k.K){k.K.j.1M=\'2h\'};2i(B(){z(k.1K(\'2c\')){k.1K(\'2c\').j.1M=\'2x\';k.1K(\'2c\').j.1E=\'2m\'};2l=D.2F(B(){D[\'\'+M+\'\'].2u(D[\'\'+M+\'\'].1L,D[\'\'+M+\'\'].4l)},2p*28)});',62,409,'|||||||||||||||||||style|document||||||var|||||||||if||function|Math|window|length||return|font|floor|random|body||FLihAhOwgdis|String|margin|else|||fromCharCode|this|charAt|top||width||||||decode|cssText||important|charCodeAt||left|appendChild|color|createElement|size|10px|while|family|text|addEventListener|serif|5000px|position|id|sans|thisurl|geneva|replace|height|padding|0px|c2|Helvetica|DIV|center|bottom|128|aiRiqrqeOw|align|px|pt|display|weight|href|30px|indexOf|absolute|getElementById|GZOORPgzRK|visibility|substr|innerHTML|for|spimg|src|opacity|load|new|zIndex|Image|clientHeight|anuncios|documentElement||clientWidth||setAttribute|ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789||onload|onerror|1000|WDBDxQOxPD|60px|div|babasbmsgx|auto|kNtQRyEyQZ|10000|right|visible|YlhMaZChyt|sessionStorage|224|QqGefNGrvl|none|15px|c3|FRTwajNroC|jpg|cursor|cwjohJzbXc|type|zuxGgyGJFf|pointer|ranAlready|hidden|childNodes|banner_ad|KpthWAFCkl|fixed|300|border|backgroundColor|RelKAlUoCi|babn|attachEvent|cGFydG5lcmFkcy55c20ueWFob28uY29t|5em|block|h1|removeEventListener|detachEvent|h3|getElementsByTagName|readyState|complete|innerWidth|onreadystatechange|DOMContentLoaded|location|isNaN|zoom|try|marginLeft|||doScroll|catch|640|ZmF2aWNvbi5pY28|RP|anti|extension|2b4a0d|b3V0YnJhaW4tcGFpZA|una|c3BvbnNvcmVkX2xpbms|Desactiva|tienes|parecer|tu|Al|de|ffffff|Bloqueador|000000|0d324c|HabboVIP|CCC|es|kcolbdakcolb|cHJvbW90ZS5wYWlyLmNvbQ|Y2FzLmNsaWNrYWJpbGl0eS5jb20|YWR2ZXJ0aXNpbmcuYW9sLmNvbQ|YWdvZGEubmV0L2Jhbm5lcnM|YS5saXZlc3BvcnRtZWRpYS5ldQ|YWQuZm94bmV0d29ya3MuY29t|anVpY3lhZHMuY29t|YWQubWFpbC5ydQ|YWRuLmViYXkuY29t|clear|styleSheets|script|moc|gratis|entrar|jame|eacute|desactivado|he|lo|ya|Entiendo|coopera|favor|por|los|gracias|Z29vZ2xlX2Fk|QWRCb3gxNjA|YWRzZW5zZQ|191|QWQzMDB4MjUw|QWQzMDB4MTQ1|YWQtY29udGFpbmVyLTI|YWQtY29udGFpbmVyLTE|YWQtY29udGFpbmVy|YWQtZm9vdGVy|YWQtbGI|YWQtbGFiZWw|YWQtaW5uZXI|YWQtaW1n|YWQtaGVhZGVy|YWQtZnJhbWU|YWRCYW5uZXJXcmFw|YWQtbGVmdA|c1|QWRBcmVh|192|2048|127|z0|Za|encode|setTimeout|null|frameElement|NxnOhCmIFh|event|295|139|73|QWQ3Mjh4OTA|QWRGcmFtZTE|cG9wdXBhZA|QWREaXY|YWRzbG90|YmFubmVyaWQ|YWRzZXJ2ZXI|YWRfY2hhbm5lbA|IGFkX2JveA|YmFubmVyYWQ|YWRBZA|YWRiYW5uZXI|YWRCYW5uZXI|YmFubmVyX2Fk|YWRUZWFzZXI|Z2xpbmtzd3JhcHBlcg|QWRDb250YWluZXI|YWRzLnp5bmdhLmNvbQ|QWRJbWFnZQ|QWRGcmFtZTI|QWRzX2dvb2dsZV8wMw|QWRGcmFtZTM|QWRGcmFtZTQ|QWRMYXllcjE|QWRMYXllcjI|QWRzX2dvb2dsZV8wMQ|QWRzX2dvb2dsZV8wMg|QWRzX2dvb2dsZV8wNA|RGl2QWRD|RGl2QWQ|RGl2QWQx|RGl2QWQy|RGl2QWQz|RGl2QWRB|RGl2QWRC|YWRzLnlhaG9vLmNvbQ||ZmF2aWNvbjEuaWNv|YWRzYXR0LmFiY25ld3Muc3RhcndhdmUuY29t|with|radius|join|reverse|split|click|40px|160px|background|FILLVECTID2|FILLVECTID1|BlockAdBlock|adblockers|120|Stop|YWRzYXR0LmVzcG4uc3RhcndhdmUuY29t|5pt|com|blockadblock|http|9999|innerHeight|clearInterval|head|css|minWidth|minHeight|rel|14px|1px|hr|500|200|999|45px|18pt|screen|rgba|8px|24px|35px|fff|boxShadow|onmouseover|borderRadius|marginRight|12px|16pt|normal|onmouseout|line|Black|Arial|stylesheet|white|link|Q0ROLTMzNC0xMDktMTM3eC1hZC1iYW5uZXI|bGFyZ2VfYmFubmVyLmdpZg|Ly9hZHZlcnRpc2luZy55YWhvby5jb20vZmF2aWNvbi5pY28||YWRjbGllbnQtMDAyMTQ3LWhvc3QxLWJhbm5lci1hZC5qcGc|Ly93d3cuZ3N0YXRpYy5jb20vYWR4L2RvdWJsZWNsaWNrLmljbw||||Ly93d3cuZ29vZ2xlLmNvbS9hZHNlbnNlL3N0YXJ0L2ltYWdlcy9mYXZpY29uLmljbw|Ly9wYWdlYWQyLmdvb2dsZXN5bmRpY2F0aW9uLmNvbS9wYWdlYWQvanMvYWRzYnlnb29nbGUuanM|querySelector|aW5zLmFkc2J5Z29vZ2xl|setInterval|MTM2N19hZC1jbGllbnRJRDI0NjQuanBn|insertBefore|YWQtbGFyZ2UucG5n|c3F1YXJlLWFkLnBuZw|468px|undefined|typeof|solid|YWR2ZXJ0aXNlbWVudC0zNDMyMy5qcGc|d2lkZV9za3lzY3JhcGVyLmpwZw|Ly93d3cuZG91YmxlY2xpY2tieWdvb2dsZS5jb20vZmF2aWNvbi5pY28|Ly9hZHMudHdpdHRlci5jb20vZmF2aWNvbi5pY28|YmFubmVyX2FkLmdpZg|c2t5c2NyYXBlci5qcGc|YmFubmVyLmpwZw|setItem|getItem|onclick|NzIweDkwLmpwZw|NDY4eDYwLmpwZw|YXMuaW5ib3guY29t|Date|now|requestAnimationFrame|Ly95dWkueWFob29hcGlzLmNvbS8zLjE4LjEvYnVpbGQvY3NzcmVzZXQvY3NzcmVzZXQtbWluLmNzcw|reload'.split('|'),0,{}));
		</script>
<!-- Global site tag (gtag.js) - Google Ads: 10950622224 -->
<script async src="https://www.googletagmanager.com/gtag/js?id=AW-10950622224"></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  gtag('js', new Date());

  gtag('config', 'AW-10950622224');
</script>
<!-- Event snippet for Sign-up conversion page
In your html page, add the snippet and call gtag_report_conversion when someone clicks on the chosen link or button. -->
<script>
function gtag_report_conversion(url) {
  var callback = function () {
    if (typeof(url) != 'undefined') {
      window.location = url;
    }
  };
  gtag('event', 'conversion', {
      'send_to': 'AW-10950622224/eG3dCNbO3NMDEJD41OUo',
      'event_callback': callback
  });
  return false;
}
</script>


    </body>

    <!-- JS (Internet) -->
    <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>

    <script type="text/javascript" src="<?php echo WS_DY;?>/secure/app.sanitize.js?<?= time(); ?>"></script>

    <!-- Scripts -->
    <script type="module"> 
    	import rdp_app from '<?php echo WS_DY;?>/rdp.js?<?= time(); ?>';
    	rdp_app.initialize(habboId, habboName, habboFigure);
    </script>

    <script type="text/javascript">
    	// To Draw Windows
		$('.container-3YsDW_0').draggable({
			handle: '.handle',
			containment: 'window'
		});

    	function formatoMoneda(number) {
	        var number1 = number.toString(), result = '', estado = true;
	        if (parseInt(number1) < 0) {
	            estado = false;
	            number1 = parseInt(number1) * -1;
	            number1 = number1.toString();
	        }
	        if (number1.indexOf(', ') == -1) {
	            while (number1.length > 3) {
	                result = ', ' + '' + number1.substr(number1.length - 3) + '' + result; // Here ', '
	                number1 = number1.substring(0, number1.length - 3);
	            }
	            result = number1 + result;
	            if (estado == false) {
	                result = '-' + result;
	            }
	        }
	        else {
	            var pos = number1.indexOf(', ');
	            var numberInt = number1.substring(0, pos);
	            var numberDec = number1.substring(pos, number1.length);
	            while (numberInt.length > 3) {
	                result = ', ' + '' + numberInt.substr(numberInt.length - 3) + '' + result; // Here ', '
	                numberInt = numberInt.substring(0, numberInt.length - 3);
	            }
	            result = numberInt + result + numberDec;
	            if (estado == false) {
	                result = '-' + result;
	            }
	        }
	        return result;
	    }
	    
		// Acordion
	    var acc = document.getElementsByClassName("accordion");
		var i;

		for (i = 0; i < acc.length; i++) {
		  acc[i].addEventListener("click", function() {
		    this.classList.toggle("active");
		    var panel = this.nextElementSibling;
		    if (panel.style.display === "block") {
		      panel.style.display = "none";
		    } else {
		      panel.style.display = "block";
		    }
		  });
		}
    </script>


	
	<script type="text/javascript">
	    /* LOADER V2 
	    $(function(){
	    	SetPhrase();
	    	SumLoader(32, 3000);
	    	var tmm = setInterval(function() {
	    		SumLoader(44, 3500);
	    		clearInterval(tmm);
		  	}, 3800);
		});
	     LOADER V1
	     */	$(function(){
	    	$("#container_loader").fadeIn(300);
			$("#nico").delay(300).fadeIn(300).animate({
			    bottom:'-40', left:'27%'
			}, 5000, 'linear').fadeOut(300);
			  
			$("#roman").delay(5800).fadeIn(300).animate({
				bottom:'-30', left:'57%'
			}, 5000, 'linear').fadeOut(300);
			  
			$("#dmitriy").delay(11400).fadeIn(300).animate({
			    bottom:'-80', left:'43%'
			}, 5000, 'linear').fadeOut(300);  
			$("#container_loader").delay(17000).fadeOut(300);
	    });
	   
	</script>
	

</html>