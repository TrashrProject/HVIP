<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once "../../../../../../../../app/init.pz.php";

// Redirect if not logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

setlocale(LC_ALL, "");

$CurNew = isset($_GET["read"]) ? AppFunctions::InjectionCleaner($_GET["read"]) : null;

$getNew = $DB->Query("SELECT * FROM rdp_news WHERE id = '".$CurNew."'");
$CurNew = mysqli_fetch_assoc($getNew);

if ($CurNew != null){
    $CTN = html_entity_decode($CurNew["content"]);
}

?>

<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>Eyefind</title>
    <meta name='viewport' content='width=device-width, initial-scale=1'>

    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css">
    <!-- Google Fonts -->
    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap">
    <!-- Bootstrap core CSS -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.4.1/css/bootstrap.min.css" rel="stylesheet">
    <!-- Material Design Bootstrap -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/mdbootstrap/4.14.0/css/mdb.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css">
    <!-- Emoji Picker -->
    <link href="http://onesignal.github.io/emoji-picker/lib/css/emoji.css" rel="stylesheet">
    <!-- MDBootstrap Datatables  -->
    <link href="https://cdn.datatables.net/1.10.20/css/jquery.dataTables.css" rel="stylesheet">

    <style type="text/css">
        .container {
            width: 75% !important;
        }
        .row {
            width: 100% !important;
        }

        .EF_Header {
            background-color: #4a8bbf;
            padding: 10px;
            width: 100%;
            border-bottom: 10px solid #fbbf0c;
        }

        .EF_Logo{
            background-image: url("../resources/images/eyefind-logo.png");
            background-size: 200px;
            background-repeat: no-repeat;
            height: 90px;
            width: 200px;
        }

        .EF_Time {
            color: white;
            text-align: right;
            right: 0;
        }

        .EF_Time .icon {
            color: #f6c00a;
            font-size: 40px;
        }

        .vertical-center {
          margin: 0;
          position: absolute;
          top: 50%;
          -ms-transform: translateY(-50%);
          transform: translateY(-50%);
        }

        .EF_SearchBar i { 
            position: absolute; 
        } 
          
        .EF_SearchBar{ 
            padding-left: 40px;
        } 
          
        .EF_SearchBar .icon { 
            padding: 4px 4px 4px 10px;
            /* min-width: 40px; */
            font-size: 30px;
            color: #638ab6; 
        } 
          
        .EF_SearchBar .input-field {
            padding: 7px 7px 7px 45px;
            width: 100%;
            border: 1px solid grey;
        }

        .EF_Btn_Random {
            padding: 5px;
            width: 100%;
            font-size: 20px;
            color: white;
            background-color: #05728d;
            border: 0;
            border-radius: 5px;
            outline: none;
        }

        .EF_Btn_Random:hover {
            background-color: #0683a2;
        }

        .EF_Btn_Random:active {
            background-color: #04657d;
        }

        .EF_Nav {
            background-color: #f0f0f0;
            padding:0;
            padding-left:27px;
            border-top:1px solid #b7b7b7;
            border-bottom:1px solid #b7b7b7;
        }
        .EF_Nav .container {
            width: 69.2% !important;
            padding: 0;
        }
        .EF_Nav .nav-item{
            padding-left: 15px;
            padding-right: 15px;
        }
        .EF_Nav .nav-item:hover {
            background-color: #eaeaea;
        }
        .EF_Nav .container .icon {
            font-size: 30px;
            color: #638ab6;
            margin-left: 10px;
        }

        .EF_Body {
            padding-left:27px;
        }

        .EF_Body .container {
            width: 69.2% !important;
            padding: 0;
            padding-top: 20px;
        }

        .EF_Body .container .row {
            padding:0;
            margin: 0;
        }

        .ef-content-box {
            background-color: #f3f3f3;
            text-align: left;
            padding: 10px;
            margin-bottom: 10px;
        }

        .ef-ad-blue {
            background-color: #4f91c2;
            font-size: 14px;
            color:white;
            text-align: center;
        }

        .EF_title_box {
            font-size: 18px;
        }

        .EF_footer_box {
            font-size: 16px;
        }

        /* NEWS */
        .media-item-ctr-1 {
            border-bottom: 1px solid #e5e3da;
            padding-bottom: 1em;
            margin-top: 1em;
            width: 100%;
            position: relative;
            display: flex;
        }
        .media-item-ctr-1 .media-item-thumbnail {
            display: block;
            position: relative
        }
        .media-item-ctr-1 .media-item-thumbnail img {
            width: 100%;
            max-height: 160px
        }
        .media-item-ctr-1 .media-item-overview {
            position: relative;
            min-height: 140px;
            padding-bottom: 15px
        }
        .media-item-ctr-1 .media-item-overview h3 {
            margin: 5px 0
        }
        .media-item-ctr-1 .media-item-overview h3 a {
            color: #222;
            text-decoration: none
        }
        .media-item-ctr-1 .media-item-overview h3 a:hover,
        .media-item-ctr-1 .media-item-overview h3 a:focus,
        .media-item-ctr-1 .media-item-overview h3 a:active {
            color: #222
        }
        .media-item-ctr-1 .media-item-overview p {
            font-weight: 300;
            margin-top: 6px
        }
        .media-item-ctr-1 .media-item-overview .media-item-cat {
            text-transform: uppercase;
            font-size: 12px
        }
        .media-item-ctr-1 .media-item-overview .media-meta-data {
            margin-top: 10px;
            color: #b5b3ac;
            font-size: 11px
        }
        .media-item-ctr-1 .social-share-ctr {
            position: absolute;
            right: 10px;
            bottom: 20px
        }
        .media-item-ctr-1 .social-share-ctr .share-media {
            border-radius: 50%;
            border: none;
            outline: none;
            width: 25px;
            height: 25px;
            font-size: 12px
        }
        .media-item-ctr-2 {
            padding-bottom: 1em;
            margin-top: 1em;
            width: 100%;
            position: relative;
            float: left
        }
        .media-item-ctr-2 .media-item-thumbnail {
            display: block;
            position: relative
        }
        .media-item-ctr-2 .media-item-thumbnail img {
            width: 100%;
            max-height: 160px
        }
        .media-item-ctr-2 .media-item-overview {
            position: relative;
            padding-bottom: 15px
        }
        .media-item-ctr-2 .media-item-overview h3 {
            margin: 5px 0
        }
        .media-item-ctr-2 .media-item-overview h3 a {
            color: #222;
            text-decoration: none
        }
        .media-item-ctr-2 .media-item-overview h3 a:hover,
        .media-item-ctr-2 .media-item-overview h3 a:focus,
        .media-item-ctr-2 .media-item-overview h3 a:active {
            color: #222
        }
        .media-item-ctr-2 .media-item-overview .media-meta-data {
            margin-top: 10px;
            color: #b5b3ac;
            font-size: 11px
        }
        .two-column-clear:nth-child(2n+1) {
            clear: both
        }
        .three-column-clear:nth-child(3n+1) {
            clear: both
        }
        .media-ribbon {
            position: absolute;
            top: 15px;
            left: -8px;
            background-color: #ff4500;
            padding: 0 3px;
            color: #fff;
            letter-spacing: 1px;
            text-transform: uppercase;
            line-height: 24px;
            font-size: 11px
        }
        .media-ribbon:before {
            position: absolute;
            top: 100%;
            left: 0;
            width: 0;
            content: '';
            border-top: 8px solid #992900;
            height: 0;
            border-left: 8px solid transparent
        }
        .media-post-ctr .media-post-header {
            position: relative
        }
        .media-post-ctr .media-post-header .category {
            display: inline-block;
            color: #fff;
            padding: 3px 15px;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase
        }
        .media-post-ctr .media-post-header .media-title {
            font-weight: bold;
            margin: 10px 0
        }
        .media-post-ctr .media-post-header .media-description {
            font-size: 16px;
            font-weight: 300
        }
        .media-post-ctr .media-post-header .media-meta-ctr {
            margin: 15px 0;
            position: relative
        }
        .media-post-ctr .media-post-header .media-meta-ctr .author-profile-img {
            float: left;
            margin-right: 20px
        }
        .media-post-ctr .media-post-header .media-meta-ctr .author-profile-img img {
            border-radius: 50%;
            width: 50px;
            height: 50px
        }
        .media-post-ctr .media-post-header .media-meta-ctr .media-meta-overview-ctr {
            margin-left: 70px
        }
        .media-post-ctr .media-post-header .media-meta-ctr .media-meta-overview-ctr .media-meta-author-info {
            border-bottom: 1px solid #ccc;
            padding-bottom: 8px
        }
        .media-post-ctr .media-post-header .media-meta-ctr .media-meta-overview-ctr .author-social-link {
            margin-left: 6px;
            color: #bbb
        }
        .media-post-ctr .media-post-header .media-meta-ctr .media-meta-overview-ctr .media-meta-post-info {
            color: #bbb;
            padding-top: 3px
        }
        .media-post-ctr .media-post-header .btn-like-media {
            position: absolute;
            top: 0;
            right: 0
        }
        .media-page-content {
            margin: 25px 0;
            font-size: 15px
        }
        .media_blockquote {
            position: relative
        }
        .media_blockquote .fa-quote-left {
            position: absolute;
            top: 15px;
            left: 15px
        }
        .media_blockquote .fa-quote-right {
            top: 15px;
            position: absolute;
            right: 15px
        }
        .media_blockquote p {
            text-align: center;
            width: 85%;
            margin: 10px auto;
            font-size: 24px;
            font-weight: bold;
            font-style: italic
        }
        .media_blockquote small {
            text-align: center;
            font-style: italic
        }

        .media-post-comments-ctr h3 {
            font-weight: bold;
        }
        .media-post-comments-ctr form {
            margin: 10px 0
        }
        .media-post-comments-ctr form .btn {
            padding: 6px 25px
        }
        .media-post-comments-ctr .media h4 {
            font-weight: bold
        }
        .media-post-comments-ctr .media h4 span {
            font-weight: 300;
            font-size: 12px;
            margin-left: 5px
        }
        .media-post-comments-ctr .media img {
            width: 64px;
            height: 64px
        }

        .media-content-share {
            position: fixed;
            top: 250px;
            left: 0;
            width: 60px
        }
        .media-content-share .btn {
            font-size: 20px;
            border-radius: 0
        }
        .media-content-share-mobile {
            position: fixed;
            bottom: 0;
            left: 0;
            width: 100%;
            height: 50px;
            background-color: rgba(255, 255, 255, 0.75);
            padding: 6px;
            z-index: 9999
        }
        .media-content-share-mobile .share-text {
            position: absolute;
            top: 16px;
            left: 10px;
            font-weight: bold
        }
        .media-content-share-mobile .btn {
            border-radius: 50%;
            float: right;
            width: 40px;
            height: 40px;
            margin-left: 8px
        }

        .media-neighbors-ctr {
            display: table;
            border-top: 1px solid #dcdcdc;
            border-bottom: #dcdcdc 1px solid;
            width: 100%
        }
        .media-neighbors-ctr .media-neighbors-items-ctr {
            padding: 10px 0;
            display: table-row
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link {
            padding: 20px 30px;
            text-align: center;
            display: table-cell;
            vertical-align: middle;
            width: 49.9%;
            text-decoration: none;
            position: relative
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link h5 {
            margin: 0;
            text-transform: uppercase;
            color: #111;
            margin-bottom: 6px;
            font-weight: bold
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link h3 {
            color: #777;
            margin: 0;
            font-size: 18px
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link.next-media-link {
            border-left: 1px solid #dcdcdc
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link.next-media-link .fa {
            right: 5px
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link.prev-media-link .fa {
            left: 5px
        }
        .media-neighbors-ctr .media-neighbors-items-ctr .neighbor-link .fa {
            position: absolute;
            top: 40%;
            font-size: 24px;
            color: #dcdcdc
        }

        /* EVENTS */
        .popular-posts-wrapper .post {
            clear: both;
            margin-bottom: 20px;
        }

        .popular-posts-wrapper .post .post-image {
            width: 60px;
            height: 60px;
            float: left;
            display: block;
            border-radius: 4px;
            -webkit-border-radius: 4px;
            background-color: #DADADA;
            background-position: center center;
            background-repeat: no-repeat;
            overflow: hidden;
                border: 1px solid #000;
            border-bottom-width: 2px;
        }
        .popular-posts-wrapper .post .post-image img {
            border-radius: 4px;
            opacity: 1;
            -webkit-transition: opacity 0.5s;
            -moz-transition: opacity 0.5s;
            -o-transition: opacity 0.5s;
            transition: opacity 0.5s;
        }

        .date {
            font-size: 12px;
        }

        .etitle {
            font-size: 14px;
        }

        .popular-posts-wrapper .post .post-content {
            margin-left: 70px;
        }
    </style>
</head>
<body>

    <!-- HEADER -->
    <nav class="navbar EF_Header justify-content-between">
        <div class="container">
            <div class="row">
                <div class="col-9">
                    <a href="#" onclick="return HomePage();"><div class="EF_Logo"></div></a>
                </div>
                <div class="col-3">
                    <div class="EF_Time vertical-center">
                        <table>
                            <tr>
                                <td style="border-right: 1px solid white;padding-right: 10px;"><?php echo $UData['username'] ?></td>
                                <td rowspan="2" style="padding-left: 20px;"><i class="fa fa-sun-o icon"></i></td>
                            </tr>
                            <tr>
                                <td style="padding-right: 10px;"><?php echo strftime("%a"); ?></td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-10">
                    <div class="EF_SearchBar">
                        <i class="fa fa-search icon"></i> 
                        <input class="input-field" type="text" placeholder="Buscar en Eyefind">
                    </div>
                </div>
                <div class="col-2" style="padding: 0 !important;">
                    <button class="EF_Btn_Random">ALEATORIO</button>
                </div>
            </div>
        </div>
    </nav>
    <!-- END HEADER -->

    <!-- NAV -->
    <nav class="navbar EF_Nav navbar-expand-lg">
        <div class="container">
            <ul class="navbar-nav mr-auto mt-lg-0 border-right">
                <li class="nav-item nav-link waves-effect waves-light">
                    <table style="border-collapse: inherit !important;">
                        <tr>
                            <td><i class="fa fa-tablet icon" style="font-size: 39px;"></i></td>
                            <td class="text-center">MEDIOS Y ENTRETENIMIENTO</td>
                        </tr>
                    </table>
                </li>
            </ul>
            <ul class="navbar-nav mr-auto mt-lg-0 border-right">
                <li class="nav-item nav-link waves-effect waves-light">
                    <table style="border-collapse: inherit !important;">
                        <tr>
                            <td><i class="fa fa-cutlery icon"></i></i></td>
                            <td class="text-center">COMIDA Y BEBIDA</td>
                        </tr>
                    </table>
                </li>
            </ul>
            <ul class="navbar-nav mr-auto mt-lg-0 border-right">
                <li class="nav-item nav-link waves-effect waves-light">
                    <table style="border-collapse: inherit !important;">
                        <tr>
                            <td><i class="fa fa-money icon" style="font-size: 36px;"></i></td>
                            <td class="text-center">DINERO Y SERVICIOS</td>
                        </tr>
                    </table>
                </li>
            </ul>
            <ul class="navbar-nav mr-auto mt-lg-0 border-right">
                <li class="nav-item nav-link waves-effect waves-light">
                    <table style="border-collapse: inherit !important;">
                        <tr>
                            <td><i class="fa fa-plane icon" style="font-size: 39px;"></i></td>
                            <td class="text-center">VIAJES Y TRANSPORTE</td>
                        </tr>
                    </table>
                </li>
            </ul>
            <ul class="navbar-nav mr-auto mt-lg-0">
                <li class="nav-item nav-link waves-effect waves-light">
                    <table style="border-collapse: inherit !important;">
                        <tr>
                            <td><i class="fa fa-shopping-bag icon"></i></td>
                            <td class="text-center">MODA Y SALUD</td>
                        </tr>
                    </table>
                </li>
            </ul>
        </div>
    </nav>
    <!-- END NAV -->

    <div class="EF_Body">
        <div class="container">
            <div id="HomePage" class="row" style="display: flex;">
                
                <div class="col-8" style="padding-left:0;">
                    <div class="ef-content-box">
                        <div class="EF_title_box">NOTICIA DEL D&Iacute;A</div>
                        <br>
                        <?php
                            $SQL2 = $DB->Query("SELECT * FROM rdp_news WHERE type = 'news' ORDER BY id DESC LIMIT 1;");
                            while($mNew = mysqli_fetch_assoc($SQL2)): ?>
                        <label style="font-weight: bold"><?php echo $mNew['title']; ?></label>
                        <br>
                        <br>
                        <p style="font-size: 14px;">
                            <?php echo $mNew['description']; ?>
                        </p>
                        <br>
                        <div class="EF_footer_box"><a href="./EyeFind.php?read=<?php echo $mNew['id']; ?>">LEER EL ART&Iacute;CULO COMPLETO >></a></div>
                        <?php endwhile; ?>
                    </div>

                    <div class="ef-content-box">
                        <div class="EF_title_box">NOTICIAS</div>
                        <hr>
                        
                        <?php
                            $SQL2 = $DB->Query("SELECT * FROM rdp_news WHERE type = 'news' AND id != (SELECT id FROM rdp_news WHERE type = 'news' ORDER BY id DESC LIMIT 1) ORDER BY id DESC LIMIT 10;");
                            while($mNew = mysqli_fetch_assoc($SQL2)): ?>
                        <div class="media-item-ctr-1">
                            <div class="col-xs-5 col-sm-3">
                                <a class="media-item-thumbnail" href="./EyeFind.php?read=<?php echo $mNew['id']; ?>">
                                    <img class="img-responsive" src="<?php echo $mNew['image']; ?>">
                                </a>
                            </div>
                            <div class="col-xs-7 col-sm-9 media-item-overview">
                                <h3><a href="./EyeFind.php?read=<?php echo $mNew['id']; ?>"><?php echo $mNew['title']; ?></a></h3>
                                <p><div class="media-item-cat" style="color: #8eb2c5"><?php echo $mNew['description']; ?></div></p>
                                <div class="media-meta-data">
                                    <a href="#"><?php echo $UserMG->GetUserDataByID($mNew['author'])["username"]; ?></a> <?php echo  date('d/m/Y \a \l\a\s H:i', $mNew['timestamp']); ?>
                                </div>
                            </div>
                        </div>

                        <?php endwhile; ?>

                    </div>
                </div>

                <div class="col-4" style="padding-left:0;padding-right: 0;">
                    <div class="ef-content-box ef-ad-blue">
                        <label>ANUNCIO PATROCINADO</label>
                        <div style="height: 96px;"></div>
                    </div>

                    <div class="ef-content-box">
                        <div class="EF_title_box">EVENTOS</div>
                        <br>
                        
                        <div class="popular-posts-wrapper">
                            <?php
                            $SQL3 = $DB->Query("SELECT * FROM rdp_news WHERE type = 'events' ORDER BY id DESC LIMIT 5;");
                            while($mEvt = mysqli_fetch_assoc($SQL3)): ?>
                            <div class="post">
                                <a href="#">
                                    <div class="post-image lazyl" data-img="<?php echo $mEvt['image']; ?>" style="background-image: url(&quot;<?php echo $mEvt['image']; ?>&quot;);">
                                    </div>
                                </a>
                                <div class="post-content">
                                    <div class="etitle"><?php echo $mEvt['title']; ?></div>
                                    <p>
                                    <i><?php echo $mEvt['content']; ?></i></p>
                                    <span class="date"><?php echo date("d/m/Y", $mEvt["timestamp"]); ?></span>
                                </div>
                            </div>
                            <?php endwhile; ?>
                        </div>

                    </div>
                </div>

            </div>

            <div id="ReadNew" class="row" style="display: none;">
                <div class="col-12" style="padding-left:0;">

                    <div class="ef-content-box">
                        <div class="EF_title_box">
                            NOTICIA DEL D&Iacute;A
                            <div style="float:right"><small><a href="#" onclick="return HomePage();"><< Volver</a></small></div>
                        </div>
                        <small id="dateandauthor">00/00/00 - Jeihden</small>
                        <hr>
                        <center><h2 id="title">asd</h2></center>
                        <br>
                        <small id="desc"></small>
                        <br>
                        <p id="contentNew" style="font-size: 14px;">
                            <?php echo $CTN; ?>
                        </p>
                        <br>
                        
                    </div>

                </div>
            </div>
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

    <script>
        function HomePage() {
            $("#ReadNew").hide();
            $("#HomePage").css("display", "flex");
        }

        function ReadNew($date, $title, $desc) {
            $("#HomePage").hide();
            $("#ReadNew").css("display", "flex");

            $("#dateandauthor").html($date);
            $("#title").html($title);
            $("#desc").html($desc);
        }
    </script>
</body>
</html>

<?php
$CurNew = isset($_GET["read"]) ? AppFunctions::InjectionCleaner($_GET["read"]) : null;

$getNew = $DB->Query("SELECT * FROM rdp_news WHERE id = '".$CurNew."'");
$CurNew = mysqli_fetch_assoc($getNew);

if ($CurNew != null) {
    echo "<script>ReadNew('".$UserMG->GetUserDataByID($CurNew["author"])["username"] . " - " . date("d/m/Y \a \l\a\s H:i", $CurNew["timestamp"])."', '".$CurNew["title"]."', '".$CurNew["description"]."');</script>";
}
?>