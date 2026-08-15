<?php
require_once "app/init.pz.php";

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    echo "<script>window.location = '/';</script>";
    exit;
endif;

if($UData['rank'] > 2 || $UPData["level"] <= 1):
    echo "<script>window.location = '/play';</script>";
    exit;
endif;

$STEP = 0;

$POLLS = $DB->Query("SELECT * FROM rdp_polls WHERE enabled = '1'");
if(mysqli_num_rows($POLLS) <= 0):
    echo "<script>window.location = '/play';</script>";
    exit;
endif;

$TotalStaffs = 0;

if(isset($_POST["poll"])){
    $P = AppFunctions::GeneralClean($_POST["poll"]);
    $Q = AppFunctions::GeneralClean($_POST["quest"]);
    $A = split(',', AppFunctions::GeneralClean($_POST["ans"]));

    $STAFFS = $DB->Query("SELECT * FROM users WHERE rank > 2 AND rank < 6 ORDER BY rank DESC, id ASC");
    $C = 0;
    $SQL = false;
    while($mStaff = mysqli_fetch_assoc($STAFFS)):
        $ANS = $A[$C];
        if($ANS < 1 || $ANS > 5) {
            $SQL = false;
            break;
        }
        $SQL = $DB->Query("INSERT rdp_polls_answers VALUES ('".$Q."', '".$P."', '".$UData['id']."', '".$mStaff['id']."', '".$ANS."', '".time()."');");
        $C++;
    endwhile;

    if(!$SQL):
        $Response['action'] = "alert";
        $Response['title'] = "¡Oh no!";
        $Response['type'] = "error";
        $Response['text'] = "No se ha podido registrar su respuesta. Intente nuevamente o consulte a un Desarrollador.";
    else:
        $Response['action'] = "nextQuest";
        $Response['title'] = "";
        $Response['type'] = "";
        $Response['text'] = "";
    endif;

    echo json_encode($Response);
    return;
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>HabboVIP - </title>
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
<body style="overflow: hidden;">
    <style type="text/css">
        .content-box {
            position: absolute;
        }
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
        }

        td, th {
          border: 1px solid #dddddd;
          padding: 8px;
        }
    </style>
    <div class="content">
        <div class="container">
            <br><br>
            <div class="row">
                <div class="col-12">
                    <?php
                        // Encuesta
                        while($mPoll = mysqli_fetch_assoc($POLLS)):

                            echo "
                                <script type=\"text/javascript\">window.document.title = 'HabboVIP - ".utf8_encode($mPoll["name"])."';</script>
                            ";

                            // Preguntas
                            $T = $DB->Query("SELECT * FROM rdp_polls_quests WHERE poll_id = " . $mPoll["id"]);
                            $TotalQuests = mysqli_num_rows($T);

                            $QUESTS = $DB->Query("SELECT * FROM rdp_polls_quests WHERE poll_id = " . $mPoll["id"] . " AND id NOT IN (SELECT quest_id FROM rdp_polls_answers WHERE quest_id = id  AND poll_id = " . $mPoll["id"] . " AND user_id = " . $UData["id"] . ")");

                            if(mysqli_num_rows($QUESTS)  > 0): ?>
                                <?php if($STEP == 0): $STEP++; ?>
                                    <div id="PollHome" class="content-box blue mb-12 animate__animated" style="padding:10px;">
                                        <div class="box-content">
                                            <center>
                                                <a class="logo" href="#"></a>
                                                <h4><b>Hola <?php echo $UData['username']; ?> ¿Sabías que tu opinión es importante para nosotros?</b></h4>
                                            </center>
                                            <br>
                                            <p><?php echo utf8_encode($mPoll['description']); ?></p>
                                            <p>Esta encuesta será totalmente <b>anónima</b>, así que te pedimos responder las preguntas con honestidad y sinceridad.</p>
                                            <p><u>Es necesario responder esta encuesta para poder acceder al juego. ¡No te tomará más de 5 minutos!</u></p>
                                            <br>
                                            <b>Muchas gracias por tu apoyo.<br>ATT: Desarrolladores de HabboVIP.</b>
                                            <p>
                                                <center>
                                                    <button id="GoPoll" type="button" class="button green enter-apex no-link-styling" onclick="return StartPoll();">Comenzar encuesta</button>
                                                </center>
                                            </p>
                                        </div>
                                    </div>
                                <?php endif; ?>
                    <?php
                            else:
                                echo "<script>window.location = '/play';</script>";
                                exit;
                            endif;

                            $Chk = 0;
                            while($mQuest = mysqli_fetch_assoc($QUESTS)):
                                if($mQuest['id'] != 1 && $STEP == 1 && $Chk == 0)
                                    $STEP = $mQuest['id'];
                                    
                    ?>
                                <div id="Quest_<?php echo $mQuest['id']; ?>" class="content-box blue mb-12 animate__animated" style="display: none;width: 100%">
                                    <div class="title">
                                        <?php echo utf8_encode($mQuest["text"]) . " (".$mQuest['id']."/".$TotalQuests.")"; ?>
                                    </div>
                                    <div class="box-content">
                                        <table>
                                          <tr>
                                            <th style="border:0px"></th>
                                            <th style="background-color:#f1f1f1;">Malo</th>
                                            <th style="background-color:#f1f1f1;">Regular</th>
                                            <th style="background-color:#f1f1f1;">Bueno</th>
                                            <th style="background-color:#f1f1f1;">Muy Bueno</th>
                                            <th style="background-color:#f1f1f1;">Excelente</th>
                                          </tr>
                                          
                                        <?php
                                            $STAFFS = $DB->Query("SELECT * FROM users WHERE rank > 2 AND rank < 6 ORDER BY rank DESC, id ASC");
                                            $TotalStaffs = mysqli_num_rows($STAFFS);
                                            $I = 0;
                                            while($mStaff = mysqli_fetch_assoc($STAFFS)):
                                                $E = "";
                                                $L = "[AYU]";

                                                if($mStaff['id'] == 5)
                                                    $E = "(antes Moonlightbae)";
                                                if($mStaff['id'] == 2149)
                                                    $E = "(antes Perreointenso)";

                                                if($mStaff['rank'] == 5)
                                                    $L = "[ADM]";
                                                else if($mStaff['rank'] == 4)
                                                    $L = "[MOD]";

                                        ?>
                                            <tr>
                                                <td style="background-color:#f1f1f1;"><?php echo "<b>" . $L . "</b> " . $mStaff['username'] . " " . $E;?></td>
                                                <td><center><input name="q<?php echo $mQuest['id']; ?>[<?php echo $I; ?>]" type="radio" value="1"></center></td>
                                                <td><center><input name="q<?php echo $mQuest['id']; ?>[<?php echo $I; ?>]" type="radio" value="2"></center></td>
                                                <td><center><input name="q<?php echo $mQuest['id']; ?>[<?php echo $I; ?>]" type="radio" value="3"></center></td>
                                                <td><center><input name="q<?php echo $mQuest['id']; ?>[<?php echo $I; ?>]" type="radio" value="4"></center></td>
                                                <td><center><input name="q<?php echo $mQuest['id']; ?>[<?php echo $I; ?>]" type="radio" value="5"></center></td>
                                            </tr>
                                        <?php $I++; endwhile; ?>                                          
                                        </table>
                                        <p>
                                            <center>
                                                 <button type="button" class="button green enter-apex no-link-styling" onclick="return NextQuest(<?php echo $mPoll['id']; ?>, <?php echo $mQuest['id']; ?>, <?php echo $TotalStaffs; ?>, <?php echo $TotalQuests; ?>);">Siguiente</button>
                                            </center>
                                        </p>
                                        <hr>
                                        <p>
                                            <center>
                                                <small>
                                                    Al dar clic en siguiente confirmas la respuesta a esta pregunta. Revisa las opciones antes de continuar.
                                                </small>
                                            </center>
                                        </p>
                                    </div>
                                </div>
                            <?php $Chk++; endwhile; ?>
                    <?php endwhile; ?>
                    <!--<div class="content-box blue mb-12">
                        <div class="title">
                            <i class="fas fa-id-card text-secondary"></i> Estad&iacute;sticas de Juego
                        </div>
                        <div class="box-content">

                        </div>
                    </div>-->
                </div>
            </div>
        </div>
    </div>

