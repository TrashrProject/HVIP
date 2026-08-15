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

	// B_AZTECA
	var B_AZTECA = new Image();
	B_AZTECA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_azteca/pz_b_azteca.png";
	// B_GROVE
	var B_GROVE = new Image();
	B_GROVE.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_grove/pz_b_grove.png";
	// B_MERCED
	var B_MERCED = new Image();
	B_MERCED.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_merced/pz_b_merced.png";
	// B_OLVIDO
	var B_OLVIDO = new Image();
	B_OLVIDO.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_olvido/pz_b_olvido.png";
	// B_RAZA
	var B_RAZA = new Image();
	B_RAZA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_raza/pz_b_raza.png";
	// B_SANTO
	var B_SANTO = new Image();
	B_SANTO.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_santo/pz_b_santo.png";
	// B_TRUCHA
	var B_TRUCHA = new Image();
	B_TRUCHA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_b_trucha/pz_b_trucha.png";
	// BARRETA
	var BARRETA = new Image();
	BARRETA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_barreta/pz_barreta.png";
	// P_LB
	var P_LB = new Image();
	P_LB.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_barrillas/pz_barrillas.png";
	// TRASH
	var TRASH = new Image();
	TRASH.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_basurero/pz_basurero.png";
	// BETHOVEN
	var BETHOVEN = new Image();
	BETHOVEN.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_bethoven/pz_bethoven.png";
	// BLUEBERRY
	var BLUEBERRY = new Image();
	BLUEBERRY.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_blueberry/pz_blueberry.png";
	// BUBBLE
	var BUBBLE = new Image();
	BUBBLE.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_bubble/pz_bubble_ext.png";
	// BUENAVISTA
	var BUENAVISTA = new Image();
	BUENAVISTA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_buenavista/pz_buenavista.png";
	// CAFE
	var CAFE = new Image();
	CAFE.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_cafeina/pz_cafeina.png";
	// CENTRO
	var CENTRO = new Image();
	CENTRO.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_centro/pz_centro.png";
	// ICECREAM
	var ICECREAM = new Image();
	ICECREAM.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_cherry/pz_cherry.png";
	// LIBRERIA
	var LIBRERIA = new Image();
	LIBRERIA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_conocimiento/pz_conocimiento.png";
	// MUNICIP
	var MUNICIP = new Image();
	MUNICIP.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_corporativo/pz_corporativo.png";
	// DAVID
	var DAVID = new Image();
	DAVID.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_david/pz_david.png";
	// EASTER
	var EASTER = new Image();
	EASTER.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_easterbasin/pz_easterbasin.png";
	// GRANGE
	var GRANGE = new Image();
	GRANGE.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_granja/pz_granja_ext.png";
	// MCDONALDS
	var MCDONALDS = new Image();
	MCDONALDS.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_hestia/pz_hestia.png";
	// BANK
	var BANK = new Image();
	BANK.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_insignia/pz_insignia.png";
	// COURT
	var COURT = new Image();
	COURT.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_legislacion/pz_legislacion.png";
	// PRISON
	var PRISON = new Image();
	PRISON.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_libertad/pz_libertad.png";
	// MAGANATE
	var MAGANATE = new Image();
	MAGANATE.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_magnate/pz_magnate.png";
	// APPART
	var APPART = new Image();
	APPART.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_marieta/pz_marieta.png";
	// MINA
	var MINA = new Image();
	MINA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_minero/pz_minero.png";
	// CLOTHING
	var CLOTHING = new Image();
	CLOTHING.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_moda/pz_moda.png";
	// MALL
	var MALL = new Image();
	MALL.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_morralla/pz_morralla.png";
	// GYM
	var GYM = new Image();
	GYM.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_olimpo/pz_olimpo.png";
	// PALOOZA
	var PALOOZA = new Image();
	PALOOZA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_palooza/pz_palooza.png";
	// PATRIA
	var PATRIA = new Image();
	PATRIA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_patria/pz_patria.png";
	// PEDERNAL
	var PEDERNAL = new Image();
	PEDERNAL.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_pedernal/pz_pedernal.png";
	// GAS
	var GAS = new Image();
	GAS.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_petrolera/pz_petrolera.png";
	// CHURCH
	var CHURCH = new Image();
	CHURCH.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_religion/pz_religion.png";
	// AMMU
	var AMMU = new Image();
	AMMU.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_revolver/pz_revolver.png";
	// P_SM
	var P_SM = new Image();
	P_SM.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_stmonica/pz_stmonica.png";
	// CASINO
	var CASINO = new Image();
	CASINO.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_trebol/pz_trebol.png";
	// TURBINA
	var TURBINA = new Image();
	TURBINA.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_turbina/pz_turbina.png";
	// VELVET
	var VELVET = new Image();
	VELVET.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_velvet/pz_velvet.png";
	// DEALER
	var DEALER = new Image();
	DEALER.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_volkswagen/pz_volkswagen.png";
	// WASHINGTON
	var WASHINGTON = new Image();
	WASHINGTON.src = "https://swf.habbovip.us/v5-0-2/MPU/pz_v2/pz_washington/pz_washington.png";
	

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
		left += 110;
		top += 15;

		if(row === 0 && col === 0){
			context.drawImage(GRANGE, left + (510*zoom), top, (GRANGE.width/10) * zoom, (GRANGE.height/10) * zoom);
			PrintText("[GRANJA] Pedernal", 22, left + (510*zoom), top + (70*zoom), zoom);
		}
		if(row === 0 && col === 1){
			context.drawImage(PALOOZA, left + (490*zoom), top + (30*zoom), (PALOOZA.width/10) * zoom, (PALOOZA.height/10) * zoom);
			PrintText("[CAMPO] Palooza Land", 22, left + (470*zoom), top + (110*zoom), zoom);
		}
		if(row === 0 && col === 2){
			context.drawImage(B_RAZA, left + (470*zoom), top + (110*zoom), (B_RAZA.width/10) * zoom, (B_RAZA.height/10) * zoom);
			PrintText("[BARRIO] La Raza", 22, left + (470*zoom), top + (170*zoom), zoom);
		}
		if(row === 0 && col === 3){
			context.drawImage(B_OLVIDO, left + (449*zoom), top + (162*zoom), (B_OLVIDO.width/10) * zoom, (B_OLVIDO.height/10) * zoom);
			PrintText("[BARRIO] El Olvido", 22, left + (440*zoom), top + (200*zoom), zoom);
		}
		if(row === 0 && col === 4){
			context.drawImage(DAVID, left + (427*zoom), top + (199*zoom), (DAVID.width/10) * zoom, (DAVID.height/10) * zoom);
			PrintText("[CALLE] El David", 22, left + (430*zoom), top + (260*zoom), zoom);
		}
		if(row === 0 && col === 5){
			context.drawImage(TURBINA, left + (404*zoom), top + (238.5*zoom), (TURBINA.width/10) * zoom, (TURBINA.height/10) * zoom);
			PrintText("[CALLE] Turbina", 22, left + (420*zoom), top + (300*zoom), zoom);
		}

		if(row === 1 && col === 0){
			context.drawImage(MALL, left + (420*zoom), top + (17*zoom), (MALL.width/10) * zoom, (MALL.height/10) * zoom);
			PrintText("[CALLE] Morralla", 22, left + (420*zoom), top + (60*zoom), zoom);
		}
		if(row === 1 && col === 1){
			context.drawImage(TRASH, left + (397*zoom), top + (68*zoom), (TRASH.width/10) * zoom, (TRASH.height/10) * zoom);
			PrintText("[CAMPO]", 22, left + (450*zoom), top + (105*zoom), zoom);
			PrintText("Basurero", 22, left + (449*zoom), top + (120*zoom), zoom);
		}
		if(row === 1 && col === 2){
			context.drawImage(PEDERNAL, left + (375*zoom), top + (95*zoom), (PEDERNAL.width/10) * zoom, (PEDERNAL.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (429*zoom), top + (150*zoom), zoom);
			PrintText("Pedernal", 22, left + (421*zoom), top + (170*zoom), zoom);
		}
		if(row === 1 && col === 3){
			context.drawImage(MAGANATE, left + (352*zoom), top + (167*zoom), (MAGANATE.width/10) * zoom, (MAGANATE.height/10) * zoom);
			PrintText("[CALLE] Magnate", 22, left + (370*zoom), top + (200*zoom), zoom);
		}
		if(row === 1 && col === 4){
			context.drawImage(BARRETA, left + (330*zoom), top + (196*zoom), (BARRETA.width/10) * zoom, (BARRETA.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (400*zoom), top + (250*zoom), zoom);			
			PrintText("Barreta", 22, left + (401*zoom), top + (266*zoom), zoom);
		}
		if(row === 1 && col === 5){
			context.drawImage(BETHOVEN, left + (307*zoom), top + (238*zoom), (BETHOVEN.width/10) * zoom, (BETHOVEN.height/10) * zoom);
			PrintText("[CALLE] Bethoven", 22, left + (305*zoom), top + (300*zoom), zoom);
		}

		if(row === 2 && col === 0){
			context.drawImage(GYM, left + (325*zoom), top + (8*zoom), (GYM.width/10) * zoom, (GYM.height/10) * zoom);
			PrintText("[CALLE] Olimpo", 22, left + (325*zoom), top + (70*zoom), zoom);
		}
		if(row === 2 && col === 1){
			context.drawImage(CASINO, left + (304*zoom), top + (38*zoom), (CASINO.width/10) * zoom, (CASINO.height/10) * zoom);
			PrintText("[CALLE] El Trébol", 22, left + (325*zoom), top + (95*zoom), zoom);
		}
		if(row === 2 && col === 2){
			context.drawImage(BANK, left + (282*zoom), top + (109*zoom), (BANK.width/10) * zoom, (BANK.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (358*zoom), top + (155*zoom), zoom);
			PrintText("La Insignia", 22, left + (340*zoom), top + (172*zoom), zoom);
		}
		if(row === 2 && col === 3){
			context.drawImage(COURT, left + (259*zoom), top + (149*zoom), (COURT.width/10) * zoom, (COURT.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (335*zoom), top + (195*zoom), zoom);
			PrintText("Legislación", 22, left + (320*zoom), top + (210*zoom), zoom);
		}
		if(row === 2 && col === 4){
			context.drawImage(VELVET, left + (235*zoom), top + (205*zoom), (VELVET.width/10) * zoom, (VELVET.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (295*zoom), top + (257*zoom), zoom);
			PrintText("Velvet", 22, left + (305*zoom), top + (275*zoom), zoom);
		}
		if(row === 2 && col === 5){
			context.drawImage(MINA, left + (215*zoom), top + (257*zoom), (MINA.width/10) * zoom, (MINA.height/10) * zoom);
			PrintText("[ZONA] Centro", 22, left + (235*zoom), top + (300*zoom), zoom);
			PrintText("Minero", 22, left + (280*zoom), top + (320*zoom), zoom);
		}

		if(row === 3 && col === 0){
			context.drawImage(CAFE, left + (226*zoom), top + (16.5*zoom), (CAFE.width/10) * zoom, (CAFE.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (285*zoom), top + (60*zoom), zoom);
			PrintText("Cafeína", 22, left + (284*zoom), top + (78*zoom), zoom);
		}
		if(row === 3 && col === 1){
			context.drawImage(PRISON, left + (208*zoom), top + (31.5*zoom), (PRISON.width/10) * zoom, (PRISON.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (265*zoom), top + (100*zoom), zoom);
			PrintText("Libertad", 22, left + (260*zoom), top + (120*zoom), zoom);
		}
		if(row === 3 && col === 2){
			context.drawImage(CENTRO, left + (186*zoom), top + (51*zoom), (CENTRO.width/10) * zoom, (CENTRO.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (240*zoom), top + (150*zoom), zoom);
			PrintText("El Centro", 22, left + (230*zoom), top + (170*zoom), zoom);
		}
		if(row === 3 && col === 3){
			context.drawImage(CLOTHING, left + (165*zoom), top + (140*zoom), (CLOTHING.width/10) * zoom, (CLOTHING.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (238*zoom), top + (190*zoom), zoom);
			PrintText("La Moda", 22, left + (233*zoom), top + (207*zoom), zoom);
		}
		if(row === 3 && col === 4){
			context.drawImage(LIBRERIA, left + (142*zoom), top + (195*zoom), (LIBRERIA.width/10) * zoom, (LIBRERIA.height/10) * zoom);
			PrintText("[CALLE] El", 22, left + (190*zoom), top + (240*zoom), zoom);
			PrintText("Conocimiento", 22, left + (170*zoom), top + (255*zoom), zoom);
		}
		if(row === 3 && col === 5){
			context.drawImage(B_SANTO, left + (120*zoom), top + (236*zoom), (B_SANTO.width/10) * zoom, (B_SANTO.height/10) * zoom);
			PrintText("[BARRIO] Santo", 22, left + (145*zoom), top + (300*zoom), zoom);
		}

		if(row === 4 && col === 0){
			context.drawImage(MCDONALDS, left + (134*zoom), top + (-15*zoom), (MCDONALDS.width/10) * zoom, (MCDONALDS.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (200*zoom), top + (40*zoom), zoom);
			PrintText("Hestia", 22, left + (235*zoom), top + (60*zoom), zoom);
		}
		if(row === 4 && col === 1){
			context.drawImage(GAS, left + (112*zoom), top + (48*zoom), (GAS.width/10) * zoom, (GAS.height/10) * zoom);
			PrintText("[CALLE] Petrolera", 22, left + (130*zoom), top + (105*zoom), zoom);
		}
		if(row === 4 && col === 2){
			context.drawImage(BUBBLE, left + (90*zoom), top + (107*zoom), (BUBBLE.width/10) * zoom, (BUBBLE.height/10) * zoom);
			PrintText("[CALLE] Bubble", 22, left + (100*zoom), top + (160*zoom), zoom);
		}
		if(row === 4 && col === 3){
			context.drawImage(AMMU, left + (68*zoom), top + (153*zoom), (AMMU.width/10) * zoom, (AMMU.height/10) * zoom);
			PrintText("[CALLE] Revólver", 22, left + (78*zoom), top + (210*zoom), zoom);
		}
		if(row === 4 && col === 4){
			context.drawImage(B_TRUCHA, left + (46*zoom), top + (204*zoom), (B_TRUCHA.width/10) * zoom, (B_TRUCHA.height/10) * zoom);
			PrintText("[BARRIO]", 22, left + (90*zoom), top + (240*zoom), zoom);
			PrintText("Trucha", 22, left + (100*zoom), top + (260*zoom), zoom);
		}
		if(row === 4 && col === 5){
			context.drawImage(B_MERCED, left + (24*zoom), top + (231*zoom), (B_MERCED.width/10) * zoom, (B_MERCED.height/10) * zoom);
			PrintText("[BARRIO] Merced", 22, left + (35*zoom), top + (290*zoom), zoom);
		}

		if(row === 4 && col === 0){
			context.drawImage(MUNICIP, left + (37.2*zoom), top + (28*zoom), (MUNICIP.width/10) * zoom, (MUNICIP.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (90*zoom), top + (75*zoom), zoom);
			PrintText("Corporativo", 22, left + (65*zoom), top + (95*zoom), zoom);
		}
		if(row === 4 && col === 1){
			context.drawImage(APPART, left + (16*zoom), top + (38*zoom), (APPART.width/10) * zoom, (APPART.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (60*zoom), top + (140*zoom), zoom);
			PrintText("Marieta", 22, left + (60*zoom), top + (160*zoom), zoom);
		}
		if(row === 4 && col === 2){
			context.drawImage(PATRIA, left + (-6*zoom), top + (130*zoom), (PATRIA.width/10) * zoom, (PATRIA.height/10) * zoom);
			PrintText("[PLAZA]", 22, left + (30*zoom), top + (195*zoom), zoom);
			PrintText("La Patria", 22, left + (20*zoom), top + (215*zoom), zoom);
		}
		if(row === 4 && col === 3){
			context.drawImage(CHURCH, left + (-28*zoom), top + (176*zoom), (CHURCH.width/10) * zoom, (CHURCH.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (10*zoom), top + (230*zoom), zoom);
			PrintText("La Religión", 22, left + (-5*zoom), top + (245*zoom), zoom);
		}
		if(row === 4 && col === 4){
			context.drawImage(B_GROVE, left + (-50*zoom), top + (246*zoom), (B_GROVE.width/10) * zoom, (B_GROVE.height/10) * zoom);
			PrintText("[BARRIO] Grove", 22, left + (-43*zoom), top + (300*zoom), zoom);
		}
		if(row === 4 && col === 5){
			context.drawImage(B_AZTECA, left + (-71.7*zoom), top + (284.3*zoom), (B_AZTECA.width/10) * zoom, (B_AZTECA.height/10) * zoom);
			PrintText("[BARRIO] Azteca", 22, left + (-65*zoom), top + (345*zoom), zoom);
		}

		if(row === 5 && col === 0){
			context.drawImage(EASTER, left + (-58*zoom), top + (47*zoom), (EASTER.width/10) * zoom, (EASTER.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (0*zoom), top + (65*zoom), zoom);
			PrintText("Easter Basin", 22, left + (-25*zoom), top + (80*zoom), zoom);
		}
		if(row === 5 && col === 1){
			context.drawImage(WASHINGTON, left + (-80*zoom), top + (73*zoom), (WASHINGTON.width/10) * zoom, (WASHINGTON.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (-30*zoom), top + (120*zoom), zoom);
			PrintText("Washington", 22, left + (-55*zoom), top + (140*zoom), zoom);
		}
		if(row === 5 && col === 2){
			context.drawImage(DEALER, left + (-102*zoom), top + (137.5*zoom), (DEALER.width/10) * zoom, (DEALER.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (-52*zoom), top + (190*zoom), zoom);
			PrintText("Volkswagen", 22, left + (-75*zoom), top + (210*zoom), zoom);
		}
		if(row === 5 && col === 3){
			context.drawImage(ICECREAM, left + (-124*zoom), top + (184.5*zoom), (ICECREAM.width/10) * zoom, (ICECREAM.height/10) * zoom);
			PrintText("[CALLE] Cherry", 22, left + (-100*zoom), top + (250*zoom), zoom);
		}

		if(row === 6 && col === 0){
			context.drawImage(BUENAVISTA, left + (-157*zoom), top + (12*zoom), (BUENAVISTA.width/10) * zoom, (BUENAVISTA.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (-90*zoom), top + (75*zoom), zoom);
			PrintText("Buenavista", 22, left + (-110*zoom), top + (95*zoom), zoom);
		}
		if(row === 6 && col === 1){
			context.drawImage(BLUEBERRY, left + (-179*zoom), top + (77*zoom), (BLUEBERRY.width/10) * zoom, (BLUEBERRY.height/10) * zoom);
			PrintText("[CALLE]", 22, left + (-122*zoom), top + (135*zoom), zoom);
			PrintText("Blueberry", 22, left + (-135*zoom), top + (155*zoom), zoom);
		}
		if(row === 6 && col === 2){
			context.drawImage(P_SM, left + (-201*zoom), top + (138*zoom), (P_SM.width/10) * zoom, (P_SM.height/10) * zoom);
			PrintText("[PLAYA]", 22, left + (-140*zoom), top + (180*zoom), zoom);
			PrintText("Santa Mónica", 22, left + (-170*zoom), top + (200*zoom), zoom);
		}
		if(row === 6 && col === 3){
			context.drawImage(P_LB, left + (-222.5*zoom), top + (181*zoom), (P_LB.width/10) * zoom, (P_LB.height/10) * zoom);
			PrintText("[PLAYA]", 22, left + (-165*zoom), top + (235*zoom), zoom);
			PrintText("Las Barrillas", 22, left + (-190*zoom), top + (255*zoom), zoom);
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
