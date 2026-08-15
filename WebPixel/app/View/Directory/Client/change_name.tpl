<?php 
	if(!EnumToBool(User($_SESSION['ID'], 'facebook_change_name'))){
		echo "<script>window.location.href = '/client';</script>";
	}
?>
<!DOCTYPE html>
<html>
<head>
	<meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title><?php echo Kernel('site_name'); ?> ~ Cambia tu nombre</title>
    <link rel="icon" type="image/png" href="<?php echo CDN; ?>/general/images/favicon.ico" />
    <meta name='viewport' content='width=device-width, initial-scale=1'>

    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css">
    <!-- Google Fonts -->
    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap">
    <!-- Bootstrap core CSS -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.4.1/css/bootstrap.min.css" rel="stylesheet">
    <!-- Material Design Bootstrap -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/mdbootstrap/4.14.0/css/mdb.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css">

    <style type="text/css">
		#error{background-color:#e53935;width:100%;color:#fff;padding:7px;font-size:17px}
		#success{background-color:#25a20e;width:100%;color:#fff;padding:7px;font-size:17px}
    </style>
</head>
<body>
	<div class="row">
		<div class="col-12">
			<div id="alerts"></div>
		</div>
	</div>
	<br><br>	
	<div class="row">
		<div class="col-4"></div>
		<div class="col-4">
			<form class="text-center border border-light p-5" id="change_name" method="post" onsubmit="return ChangeName();">

			    <p class="h4 mb-4">Bienvenido(a) <?php echo User($_SESSION['ID'], 'username'); ?></p>
			    <p>Para poder entrar a Rolear, es necesario que definas un nombre de usuario v&aacute;lido y &uacute;nico.</p>
			    <input id="fbusername" type="text" class="form-control mb-4" placeholder="Nuevo nombre de usuario">
			    <input id="fbid_change" type="hidden" value="<?php echo User($_SESSION['ID'], 'facebook_id'); ?>">

			    <button class="btn btn-info btn-block my-4" type="submit">Continuar</button>

			    <img src="<?php echo CDN; ?>/general/images/fb.gif">
			</form>
		</div>
	</div>

    <!-- JQuery -->
	<script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
	<!-- Bootstrap tooltips -->
	<script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.4/umd/popper.min.js"></script>
	<!-- Bootstrap core JavaScript -->
	<script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.4.1/js/bootstrap.min.js"></script>
	<!-- MDB core JavaScript -->
	<script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/mdbootstrap/4.14.0/js/mdb.min.js"></script>

	<script type="text/javascript">
		function ChangeName(){
			var fbid_change = document.getElementById("fbid_change").value;
			var fb_username = document.getElementById("fbusername").value;

			var data = "fbid_change=" + fbid_change + "&fb_username=" + fb_username;
			console.log(data);

			$.ajax({
				type:'POST',
				url:'init.php',
				data:data,
				success:function(resp){
					$("#alerts").html(resp);
				}
			});
			return false;
		}
	</script>
</body>
</html>