</body>
<script type="text/javascript">
    var STEP = <?php echo $STEP; ?>;

    if(STEP > 1) {
        $("#GoPoll").html("Continuar encuesta");
    }

    function StartPoll() {        
        $("#PollHome").addClass("animate__backOutLeft");
        $("#Quest_" + STEP).addClass("animate__backInRight").show();
    }

    function NextQuest(poll, quest, totalStaffs, totalQuests){
        var Answers = [];
        $('input[type=radio]:checked').each(function() {
            Answers.push($(this).val());
            //console.log($(this).val(), $(this).attr('name'));
        });

        if(Answers.length < totalStaffs) {
            Swal.fire(
              "¡Oh no!",
              "Debes seleccionar una opción para cada miembro Staff.",
              "error"
            );
            return false;
        }

        var data = "poll=" + poll + "&quest=" + quest + "&ans=" + Answers;

        $.ajax({
            type:'POST',
            url:'/poll.php',
            data:data,
            success:function(resp){
                var Data = jQuery.parseJSON(resp);
                if(Data["action"] == "alert") {
                    Swal.fire(
                      Data["title"],
                      Data["text"],
                      Data["type"]
                    );
                }
                if(Data["action"] == "nextQuest") {
                    nextQuest(totalQuests);
                }              
            }
        });
        return false;
    }

    function nextQuest(totalQuests) {
        var OldStep = STEP;
        STEP++;

        if(STEP <= totalQuests)
        {
            $("#Quest_" + OldStep).addClass("animate__backOutLeft");
            $("#Quest_" + STEP).addClass("animate__backInRight").show();

            setInterval(function(){ 
                $("#Quest_" + OldStep).remove();
            }, 3000);
        }
        else {
            Swal.fire(
              "¡Bien hecho!",
              "¡Encuesta terminada! Muchas gracias por tus respuestas. Ahora puedes entrar al juego.",
              "success"
            ).then(function() {
                window.location = "/play";
            });
        }
    }
</script>
</html>