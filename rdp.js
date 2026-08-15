/* RDPWebSocket Systematic  */
var rdp_app = {

	startedSocket: false,
	debugMode: false,
	
	webSocket: null,
	reconnectionInterval: null,
	pingInterval: null,
	
	host: null, 
	port: null,

	UserID: null,
	UName: null,
	Figure: null,

    initialize: function(myID, myUsername, myFigure) {

        rdp_app['UserID'] = myID;
		rdp_app['UName'] = myUsername;
		rdp_app['Figure'] = myFigure;
	
		rdp_app['host'] = flashvars['connection.socket.host'];
		rdp_app['port'] = flashvars['connection.socket.port'];
		
        rdp_app['StartUpFunctions']();
        rdp_app['initSockets']();
        rdp_app['bindEvents']();
    },

    bindEvents: function() {    
        // INITALIZE UR JQUERY EVENTS HERE LIKE .CLICK AND STUFF. 
        rdp_app['bindTarget']();       
        rdp_app['bindATM']();
    	rdp_app['bindHouses']();
    	rdp_app['bindGroups']();
    	rdp_app['bindVehicles']();
        rdp_app['bindProducts']();
        rdp_app['bindMap']();
        rdp_app['bindPhones']();
        rdp_app['bindBusiness']();
        rdp_app['bindCommands']();
        rdp_app['bindRules']();
        rdp_app['bindInternet']();
        rdp_app['bindApartments']();
        rdp_app['bindStats']();
        rdp_app['bindGangs']();
        rdp_app['bindTutorial']();
        rdp_app['bindTaxi']();
        rdp_app['bindMunicipalidad']();
        rdp_app['bindCamionero']();
        rdp_app['bindBasurero']();
        rdp_app['bindArmero']();
        rdp_app['bindHospital']();
        rdp_app['bindBodyGuard']();
    },

    initSockets: function(){

    	clearInterval(rdp_app['reconnectionInterval']);
		clearInterval(rdp_app['pingInterval']);
	 
		var path = 'wss://' + rdp_app['host'] + ':' + rdp_app['port'] + '/' + rdp_app['UserID'];
		
		if(typeof(WebSocket) == undefined){
			//Amarillo         
		}
		else{
		  rdp_app['webSocket'] = new WebSocket(path);
		}

	    rdp_app['webSocket'].onopen = function() {	
    		rdp_app['SocketStatus'](1);

    		rdp_app['startedSocket'] = true;
    		
    		
    		rdp_app['pingInterval'] = setInterval(function() {
    			rdp['sendData']('event_pong', '', true, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
    		}, 30000);

            console.clear();
            console.log("%c      ################################################################################################", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #                                                                                              #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  ##########################################################################################  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,,_ _ _ _ ,,,,,_________,,,,, _ _ _ _ ,,,,,___________,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|   ___  \\  ,|   ____  \\   ,|   ___  \\  ,|  _________|,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  |   \\  \\ ,|  |    \\  \\  ,|  |   \\  \\ ,|  |,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  |    \\  \\,|  |     \\  \\ ,|  |    \\  \\,|  |,,,,,,,,,,,_____,,,,,,,_____,,_,,,,,,,,_,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  |____/  /,|  |      \\  @,|  |____/  /,|  |________ ,|  __ \\,,,,,/ __  || |      | | #  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|   _   __/ ,|  |      @  @,|   ______/ ,|   ________|,| |,,\\ \\,,,/ /,,| || |      | | #  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  | \\  \\   ,|  |      /  @,|  |        ,|  |         ,| |,,,\\ \\_/ /,,,| || |      | | #  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  |  \\  \\  ,|  |     /  / ,|  |        ,|  |         ,| |,,,,\\___/,,,,| || |      | | #  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|  |   \\  \\ ,|  |____/  /  ,|  |        ,|  |________ ,| |,,,,,,,,,,,,,| |\\ \\_____ / / #  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,|__|    \\__\\,|_________/   ,|__|        ,|___________|,|_|,,,,,,,,,,,,,|_|,\\________/,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,V:1.0.0.1,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  #,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  ##########################################################################################  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      #  Licensed To: GTH2018                                                        Jeihden & Zedd  #", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("%c      ################################################################################################", 'color: #daddef;text-shadow: 1px 1px 1px rgba(29, 44, 154, 0.8), -1px -1px 1px rgba(33, 52, 187, 0.8), 1px -1px 1px rgba(28, 51, 162, 0.8), -1px 1px 1px rgba(16, 42, 169, 0.8);');
            console.log("");
            console.log("");
    		console.log("[RDP][WEBSOCKET] Conexión exitosa con el usuario [" + rdp_app['UName'] +"]["+rdp_app['UserID']+"]");	
    		
        };



        rdp_app['webSocket'].onclose = function () {
			 // Red

			 clearInterval(rdp_app['pingInterval']);
			 rdp_app['LogConsole']('Desconectado del WebSocket');
			 rdp_app['startedSocket'] = false;
			 rdp_app['webSocket'].close();
			 
			 rdp_app['reconnectionInterval'] = setInterval(rdp_app['tryReconnect'], 2500);	
			 return;
		};

		rdp_app['webSocket'].onerror = function(event) {		
            rdp_app['LogConsole']('Error en descompilacion JSON', JSON.stringify(event));
        };

        rdp_app['webSocket'].onmessage = function(event) {
        	if(rdp_app['debugMode'])
        		rdp_app['LogPackets'](event.data);

		    var eventData = event.data.split('|');//Ojo
		    var RDPEvent = jQuery.trim(eventData[0]);
		    var extraData = eventData[1];

		    rdp['IncomingPacket'](RDPEvent, extraData, eventData, rdp_app['UName'], rdp_app['UserID']);
		};
    },

    StartUpFunctions: function() {
        // Despliega Menu de Usuario
        $(".1").on('click', '._1H8A-', function(){
            $('._2Gc5C').slideToggle();
        });

        // Clic botones de la izquierda
        $('.topBar-16t9O_0').on('click', '.menuButton-yNbz6_0', function(){
            var Button = $(this)[0].getAttribute("data-balloon");

            switch(Button)
            {
                case "Empresa":
                    rdp['sendData']('event_business', "open_my,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
                break;

                default:
                break;
            }
        });

        // Purge Close
        $('#ClosePurgeTV').click(function() {
            $('#PurgeWindow').hide("slow");
        });
    },

    bindTarget: function () {
        // Clic al candado Abierto          
        $(".2").on('click', '._3olRi', function(){
            $('.2 ._3olRi').addClass('SAYNL');// Cerramos candado
            $('.2 ._3olRi')[0].setAttribute("data-balloon", "Desfijar Target");// Cambiamos Texto            

            var data = 'lock,';
            rdp['sendData']('event_target', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Clic al candado Cerrado
        $(".2").on('click', '.SAYNL', function(){
            $('.2 ._3olRi').removeClass('SAYNL');// Abrimos candado
            $('.2 ._3olRi')[0].setAttribute("data-balloon", "Fijar Target");// Cambiamos Texto            
 
            var data = 'unlock,';
            rdp['sendData']('event_target', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Clic a la X
        $(".2").on('click', '._2WRhy', function(){
            $('.2').fadeOut();
            var data = 'close,';
            rdp['sendData']('event_target', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindGroups: function () {
        $('#GroupAction').on('click', function(){ 
            rdp['sendData']('event_group', "send", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });
        $('#CloseGroupInfo').click(function() {
            $('#GroupPanelInfo').hide();
            $('#GroupMsg').html("");
        });
        $('#SendRequest').on('click', function(){ 
            var hr = parseInt($('input[class=hours]').val());
            var country = $('select[class=country]').val(); 
            var descr = $('textarea[class=descript]').val(); 

            const regex = /,/gi; // Reemplazar comas por su code para evitar conflicto en splits del emu.
            descr = descr.replace(regex, '&#44;');
            
            var data = 'request,' + hr + ',' + descr + ',' + country + ',';
            rdp['sendData']('event_group', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindBusiness: function () {
        // Multiselector de Menu
        $("#BusinessTool").on('click', '.tab-2ddeR_0', function(){
            $("#BusinessTool").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            // Tabs Validation
            if($(this)[0].classList.contains('Stats_Tab'))
                rdp['sendData']('event_business', "open_room,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('Employees_Tab'))
                rdp['sendData']('event_business', "employees,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('Request_Tab'))
                rdp['sendData']('event_business', "requests,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('Manage_Tab'))
                rdp['sendData']('event_business', "manage,open", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('Finan_Tab'))
                rdp['sendData']('event_business', "finance", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // End Multiselector de Menu

        // Multiselector Toggles
        $("#Employees").on('click', '.px-1', function(){
            var Rank = parseInt($(this)[0].getAttribute("data-rank"));
            var User = parseInt($(this)[0].getAttribute("data-user"));
            var Action = $(this)[0].getAttribute("data-action");
            if(!isNaN(Rank)){
                // Send WS Ranks
                rdp['sendData']('event_business', "rank_tools,"+Rank+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
            else{
                // Send WS User
                rdp['sendData']('event_business', "member_tools,"+User+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });

        $("#Request").on('click', '.px-1', function(){
            var User = parseInt($(this)[0].getAttribute("data-user"));
            var Action = $(this)[0].getAttribute("data-action");
            if(User != null){
                // Send WS User Request
                rdp['sendData']('event_business', "request_tools,"+User+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });
        // End Multiselector Toggles

        // Manage Tools
        $('#AddRank').on('click', function(){
            var NameRank = $('input[id=InputRank]').val();   
            var data = 'manage,addrank,' + NameRank;

            rdp['sendData']('event_business', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#SaveLogo').on('click', function(){
            var UrlLogo = $('input[id=InputLogo]').val();   
            var data = 'manage,savelogo,' + UrlLogo;

            rdp['sendData']('event_business', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);         
        });
        // End Manage Tools

        // Edit Rank
        // Buttons
        $('#EditRank').on('click', '.dark-button', function(){
            var Action = $(this)[0].getAttribute("data-action");
            var Rank = parseInt($(this)[0].getAttribute("data-rank"));

            if(Action == "SaveRank"){               

                var RankNewName = $('input[id=RankNewName]').val(); 
                var RankPay = parseInt($('input[id=RankPay]').val()); 
                var RankTimer = parseInt($('input[id=RankTimer]').val());   
                var data = 'editrank,saverank,' + Rank + ',' + RankNewName + ',' + RankPay + ',' + RankTimer;

            }
            else if(Action == "SaveLook"){

                var Look = $('input[id=avatar-code]').val();   
                var data = 'editrank,savelook,' + Rank + ',' + Look;

            }

            rdp['sendData']('event_business', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);         
        });

        // Permissions Toggles
        $("#EditRank").on('click', '.px-1', function(){
            
            var TypeAction = $(this)[0].getAttribute("data-action");
            var Rank = parseInt($(this)[0].getAttribute("data-rank"));

            rdp['sendData']('event_business', 'editrank,permissions,' + Rank + ',' + TypeAction, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // End Edit Rank
        
        $('#BuyCorp').on('click', function(){
            rdp['sendData']('event_business', 'buy,', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#CreateBusiness').on('click', function(){
            var GName = $('input[id=InputGName]').val();
            var GActivity = $('input[id=SelectGActivity]').val();

            rdp['sendData']('event_business', 'create,' + GName + ',' + GActivity, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#Badge').on('click', function(){
            rdp['sendData']('event_business', "open_room,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseBusinessTool').click(function() {
            $('#BusinessTool').hide();
            rdp['sendData']('event_business', "close_my,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseBusinessCreate').click(function() {
            $('#BusinessCreate').hide();
        });

        $('#BusinessTool').on('click', '.Bus_Supply', function(){
            rdp['sendData']('event_business', 'supply,', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#Finan').on('click', '#Bus_Btn_Donation', function(){
            var data = $('#Bus_Input_Donation').val();
            rdp['sendData']('event_business', 'finan,donation,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#Finan').on('click', '#Bus_Btn_Withdraw', function(){
            var data = $('#Bus_Input_Withdraw').val();
            rdp['sendData']('event_business', 'finan,withdraw,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });
    },

    bindATM: function () {
                
        $('#AtmCloseBtn').on('click', function(){
            $('#ActivityOverlay').hide();
            $('#AtmMachine').hide();
            $('#AtmMsg').html('');
            rdp['sendData']('event_atm', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('.deposit').on('click', function(){
            $('.AtmHomeScreen').hide();
            $('.AtmDepositScreen').show();
            $('#AtmMsg').html('');
        });
    
        $('.withdraw').on('click', function(){
            $('.AtmHomeScreen').hide();
            $('.AtmWithdrawScreen').show();
            $('#AtmMsg').html('');
        });

        $('.transactions').on('click', function(){
            $('.AtmHomeScreen').hide();
            $('.AtmTransactionsScreen').show();
            $('#AtmMsg').html('');
        });
        
        $('.atmback').on('click', function(){
            $('.AtmWithdrawScreen, .AtmDepositScreen, .AtmTransactionsScreen').hide();
            $('.AtmHomeScreen').show();
            $('#AtmMsg').html('');
        }); 
        // Mandar info al emu
        $('.deposit_submit').on('click', function(){
            var depositAmount = parseInt($('input[class=deposit_amount]').val());   
            var data = 'deposit,' + depositAmount;

            rdp['sendData']('event_atm', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
        });
        
        $('.withdraw_submit').on('click', function(){           
            var withdrawAmount = parseInt($('input[class=withdraw_amount]').val());
            var data = 'withdraw,' + withdrawAmount;
            
            rdp['sendData']('event_atm', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
        });
    },

    bindHouses: function () {
                
        $('#CloseHouseWindow').on('click', function(){
            $('#HouseInfo').hide();
            $('#HouseAction').hide();
            $('#HouseMsg').html('');
            rdp['sendData']('event_house', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Mandar info al emu
        $('#BuyHouse').on('click', function(){
            rdp['sendData']('event_house', "buy,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);            
        });
    },

    bindVehicles: function () {
    	$('#CloseBaulWindow').click(function() {
    	 	$('#BaulInfo').hide();
    	 	$('#BaulInt').html("");
    	 	rdp['sendData']('event_vehicle', "closebaul,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
    	});

    	$('#CloseShopWindow').click(function() {
    	 	$('#CarShopInfo').hide();
    	 	rdp['sendData']('event_vehicle', "closeshop,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
    	});

        $("#CarsList").on('click', '.shopcar', function(){
          var data = this.id + ',';
          rdp['sendData']('event_vehicle', "shop,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindProducts: function () {

        $('#CloseMallWindow').click(function() {
            $('#Trabas').hide();
            $('#Alarmas').hide();
            $('#Loader').hide();
            $('#ProductMallBack').hide();
            $('#ProductsPrincipal').show();
            $('#ProductsMallInfo').hide();
            $('#ProdMsg').html("");
            $('#GruaMsg').html("");
            $('#FerreMsg').html("");
            $('#SellCarMsg').html("");
            rdp['sendData']('event_products', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#ProductMallBack').click(function() {
            $('#ProductsPrincipal').show();
            $('#Trabas').hide();
            $('#Alarmas').hide();
            $('#ProductMallBack').hide();
            $('#ProdMsg').html("");
            $('#GruaMsg').html("");
            $('#FerreMsg').html("");
            $('#SellCarMsg').html("");
        });

        $('.shopproducts').on('click', '.myButton', function(){
            if(this.id == "gotraba") {
                //Enviar indicación al emu para que éste retorne la info con los autos.
                $('#Loader').show();
                rdp['sendData']('event_products', "open_trabas,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
            else if(this.id == "goalarm") {
                //Enviar indicación al emu para que éste retorne la info con los autos.
                $('#Loader').show();
                rdp['sendData']('event_products', "open_alarmas,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
            else {
                var data = this.id + ',';
                rdp['sendData']('event_products', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });

        $("#Trabas").on('click', '.myButton', function(){
          var data = this.id + ',';
          rdp['sendData']('event_products', "buy_traba,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $("#Alarmas").on('click', '.myButton2', function(){
          var data = this.id + ',';
          rdp['sendData']('event_products', "buy_alarm,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseFerreWindow').click(function() {
            $('#ProductsFerreInfo').hide();
            $('#ProdMsg').html("");
            $('#GruaMsg').html("");
            $('#FerreMsg').html("");
            $('#SellCarMsg').html("");
            rdp['sendData']('event_products', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseGruaWindow').click(function() {
            $('#GruaService').hide();
            $('#Loader').hide();
            $('#GruaMsg').html("");
            //rdp['sendData']('event_products', "close_grua,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $("#MyVehicles").on('click', '.myButton3', function(){
          var data = this.id + ',';
          rdp['sendData']('event_products', "buy_grua,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseSellCar').click(function() {
            $('#SellCar').hide();
            $('#SellCarMsg').html("");
            //rdp['sendData']('event_products', "close_sellcar,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $("#MyVehiclesSell").on('click', '.myButton3', function(){
          var data = this.id + ',';
          rdp['sendData']('event_products', "buy_sellcar,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseCosechadorWindow').click(function() {
            $('#CosechadorInfo').hide();
            $('#CosechadorSeedsMsg').html("");
            //rdp['sendData']('event_products', "close_grange_seeds,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindMap: function () {
        $('#CloseMapWindow').click(function() {
            $('#MapInfo').hide();
        });
    },

    bindPhones: function () {
        // Window Phone Shop
        $('#ClosePhoneShopWindow').click(function() {
            $('#PhoneShopInfo').hide();
            rdp['sendData']('event_phone', "close_shop,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Listado de teléfonos a comprar
        $("#PhoneList").on('click', '.shopphone', function(){
          var data = this.id + ',';
          rdp['sendData']('event_phone', "buy_phone,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Click en icono inferior derecho de teléfono
        $("#PhoneButton").click(function() {
            rdp['sendData']('event_phone', "toggle_phone,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Click en el botón de cerrar teléfono
        $('#phone_close').click(function() {
            $('#phone').hide();// Cerramos visualmente
            rdp['sendData']('event_phone', "close_phone,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Click en el botón home (Cerrar todo)
        $('#phone_home').click(function() {

            $('.column').addClass('animated bounceIn');
 
            $('#AppViewer').hide();

            $('#app_Settings').hide();
            $('#app_Contacts').hide();
            $('#app_Messages').hide();
            $('#app_Health').hide();
            $('#app_Services').hide();
            $('#app_Phone').hide();
            $('#app_WhatsApp').hide();

            // Enviamos evento in_app
            rdp['sendData']('event_phone', "in_app,none,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Click en una APP
        $("#phone_content").on('click', '.appicon', function(){
            // Obtenemos información de la App (NameAPP)
            var data = $(this)[0].getAttribute("data-app");

            // Enviamos evento in_app
            rdp['sendData']('event_phone', "in_app,"+data+",", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // MAPS
        $('#Closeapp_Maps').click(function() {
            $('#app_Maps').hide();
        });
        $('#Close_MapInfo').click(function() {
            $('#MapInfo').hide();
        });
        // END MAPS

        // MESSAGES
        // Click en botón Enviar Mensaje Nuevo
        $("#app_Messages_New").on('click', '.SendButtonMsg', function(){
            var To = $('#app_Messages_New input[class=number]').val();
            var Msg = $('textarea[class=mensaje]').val();
            var ws = Msg + '|' + To;

            rdp['sendData']('event_phone', "send_message,"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Si clica en < Mensajes en ventana Mensaje Nuevo refrescamos Chatrooms
        $("#app_Messages_New").on('click', '.app_title_options', function(){
            rdp['sendData']('event_phone', "in_app,app_Messages,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Click en un chat específico
        $("#WS_Messages").on('click', '.app_msg_content', function(){
            var data =  $(this)[0].getAttribute("data-chatname");

            rdp['sendData']('event_phone', "open_messages," + data + ',', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Si clica en < Mensajes en ventana de Chatting refrescamos Chatrooms
        $("#WS_Messages_Chatting").on('click', '.app_title_options', function(){
            rdp['sendData']('event_phone', "in_app,app_Messages,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Click en el botón enviar dentro de un chat
        $("#WS_Messages_Chatting").on('click', '.SendButtonMsg', function(){
            var To = $('.app_messages_name')[0].innerHTML;
            var FilterMSG = $('#WS_Messages_Chatting .TextTo')[1].innerHTML;
            // Saltos de línea.
            FilterMSG = FilterMSG.replace(new RegExp('<div>', 'g'),'::br::');
            FilterMSG = FilterMSG.replace(new RegExp('</div>', 'g'),'');
            FilterMSG = FilterMSG.replace(new RegExp('<br>', 'g'),'::br::');
            var Msg = FilterMSG;
            var ws = Msg + '|' + To;
            rdp['sendData']('event_phone', "send_message,"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // ENTER en el input dentro de un chat
        $("#WS_Messages_Chatting").on('keydown', '.emoji-wysiwyg-editor', function(e){
            if (e.keyCode == 13 && !e.shiftKey)
            {
                var To = $('.app_messages_name')[0].innerHTML;
                var FilterMSG = $('#WS_Messages_Chatting .TextTo')[1].innerHTML;
                // Saltos de línea.
                FilterMSG = FilterMSG.replace(new RegExp('<div>', 'g'),'::br::');
                FilterMSG = FilterMSG.replace(new RegExp('</div>', 'g'),'');
                FilterMSG = FilterMSG.replace(new RegExp('<br>', 'g'),'::br::');
                var Msg = FilterMSG;
                var ws = Msg + '|' + To;
                rdp['sendData']('event_phone', "send_message,"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
                e.preventDefault();
            }
        });
        /* No se usa, ENTER hace salto de línea. */
        /*
        $("#WS_Messages_Chatting").on('keyup', '.emoji-wysiwyg-editor', function(e){
            if (e.keyCode == 13) {
                console.log("Entra");
                var To = $('.app_messages_name')[0].innerHTML;
                var FilterMSG = $('#WS_Messages_Chatting .TextTo')[1].innerHTML;
                // Saltos de línea.
                FilterMSG = FilterMSG.replace(new RegExp('<div><br></div>', 'g'), '');
                FilterMSG = FilterMSG.replace(new RegExp('<div>', 'g'),'::br::');
                FilterMSG = FilterMSG.replace(new RegExp('</div>', 'g'),'');
                FilterMSG = FilterMSG.replace(new RegExp('<br>', 'g'),'::br::');
                var Msg = FilterMSG;
                var ws = Msg + '|' + To;
                rdp['sendData']('event_phone', "send_message,"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });
        */
        // END MESSAGES

        // WHATSAPP
        // Click en un contacto específico
        $("#WS_WhatsApp_Contacts").on('click', '.app_msg_content', function(){
            var data =  $(this)[0].getAttribute("data-whatsname");

            rdp['sendData']('event_phone', "open_whatschats," + data + ',', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Click en un chat específico
        $("#WS_WhatsApp").on('click', '.app_msg_content', function(){
            var data =  $(this)[0].getAttribute("data-whatsname");

            rdp['sendData']('event_phone', "open_whatschats," + data + ',', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // Click en el botón enviar dentro de un chat
        $("#WS_WhatsApp_Chatting").on('click', '.SendButtonMsg', function(){
            var To = $('.app_whats_name')[0].innerHTML;
            var FilterMSG = $('#WS_WhatsApp_Chatting .TextTo')[1].innerHTML;
            // Saltos de línea.
            FilterMSG = FilterMSG.replace(new RegExp('<div>', 'g'),'::br::');
            FilterMSG = FilterMSG.replace(new RegExp('</div>', 'g'),'');
            FilterMSG = FilterMSG.replace(new RegExp('<br>', 'g'),'::br::');
            var Msg = FilterMSG;
            var ws = Msg + '|' + To;
            rdp['sendData']('event_phone', "send_whatsapp,|"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#What_Menu").on('click', '.Whats_Menu', function(){
            rdp['sendData']('event_phone', "in_app,app_WhatsApp,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // ENTER en el input dentro de un chat
        $("#WS_WhatsApp_Chatting").on('keydown', '.emoji-wysiwyg-editor', function(e){
            if (e.keyCode == 13 && !e.shiftKey)
            {
                var To = $('.app_whats_name')[0].innerHTML;
                var FilterMSG = $('#WS_WhatsApp_Chatting .TextTo')[1].innerHTML;
                // Saltos de línea.
                FilterMSG = FilterMSG.replace(new RegExp('<div>', 'g'),'::br::');
                FilterMSG = FilterMSG.replace(new RegExp('</div>', 'g'),'');
                FilterMSG = FilterMSG.replace(new RegExp('<br>', 'g'),'::br::');
                var Msg = FilterMSG;
                var ws = Msg + '|' + To;
                rdp['sendData']('event_phone', "send_whatsapp,|"+ws, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
                e.preventDefault();
            }
        });
        // END WHATSAPP

        // CONTACTS
        // enviar mensaje
        $("#app_Contacts").on('click', '.app_contacts_msg', function(){
            var data =  $(this)[0].getAttribute("data-contact");
            $('#app_Contacts').hide();
            $('.Messages_Title').hide();
            $('#WS_Messages').hide();
            $('#app_Messages').show();
            $('.app_Messages').show();
            $('#app_Messages_New .number').val(data);
            $('#app_Messages_New').show();            
            rdp['sendData']('event_phone', "in_app,app_Messages,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // llamar
        $("#app_Contacts").on('click', '.app_contacts_call', function(){
          var data =  $(this)[0].getAttribute("data-contact");
          rdp['sendData']('event_phone', "call_contact,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // eliminar contacto
        $("#app_Contacts").on('click', '.app_contacts_del', function(){
          var data =  $(this)[0].getAttribute("data-contact");
          rdp['sendData']('event_phone', "del_contact,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // search friends
        $("#Searching_Friends").on('click', '.SendButtonMsg', function(){// Clic en el botón buscar contacto
            var Username = $('input[class=search_friend]').val();
            rdp['sendData']('event_phone', "search_friend,"+Username, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#Searching_Friends").on('keyup', '#search_friend', function(e){// ENTER en el input buscar contacto
            if (e.keyCode == 13) {
                var Username = $('input[class=search_friend]').val();
                rdp['sendData']('event_phone', "search_friend,"+Username, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });
        // END CONTACTS

        // SERVICIOS APP
        $('#Serv_Medic').click(function() {
            rdp['sendData']('event_phone', "servicios,medico", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $('#Serv_Mec').click(function() {
            rdp['sendData']('event_phone', "servicios,mecanico", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        }); 
        $('#Serv_Arm').click(function() {
            rdp['sendData']('event_phone', "servicios,armero", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // END SERVICIOS APP
    },

    bindCommands: function () {
        // Multiselector de Menu
        $("#CommandsInfo").on('click', '.tab-2ddeR_0', function(){
            $("#CommandsInfo").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            $("#CMDS_Normales").hide();
            $("#CMDS_VIP").hide();
            $("#CMDS_Roleplay").hide();
            $("#CMDS_Vehiculos").hide();
            $("#CMDS_Viviendas").hide();
            $("#CMDS_Bandas").hide();
            $("#CMDS_Trabajos").hide();
            $("#CMDS_Empresas").hide();
            $("#CMDS_Terrenos").hide();
            $("#CMDS_Staff").hide();

            // Tabs Validation
            if($(this)[0].classList.contains('CMDS_Normales'))
                $("#CMDS_Normales").show();
            else if($(this)[0].classList.contains('CMDS_VIP'))
                $("#CMDS_VIP").show();
            else if($(this)[0].classList.contains('CMDS_Roleplay'))
                $("#CMDS_Roleplay").show();
            else if($(this)[0].classList.contains('CMDS_Vehiculos'))
                $("#CMDS_Vehiculos").show();
            else if($(this)[0].classList.contains('CMDS_Viviendas'))
                $("#CMDS_Viviendas").show();
            else if($(this)[0].classList.contains('CMDS_Bandas'))
                $("#CMDS_Bandas").show();
            else if($(this)[0].classList.contains('CMDS_Trabajos'))
                $("#CMDS_Trabajos").show();
            else if($(this)[0].classList.contains('CMDS_Empresas'))
                $("#CMDS_Empresas").show();
            else if($(this)[0].classList.contains('CMDS_Terrenos'))
                $("#CMDS_Terrenos").show();
            else if($(this)[0].classList.contains('CMDS_Staff'))
                $("#CMDS_Staff").show();
        });

        $('#CloseCommandsInfo').click(function() {
            $('#CommandsInfo').hide();
        });

        $('#CloseChangeLogs').click(function() {
            $('#ChangeLogs').hide();
        });
    },

    bindRules: function () {
        // Multiselector de Menu
        $("#RulesInfo").on('click', '.tab-2ddeR_0', function(){
            $("#RulesInfo").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            $("#RULES_Generales").hide();
            $("#RULES_Rol").hide();
            $("#RULES_PolRobSec").hide();
            $("#RULES_Jobs").hide();
            $("#RULES_VehViv").hide();
            $("#RULES_Gangs").hide();

            // Tabs Validation
            if($(this)[0].classList.contains('RULES_Generales'))
                $("#RULES_Generales").show();
            else if($(this)[0].classList.contains('RULES_Rol'))
                $("#RULES_Rol").show();
            else if($(this)[0].classList.contains('RULES_PolRobSec'))
                $("#RULES_PolRobSec").show();
            else if($(this)[0].classList.contains('RULES_Jobs'))
                $("#RULES_Jobs").show();
            else if($(this)[0].classList.contains('RULES_VehViv'))
                $("#RULES_VehViv").show();
            else if($(this)[0].classList.contains('RULES_Gangs'))
                $("#RULES_Gangs").show();
        });

        $('#CloseRulesInfo').click(function() {
            $('#RulesInfo').hide();
        });
    },

    bindInternet: function () {
        $('#CloseInternet').click(function() {
            $('#InternetOverlay').hide();
        });

        $('#IntBtnTimes').click(function() {
            $('#InternetInputSearch').val("");
        });
    },

    bindApartments: function () {
        $('#CloseApartShopWindow').click(function() {
            $('#ApartShop').hide();
        });

        $('#CloseApartElevatorWindow').click(function() {
            $('#ApartElevator').hide();
        });

        $('#CloseApartLockWindow').click(function() {
            $('#ApartLock').hide();
            $('#AP_Elevator_Pass').html("");
        });

        // Multiselector de Menu
        $("#ApartShop").on('click', '.tab-2ddeR_0', function(){
            $("#ApartShop").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            // Tabs Validation
            if($(this)[0].classList.contains('AP_Welcome_Tab'))
                rdp['sendData']('event_apart', "welcome,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('AP_New_Tab'))
                rdp['sendData']('event_apart', "new_apart,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('AP_Buy_Tab'))
                rdp['sendData']('event_apart', "offer_aparts,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('AP_Sell_Tab'))
                rdp['sendData']('event_apart', "my_offer_aparts,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        
        });
        // End Multiselector de Menu

        $("#AP_New_RoomModels").on('click', '.buyapart', function(){
          var data = this.id + ',' + $("#AP_New_Name").val().trim() + ',' + $("#AP_New_FloorEditor").val() + ',';
          console.log("=> " + $("#AP_New_FloorEditor").val());
          rdp['sendData']('event_apart', "buyapart,"+data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Multiselector de Menu
        $("#ApartElevator").on('click', '.tab-2ddeR_0', function(){
            $("#ApartElevator").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            // Tabs Validation
            if($(this)[0].classList.contains('AP_Elevator_Tab')) {
                $('#AP_Elevator_Search').val("");
                rdp['sendData']('event_apart', "apart_list,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
            else if($(this)[0].classList.contains('AP_MyAparts_Tab'))
                rdp['sendData']('event_apart', "my_aparts,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
        });
        // End Multiselector de Menu

        $("#apartlist").on('click', 'tr', function(){
            var data =  $(this)[0].getAttribute("data-room-id");
            $('#AP_Elevator_Display').val(data);
        });

        $('#AP_Elevator_Enter').click(function() {
            var data = $("#AP_Elevator_Display").val().trim();
            rdp['sendData']('event_apart', "enter_apart," + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $('#AP_Elevator_Pass_Try').click(function() {
            var data = $("#AP_Elevator_Pass_Roomid").val().trim();
            data += ',' + $("#AP_Elevator_Pass").val();
            rdp['sendData']('event_apart', 'enter_apart,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $("#AP_Elevator_Tab").on('keyup', '#AP_Elevator_Search', function(e){
            $("#ApartElevator").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $('.AP_Elevator_Tab').addClass('selected-3s9hj_0');

            var data = $("#AP_Elevator_Search").val().trim();
            rdp['sendData']('event_apart', 'search_apart,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $("#AP_My_Offers").on('click', '.btn_my_offer_apart',function() {
            var data = $(this)[0].getAttribute("value");
            rdp['sendData']('event_apart', 'toggle_offer_apart,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $("#AP_Offers").on('click', '.btn_offer_apart',function() {
            var data = $(this)[0].getAttribute("value");
            rdp['sendData']('event_apart', 'buy_offer_apart,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });
    },

    bindStats: function () {
        $(".menu-1sOe3_0").on('click', '.button-3IzmP_0',function() {
            var data = $(this)[0].getAttribute("data-overlay");
            var action = "";

            if(data == "GangsWindow")
                action = "event_gang";
            if(data == "CombatMode")
                action = "event_target";
            if(data == "WantedList")
                action = "event_stats";
            if(data == "PSVMode")
                action = "event_psv";

            rdp['sendData'](action, 'open,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $('#CloseWantedList').click(function() {
            $('#WantedList').hide();
        });
    },

    bindGangs: function () {
        $('#CloseGangsWindow').click(function() {
            $('#GangsWindow').hide();
            $('#GangsMsg').html("");
            $('#GangsMsg').hide();
        });

        // Multiselector de Menu
        $("#GangsWindow").on('click', '.tab-2ddeR_0', function(){
            $("#GangsWindow").find('.tab-2ddeR_0').removeClass('selected-3s9hj_0');
            $(this).addClass('selected-3s9hj_0');

            // Tabs Validation
            if($(this)[0].classList.contains('GA_Bandas_Tab'))
                rdp['sendData']('event_gang', "open,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_Invitations_Tab'))
                rdp['sendData']('event_gang', "invitations_re,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_MyNew_Tab'))
                rdp['sendData']('event_gang', "mynew,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        // End Multiselector de Menu

        $('#GA_New_Btn').click(function() {
            var data = $("#GA_New_Name").val().trim();
            data += "," + $("#GA_New_Type").val().trim();
            data += "," + rgb2hex($("#ColorPrim").css('background-color')).split('#')[1];
            data += "," + rgb2hex($("#ColorSec").css('background-color')).split('#')[1];
            //data += "," + $(".prevplaca_box").css('background-image').replace('url(','').replace(')','').replace(/\"/gi, "").replace(habboReqSite, "");

            var badge = $(".prevplaca_box").css('background-image').replace('url(','').replace(')','').replace(/\"/gi, "");

            data += "," + badge.split("badge/")[2];

            rdp['sendData']('event_gang', "create," + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        // Multiselector de Menu Tabbed
        $("#GangsWindow").on('click', '.Tabbed_tab_1apzZ', function(){
            $("#GangsWindow").find('.Tabbed_tab_1apzZ').removeClass('Tabbed_selected_3aJyT');
            $(this).addClass('Tabbed_selected_3aJyT');

            // Tabs Validation
            if($(this)[0].classList.contains('GA_My_Members'))
                rdp['sendData']('event_gang', "mynew,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_My_Ranks'))
                rdp['sendData']('event_gang', "ranks,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_My_Requests'))
                rdp['sendData']('event_gang', "requests,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_My_Stats'))
                rdp['sendData']('event_gang', "stats,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_My_Edit'))
                rdp['sendData']('event_gang', "edit,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
            else if($(this)[0].classList.contains('GA_Inv_Received'))
                rdp['sendData']('event_gang', "invitations_re,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            else if($(this)[0].classList.contains('GA_Inv_Sended'))
                rdp['sendData']('event_gang', "invitations_se,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            
            else if($(this)[0].classList.contains('GA_View_Members')) {
                var Gang = parseInt($(this)[0].getAttribute("data-v-gang"));
                rdp['sendData']('event_gang', "view," + Gang, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
            else if($(this)[0].classList.contains('GA_View_Stats')) {

                var Gang = parseInt($(this)[0].getAttribute("data-v-gang"));
                rdp['sendData']('event_gang', "view_stats," + Gang, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        
        });
        // End Multiselector de Menu Tabbed

        $("#GA_My_Tabbed_body").on('click', '.px-1', function(){
            var Rank = parseInt($(this)[0].getAttribute("data-rank"));
            var User = parseInt($(this)[0].getAttribute("data-user"));
            var Action = $(this)[0].getAttribute("data-action");
            if(!isNaN(Rank)){
                if(!isNaN(User) || Action == "settings" || Action == "up" || Action == "down" || Action == "cross") {
                    // Send WS Ranks
                    rdp['sendData']('event_gang', "rank_tools,"+Rank+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
                }
                else {
                    // Send permissions ranks
                    rdp['sendData']('event_gang', 'editrank,permissions,' + Rank + ',' + Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
                }
            }
            else{
                // Send WS User
                rdp['sendData']('event_gang', "member_tools,"+User+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });

        $('#GA_My_Tabbed_body').on('click', '#GA_My_AddRank',  function(){
            var NameRank = $('input[id=GA_InputRank]').val();   
            var data = 'manage,addrank,' + NameRank;

            rdp['sendData']('event_gang', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $('#GA_My_Tabbed_body').on('click', '.dark-button', function(){
            var Action = $(this)[0].getAttribute("data-action");
            var Rank = parseInt($(this)[0].getAttribute("data-rank"));
            var data = null;

            if(Action == "SaveRank") {
                var RankNewName = $('input[id=GangRankNewName]').val();  
                var data = 'editrank,saverank,' + Rank + ',' + RankNewName + ',' + RankPay + ',' + RankTimer;
            }
            else if(Action == "EditName") {
                var NewGangName = $('input[id=GA_Edit_Name]').val();  
                var data = 'edition,newname,' + NewGangName;
            }
            else if(Action == "Transfer") {
                var NewAdmin = $('select[id=GA_Edit_Trans_U]').children("option:selected").val();
                var data = 'edition,transfer,' + NewAdmin;
            }
            else if(Action == "Delete") {
                var data = 'edition,delete';
            }
            else if(Action == "Withdraw") {
                var Amount = $('input[id=GA_Withdraw]').val();  
                var data = 'edition,withdraw,' + Amount;
            }
            else if(Action == "SaveGang") { 
                var data = 'edition,savegang';
            }

            if(data != null)
                rdp['sendData']('event_gang', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);         
        });

        $('#GA_Inv_Tabbed_body').on('click', '#GA_Send_Inv',  function(){
            var Username = $('input[id=Input_G_I_S_User]').val();   
            var data = 'send_invitation,' + Username;

            rdp['sendData']('event_gang', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $("#GA_Inv_Tabbed_body").on('click', '.px-1', function(){
            var Gang = parseInt($(this)[0].getAttribute("data-gang"));
            var Action = $(this)[0].getAttribute("data-action");

            if(!isNaN(Gang)){
                rdp['sendData']('event_gang', "inv_re_tools,"+Gang+","+Action, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });

        $('#GA_My_Tabbed_body').on('change', '#GA_Edit_Type', function(){
            var NewGangType = $(this).children("option:selected").val(); 
            var data = 'edition,newtype,' + NewGangType;

            rdp['sendData']('event_gang', data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);       
        });

        $("#GA_Bandas_Tab").on('keyup', '#GA_Search', function(e){
            var data = $("#GA_Search").val().trim();
            rdp['sendData']('event_gang', 'search,' + data, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
        });

        $("#GA_Bandas_Tab").on('click', '.data-gang', function(){
            var Gang = parseInt($(this)[0].getAttribute("data-gang"));
            if(!isNaN(Gang)){
                rdp['sendData']('event_gang', "view,"+Gang, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });

        $("._GangStat").on('click', '.data-gang', function(){
            var Gang = parseInt($(this)[0].getAttribute("data-gang"));
            if(!isNaN(Gang)){
                rdp['sendData']('event_gang', 'open,', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']); 
                rdp['sendData']('event_gang', "view,"+Gang, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
            }
        });
    },

    bindTutorial: function () {
        $("#TutorialCanvas").on('click', '.tuto11', function(){
            $("#TutorialCanvas").find('.tuto11').removeClass('tuto11');
            rdp['sendData']('event_tutorial', 'step,11', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#TutorialCanvas").on('click', '.tuto13', function(){
            $("#TutorialCanvas").find('.tuto13').removeClass('tuto13');
            rdp['sendData']('event_tutorial', 'step,13', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#TutorialCanvas").on('click', '.tuto18', function(){
            $("#TutorialCanvas").find('.tuto18').removeClass('tuto18');
            rdp['sendData']('event_tutorial', 'step,18', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#TutorialCanvas").on('click', '.tuto23', function(){
            $("#TutorialCanvas").find('.tuto23').removeClass('tuto23');
            rdp['sendData']('event_tutorial', 'step,23', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#TutorialCanvas").on('click', '.tuto36', function(){
            $("#TutorialCanvas").find('.tuto36').removeClass('tuto36');
            rdp['sendData']('event_tutorial', 'step,36', false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindTaxi: function () {
        $('#CloseTaxiWindow').click(function() {
            $('#TaxiInfo').hide();
            rdp['sendData']('event_taxi', "closeinfo,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $("#TaxiList").on('click', '.group', function(){
            var Id = $(this)[0].getAttribute("data-id");
            rdp['sendData']('event_taxi', 'calltaxi,' + Id, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindMunicipalidad: function () {
        $('#CloseChangeName').click(function() {
            $('#ChangeName').hide();
            rdp['sendData']('event_changename', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
        $('#Btn_ChangeName').click(function() {
            var Newname = $('#CHN_NewName').val();
            var reg = /^[A-Za-z0-9]*$/;

            if(Newname.length < 3 || Newname.length > 18){
                $('#ChangeNameMsg').html("<b style='color:red'>Tu nuevo nombre debe tener entre 3 y 18 caracteres.</b>");
            }
            else if(!reg.test(Newname))
            {
                $('#ChangeNameMsg').html("<b style='color:red'>Tu nuevo nombre no puede contener caracteres especiales ni espacios.</b>");
            }
            else
                rdp['sendData']('event_changename', "changename," + Newname, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindCamionero: function () {
        $('#CloseCargasWindow').click(function() {
            $('#CargasInfo').hide();
            rdp['sendData']('event_camionero', "close,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $("#CargasInfo").on('click', '.selectcarga', function(){
            var CargaId = parseInt($(this)[0].getAttribute("data-carga"));
            rdp['sendData']('event_camionero', "cargar," + CargaId, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#ActionCam').click(function() {
            var camAction = $(this)[0].getAttribute("data-action");
            rdp['sendData']('event_camionero', camAction + ",", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#AbandonarCam').click(function() {
            rdp['sendData']('event_camionero', "abandonar,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindBasurero: function () {
        $('#ActionBasu').click(function() {
            rdp['sendData']('event_basurero', "descargar,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#AbandonarBasu').click(function() {
            rdp['sendData']('event_camionero', "abandonar,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindArmero: function () {
        $('#CloseArmPiecesWindow').click(function() {
            $('#ArmPiecesInfo').hide();
            rdp['sendData']('event_armero', "close_pieces,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CloseArmCreateWindow').click(function() {
            $('#ArmCreateInfo').hide();
            rdp['sendData']('event_armero', "close_weapons,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#CreatePieces').click(function() {
            rdp['sendData']('event_armero', "create_pieces,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('.CreateGun').click(function() {
            var gunName = $(this)[0].getAttribute("data-gun");
            rdp['sendData']('event_armero', "create_weapons," + gunName, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindHospital: function () {
        $('#CloseHospWindow').click(function() {
            $('#HospBotiq').hide();
            rdp['sendData']('event_hospital', "close_botiq,", false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#HospUseBotiq').click(function() {
            var botiquinID = $('#botiquinID').find(":selected").val();
            rdp['sendData']('event_hospital', "use_botiq," + botiquinID, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });

        $('#HospActionBtn').click(function() {
            var action = $(this)[0].getAttribute("data-action");
            var target = $('.2 ._1KZQE').text();
            rdp['sendData']('event_hospital', action + "," + target, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    bindBodyGuard: function () {
        $('#GuardToggleBtn').click(function() {
            $("#GuardSellPanel").animate({width: 'toggle'});
        });

        $('#GuardSellBtn').click(function() {
            var price = $("#GuardVestPrice").val();
            var target = $('.2 ._1KZQE').text();
            $("#GuardVestPrice").val("");
            rdp['sendData']('event_offers', "bodyguard,cubrir," + price + "," + target, false, false, rdp_app['webSocket'], rdp_app['startedSocket'], rdp_app['UserID']);
        });
    },

    RepositionToolTip: function(x,y){
    	$('#tooltip').css({'left': x + 15,'top': y + 5});
    },

    tryReconnect: function () {
		
		rdp_app['LogConsole']('Tratando de reconectar con servidor WebSocket');
		rdp_app['webSocket'].close();
		rdp_app['webSocket'] = null;
		rdp_app['initSockets']();
		
	},

   	SocketStatus: function(Status){

     $('#ws-status .component-body .ws-st').text("Conectado!");
     $('#ws-status .component-body .ws-st').css('text-shadow: 1px 1px 1px rgba(36, 148, 0, 0.8);');

	},
       
   	LogConsole: function(string, extra = "", extra2 = ""){
       var DataExtra = "";
       var DataExtra2 = "";

		if(extra != "")
		 DataExtra = " [" + extra + "]";

		if(extra2 != "")
		 DataExtra2 = "[" + extra2 + "]";

		console.log("[RDP][WebSocket] -> " + string + DataExtra + DataExtra2 + "");
	},

   	LogPackets: function(event){
        console.log("%c#############################","color: #a418d0;");
      	console.log("%c#[DEBUG] :: [RDP][WebSocket]# Packet Type -> " + ( (rdp_app['isJSONData'](event.data)) ? "json" : "split_text") + "", "color: #a418d0;");
      	console.log("%c#[DEBUG] :: [RDP][WebSocket]# Data Info -> " + ( (rdp_app['isJSONData'](event.data)) ? JSON.stringify(JSON.parse(event.data)) : event.data) + "","color: #a418d0;");
      	console.log("%c#############################", "color: #a418d0;");
	},

   	isJSONData: function(data){
		try {
			JSON.parse(data);
		} catch (e) {
			return false;
		}
		return true;
	}
};

import rdp from './modules/rdp.c.js?p=stafftoolupdatev5';
export default rdp_app;