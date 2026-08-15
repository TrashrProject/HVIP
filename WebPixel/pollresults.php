<?php
require_once "app/init.pz.php";

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    echo "<script>window.location = '/';</script>";
    exit;
endif;

$SQL = $DB->Query("SELECT * FROM `rdp_polls_answers` GROUP BY `user_id`");
$Total = mysqli_num_rows($SQL);
?>
<!DOCTYPE html>
<html>
<head>
    <title>PixelZone - Resultados de la encuesta</title>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=1140">
    <link rel="shortcut icon" href="<?php echo DY; ?>/img/favicon.ico?v2" type="image/vnd.microsoft.icon">
    <meta name="csrf-token" content="vDd2f87t7d1JiOyDc3VoJSZKT6tRbszQB1aEtSMv">
    <link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700&amp;display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" integrity="sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T" crossorigin="anonymous">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css" integrity="sha384-oS3vJWv+0UjzBfQzYUhtDYW+Pj2yciDJxpsK1OYPAYjqT085Qq/1cq5FLXAZQ7Ay" crossorigin="anonymous">
    <link href="https://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css" rel="stylesheet">

    <link rel="stylesheet" href="<?php echo DY; ?>/css/pixelzone.css?1594536238">
    <link rel="stylesheet" href="<?php echo DY; ?>/css/dynamics.css?1594536238">

    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/animate.css/4.1.1/animate.min.css" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@10"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
</head>
<body>
<style type="text/css">
    table {
      font-family: arial, sans-serif;
      border-collapse: collapse;
      width: 100%;
      color:black;
    }

    th {
        text-align: center;
    }

    td {
        text-align: left;
        color:white;
    }

    td, th {
      border: 1px solid #dddddd;
      padding: 8px;
    }

    .staffname {
        background-color:#f1f1f1;
        color:black;
    }

    .malo, .regular, .bueno, .muy_bueno, .excelente {
        float: right;
    }
    .malo { color:red; }
    .regular { color:orange; }
    .bueno { color:green; }
    .muy_bueno { color:cyan; }
    .excelente { color:yellow; }
</style>

    <div class="content">
        <div class="container">
            <br><br>
            <div class="row">
                <div class="col-12">
                    <div class="content-box blue mb-12 animate__animated" style="padding:10px;">
                        <div class="box-content">
                            <center>
                                <a class="logo" href="#"></a>
                                <h4><b>Hola <?php echo $UData['username']; ?>, te presentamos los resultados de la encuesta que se realizó anteriormente.</b><br>De un total de <b><?php echo $Total; ?></b> encuestados los resultados son los siguientes:</h4><hr>
                            </center>

                            <?php
                                $Q = $DB->Query("SELECT * FROM `rdp_polls_quests`");
                                while($mQ = mysqli_fetch_assoc($Q)) {
                                    echo "<center><b style='font-size:20px'>(". $mQ["id"] . "/8) " . utf8_encode($mQ["text"]) . "</b></center><br>";
                            ?>
                                <table>
                                  <tr>
                                    <th style="border:0px"></th>
                                    <th style="background-color:#f1f1f1;">Malo</th>
                                    <th style="background-color:#f1f1f1;">Regular</th>
                                    <th style="background-color:#f1f1f1;">Bueno</th>
                                    <th style="background-color:#f1f1f1;">Muy Bueno</th>
                                    <th style="background-color:#f1f1f1;">Excelente</th>
                                  </tr>

                                <?php $S = $DB->Query("SELECT * FROM `users` WHERE `rank` >= 3 AND `rank` <= 5");
                                    while($mS = mysqli_fetch_assoc($S)) {
                                        $E = "";
                                        $L = "[AYU]";

                                        if($mS['id'] == 5)
                                            $E = "(antes Moonlightbae)";
                                        if($mS['id'] == 2149)
                                            $E = "(antes Perreointenso)";
                                        if($mS['id'] == 2780)
                                            $E = "(antes Akrylatkanone)";

                                        if($mS['rank'] == 5)
                                            $L = "[ADM]";
                                        else if($mS['rank'] == 4)
                                            $L = "[MOD]";

                                        $R = $DB->Query('SELECT (SELECT COUNT(*) FROM `rdp_polls_answers` WHERE `quest_id` = '.$mQ["id"].' AND `answer` = 1 AND `staff_id` = '.$mS["id"].') "MALO", (SELECT COUNT(*) FROM `rdp_polls_answers` WHERE `quest_id` = '.$mQ["id"].' AND `answer` = 2 AND `staff_id` = '.$mS["id"].') "REGULAR", (SELECT COUNT(*) FROM `rdp_polls_answers` WHERE `quest_id` = '.$mQ["id"].' AND `answer` = 3 AND `staff_id` = '.$mS["id"].') "BUENO", (SELECT COUNT(*) FROM `rdp_polls_answers` WHERE `quest_id` = '.$mQ["id"].' AND `answer` = 4 AND `staff_id` = '.$mS["id"].') "MUY BUENO", (SELECT COUNT(*) FROM `rdp_polls_answers` WHERE `quest_id` = '.$mQ["id"].' AND `answer` = 5 AND `staff_id` = '.$mS["id"].') "EXCELENTE"');
                                         while($mR = mysqli_fetch_assoc($R)) { 
                                            $NumMayor = -1;
                                            $Opcion = "";

                                            if($mR["MALO"] > $NumMayor){
                                                $NumMayor = $mR["MALO"];
                                                $Opcion = "<b class='malo'>Malo</b>";
                                            }
                                            if($mR["REGULAR"] > $NumMayor) {
                                                $NumMayor = $mR["REGULAR"];
                                                $Opcion = "<b class='regular'>Regular</b>";
                                            }
                                            if($mR["BUENO"] > $NumMayor) {
                                                $NumMayor = $mR["BUENO"];
                                                $Opcion = "<b class='bueno'>Bueno</b>";
                                            }
                                            if($mR["MUY BUENO"] > $NumMayor){
                                                $NumMayor = $mR["MUY BUENO"];
                                                $Opcion = "<b class='muy_bueno'>Muy Bueno</b>";
                                            }
                                            if($mR["EXCELENTE"] > $NumMayor){
                                                $NumMayor = $mR["EXCELENTE"];
                                                $Opcion = "<b class='excelente'>Excelente</b>";
                                            }
                                        ?>
                                            <tr>
                                                <td class="staffname"><?php echo "<b>" . $L . "</b> " . $mS['username'] . " " . $E . " " . $Opcion;?></td>
                                                <td><center><?php echo $mR["MALO"]; ?> (<?php echo round(($mR["MALO"]*100)/$Total); ?>%)</center></td>
                                                <td><center><?php echo $mR["REGULAR"]; ?> (<?php echo round(($mR["REGULAR"]*100)/$Total); ?>%)</center></td>
                                                <td><center><?php echo $mR["BUENO"]; ?> (<?php echo round(($mR["BUENO"]*100)/$Total); ?>%)</center></td>
                                                <td><center><?php echo $mR["MUY BUENO"]; ?> (<?php echo round(($mR["MUY BUENO"]*100)/$Total); ?>%)</center></td>
                                                <td><center><?php echo $mR["EXCELENTE"]; ?> (<?php echo round(($mR["EXCELENTE"]*100)/$Total); ?>%)</center></td>
                                            </tr>
                                <?php   }
                                    } ?>
                                </table>
                                <br>
                                <hr>
                            <?php
                                }
                              ?>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>