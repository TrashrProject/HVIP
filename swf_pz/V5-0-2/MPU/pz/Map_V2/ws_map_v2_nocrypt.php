<!DOCTYPE HTML>
<html>
<head>
	<title>PixelZone - Mapa</title>
	<link rel="stylesheet" href="asset/ui.css?p=<?php echo time(); ?>" />
	<link href="https://fonts.googleapis.com/css2?family=Raleway:wght@900&display=swap" rel="stylesheet">
	<script src="https://code.jquery.com/jquery-3.5.1.min.js" integrity="sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=" crossorigin="anonymous"></script>
	<script src="src/Raf.js"></script>
	<script src="src/Animate.js"></script>
	<script src="src/Scroller.js?p=<?php echo time(); ?>"></script>

	<script src="asset/Tiling.js?p=<?php echo time(); ?>"></script>
</head>
<body>
	<div id="container">
		<canvas id="canvasMap"></canvas>

		<div id="settings">
			<div>
				<button id="ToggleText" class="map-button" onclick="ToggleText();">Ocultar nombres</button>
				<button id="zoomIn" class="widget-zoom-button widget-zoom-in"><div class="widget-zoom-icon"></div></button>
				<button id="zoomOut" class="widget-zoom-button widget-zoom-out"><div class="widget-zoom-icon"></div></button>
			</div>

			<div class="MoveControls">
				<button id="scrollByUp" class="widget-zoom-button" style="left:28px;bottom:0">&uarr;</button>
				<button id="scrollByDown" class="widget-zoom-button" style="left:28px">&darr;</button>
				<button id="scrollByLeft" class="widget-zoom-button">&larr;</button>
				<button id="scrollByRight" class="widget-zoom-button" style="left:56px">&rarr;</button>
			</div>

			<div style="display: none;">
				<div><label for="scrollingX">ScrollingX: </label><input type="checkbox" id="scrollingX" checked/></div>
				<div><label for="scrollingY">ScrollingY: </label><input type="checkbox" id="scrollingY" checked/></div>
				<div><label for="animating">Animating: </label><input type="checkbox" id="animating" checked/></div>
				<div><label for="bouncing">Bouncing: </label><input type="checkbox" id="bouncing" checked/></div>
				<div><label for="locking">Locking: </label><input type="checkbox" id="locking" checked/></div>

				<div><label for="zooming">Zooming: </label><input type="checkbox" id="zooming" checked/></div>
				<div><label for="minZoom">Min Zoom: </label><input type="text" id="minZoom" size="5" value="0.5"/></div>
				<div><label for="maxZoom">Max Zoom: </label><input type="text" id="maxZoom" size="5" value="3"/></div>
				<div><label for="zoomLevel">Zoom Level: </label><input type="text" id="zoomLevel" size="5"/></div>
				<div><button id="zoom">Zoom to Level</button></div>
				<div><label for="scrollLeft">Scroll Left: </label><input type="text" id="scrollLeft" size="9"/></div>
				<div><label for="scrollTop">Scroll Top: </label><input type="text" id="scrollTop" size="9"/></div>
				<div><button id="scrollTo">Scroll to Coords</button></div>
			</div>
		</div>
	</div>
	
	<!-- Custom rendering code -->
	<script type="text/javascript">

	// Settings
	var contentWidth = 1254.4;
	var contentHeight = 544.2;
	var cellWidth = 118;
	var cellHeight = 53;
	
	var canvasMap = document.getElementById('canvasMap');
	var context = canvasMap.getContext('2d');
	var tiling = new Tiling;

	var ShowText = true;

	// GRANGE
	var GRANGE = new Image();
	GRANGE.src = "https://swf.habbovip.us/MPU/pz/pz_grange.png";
	// MALL
	var MALL = new Image();
	MALL.src = "https://swf.habbovip.us/MPU/pz/pz_mall.png";
	// PEDERNAL
	var PEDERNAL = new Image();
	PEDERNAL.src = "https://swf.habbovip.us/MPU/pz/pz_pedernal.png";
	// TRASH
	var TRASH = new Image();
	TRASH.src = "https://swf.habbovip.us/MPU/pz/pz_trash.png";
	// PALOOZA
	var PALOOZA = new Image();
	PALOOZA.src = "https://swf.habbovip.us/MPU/pz/pz_palooza.png";
	// DAVID
	var DAVID = new Image();
	DAVID.src = "https://swf.habbovip.us/MPU/pz/pz_david.png";
	// TURBINA
	var TURBINA = new Image();
	TURBINA.src = "https://swf.habbovip.us/MPU/pz/pz_turbina.png";	
	// BARRETA
	var BARRETA = new Image();
	BARRETA.src = "https://swf.habbovip.us/MPU/pz/pz_barreta.png";
	// BETHOVEN
	var BETHOVEN = new Image();
	BETHOVEN.src = "https://swf.habbovip.us/MPU/pz/pz_bethoven.png";

	// GYM
	var GYM = new Image();
	GYM.src = "https://swf.habbovip.us/MPU/pz/pz_gym.png";
	// NARANJAL
	var NARANJAL = new Image();
	NARANJAL.src = "https://swf.habbovip.us/MPU/pz/pz_naranjal.png";
	// CASINO
	var CASINO = new Image();
	CASINO.src = "https://swf.habbovip.us/MPU/pz/pz_casino.png";
	// BANK
	var BANK = new Image();
	BANK.src = "https://swf.habbovip.us/MPU/pz/pz_bank.png";
	// COURT
	var COURT = new Image();
	COURT.src = "https://swf.habbovip.us/MPU/pz/pz_court.png";
	// MAGANATE
	var MAGANATE = new Image();
	MAGANATE.src = "https://swf.habbovip.us/MPU/pz/pz_magnate.png";
	// VELVET
	var VELVET = new Image();
	VELVET.src = "https://swf.habbovip.us/MPU/pz/pz_velvet.png";
	// MINA
	var MINA = new Image();
	MINA.src = "https://swf.habbovip.us/MPU/pz/pz_mina.png";

	// CAFE
	var CAFE = new Image();
	CAFE.src = "https://swf.habbovip.us/MPU/pz/pz_cafe.png";
	// MCDONALDS
	var MCDONALDS = new Image();
	MCDONALDS.src = "https://swf.habbovip.us/MPU/pz/pz_mcdonalds.png";
	// PRISON
	var PRISON = new Image();
	PRISON.src = "https://swf.habbovip.us/MPU/pz/pz_prison.png";
	// GAS
	var GAS = new Image();
	GAS.src = "https://swf.habbovip.us/MPU/pz/pz_gas.png";
	// CENTRO
	var CENTRO = new Image();
	CENTRO.src = "https://swf.habbovip.us/MPU/pz/pz_centro.png";
	// CLOTHING
	var CLOTHING = new Image();
	CLOTHING.src = "https://swf.habbovip.us/MPU/pz/pz_clothing.png";
	// PHONES
	var PHONES = new Image();
	PHONES.src = "https://swf.habbovip.us/MPU/pz/pz_phones.png";
	// SUBWAY
	var SUBWAY = new Image();
	SUBWAY.src = "https://swf.habbovip.us/MPU/pz/pz_subway.png";
	// LIBRERIA
	var LIBRERIA = new Image();
	LIBRERIA.src = "https://swf.habbovip.us/MPU/pz/pz_libreria.png";
	// B_SANTO
	var B_SANTO = new Image();
	B_SANTO.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_santo.png";

	// MUNICIP
	var MUNICIP = new Image();
	MUNICIP.src = "https://swf.habbovip.us/MPU/pz/pz_municipalidad.png";
	// APPART
	var APPART = new Image();
	APPART.src = "https://swf.habbovip.us/MPU/pz/pz_appartments.png";
	// PATRIA
	var PATRIA = new Image();
	PATRIA.src = "https://swf.habbovip.us/MPU/pz/pz_patria.png";
	// NOCHEROS
	var NOCHEROS = new Image();
	NOCHEROS.src = "https://swf.habbovip.us/MPU/pz/pz_nocheros.png";
	// AMMU
	var AMMU = new Image();
	AMMU.src = "https://swf.habbovip.us/MPU/pz/pz_ammunation.png";
	// B_TRUCHA
	var B_TRUCHA = new Image();
	B_TRUCHA.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_trucha.png";
	// B_MERCED
	var B_MERCED = new Image();
	B_MERCED.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_merced.png";
	// B_OLVIDO
	var B_OLVIDO = new Image();
	B_OLVIDO.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_olvido.png";
	// B_AZTECA
	var B_AZTECA = new Image();
	B_AZTECA.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_azteca.png";

	// EASTER
	var EASTER = new Image();
	EASTER.src = "https://swf.habbovip.us/MPU/pz/pz_easterbasin.png";
	// WASHINGTON
	var WASHINGTON = new Image();
	WASHINGTON.src = "https://swf.habbovip.us/MPU/pz/pz_washington.png";
	// CHURCH
	var CHURCH = new Image();
	CHURCH.src = "https://swf.habbovip.us/MPU/pz/pz_church.png";
	// B_GROVE
	var B_GROVE = new Image();
	B_GROVE.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_grove.png";
	// B_RAZA
	var B_RAZA = new Image();
	B_RAZA.src = "https://swf.habbovip.us/MPU/pz/pz_barrio_raza.png";

	// TAXIS
	var TAXIS = new Image();
	TAXIS.src = "https://swf.habbovip.us/MPU/pz/pz_taxis.png";
	// BLUEBERRY
	var BLUEBERRY = new Image();
	BLUEBERRY.src = "https://swf.habbovip.us/MPU/pz/pz_blueberry.png";
	// DEALER
	var DEALER = new Image();
	DEALER.src = "https://swf.habbovip.us/MPU/pz/pz_dealer.png";
	// SPA
	var SPA = new Image();
	SPA.src = "https://swf.habbovip.us/MPU/pz/pz_spa.png";
	// ICECREAM
	var ICECREAM = new Image();
	ICECREAM.src = "https://swf.habbovip.us/MPU/pz/pz_icecream.png";

	// CINE
	var CINE = new Image();
	CINE.src = "https://swf.habbovip.us/MPU/pz/pz_cine.png";

	// P_SM
	var P_SM = new Image();
	P_SM.src = "https://swf.habbovip.us/MPU/pz/pz_playa_sm.png";
	// P_LB
	var P_LB = new Image();
	P_LB.src = "https://swf.habbovip.us/MPU/pz/pz_playa_lb.png";

	// Canvas renderer
	var render = function(left, top, zoom) {
		
		// Sync current dimensions with canvas
		canvasMap.width = clientWidth;
		canvasMap.height = clientHeight;
		
		// Full clearing
		context.clearRect(0, 0, clientWidth, clientHeight);

		// Use tiling
		tiling.setup(clientWidth, clientHeight, contentWidth, contentHeight, cellWidth, cellHeight);
		tiling.render(left, top, zoom, paint);
	};
	
	
	// Cell Paint Logic
	var paint = function(row, col, left, top, width, height, zoom) {
		// Adjusts
		left += 68;
		top += 10;

		if(row === 0 && col === 0){
			context.drawImage(GRANGE, left + (510*zoom), top, (GRANGE.width/10) * zoom, (GRANGE.height/10) * zoom);
			PrintText("[GRANJA] Pedernal", 22, left + (540*zoom), top + (50*zoom), zoom);
		}
		if(row === 0 && col === 1){
			context.drawImage(MALL, left + (326*zoom), top + (50*zoom), (MALL.width/10) * zoom, (MALL.height/10) * zoom);
			PrintText("[CALLE] Morralla", 18, left + (315*zoom), top + (80*zoom), zoom);
		}
		if(row === 0 && col === 2){
			context.drawImage(PEDERNAL, left + (310*zoom), top + (85*zoom), (PEDERNAL.width/10) * zoom, (PEDERNAL.height/10) * zoom);
			PrintText("[CALLE]", 18, left + (360*zoom), top + (105*zoom), zoom);
			PrintText("Pedernal", 18, left + (353*zoom), top + (118*zoom), zoom);
		}
		if(row === 0 && col === 3){
			context.drawImage(TRASH, left + (143*zoom), top + (98*zoom), (TRASH.width/10) * zoom, (TRASH.height/10) * zoom);
			PrintText("[CAMPO]", 18, left + (152*zoom), top + (105*zoom), zoom);
			PrintText("Basurero", 18, left + (150*zoom), top + (118*zoom), zoom);
		}
		if(row === 0 && col === 4){
			context.drawImage(PALOOZA, left + (141*zoom), top + (13*zoom), (PALOOZA.width/10) * zoom, (PALOOZA.height/10) * zoom);
			PrintText("[CAMPO] Palooza Land", 22, left + (220*zoom), top + (130*zoom), zoom);
		}
		if(row === 0 && col === 5){
			context.drawImage(DAVID, left + (272*zoom), top + (132*zoom), (DAVID.width/10) * zoom, (DAVID.height/10) * zoom);
			PrintText("[CALLE] El David", 20, left + (285*zoom), top + (185*zoom), zoom);
		}
		if(row === 0 && col === 6){
			context.drawImage(TURBINA, left + (243*zoom), top + (170*zoom), (TURBINA.width/10) * zoom, (TURBINA.height/10) * zoom);
			PrintText("[CALLE] Turbina", 20, left + (290*zoom), top + (250*zoom), zoom);
		}
		if(row === 0 && col === 7){
			context.drawImage(BARRETA, left + (-57*zoom), top + (185*zoom), (BARRETA.width/10) * zoom, (BARRETA.height/10) * zoom);
			PrintText("[CALLE] Barreta", 20, left + (-40*zoom), top + (230*zoom), zoom);
		}
		if(row === 0 && col === 8){
			context.drawImage(BETHOVEN, left + (-85*zoom), top + (221*zoom), (BETHOVEN.width/10) * zoom, (BETHOVEN.height/10) * zoom);
			PrintText("[CALLE] Bethoven", 20, left + (-50*zoom), top + (290*zoom), zoom);
		}
		if(row === 1 && col === 0){
			context.drawImage(GYM, left + (389*zoom), top + (42*zoom), (GYM.width/10) * zoom, (GYM.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (400*zoom), top + (60*zoom), zoom);
			PrintText("Olimpo", 20, left + (405*zoom), top + (75*zoom), zoom);
		}
		if(row === 1 && col === 1){
			context.drawImage(NARANJAL, left + (324*zoom), top + (63*zoom), (NARANJAL.width/10) * zoom, (NARANJAL.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (365*zoom), top + (80*zoom), zoom);
			PrintText("Naranjal", 19, left + (360*zoom), top + (95*zoom), zoom);
		}
		if(row === 1 && col === 2){
			context.drawImage(CASINO, left + (289*zoom), top + (68*zoom), (CASINO.width/10) * zoom, (CASINO.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (342*zoom), top + (100*zoom), zoom);
			PrintText("El Trébol", 19, left + (337*zoom), top + (115*zoom), zoom);
		}
		if(row === 1 && col === 3){
			context.drawImage(BANK, left + (231*zoom), top + (118*zoom), (BANK.width/10) * zoom, (BANK.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (257*zoom), top + (140*zoom), zoom);
			PrintText("La Insignia", 19, left + (243*zoom), top + (155*zoom), zoom);
		}
		if(row === 1 && col === 4){
			context.drawImage(COURT, left + (177*zoom), top + (143*zoom), (COURT.width/10) * zoom, (COURT.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (220*zoom), top + (180*zoom), zoom);
			PrintText("Legislación", 19, left + (200*zoom), top + (195*zoom), zoom);
		}
		if(row === 1 && col === 5){
			context.drawImage(MAGANATE, left + (116*zoom), top + (192*zoom), (MAGANATE.width/10) * zoom, (MAGANATE.height/10) * zoom);
			PrintText("[CALLE] Magnate", 19, left + (135*zoom), top + (230*zoom), zoom);
		}
		if(row === 1 && col === 6){
			context.drawImage(VELVET, left + (62*zoom), top + (231*zoom), (VELVET.width/10) * zoom, (VELVET.height/10) * zoom);
			PrintText("[CALLE] Velvet", 19, left + (58*zoom), top + (270*zoom), zoom);
		}
		if(row === 1 && col === 7){
			context.drawImage(MINA, left + (10*zoom), top + (262*zoom), (MINA.width/10) * zoom, (MINA.height/10) * zoom);
			PrintText("[ZONA] Centro", 20, left + (-5*zoom), top + (300*zoom), zoom);
			PrintText("Minero", 20, left + (30*zoom), top + (315*zoom), zoom);
		}

		if(row === 2 && col === 0){
			context.drawImage(CAFE, left + (245*zoom), top + (-8*zoom), (CAFE.width/10) * zoom, (CAFE.height/10) * zoom);
			PrintText("[CALLE] Cafeína", 20, left + (245*zoom), top + (60*zoom), zoom);
		}
		if(row === 2 && col === 1){
			context.drawImage(MCDONALDS, left + (70*zoom), top + (67*zoom), (MCDONALDS.width/10) * zoom, (MCDONALDS.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (92*zoom), top + (100*zoom), zoom);
			PrintText("Hestia", 20, left + (102*zoom), top + (115*zoom), zoom);
		}
		if(row === 2 && col === 2){
			context.drawImage(PRISON, left + (122*zoom), top + (23*zoom), (PRISON.width/10) * zoom, (PRISON.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (180*zoom), top + (90*zoom), zoom);
			PrintText("Libertad", 20, left + (175*zoom), top + (105*zoom), zoom);
		}
		if(row === 2 && col === 3){
			context.drawImage(GAS, left + (-91*zoom), top + (89*zoom), (GAS.width/10) * zoom, (GAS.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (-20*zoom), top + (140*zoom), zoom);
			PrintText("Petrolera", 20, left + (-30*zoom), top + (155*zoom), zoom);
		}
		if(row === 2 && col === 4){
			context.drawImage(CENTRO, left + (-68*zoom), top + (35*zoom), (CENTRO.width/10) * zoom, (CENTRO.height/10) * zoom);
			PrintText("[CALLE] El Centro", 21, left + (-5*zoom), top + (170*zoom), zoom);
		}
		if(row === 2 && col === 5){
			context.drawImage(CLOTHING, left + (-43*zoom), top + (140*zoom), (CLOTHING.width/10) * zoom, (CLOTHING.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (6*zoom), top + (195*zoom), zoom);
			PrintText("La Moda", 20, left + (3*zoom), top + (210*zoom), zoom);
		}
		if(row === 2 && col === 6){
			context.drawImage(PHONES, left + (-53*zoom), top + (189.7*zoom), (PHONES.width/10) * zoom, (PHONES.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (-28*zoom), top + (210*zoom), zoom);
			PrintText("Marcador", 19, left + (-38*zoom), top + (225*zoom), zoom);
		}
		if(row === 2 && col === 7){
			context.drawImage(SUBWAY, left + (-208*zoom), top + (218.6*zoom), (SUBWAY.width/10) * zoom, (SUBWAY.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (-180*zoom), top + (250*zoom), zoom);
			PrintText("Croissant", 19, left + (-185*zoom), top + (265*zoom), zoom);
		}
		if(row === 2 && col === 8){
			context.drawImage(LIBRERIA, left + (-238*zoom), top + (217.5*zoom), (LIBRERIA.width/10) * zoom, (LIBRERIA.height/10) * zoom);
			PrintText("[CALLE] El", 16, left + (-200*zoom), top + (255*zoom), zoom);
			PrintText("Conocimiento", 16, left + (-215*zoom), top + (267*zoom), zoom);
		}
		if(row === 2 && col === 9){
			context.drawImage(B_SANTO, left + (-297*zoom), top + (261*zoom), (B_SANTO.width/10) * zoom, (B_SANTO.height/10) * zoom);
			PrintText("[BARRIO]", 20, left + (-285*zoom), top + (290*zoom), zoom);
			PrintText("Santo", 20, left + (-270*zoom), top + (305*zoom), zoom);
		}

		if(row === 3 && col === 0){
			context.drawImage(MUNICIP, left + (120*zoom), top + (21.5*zoom), (MUNICIP.width/10) * zoom, (MUNICIP.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (155*zoom), top + (80*zoom), zoom);
			PrintText("Corporativo", 20, left + (140*zoom), top + (92*zoom), zoom);
		}
		if(row === 3 && col === 1){
			context.drawImage(APPART, left + (102*zoom), top + (28.8*zoom), (APPART.width/10) * zoom, (APPART.height/10) * zoom);
			PrintText("[CALLE]", 19, left + (130*zoom), top + (135*zoom), zoom);
			PrintText("La Marieta", 19, left + (115*zoom), top + (150*zoom), zoom);
		}
		if(row === 3 && col === 2){
			context.drawImage(PATRIA, left + (66*zoom), top + (96.3*zoom), (PATRIA.width/10) * zoom, (PATRIA.height/10) * zoom);
			PrintText("[PLAZA] La Patria", 20, left + (100*zoom), top + (165*zoom), zoom);
		}
		if(row === 3 && col === 3){
			context.drawImage(NOCHEROS, left + (128*zoom), top + (171*zoom), (NOCHEROS.width/10) * zoom, (NOCHEROS.height/10) * zoom);
			PrintText("[CALLE] Los", 16, left + (140*zoom), top + (190*zoom), zoom);
			PrintText("Nocheros", 16, left + (155*zoom), top + (203*zoom), zoom);
		}
		if(row === 3 && col === 4){
			context.drawImage(AMMU, left + (74*zoom), top + (187*zoom), (AMMU.width/10) * zoom, (AMMU.height/10) * zoom);
			PrintText("[CALLE]", 18, left + (110*zoom), top + (225*zoom), zoom);
			PrintText("Revólver", 18, left + (108*zoom), top + (238*zoom), zoom);
		}
		if(row === 3 && col === 5){
			context.drawImage(B_TRUCHA, left + (66*zoom), top + (212*zoom), (B_TRUCHA.width/10) * zoom, (B_TRUCHA.height/10) * zoom);
			PrintText("[BARRIO]", 18, left + (95*zoom), top + (235*zoom), zoom);
			PrintText("Trucha", 18, left + (105*zoom), top + (248*zoom), zoom);
		}
		if(row === 3 && col === 6){
			context.drawImage(B_MERCED, left + (6*zoom), top + (239.2*zoom), (B_MERCED.width/10) * zoom, (B_MERCED.height/10) * zoom);
			PrintText("[BARRIO]", 16, left + (30*zoom), top + (267*zoom), zoom);
			PrintText("La Merced", 16, left + (25*zoom), top + (280*zoom), zoom);
		}
		if(row === 3 && col === 7){
			context.drawImage(B_OLVIDO, left + (-222*zoom), top + (236.1*zoom), (B_OLVIDO.width/10) * zoom, (B_OLVIDO.height/10) * zoom);
			PrintText("[BARRIO]", 17, left + (-205*zoom), top + (255*zoom), zoom);
			PrintText("El Olvido", 17, left + (-205*zoom), top + (268*zoom), zoom);
		}
		if(row === 3 && col === 8){
			context.drawImage(B_AZTECA, left + (-282*zoom), top + (265*zoom), (B_AZTECA.width/10) * zoom, (B_AZTECA.height/10) * zoom);
			PrintText("[BARRIO]", 16, left + (-260*zoom), top + (297*zoom), zoom);
			PrintText("Azteca", 16, left + (-250*zoom), top + (310*zoom), zoom);
		}
		
		if(row === 4 && col === 0){
			context.drawImage(EASTER, left + (-86.2*zoom), top + (-51.8*zoom), (EASTER.width/10) * zoom, (EASTER.height/10) * zoom);
			PrintText("[CALLE]", 20, left + (50*zoom), top + (50*zoom), zoom);
			PrintText("Easter Basin", 20, left + (30*zoom), top + (65*zoom), zoom);
		}
		if(row === 4 && col === 1){
			context.drawImage(WASHINGTON, left + (-12*zoom), top + (46*zoom), (WASHINGTON.width/10) * zoom, (WASHINGTON.height/10) * zoom);
			PrintText("[CALLE]", 18, left + (50*zoom), top + (110*zoom), zoom);
			PrintText("Washington", 18, left + (30*zoom), top + (125*zoom), zoom);
		}
		if(row === 4 && col === 2){
			context.drawImage(CHURCH, left + (176.2*zoom), top + (113.1*zoom), (CHURCH.width/10) * zoom, (CHURCH.height/10) * zoom);
			PrintText("[CALLE] La", 20, left + (215*zoom), top + (180*zoom), zoom);
			PrintText("Religión", 20, left + (228*zoom), top + (195*zoom), zoom);
		}
		if(row === 4 && col === 3){
			context.drawImage(B_GROVE, left + (180*zoom), top + (204*zoom), (B_GROVE.width/10) * zoom, (B_GROVE.height/10) * zoom);
			PrintText("[BARRIO]", 20, left + (200*zoom), top + (235*zoom), zoom);
			PrintText("Grove", 20, left + (215*zoom), top + (250*zoom), zoom);
		}
		if(row === 4 && col === 4){
			context.drawImage(B_RAZA, left + (141*zoom), top + (226*zoom), (B_RAZA.width/10) * zoom, (B_RAZA.height/10) * zoom);
			PrintText("[BARRIO]", 16, left + (165*zoom), top + (273*zoom), zoom);
			PrintText("La Raza", 16, left + (172*zoom), top + (286*zoom), zoom);
		}

		if(row === 5 && col === 0){
			context.drawImage(TAXIS, left + (-61*zoom), top + (-37*zoom), (TAXIS.width/10) * zoom, (TAXIS.height/10) * zoom);
			PrintText("[CALLE] Unity", 20, left + (-60*zoom), top + (50*zoom), zoom);
			PrintText("Station", 20, left + (-30*zoom), top + (65*zoom), zoom);
		}
		if(row === 5 && col === 1){
			context.drawImage(BLUEBERRY, left + (-72*zoom), top + (50.5*zoom), (BLUEBERRY.width/10) * zoom, (BLUEBERRY.height/10) * zoom);
			PrintText("[CALLE] Blue Berry", 18, left + (-70*zoom), top + (110*zoom), zoom);
		}
		if(row === 5 && col === 2){
			context.drawImage(DEALER, left + (-53*zoom), top + (62.3*zoom), (DEALER.width/10) * zoom, (DEALER.height/10) * zoom);
			PrintText("[CALLE]", 18, left + (10*zoom), top + (85*zoom), zoom);
			PrintText("Volkswagen", 18, left + (-10*zoom), top + (100*zoom), zoom);
		}
		if(row === 5 && col === 3){
			context.drawImage(SPA, left + (-50*zoom), top + (90*zoom), (SPA.width/10) * zoom, (SPA.height/10) * zoom);
			PrintText("[CALLE] El", 16, left + (-45*zoom), top + (115*zoom), zoom);
			PrintText("Paraíso", 16, left + (-30*zoom), top + (128*zoom), zoom);
		}
		if(row === 5 && col === 4){
			context.drawImage(ICECREAM, left + (-110.5*zoom), top + (111*zoom), (ICECREAM.width/10) * zoom, (ICECREAM.height/10) * zoom);
			PrintText("[CALLE] La", 20, left + (-75*zoom), top + (161*zoom), zoom);
			PrintText("Cereza", 20, left + (-57*zoom), top + (175*zoom), zoom);
		}

		if(row === 6 && col === 0){
			context.drawImage(CINE, left + (234*zoom), top + (56.5*zoom), (CINE.width/10) * zoom, (CINE.height/10) * zoom);
			PrintText("[CALLE] La Fama", 18, left + (270*zoom), top + (130*zoom), zoom);
		}

		if(row === 7 && col === 0){
			context.drawImage(P_SM, left + (120*zoom), top + (28*zoom), (P_SM.width/10) * zoom, (P_SM.height/10) * zoom);
			PrintText("[PLAYA]", 20, left + (160*zoom), top + (70*zoom), zoom);
			PrintText("Santa Mónica", 20, left + (130*zoom), top + (85*zoom), zoom);
		}
		if(row === 7 && col === 1){
			context.drawImage(P_LB, left + (111*zoom), top + (84.3*zoom), (P_LB.width/10) * zoom, (P_LB.height/10) * zoom);
			PrintText("[PLAYA]", 20, left + (170*zoom), top + (115*zoom), zoom);
			PrintText("Las Barrillas", 20, left + (145*zoom), top + (130*zoom), zoom);
		}
		/*
		context.fillStyle = row%2 + col%2 > 0 ? "#ddd" : "#fff";
		context.fillRect(left, top, width, height);
		
		context.fillStyle = "black";
		context.font = (14 * zoom).toFixed(2) + 'px "Helvetica Neue", Helvetica, Arial, sans-serif';
		
		// Pretty primitive text positioning :)
		context.fillText(row + "," + col, left + (6 * zoom), top + (18 * zoom));
		*/	
	};

	function PrintText(text, size, left, top, zoom){
		if(!ShowText)
			return;

		context.fillStyle = "white";
		context.strokeStyle = 'black';
		context.lineWidth = 0.5;
		context.font = 'bold ' + (size * zoom).toFixed(2) + 'px "Raleway", sans-serif';
		context.fillText(text, left, top);
		context.strokeText(text, left, top);
	}

	function ToggleText() {
		ShowText = !ShowText;
		if(ShowText)
			$("#ToggleText").html('Ocultar nombres');
		else
			$("#ToggleText").html('Mostrar nombres');
		scroller.zoomTo(parseFloat(document.getElementById("zoomLevel").value));
	}

	function Init() {
		var tmm = setInterval(function() {
			scroller.zoomTo(parseFloat(0.80));
			clearInterval(tmm);
        }, 1000);

        var tmm2 = setInterval(function() {
			scroller.zoomTo(parseFloat(0.64));
			clearInterval(tmm2);
        }, 2000);
	}

	$( document ).ready(function() {
		Init();
	});
	</script>
	
	<!-- Create Scroller, bind UI layer and mouse/touch events -->
	<script src="asset/ui.js"></script>
</body>
</html>
