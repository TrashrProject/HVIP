/* RDPWebSocket Systematic  */
var rdp = {

	// Incoming Packets
	IncomingPacket: function(RDPEvent, ExtraData, EventData, UName, UID){
		switch (RDPEvent) {
			case 'compose_basurero':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "showinfo":
						$('#BasuCount').html("Basura: " + EventData[4]);
						$('#BasuChofer').html("Chofer: " + EventData[2]);
						$('#BasuRecolector').html("Recolector: " + EventData[3]);

						// if es chofer
						if (EventData[5] == "True") {
							$("#ActionBasu").show();
							$("#AbandonarBasu").show();
						}

						$('#BasuInfo').show();
					break;

					case "basucount":
						$('#BasuCount').html("Basura: " + EventData[2]);
					break;
				}
				break;
			}
			case 'compose_camionero':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "open":
						var Amn = formatoMoneda(parseInt(EventData[2]));
						var Med = formatoMoneda(parseInt(EventData[3]));
						var Crack = formatoMoneda(parseInt(EventData[4]));
						var Piezas = formatoMoneda(parseInt(EventData[5]));

						$('#PayListProds').html("<li><ul style='color:green'>$ " + Amn + "</ul></li>");
						$('#PayListCloth').html("<li><ul style='color:green'>$ " + Amn + "</ul></li>");
						$('#PayListDrug').html("<li><ul style='color:green'>$ " + Amn + "</ul><ul style='color:red'>" + Med + " Medicamentos</ul><ul style='color:red'>" + Crack + " g. de Crack</ul></li>");
						$('#PayListGun').html("<li><ul style='color:green'>$ " + Amn + "</ul><ul style='color:red'>" + Piezas + " piezas de armas</ul><ul style='color:transparent'>-</ul></li>");
						$('#CargasInfo').show();
					break;
					case "cammsg":
						$('#CamCargasMsg').html(EventData[2]);
					break;
					case "close":
						$('#CargasInfo').hide();
						$('#CamCargasMsg').html("");
					break;
					case "showinfo":
						$('#Cam_Cap').html("Cargamento: " + EventData[3]);
						$('#CamCargaChofer').html("Chofer: " + EventData[4]);
						$('#CamCargaDestino').html("Dirígete a: " + EventData[5]);

						if (EventData[2] == "depositar") {
							$('#ActionCam').html("Depositar carga");
							document.getElementById("ActionCam").setAttribute('data-action', 'depositar');
						}
						else if (EventData[2] == "entregar") {
							$('#ActionCam').html("Entregar camión");
							document.getElementById("ActionCam").setAttribute('data-action', 'entregar');
						}

						$('#CamCargaInfo').show();
					break;
				}
				break;
			}
			case 'compose_hospital':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "open_botiq":
						$('#HospBotiq').show();
					break;
					case "close_botiq":
						$('#HospBotiq').hide();
					break;
					case "use_botiq":
						$('#HospBotiq').hide();
						$('#botiquinID').val("0").change();
					break;
					case "open_actionbtn":
						$('#HospActionBtnTxt').html(EventData[2]);
						document.getElementById("HospActionBtn").setAttribute('data-action', EventData[3]);
						$('#HospCommandBtn').show();
					break;
					case "close_actionbtn":
						$('#HospCommandBtn').hide();
					break;
				}
				break;
			}
			case 'compose_bodyguard':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "show_sell_button":
						$('#GuardCommandBtn').show();
					break;
					case "hide_sell_button":
						$('#GuardCommandBtn').hide();
					break;
				}
				break;
			}
			case 'compose_taxi':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "load":
						$('#TaxiList').html(EventData[2]);
						$('#TaxiInfo').show();
					break;
					case "open":
						$('#TaxiInfo').show();
					break;
					case "close":
						$('#TaxiInfo').hide();
					break;
				}
				break;
			}
			case 'compose_armero':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "open_pieces":
						$('.MyPiecesCounter').html("Mis Piezas: " + EventData[2]);
						$('#TotalMats').html("x" + EventData[3]);
						$('#TotalPieces').html("x" + EventData[4]);
						$('#ArmPiecesInfo').show();
					break;
					case "open_weapons":
						$('.MyPiecesCounter').html("Mis Piezas: " + EventData[2]);
						$('#ArmCreateInfo').show();
					break;
					case "close_pieces":
						$('#ArmPiecesInfo').hide();
						$('.ArmPiecesMsg').html("");
					break;
					case "close_weapons":
						$('#ArmCreateInfo').hide();
						$('.ArmPiecesMsg').html("");
					break;
					case "armmsg":
						$('.ArmPiecesMsg').html(EventData[2]);
					break;
				}
				break;
			}
			case 'compose_changename':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "open":
						$('#Btn_ChangeName').html(EventData[2]);
						$('#ChangeName').show();
					break;
					case "close":
						$('#ChangeName').hide();
						$('#ChangeNameMsg').html("");
						$('#CHN_NewName').val("");
					break;
					case "chnamemsg":
						var Error = EventData[2];					
						$('#ChangeNameMsg').html("<b style='color:red'>"+Error+"</b>");
					break;
					case "chnamemsg_green":
						var Error = EventData[2];					
						$('#ChangeNameMsg').html("<b style='color:green'>"+Error+"</b>");
					break;
				}
				break;
			}
			case 'compose_purge':
			{
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "open":
						var html = "";
						html += '<div id="PurgeTV"></div>';
						html += '<video width="420" autoplay>';
						html += '<source src="'+WS_DY+'/ws_overlays/Purge/resources/video/Purge_PZ.mp4" type="video/mp4">';
						html += '<source src="'+WS_DY+'/ws_overlays/Purge/resources/video/Purge_PZ.ogg" type="video/ogg">';
						html += 'Tu navegador no soporta la reproducción de videos.';
						html += '</video>';
						$('#PurgeContent').html(html);
						$('#PurgeWindow').fadeTo("slow", 1);

						setInterval(function() {
							$('#PurgeWindow').hide("slow");
						}, 71000);
						setInterval(function() {
							$('#PurgeContent').html("");
						}, 85000);
					break;
					case "close":
						$('#PurgeWindow').hide("slow", function (){
							$('#PurgeContent').html("");
						});
					break;
					case "timer":
						var TimeLeft = parseInt(EventData[2]);
						var Mins = Math.floor(TimeLeft/60);
						var Secs = TimeLeft%60;
						$('.WorldEvent_name_2gJ1M').html("La Purga");
						$('.WorldEvent_description_6iynM').html("¡Todo es legal! No hay ninguna ley para ninguna actividad.");
						var html = "";
						html += '<div>Termina en:</div>';
						html += '<div class="text-lg">'+Mins+' min(s) '+Secs+' sec(s)</div>';
						$('.WorldEvent_text').html(html);
						$('.WorldEvent_worldEvent_3Oxpk').show();
					break;
					case "timer_off":
						$('.WorldEvent_worldEvent_3Oxpk').hide("slow", function (){
							$('.WorldEvent_worldEvent_3Oxpk').html("");
						});
					break;
				}
				break;
			}
			case 'compose_loader':
			{
				SumLoader(parseInt(EventData[1]), 800);
				break;
			}
			case 'compose_tutorial':
			{
				//var fnName = "TutoStep" + EventData[1];
      			//window[fnName]();
      			NextStep(parseInt(EventData[1]));
				break;
			}
			case 'compose_my_tutorial':
			{
				//var fnName = "TutoStep" + EventData[1];
      			//window[fnName]();
      			var Step = parseInt(EventData[1]) - 1;
      			if(Step <= 0) Step = 0;
      			NextStep(Step);
				break;
			}
			//Inicializa Sistema de avatar
			case 'compose_characterbar':
			case 'compose_initcharacter':
		    {
		    	rdp['initStadistics'](ExtraData);
		     	rdp_app['LogConsole']('Información de estadísticas recibidos', UName, UID);
		     	break;
		    }
		    case 'compose_clear_characterbar': {
				rdp['initStadistics'](ExtraData, true);
				break;
			}
			case 'compose_wanted_stars': {
					
				var Action = jQuery.trim(ExtraData);
				var ExtData = (jQuery.trim(EventData[1])).split('|');
				var Level = ExtData[0];
				switch(Level)
				{
					case "0":
						$('.1 .stars').removeClass('one');
						$('.1 .stars').removeClass('two');
						$('.1 .stars').removeClass('three');
						$('.1 .stars').removeClass('four');
						$('.1 .stars').removeClass('five');
						break;
					case "1":
						$('.1 .stars').addClass('one');
						$('.1 .stars').removeClass('two');
						$('.1 .stars').removeClass('three');
						$('.1 .stars').removeClass('four');
						$('.1 .stars').removeClass('five');
						break;
					case "2":
						$('.1 .stars').removeClass('one');
						$('.1 .stars').addClass('two');
						$('.1 .stars').removeClass('three');
						$('.1 .stars').removeClass('four');
						$('.1 .stars').removeClass('five');
						break;
					case "3":
						$('.1 .stars').removeClass('one');
						$('.1 .stars').removeClass('two');
						$('.1 .stars').addClass('three');
						$('.1 .stars').removeClass('four');
						$('.1 .stars').removeClass('five');
						break;
					case "4":
						$('.1 .stars').removeClass('one');
						$('.1 .stars').removeClass('two');
						$('.1 .stars').removeClass('three');
						$('.1 .stars').addClass('four');
						$('.1 .stars').removeClass('five');
						break;
					case "5":
						$('.1 .stars').removeClass('one');
						$('.1 .stars').removeClass('two');
						$('.1 .stars').removeClass('three');
						$('.1 .stars').removeClass('four');
						$('.1 .stars').addClass('five');
						break;
					default:
						//alert(Action);
						break;
				}
				break;
			}
			case 'compose_update_money': {
				var Action = jQuery.trim(ExtraData);
				var ExtData = (jQuery.trim(EventData[1])).split('|');
				var Money = formatoMoneda(parseInt(ExtData[0]));

				// Money
				$('.1 .money').text('$ ' + Money);
				break;
			}
			case 'compose_update_platinos': {
				var Action = jQuery.trim(ExtraData);
				var ExtData = (jQuery.trim(EventData[1])).split('|');
				var Platinos = formatoMoneda(parseInt(ExtData[0]));
				console.log("> " + ExtData[0]);
				// Platinos
				$('.1 .platinos').html(Platinos + ' <img src="https://dynamics.habbovip.us/img/extras/platinos_icon_s.png">');
				break;
			}
			case 'compose_group': {
					
				var Action = jQuery.trim(ExtraData);
				var UserData = (jQuery.trim(EventData[2])).split(';');
				var Badge = UserData[0];
				var Caption = UserData[1];
				var GType = UserData[2];
				var State = UserData[3];
				var Mine = UserData[4];
				var Member = UserData[5];
				var Rqus = UserData[6];
				var BadgeImage = "";
				var GifExtension = (GType == 3) ? ".gif" : "";

				if (GType == 3){
					BadgeImage = 'https://swf.habbovip.us/group-badge/badge/'+ Badge;
				}
				else if (/*GType != 1*/ true){
					BadgeImage = habboSWFUrl+'/habbo-imaging/badge/'+ Badge + GifExtension;
				}
				else{
					BadgeImage = Badge;
				}

				switch(Action)
				{
					case "open":
						$('#GroupInfo').show();
						$('#Badge').html('<img id="ImgBadge" src="'+BadgeImage+'" draggable="false" ondragstart="return false;">');
						$('#Caption').html(Caption);
						if(Mine == "False"){
							if(Member == "False"){
								if(State == "OPEN")
									$('#GroupAction').html('<div class="JoinButton">Unirse al Trabajo</div>');
								else if(State == "LOCKED"){
									if(Rqus == "False")
										$('#GroupAction').html('<div class="JoinButton">Solicitar Unirse</div>');
									else
										$('#GroupAction').html('<div class="JoinButton_Block">En Aprobaci&oacute;n</div>');
								}
								else
									$('#GroupAction').hide();
							}
							else{
								if(GType != "3")
									$('#GroupAction').html('<div class="JoinButton">Dejar Trabajo</div>');
								else
									$('#GroupAction').html('<div class="JoinButton">Dejar Banda</div>');
							}
						}
						else{
							$('#GroupAction').html('<div class="JoinButton">Gestionar</div>');
						}					
					break;
					case "close":
						$('#GroupInfo').hide();
					break;
					case "error":
						var Error = EventData[2];
						$('#GroupMsg').html(Error);
					break;
					case "solicitud": 

						var BdImg = "";

						if(/*UserData[6] != 1*/ true){
							BdImg = habboSWFUrl+'/habbo-imaging/badge/'+ UserData[1];
						}
						else{
							BdImg = UserData[1];
						}

						$('#GroupPanelInfo').show();	
						$('#Caption_Soli').html(UserData[0]);
						$('#Badge_Soli').html('<img src="'+BdImg+'" draggable="false" ondragstart="return false;">');
						$('#Found_Soli').html('<b>Fundador: </b>' + UserData[2]);
						$('#Rank_Soli').html('<b>Puesto: </b>' + UserData[3]);
						$('#Pay_Soli').html('<b>Paga: </b><i style="color:green;">' + UserData[4] + '</i>');
						$('#Time_Soli').html('<b>Cada: </b>' + UserData[5]);
					break;
					case "close_rq":
						$('#GroupPanelInfo').hide();
    	 				$('#GroupMsg').html("");
					break;
					default:
						//alert(Action);
					break;
				}	
				break;	
			}
			case 'compose_house': {
					
				var Action = jQuery.trim(ExtraData);
				var UserData = (jQuery.trim(EventData[2])).split(';');
				var Owner = UserData[0];
				var Price = UserData[1];
				var Level = UserData[2];
				var ForSale = UserData[3];
				var Mine = UserData[4];
				switch(Action)
				{
					case "open":
						$('#HouseInfo').show();
						$('#owner').html(Owner);
						$('#price').html(Price);
						$('#level').html(Level);
						$('#HouseMsg').html('');	
						if(ForSale == "True"){
							$('#HouseAction').show();
							if(Mine == "true"){
								$('#BuyHouse').html('No Vender Casa');
							}
							else{
								$('#BuyHouse').html('Comprar Casa');
							}
						}				
						else{
							if(Mine == "true"){
								$('#HouseAction').show();
								$('#BuyHouse').html('Vender Casa');
							}
							else
							{
								$('#HouseAction').hide();
								$('#BuyHouse').html('No está en Venta');
							}
						}
					break;
					case "close":
						$('#HouseInfo').hide();
						$('#HouseAction').hide();
						$('#HouseMsg').html('');		
					break;
					case "error":
						var Error = EventData[2];
						$('#HouseMsg').html(Error);
					break;
					default:
						//alert(Action);
					break;
				}		
					
				break;
			}
			case 'compose_business': {
				var Action = jQuery.trim(ExtraData);
				var ExtData = (jQuery.trim(EventData[2])).split(';');
				
				switch(Action)
				{
					case "close":
						$('#BusinessTool').hide();
						$('#BusinessToolMsg').hide();
					break;
					case "create":
						$('#BusinessCreate').show();
					break;
					case "open":
						// Get Vars
						var Name = ExtData[0];
						var Logo = '<img src="'+habboSWFUrl+'/habbo-imaging/badge/'+ ExtData[1]+'" draggable="false" ondragstart="return false;">';
						var Tabs = '<div class="tab-2ddeR_0 Stats_Tab selected-3s9hj_0">Estad&iacute;sticas</div><div class="tab-2ddeR_0 Employees_Tab">Empleados</div>' + ExtData[2] + '<div class="tab-2ddeR_0 Finan_Tab">Financiamiento</div>';
						var Stock = ExtData[3] +'/100';
						var Employees = ExtData[4];
						var Shifts = ExtData[5];
						var Request = ExtData[6];
						var Ranks = ExtData[7];
						var Sells = formatoMoneda(ExtData[8]);
						var Actions = ExtData[9];
						var Bank = '$ ' + formatoMoneda(ExtData[10]);
						var Founder = ExtData[11];
						var isAdmin = ExtData[12];
						var Removable = ExtData[13];
						var Spend = formatoMoneda(ExtData[14]);
						var Profits = formatoMoneda(ExtData[15]);
						var StockCost = ExtData[16];
						var CanSupply = ExtData[17];

						$('.progressBar-360C0_0 .progress-xqabV_0').animate({width: ExtData[3] + '%'});

						if(Founder == "Gobierno"){
							Stock = "∞/∞";
							$('.progressBar-360C0_0 .progress-xqabV_0').animate({width: '100%'});
						}

						$('#BusinessTool').show();
						$('.Founder').html(Founder);
						$('#Tool_Tabs').html(Tabs);
						$('#Tool_Title').html(Name);
						$('#Tool_Text').html(Name);
						$('#Tool_Colours').html(Logo);
						$('.text-3_n3-_0').html(Stock);
						$('.Employees').html(Employees);
						$('.Shifts').html(Shifts);
						$('.Request').html(Request);
						$('.Ranks').html(Ranks);
						$('.Sells').html("$ " + Sells);
						$('.Spend').html("$ " + Spend);
						$('.Profits').html("$ " + Profits);

						$('.Bank').html("<div class=\"mr-2\">Banco</div><div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">"+ Bank +"</div>");

						// ReSets
						$('#BuyCorp').show();							
						$('.heading_stock').show();						
						$('.progressBarWrapper_stock').show();				
						$('.mr-2_sells').html("Ventas");// Establecemos palabra "Ventas"
						$('.heading_stock').removeClass('Bus_textleft');
						$('.heading_stock').html("Stock");
						// End ReSets

						// Trabajo Policía no es Empresa
						if(Name != "Policía"){
							if(CanSupply == "True"){
								$('.heading_stock').addClass('Bus_textleft');
								$('.heading_stock').html("Stock <div class=\"Bus_Supply\">A|tecer ($ "+ StockCost +" c/u) <i class=\"fas fa-cart-plus\"></i></div>");
							}
							if(isAdmin == "True" && Removable == "True"){
								$('#BuyCorp').html("Cerrar Empresa");
							}
							else if(isAdmin == "True" && Removable == "False"){
								$('#BuyCorp').html("Vender Empresa");
							}
							else if(Founder == "Gobierno"){
								$('.Bank').html("<div class=\"mr-2\">En venta por:</div><div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">$ 50, 000</div>");							
								$('#BuyCorp').html("Comprar Empresa");
							}
							else{
								$('#BuyCorp').hide();
							}
						}
						else{
							// Trabajo Policía (GType == 1) pero no Comprable
							$('#BuyCorp').hide();							
							$('.heading_stock').hide();						
							$('.progressBarWrapper_stock').hide();
							$('.mr-2_sells').html("Fianzas");// Cambiamos palabra "Ventas"		
						}

						// Body
						$('#Stats').show();
						$('#Employees').hide();
						$('#Request').hide();
						$('#Manage').hide();
						$('#EditRank').hide();
						$('#Finan').hide();
					break;
					case "employees":
						$('.Stats_Tab').removeClass('selected-3s9hj_0');
						$('.Employees_Tab').addClass('selected-3s9hj_0');
						$('.Request_Tab').removeClass('selected-3s9hj_0');
						$('.Manage_Tab').removeClass('selected-3s9hj_0');
						$('.Finan_Tab').removeClass('selected-3s9hj_0');

						// Post HTML
						$('#Employees').html(EventData[2]);

						$('#Stats').hide();
						$('#Employees').show();
						$('#Request').hide();
						$('#Manage').hide();
						$('#EditRank').hide();
						$('#Finan').hide();
					break;
					case "requests":
						$('.Stats_Tab').removeClass('selected-3s9hj_0');
						$('.Employees_Tab').removeClass('selected-3s9hj_0');
						$('.Request_Tab').addClass('selected-3s9hj_0');
						$('.Manage_Tab').removeClass('selected-3s9hj_0');
						$('.Finan_Tab').removeClass('selected-3s9hj_0');

						// Post HTML
						$('#Request').html(EventData[2]);

						$('#Stats').hide();
						$('#Employees').hide();
						$('#Request').show();
						$('#Manage').hide();
						$('#EditRank').hide();
						$('#Finan').hide();
					break;
					case "manage":
						$('.Stats_Tab').removeClass('selected-3s9hj_0');
						$('.Employees_Tab').removeClass('selected-3s9hj_0');
						$('.Request_Tab').removeClass('selected-3s9hj_0');
						$('.Manage_Tab').addClass('selected-3s9hj_0');
						$('.Finan_Tab').removeClass('selected-3s9hj_0');

						// Clean HTML
						document.getElementById("InputRank").value = "";
						document.getElementById("InputLogo").value = "";

						$('#Stats').hide();
						$('#Employees').hide();
						$('#Request').hide();
						$('#Manage').show();
						$('#EditRank').hide();
						$('#Finan').hide();
					break;
					case "settings":
						// Reset Generator
						AG.importFigure( "lg.hd-180-1.ch.cc.hr.sh.ha.he.ea.fa.ca.wa.cp", true );

						// Gender Data						
						var gender = "M";
				        if( gender != AG.getGender() ){				        				            

					        var oldGender = ( gender == "M" ) ? "F" : "M";

					        $( 'a[data-gender="' + oldGender + '"]' ).removeClass( 'nav-selected' );
					        $( this ).addClass( 'nav-selected' );

					        AG.switchGender();
					        AG.loadToClothes( AG.getCurrentSet() );
					        AG.updateAvatar();
					    }				        
						
						// Navs
						var toSet = "hd";
				        var subnav = "gender";

				        $( '.nav-selected' ).removeClass( 'nav-selected' );
				        $( this ).addClass( 'nav-selected' );

				        AG.setCurrentSet( toSet );
				        AG.loadToClothes( toSet );
				        AG.loadToColors( toSet );

				        if( typeof subnav !== 'undefined' ) {
				            $( '.sub-navigation .display' ).removeClass( 'display' ).addClass( 'hidden' );
				            $( '#' + subnav ).removeClass( 'hidden' ).addClass( 'display' );
				        }

						$('.Stats_Tab').removeClass('selected-3s9hj_0');
						$('.Employees_Tab').removeClass('selected-3s9hj_0');
						$('.Request_Tab').removeClass('selected-3s9hj_0');
						$('.Manage_Tab').addClass('selected-3s9hj_0');
						$('.Finan_Tab').removeClass('selected-3s9hj_0');

						// Post HTML
						$('#RankSettingContent').html(EventData[2]);

						var RankLook = "https://www.habbo.com.tr/habbo-imaging/avatarimage?head_direction=4&direction=4&figure=hd-180-1."+ EventData[3] +"&gender=M";

						document.getElementById("avatar-code").value = EventData[3] + "&gender=M";
						document.getElementById("myHabbo").src = RankLook;

						// Modoficamos valores el Generator
						AG.importFigure("hd-180-1" + EventData[3]);
						AG.importFigureFemale("hd-600-1" + EventData[4]);

						// Set Rank at Button Save Look
						document.getElementById("ButtonLook").setAttribute("data-rank", ""+EventData[5]);
						
						$('#Stats').hide();
						$('#Employees').hide();
						$('#Request').hide();
						$('#Manage').hide();
						$('#EditRank').show();
						$('#Finan').hide();
					break;
					case "finance":
						$('.Stats_Tab').removeClass('selected-3s9hj_0');
						$('.Employees_Tab').removeClass('selected-3s9hj_0');
						$('.Request_Tab').removeClass('selected-3s9hj_0');
						$('.Manage_Tab').removeClass('selected-3s9hj_0');
						$('.Finan_Tab').addClass('selected-3s9hj_0');

						// Post HTML
						$('#Finan').html(EventData[2]);

						$('#Stats').hide();
						$('#Employees').hide();
						$('#Request').hide();
						$('#Manage').hide();
						$('#EditRank').hide();
						$('#Finan').show();
					break;
					case "msg_error":
						$('#BusinessToolMsg').html("<b style='color:red'>"+EventData[2]+"</b>");
						$('#BusinessToolMsg').show();
					break;
					case "msg_success":
						$('#BusinessToolMsg').html("<b style='color:green'>"+EventData[2]+"</b>");
						$('#BusinessToolMsg').show();
					break;
					default:
					break;
				}
				break;	
			}
			case 'compose_apart': {
					
				var Action = jQuery.trim(ExtraData);
				var ExtData = (jQuery.trim(EventData[2])).split('|');

				switch(Action)
				{
					case "open":
						$('#ApartShop').show();
					break;
					case "close":
						$('#ApartShop').hide();
						$('#AprtShopMsg').html("");
					break;
					case "welcome":
						$('#AP_Welcome_Title').html(ExtData[0]);

						$('#ApartShop').show();
						$('#AP_Welcome_Tab').show();
						$('#AP_New_Tab').hide();
						$('#AP_Buy_Tab').hide();
						$('#AP_Sell_Tab').hide();

						$("#ApartShop").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            			$('.AP_Welcome_Tab').addClass('selected-3s9hj_0');
					break;
					case "new_apart":
						$('#AP_New_RoomModels').html(ExtData[0]);

						$('#ApartShop').show();
						$('#AP_Welcome_Tab').hide();
						$('#AP_New_Tab').show();
						$('#AP_Buy_Tab').hide();
						$('#AP_Sell_Tab').hide();
					break;
					case "msg_error":
						$('#AprtShopMsg').html("<b style='color:red'>"+ExtData[0]+"</b>");
					break;
					case "msg_success":
						$('#AprtShopMsg').html("<b style='color:green'>"+ExtData[0]+"</b>");
						$("#AP_New_Name").val("");
					break;
					case "msg_ele_error":
						$('#AprtEleMsg').html("<b style='color:red'>"+ExtData[0]+"</b>");
					break;
					case "apart_close":
						$('#ApartElevator').hide();
						$('#AP_Elevator_Display').val("");
						$('#AprtEleMsg').html("");
						$('#AprtLocksMsg').html("");
						$('#ApartLock').hide();
						$('#AP_Elevator_Pass').val("");
						$('#AP_Elevator_Search').val("");
						$("#ApartElevator").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            			$('.AP_Elevator_Tab').addClass('selected-3s9hj_0');
					break;
					case "apart_list":
						$('#apartlist').html(ExtData[0]);
						$('#AP_Elevator_Tab').show();
						$('#ApartElevator').show();
					break;
					case "open_apart_lock":
						$('#AprtLocksMsg').html(ExtData[0]);
						$('#ApartLock').show();
					break;
					case "my_apart_list":
						$('#apartlist').html(ExtData[0]);
						$('#AP_Elevator_Tab').show();
						$('#ApartElevator').show();
						$('#AP_Elevator_Search').val(EventData[3]);
					break;
					case "my_offer_aparts":
						$('#AP_My_Offers').html(ExtData[0]);

						$('#ApartShop').show();
						$('#AP_Welcome_Tab').hide();
						$('#AP_New_Tab').hide();
						$('#AP_Buy_Tab').hide();
						$('#AP_Sell_Tab').show();
					break;
					case "offer_aparts":
						$('#AP_Offers').html(ExtData[0]);

						$('#ApartShop').show();
						$('#AP_Welcome_Tab').hide();
						$('#AP_New_Tab').hide();
						$('#AP_Buy_Tab').show();
						$('#AP_Sell_Tab').hide();
					break;
					default:
						//alert(Action);
					break;
				}		
					
				break;
			}
			case 'compose_atm': {
				var Action = jQuery.trim(ExtraData);

				var UserData = (jQuery.trim(EventData[2])).split(',');
				var Money = UserData[0];

				switch(Action)
				{
					case "open":
						$('#AtmMachine').show();
						$('.c_amt').html(EventData[2]);
						$('#AtmMsg').html('');					
					break;
					case "close":
						$('#ActivityOverlay').hide();
			            $('#AtmMachine').hide();
			            $('#AtmMsg').html('');				
					break;
					case "error":
						var Error = EventData[2];
						$('#AtmMsg').html('<div id="AtmError">' + Error + '</div>');
					break;	
					case "change_balance_1":
						var Amount = EventData[2];
						$('.c_amt').html(Amount);
						$('#AtmMsg').html('');	
					break;
					case "change_balance_2":
						var Amount = EventData[2];
						$('.s_amt').html(Amount);
						$('#AtmMsg').html('');
					break;
					default:
						//alert(Action);
					break;
				}
				break;
			}
			case 'compose_products': {
					
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "close_products":
						$('#ProductsFerreInfo').hide();
	            		$('#Trabas').hide();
			            $('#Alarmas').hide();
			            $('#Loader').hide();
			            $('#ProductMallBack').hide();
			            $('#ProductsPrincipal').show();
			            $('#ProductsMallInfo').hide();
			            $('#GruaService').hide();
			            $('#ProdMsg').html("");
			            $('#GruaMsg').html("");
			            $('#FerreMsg').html("");
			            $('#SellCar').hide();
            			$('#SellCarMsg').html("");
					break;
					case "open_mall":
						$('#ProductsPrincipal').html(EventData[2]);
						$('#ProductsMallInfo').show();
					break;
					case "open_trabas":
						$('#ProductsPrincipal').hide();
						$('#Loader').hide();
			            $('#Trabas').show();
			            $('#ProductMallBack').show();
			            $('#Trabas').html("<div style='position: absolute;font-weight: bold;'>Tus Vehículos que no tienen Traba:</div><br>" + EventData[2]);
					break;
					case "open_alarmas":
						$('#ProductsPrincipal').hide();
						$('#Loader').hide();
			            $('#Alarmas').show();
			            $('#ProductMallBack').show();
			            $('#Alarmas').html("<div style='position: absolute;font-weight: bold;'>Tus Vehículos que no tienen Alarma:</div><br>" + EventData[2]);
					break;
					case "open_ferre":
						$('#ProductsPrincipal.ferreteria').html(EventData[2]);
						$('#ProductsFerreInfo').show();
					break;
					case "open_grange":
						$('#GrangeItemsPrincipal').html(EventData[2]);
						$('#CosechadorInfo').show();
					break;
					case "productmsg":
						var Error = EventData[2];
						$('#ProdMsg').html("");
						$('#GruaMsg').html("");
						$('#FerreMsg').html("");
						$('#SellCarMsg').html("");
						$('#CosechadorSeedsMsg').html("");

						$('#ProdMsg').html("<b style='color:red'>"+Error+"</b>");						
						$('#GruaMsg').html("<b style='color:red'>"+Error+"</b>");						
						$('#FerreMsg').html("<b style='color:red'>"+Error+"</b>");						
						$('#SellCarMsg').html("<b style='color:red'>"+Error+"</b>");
						$('#CosechadorSeedsMsg').html("<b style='color:red'>"+Error+"</b>");
					break;
					case "productmsg_green":
						var Error = EventData[2];
						$('#ProdMsg').html("");
						$('#GruaMsg').html("");
						$('#FerreMsg').html("");
						$('#SellCarMsg').html("");
						$('#CosechadorSeedsMsg').html("");

						$('#ProdMsg').html("<b style='color:green'>"+Error+"</b>");
						$('#GruaMsg').html("<b style='color:green'>"+Error+"</b>");
						$('#FerreMsg').html("<b style='color:green'>"+Error+"</b>");
						$('#SellCarMsg').html("<b style='color:green'>"+Error+"</b>");
						$('#CosechadorSeedsMsg').html("<b style='color:green'>"+Error+"</b>");
					break;
					case "open_grua":
						$('#GruaService').show();
			            $('#MyVehicles').html("<div style='position: absolute;font-weight: bold;'>Selecciona el Vehículo que quieras recuperar:</div><br>" + EventData[2]);
					break;
					case "close_grua":
						$('#GruaService').hide();
			            $('#Loader').hide();
			            $('#GruaMsg').html("");
					break;
					case "loader-on":
						$('#Loader').show();
					break;
					case "loader-off":
						$('#Loader').hide();
					break;
					case "open_sellcar":
						$('#SellCar').show();
			            $('#MyVehiclesSell').html("<div style='position: absolute;font-weight: bold;'>Da clic en 'Vender' en un vehículo:</div><br>" + EventData[2]);
					break;
					default:
						//alert(Action);
					break;
				}		
					
				break;
			}
			case 'compose_vehicle': {
				var Action = jQuery.trim(ExtraData);
				var UserData = (jQuery.trim(EventData[2])).split(';');
				var Interior = UserData[0];
				switch(Action)
				{
					case "baul":
						$('#BaulInfo').show();
						$('#BaulInt').html(Interior);
					break;
					case "closebaul":
						$('#BaulInfo').hide();
						$('#BaulInt').html('');		
					break;
					case "openshop":
						$('#CarsList').html(EventData[2]);
						$('#CarShopInfo').show();
					break;
					case "closeshop":
						$('#CarShopInfo').hide();
						$('#CarShopMsg').html("");
					break;
					case "shopmsg":
						var Error = EventData[2];
						$('#CarShopMsg').html("");
						$('#CarShopMsg').html("<b style='color:red'>"+Error+"</b>");
					break;
					case "shopmsg_green":
						var Error = EventData[2];
						$('#CarShopMsg').html("");
						$('#CarShopMsg').html("<b style='color:green'>"+Error+"</b>");
					break;
					default:
						//alert(Action);
					break;
				}
				break;
			}
			case 'compose_fuel': {
				var Action = jQuery.trim(ExtraData);
				var UserData = (jQuery.trim(EventData[2])).split(';');
				var Fuel = UserData[0];
				var MaxFuel = UserData[1];
				switch(Action)
				{
					case "open":
						$('#FuelInfo').show();
						var fuelPct = (Fuel / MaxFuel) * 100;
						$('#my-vh .stat.energy .bar').animate({width: fuelPct + '%'});
						$('#my-vh .stat.energy .value').text(Fuel + '/' + MaxFuel);
					break;
					case "close":
						$('#FuelInfo').hide();	
						$('#CamCargaInfo').hide();	
						$('#BasuInfo').hide();	
					break;
					default:
						//alert(Action);
					break;
				}
				break;
			}
			case 'compose_commands': {
				var Action = jQuery.trim(ExtraData);
				switch(Action)
				{
					case "show_police_cmds":
						$('.police_info').hide();
						$('.police_cmds').show();
					break;
					case "hide_police_cmds":
						$('.police_info').show();
						$('.police_cmds').hide();
					break;
					case "map":
						$('#MapInfo').show();
					break;
					case "open":
						$('#CommandsInfo').show();
					break;
					case "jobs":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").addClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").show();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "houses":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").addClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").show();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "vehicles":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").addClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").show();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "bussines":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").addClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").show();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "terrains":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").addClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").show();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "marijane":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").addClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").show();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").hide();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;
					case "gangs":
						
           				$(".CMDS_Normales").removeClass('selected-3s9hj_0');
						$("#CMDS_Normales").hide();
						$(".CMDS_VIP").removeClass('selected-3s9hj_0');
				        $("#CMDS_VIP").hide();
				        $(".CMDS_Roleplay").removeClass('selected-3s9hj_0');
				        $("#CMDS_Roleplay").hide();
				        $(".CMDS_Vehiculos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Vehiculos").hide();
				        $(".CMDS_Viviendas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Viviendas").hide();
				        $(".CMDS_Bandas").addClass('selected-3s9hj_0');
				        $("#CMDS_Bandas").show();
				        $(".CMDS_Trabajos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Trabajos").hide();
				        $(".CMDS_Empresas").removeClass('selected-3s9hj_0');
				        $("#CMDS_Empresas").hide();
				        $(".CMDS_Terrenos").removeClass('selected-3s9hj_0');
				        $("#CMDS_Terrenos").hide();
				        $(".CMDS_Staff").removeClass('selected-3s9hj_0');
				        $("#CMDS_Staff").hide();

						$('#CommandsInfo').show();
					break;

					default:
						//alert(Action);
					break;
				}
				break;
			}
			case 'compose_phone': {
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					// PHONE SHOP
					case "close_shop":
						$('#PhoneShopInfo').hide();
					break;
					case "shopmsg":
						var Error = EventData[2];
						$('#PhoneShopMsg').html("");
						$('#PhoneShopMsg').html("<b style='color:red'>"+Error+"</b>");
					break;
					case "shopmsg_green":
						var Error = EventData[2];
						$('#PhoneShopMsg').html("");
						$('#PhoneShopMsg').html("<b style='color:green'>"+Error+"</b>");
					break;
					case "open_shop_phone":
						$('#PhoneList').html(EventData[2]);
						$('#PhoneShopInfo').show();
					break;

					case "load_apps":
						$('#screen_apps').html(EventData[2]);
						$('.dock_apps').html(EventData[3]);
						InitSwiperPhones();
					break;
					case "in_app":
						// Hide all active iframes
						$('#app_bg_v2').children().hide();

						if(EventData[2] != "Messages" && EventData[2] != "Contacts" && EventData[2] != "WhatsApp" && EventData[2] != "Health" && EventData[2] != "Services"){
							// Add code from app
							if($('iframe#' + EventData[2]).length > 0) {
								$('iframe#' + EventData[2]).show();
							}
							else{
								$('#app_bg_v2').append(EventData[3]);
							}

							$('.column').removeClass('animated bounceIn');
							$('#AppViewer').show();
							$('#app_bg_v2').addClass('animated zoomIn faster');  
			            	$('#app_bg_v2').css("display", "inherit");	
		            	}
		            	else {
		            		$('#app_'+EventData[2]).show();
				            $('.app_'+EventData[2]).addClass('animated zoomIn faster');  
				            $('.app_'+EventData[2]).css("display", "inherit");
		            	}
					break;
					case "in_app_error":
						Unavailable(EventData[2]);		           
					break;
					case "in_web_page":
						$('#InternetOverlay').removeClass('animated zoomIn faster');
						$('#InternetOverlay').addClass('animated zoomIn faster');
						$("#InternetOverlay").show();
						
						/*
						var URL = EventData[2];
						const regex = /\./gi;
           				var iF_Name = URL.replace(regex, '');
						var iFrame = EventData[3];

						// Hide all active iframes
						$('#InternetWebPage').children().hide();

						// Add code from web page
						if($('iframe#' + iF_Name).length > 0) {
							$('iframe#' + iF_Name).show();
						}
						else{
							$('#InternetWebPage').append(iFrame);
						}
						*/
					break;

					case "show_button":
						$('#PhoneButton').show();
					break;
					case "open_phone":
						$('#phone').show();// Abrimos Teléfono
					break;
					case "close_phone":
						$('#phone').hide();// Aseguramos cierre para evitar glitches
					break;
					// MESSAGES
					case "open_chatrooms":
						// Insertamos la lista de chatrooms
			            $('#WS_Messages').html(EventData[2]);
					break;
					case "open_messages":
						// Insertamos la lista de mensajes en una conversación
			            $('.app_messages_name').html(EventData[3]);
			            $('.app_messages_chats').html(EventData[2]);
			            // Basic Viewer
			            $('.Messages_Title').hide();
						$('#WS_Messages').hide();
						$('#WS_Messages_Chatting').show();
						$('#app_Messages_New').hide();
						$('#app_Messages_New .number').val('');
						$('#app_Messages_New .mensaje').val('');
						$('#WS_Messages_Chatting .TextTo')[1].innerHTML = "";

						var messageBody = document.querySelector('.app_messages_chats');
						messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
					break;
					case "update_messages":
						// Insertamos la lista de mensajes en una conversación
			            $('.app_messages_name').html(EventData[3]);
			            $('.app_messages_chats').html(EventData[2]);

			            var messageBody = document.querySelector('.app_messages_chats');
						messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
					break;
					// END MESSAGES
					// WHATSAPP
					case "open_whatsapp":
						// Insertamos la lista de whatschats & contacts
			            $('#WS_WhatsApp').html(EventData[2]);
			            $('#WS_WhatsApp_Contacts').html(EventData[3]);
					break;
					case "open_whatschats":
						// Insertamos la lista de mensajes en una conversación
			            $('.app_whats_photo').html(EventData[3]);
			            $('.app_whats_name').html(EventData[4]);
			            $('.app_whats_lastonline').html(EventData[5]);
			            $('.app_whats_messages').html(EventData[2]);
			            // Basic Viewer
			            $('.Whats_Title').hide();
						$('#What_Menu').hide();
						$('#WS_WhatsApp').hide();
						$('#WS_WhatsApp_Contacts').hide();
						$('#WS_WhatsApp_Chatting').show();
						$('#WS_WhatsApp_Chatting .TextTo')[1].innerHTML = "";

						var messageBody = document.querySelector('.app_WhatsApp');
						messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
					break;
					case "update_whatschats":
						// Insertamos la lista de mensajes en una conversación
			            $('.app_whats_photo').html(EventData[3]);
			            $('.app_whats_name').html(EventData[4]);
			            $('.app_whats_lastonline').html(EventData[5]);
			            $('.app_whats_messages').html(EventData[2]);

			            var messageBody = document.querySelector('.app_WhatsApp');
						messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
					break;
					// END WHATSAPP
					case "open_contacts":
						// Insertamos la lista de contactos
						$('#WS_Contacts').html(EventData[2]);

					break;
					default:
						//alert(Action);
					break;
				}
				break;
			}
			case 'compose_gang': {
					
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "open":
						$('#GA_Bandas_Tab').html(EventData[2]);

						// HasGang
						if(EventData[3] == "True") {
							$('.GA_MyNew_Tab').html("Mi Banda");
							$('#GA_New').hide();
							$('#GA_My').show();
						}
						else{
							$('.GA_MyNew_Tab').html("Crear Banda");
							$('#GA_New_Btn').html(EventData[4]);
							$('#GA_New').show();
							$('#GA_My').hide();
						}

						$('#GA_Bandas_Tab').show();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').hide();

						$("#GangsWindow").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            			$('.GA_Bandas_Tab').addClass('selected-3s9hj_0');

            			$('#GA_My_Tabbeds').html(EventData[5]);

						$('#GangsWindow').show();
					break;
					case "close":
						$('#GangsMsg').html("");
						$('#GangsMsg').hide();
						$('#GangsWindow').hide();
					break;
					case "new_gang":
						$('.GA_MyNew_Tab').html("Crear Banda");
						$('#GA_New').show();
						$('#GA_My').hide();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "my_gang":
						$('.GA_MyNew_Tab').html("Mi Banda");

						// Reset the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_My_Members").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_My_Tabbed_body").html(EventData[2]);

            			$('#GA_My_Tabbeds').html(EventData[3]);

						$('#GA_New').hide();
						$('#GA_My').show();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "ranks":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_My_Ranks").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_My_Tabbed_body").html(EventData[2]);

						$('#GA_New').hide();
						$('#GA_My').show();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "invitations_re":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_Inv_Received").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_Inv_Tabbed_body").html(EventData[2]);

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').show();
						$('#GA_MyNew_Tab').hide();
					break;
					case "invitations_se":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_Inv_Sended").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_Inv_Tabbed_body").html(EventData[2]);

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').show();
						$('#GA_MyNew_Tab').hide();
					break;
					case "requests":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_My_Requests").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_My_Tabbed_body").html(EventData[2]);

						$('#GA_New').hide();
						$('#GA_My').show();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "edit":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_My_Edit").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_My_Tabbed_body").html(EventData[2]);

						$('#GA_New').hide();
						$('#GA_My').show();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "stats":
						// Set the tabbeds
						$("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            			$("#GA_My_Stats").addClass('Tabbed_selected_3aJyT');

            			// Add Boddy
            			$("#GA_My_Tabbed_body").html(EventData[2]);

						$('#GA_New').hide();
						$('#GA_My').show();

						$('#GA_Bandas_Tab').hide();
						$('#GA_Invitations_Tab').hide();
						$('#GA_MyNew_Tab').show();
					break;
					case "gang_list":
            			// Add results
            			$("#GA_List").html(EventData[2]);
					break;
					case "view":
						var html = "";
						html += "<div class=\"Tabbed_tabbed_2-zU0\">";
						html += "<div class=\"Tabbed_tabs_1-zM_ Tabbed_center_gqgV8\">";
						html += "<div id=\"GA_View_Members\" data-v-gang=\""+EventData[3]+"\" class=\"Tabbed_tab_1apzZ GA_View_Members Tabbed_selected_3aJyT\" style=\"min-width: 75px;\">";
						html += "Miembros";
						html += "</div>";
						html += "<div id=\"GA_View_Stats\" data-v-gang=\""+EventData[3]+"\" class=\"Tabbed_tab_1apzZ GA_View_Stats\" style=\"min-width: 75px;\">";
						html += "Estad&iacute;sticas";
						html += "</div>";
						html += "</div><br>";
						html += EventData[2];
						html += "</div>";
            			// Add Boddy
            			$("#GA_Bandas_Tab").html(html);
					break;
					case "view_stats":
						var html = "";
						html += "<div class=\"Tabbed_tabbed_2-zU0\">";
						html += "<div class=\"Tabbed_tabs_1-zM_ Tabbed_center_gqgV8\">";
						html += "<div id=\"GA_View_Members\" data-v-gang=\""+EventData[3]+"\" class=\"Tabbed_tab_1apzZ GA_View_Members\" style=\"min-width: 75px;\">";
						html += "Miembros";
						html += "</div>";
						html += "<div id=\"GA_View_Stats\" data-v-gang=\""+EventData[3]+"\" class=\"Tabbed_tab_1apzZ GA_View_Stats Tabbed_selected_3aJyT\" style=\"min-width: 75px;\">";
						html += "Estad&iacute;sticas";
						html += "</div>";
						html += "</div><br>";
						html += EventData[2];
						html += "</div>";
            			// Add Boddy
            			$("#GA_Bandas_Tab").html(html);
					break;
					case "msg_error":
						$('#GangsMsg').html("<b style='color:red'>"+EventData[2]+"</b>");
						$('#GangsMsg').show();
					break;
					case "msg_success":
						$('#GangsMsg').html("<b style='color:green'>"+EventData[2]+"</b>");
						$('#GangsMsg').show();
					break;
					case "capturing":
						// Progress bar
						var Percent = EventData[5];
						$('.ProgressBar_progress_tfs6y').animate({width: Percent + '%'});

						$('.Turf_title_aupur').html(EventData[2]);
						$('.Turf_info_Y6LIm').html(EventData[3]);
						$('.ProgressBar_text_1Dzge').html(EventData[4]);

						$('.Turf_turf_3Ic15').show();
						$('.Turf_turf_3Ic15').css("opacity", "1");
					break;
					case "capturing_off":
						var delay = 3000;
						setTimeout(function() 
					    { 
					        $('.Turf_turf_3Ic15').fadeTo("slow",
					            0);

					    }, delay);

						setTimeout(function() 
					    {
						    // Resets
							var Percent = 0;
							$('.ProgressBar_progress_tfs6y').animate({width: Percent + '%'});
							
							$('.Turf_title_aupur').html("");
							$('.Turf_info_Y6LIm').html("");
							$('.ProgressBar_text_1Dzge').html("0%");
						}, 4000);
					break;
				}
				break;
			}
			case 'compose_combat_mode': {
					
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "active":
						$('#CombatMode').css("background-color", "rgba(42, 148, 4, 0.69)");
					break;
					case "desactive":
						$('#CombatMode').css("background-color", "rgba(0, 0, 0, 0.1)");
					break;
				}
				break;
			}
			case 'compose_psv_mode': {
					
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "active":
						$('#PSVMode').css("background-color", "rgba(42, 148, 4, 0.69)");
					break;
					case "desactive":
						$('#PSVMode').css("background-color", "rgba(0, 0, 0, 0.1)");
					break;
				}
				break;
			}

			case 'compose_live': {
				/*rdp['VerifyNode']();*/
				//rdp['addFeed']("<span class='green bold'>Zedd</span> Mato a <span class='red bold'>Jeihden</span>");
				//rdp_app['LogConsole']('AddingFeed', UName, UID);
				//break;
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "alert":
						rdp['addFeed']("<span class='green bold'>"+EventData[2]+"</span> "+EventData[4]+" <span class='red bold'>"+EventData[3]+"</span>");
					break;
					case "sound":
						rdp['addSound']("<audio controls autoplay><source src=\""+habboSWFUrl+"/habbosounds/"+EventData[2]+".ogg\" type=\"audio/ogg\"><source src=\""+habboSWFUrl+"/habbosounds/"+EventData[2]+".mp3\" type=\"audio/mpeg\"></audio>");
					break;
					default:
					break;
				}
				break;
			}
			case 'compose_stats': {
				var Action = jQuery.trim(ExtraData);

				switch(Action)
				{
					case "wanted":
						$('#WS_WList').html(EventData[2]);
						$('#WantedList').show();
					break;
					default:
					break;
				}
				break;
			}
			
			case 'compose_ws_dialogues':{
				// Mostramos todos los botones WS
				//$('#PhoneButton').show();

				// Preparamos el Teléfono
				$("#phone").hide();
				$("#phone").css("top", "50px");
				break;
			}
			default: {
				rdp_app['LogConsole']('No se encontro el evento solicitado', RDPEvent);
				break;
			}             
	    }
	},

	sendData: function(RDPEvent, data, bypass, json, WS, WSS, UID){
		if(typeof WS == undefined)
			return;
		
		if(WSS == false || WS == null)
		{
			rdp_app['LogConsole']('No se puedo enviar el evento [' + RDPEvent + '], debido a que no se ha establecido una conexión WebSocket');
			return;
		}
		
		if(rdp_app['debugMode'])
			rdp_app['LogPackets'](RDPEvent);
		

		bypass = typeof bypass === 'undefined' ? false : bypass;
		
		WS.send(JSON.stringify({UserId: UID,EventName: RDPEvent,Bypass: bypass,ExtraData: data,JSON: json,}));
	},

	addSound: function(rawHtml) {
		var html = $.parseHTML(rawHtml);var feedEntry = $('<div>');
		feedEntry.append(html);
		feedEntry.fadeIn("fast");
		$('#live-sounds').append(feedEntry);
		var animationTime = 8000;
		feedEntry.animate({top: (parseInt(feedEntry.css('top')) / 2) + 'px'}, animationTime / 2, 'linear', 
		function() {
			feedEntry.animate({
				top: '0px',
				opacity: 0
			},animationTime / 2, 'linear', 
			function() {
				feedEntry.remove();
			});
		});
	},

	addFeed: function(rawHtml) {
		var html = $.parseHTML(rawHtml);var feedEntry = $('<div>').addClass('new-feed-entry LiveFeed_item_37908 velocity-animating');
		feedEntry.append(html);
		feedEntry.fadeIn("fast");
		$('#live-feed').prepend(feedEntry);
		var animationTime = 8000;
		feedEntry.animate({top: (parseInt(feedEntry.css('top')) / 2) + 'px'}, animationTime / 2, 'linear', 
		function() {
			feedEntry.animate({
				top: '0px',
				opacity: 0
			},animationTime / 2, 'linear', 
			function() {
				feedEntry.remove();
			});
		});
	},

	// Stat Bars
	initStadistics: function(usersdata, clear){
		if(ChangeLogs) {
			$('#ChangeLogs').show();
			ChangeLogs = false;
		}
		
		clear = typeof clear === 'undefined' ? false : clear;
		
		if(!clear)
		{
			var DataParts = usersdata.split(',');

			var UserID = parseInt(DataParts[0]);
			var UsersFigure = DataParts[1];
			var CurHealth = parseInt(DataParts[2]);
			var MaxHealth = parseInt(DataParts[3]);
			var CurArmor = parseInt(DataParts[4]);
			var Hunger = parseInt(DataParts[5]);
			var CurLevel = parseInt(DataParts[6]);
			var CurXP = parseInt(DataParts[7]);
			var NeedXP = parseInt(DataParts[8]);
			var Money = formatoMoneda(parseInt(DataParts[9]));
			var Weapon = DataParts[10];
			var Username = DataParts[11];
			var GangName = DataParts[12];
			var GangId = DataParts[13];
			var Platinos = DataParts[14];
			var GangBadge = DataParts[15];
			var IsNewsReporter = DataParts[16];

			var AvatarFigure = "https://www.habbo.nl/habbo-imaging/avatarimage?figure=" + UsersFigure + "&gesture=sml";
			//var WeaponImage = "<?php echo CDNGallery; ?>/client_sources/js/websockets/ws_resources/hud/" + Weapon + ".png";
			var hungerPct = (Hunger / 100) * 100;
			var healthPct = (CurHealth / MaxHealth) * 100;
			var MaxArmor = 100;
			var ArmorPct = (CurArmor / MaxArmor) * 100;
			var XPPct = (CurXP / NeedXP) * 100;
			if(UserID == flashvars['account_id'])
			{
				// Muestra barras Stats
				$('.1').fadeIn();
				// Muestra el Menu de Acciones
        		$('.topBar-16t9O_0').fadeIn();				
				
				// My Name
				$('.1 ._1KZQE').text(Username);// habboName = Get this var from Client.php
				// Figure
				$('.1 ._1H8A-').css('background-image', 'url(' + AvatarFigure + ')');
				// Armor
				$('.1 ._1TDMMG').animate({width: ArmorPct + '%'});
				$('.1 ._1TDMMG ._28X2E').text(CurArmor + '/' + MaxArmor);
				// Health
				$('.1 ._1ft4Z').animate({width: healthPct + '%'});
				$('.1 ._1ft4Z ._28X2E').text(CurHealth + '/' + MaxHealth);
				// Level
				$('.1 ._1H8A- .C6EXx').text(CurLevel);
				// XP
				$('.1 ._1pu34').animate({width: XPPct + '%'});
				// Money
				$('.1 .money').text('$ ' + Money);
				// Platinos
				$('.1 .platinos').html(Platinos + ' <img src="https://dynamics.habbovip.us/img/extras/platinos_icon_s.png">');
			
				if (IsNewsReporter == "False") {
					$('#goToStaffTool').hide();
					HomePage();
					$("#staffToolFrame").remove();
				}
			}
			else{
				$('.2').fadeIn();

				AvatarFigure = "https://www.habbo.nl/habbo-imaging/avatarimage?figure=" + UsersFigure + "&gesture=sml&direction=4";
				// Username
				$('.2 ._1KZQE').text(Username);
				// Figure
				$('.2 ._1H8A-').css('background-image', 'url(' + AvatarFigure + ')');
				// Armor
				$('.2 ._1TDMMG').animate({width: ArmorPct + '%'});
				$('.2 ._1TDMMG ._28X2E').text(CurArmor + '/' + MaxArmor);
				// Health
				$('.2 ._1ft4Z').animate({width: healthPct + '%'});
				$('.2 ._1ft4Z ._28X2E').text(CurHealth + '/' + MaxHealth);
				// Level
				$('.2 ._1H8A- .C6EXx').text(CurLevel);
				// XP
				$('.2 ._1pu34').animate({width: XPPct + '%'});

				// Gang Badge
				if(GangName.length > 0){
					$('.2 ._GangStat').html('<center class="data-gang" data-gang="'+GangId+'" data-balloon="'+GangName+'" data-balloon-pos="right"><img src="https://swf.habbovip.us/group-badge/badge//'+GangBadge+'" draggable="false" ondragstart="return false;" data-balloon="asd" data-balloon-pos="right"></center>');
					$('.2 ._GangStat').show();
				}
				else
					$('.2 ._GangStat').hide();
			}
		}
		else{
			$('.2').fadeOut();
		}
    },
};
import rdp_app from '../rdp.js?<?= time(); ?>';
export default rdp;