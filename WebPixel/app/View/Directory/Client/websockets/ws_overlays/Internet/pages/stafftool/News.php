<?php
$reporterAccess = true;
include_once "./Header.php";

$errorMsg = "";
function Alert($msg){
    echo "<script>alert('$msg');</script>";
}

if (isset($_POST["new"])) {
    $title = AppFunctions::InjectionCleaner($_POST["title"]);
    $type = AppFunctions::InjectionCleaner($_POST["type"]);
    $image = AppFunctions::InjectionCleaner($_POST["image"]);
    $desc = AppFunctions::InjectionCleaner($_POST["desc"]);
    $content = AppFunctions::InjectionCleaner($_POST["content"]);

    if (empty($title) || empty($type) || empty($desc) || empty($content)) {
        $errorMsg = "El único campo que puede quedar vacío es el de la imagen.";
    }
    else {
        $SQL = $DB->Query("INSERT rdp_news VALUES(NULL, '".$title."', '".$type."', '".$image."', '".$desc."', '".$content."', '".$UData['id']."', '".time()."');");

        if ($SQL) {
            Alert("Noticia/Evento cread@ exitosamente.");
        }
        else {
            $errorMsg = "Ocurrió un error al intentar crear la noticia/evento.";
        }
    }
}

if (isset($_POST["edit"])) {
    $title = AppFunctions::InjectionCleaner($_POST["title"]);
    $type = AppFunctions::InjectionCleaner($_POST["type"]);
    $image = AppFunctions::InjectionCleaner($_POST["image"]);
    $desc = AppFunctions::InjectionCleaner($_POST["desc"]);
    $content = AppFunctions::InjectionCleaner($_POST["content"]);
    $curId = AppFunctions::InjectionCleaner($_POST["curId"]);

    if (empty($title) || empty($type) || empty($desc) || empty($content)) {
        $errorMsg = "El único campo que puede quedar vacío es el de la imagen.";
    }
    else {
        $SQL = $DB->Query("UPDATE rdp_news SET title = '".$title."', type = '".$type."', image = '".$image."', description = '".$desc."', content = '".$content."' WHERE id = '".$curId."';");

        if ($SQL) {
            Alert("Noticia/Evento editad@ exitosamente.");
        }
        else {
            $errorMsg = "Ocurrió un error al intentar editar la noticia/evento.";
        }
    }
}

if (isset($_POST["del"])) {
    $curId = AppFunctions::InjectionCleaner($_POST["curId"]);

    if (empty($curId)) {
        $errorMsg = "No se encontró la noticia o evento a eliminar";
    }
    else {
        $SQL = $DB->Query("DELETE FROM rdp_news WHERE id = '".$curId."' LIMIT 1");

        if ($SQL) {
            Alert("Noticia/Evento eliminad@ exitosamente.");
        }
        else {
            $errorMsg = "Ocurrió un error al intentar eliminar la noticia/evento.";
        }
    }
}

$CurNew = isset($_GET["id"]) ? AppFunctions::InjectionCleaner($_GET["id"]) : null;

$getNew = $DB->Query("SELECT * FROM rdp_news WHERE id = '".$CurNew."'");
$CurNew = mysqli_fetch_assoc($getNew);

?>

<div class="main_content_iner ">
    <div class="container-fluid p-0">
        <div class="row justify-content-center">
            
            <div class="col-lg-12">
                <?php if ($CurNew == null): ?>
                <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Crear noticia/Evento</h3>
                            <b style="color:red;"><?php echo $errorMsg; ?></b>
                        </div>
                    </div>
                    <form method="POST" action="">
                        <div class="form-group">
                            <label for="titleInput">Título</label>
                            <input type="text" name="title" class="form-control" id="titleInput" placeholder="Título para la noticia" value="<?php echo (isset($_POST["title"])) ? $_POST["title"] : ""; ?>" autocomplete="off">
                        </div>
                        <div class="form-group">
                            <label for="typeSelect">Tipo</label>
                            <select name="type" class="form-control" id="typeSelect">
                                <option value="news">Noticia</option>
                                <option value="events">Evento</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <label for="imgInput">Imagen</label>
                            <input name="image" type="text" class="form-control" id="imgInput" placeholder="Url de la imagen" value="<?php echo (isset($_POST["image"])) ? $_POST["image"] : ""; ?>">
                        </div>
                        <div class="form-group">
                            <label for="descInput">Descripción</label>
                            <input name="desc" type="text" class="form-control" id="descInput" placeholder="Descripción para la noticia" value="<?php echo (isset($_POST["desc"])) ? $_POST["desc"] : ""; ?>" autocomplete="off">
                        </div>

                        <div class="form-group">
                            <label for="contentInput">Contenido</label>
                            <textarea name="content" class="form-control" id="contentInput" placeholder="Contenido para la noticia/Evento" rows="5">
                                <?php echo (isset($_POST["content"])) ? $_POST["content"] : ""; ?>
                            </textarea>
                        </div>

                        <div class="form-group">
                            <button name="new" type="submit" class="btn btn-primary">Crear</button>
                        </div>
                    </form>
                </div>
                <?php else: ?>
                    <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Editar noticia/Evento (ID: <?php echo $CurNew["id"]; ?>)</h3>
                            <b style="color:red;"><?php echo $errorMsg; ?></b>
                        </div>
                    </div>
                    <form method="POST" action="">
                        <div class="form-group">
                            <label for="titleInput">Título</label>
                            <input type="text" name="title" class="form-control" id="titleInput" placeholder="Título para la noticia" value="<?php echo $CurNew["title"]; ?>" autocomplete="off">
                        </div>
                        <div class="form-group">
                            <label for="typeSelect">Tipo</label>
                            <select name="type" class="form-control" id="typeSelect">
                                <option value="news" <?php if($CurNew["type"] == "news") echo "selected"; ?>>Noticia</option>
                                <option value="events" <?php if($CurNew["type"] == "events") echo "selected"; ?>>Evento</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <label for="imgInput">Imagen</label>
                            <input name="image" type="text" class="form-control" id="imgInput" placeholder="Url de la imagen" value="<?php echo $CurNew["image"]; ?>">
                        </div>
                        <div class="form-group">
                            <label for="descInput">Descripción</label>
                            <input name="desc" type="text" class="form-control" id="descInput" placeholder="Descripción para la noticia" value="<?php echo $CurNew["description"]; ?>" autocomplete="off">
                        </div>

                        <div class="form-group">
                            <label for="contentInput">Contenido</label>
                            <textarea name="content" class="form-control" id="contentInput" placeholder="Contenido para la noticia/Evento" rows="5"><?php echo $CurNew["content"]; ?></textarea>
                        </div>
                        <input type="hidden" name="curId" value="<?php echo $CurNew["id"]; ?>"/>
                        <div class="form-group">
                            <button name="edit" type="submit" class="btn btn-primary">Editar</button>
                        </div>
                    </form>
                </div>
                <?php endif; ?>
            </div>

            <div class="col-lg-6">
                <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Lista de noticias</h3>
                        </div>
                    </div>
                    
                    <table class="table lms_table_active">
                        <thead>
                            <tr>
                                <th scope="col">ID</th>
                                <th scope="col">Título</th>
                                <th scope="col">Descripción</th>
                                <th scope="col">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php
                            $SQL2 = $DB->Query("SELECT * FROM rdp_news WHERE type = 'news' ORDER BY id DESC;");
                            while($mNew = mysqli_fetch_assoc($SQL2)): ?>
                            <tr>
                                <td><?php echo $mNew["id"]; ?></td>
                                <td><?php echo $mNew["title"]; ?></td>
                                <td><?php echo $mNew["description"]; ?></td>
                                <td>
                                    <form method="POST" action="" class="text-center">
                                        <a href="./News.php?id=<?php echo $mNew['id']; ?>"><button type="button" class="btn btn-outline-info" style="padding: .0rem 0.3rem !important;"><small><i class="fas fa-edit"></i></small></button></a>
                                        <input type="hidden" name="curId" value="<?php echo $mNew['id']; ?>" />
                                        <button name="del" type="submit" class="btn btn-outline-danger" style="padding: .0rem 0.35rem !important;"><small><i class="fas fa-trash-alt"></i></small></button>
                                    </form>
                                </td>
                            </tr>
                            <?php endwhile; ?>
                        </tbody>
                    </table>
                    
                </div>
            </div>

            <div class="col-lg-6">
                <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Lista de eventos</h3>
                        </div>
                    </div>

                    <table class="table lms_table_active">
                        <thead>
                            <tr>
                                <th scope="col">ID</th>
                                <th scope="col">Título</th>
                                <th scope="col">Descripción</th>
                                <th scope="col">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php
                            $SQL2 = $DB->Query("SELECT * FROM rdp_news WHERE type = 'events' ORDER BY id DESC;");
                            while($mNew = mysqli_fetch_assoc($SQL2)): ?>
                            <tr>
                                <td><?php echo $mNew["id"]; ?></td>
                                <td><?php echo $mNew["title"]; ?></td>
                                <td><?php echo $mNew["description"]; ?></td>
                                <td>
                                    <form method="POST" action="" class="text-center">
                                        <a href="./News.php?id=<?php echo $mNew['id']; ?>"><button type="button" class="btn btn-outline-info" style="padding: .0rem 0.3rem !important;"><small><i class="fas fa-edit"></i></small></button></a>
                                        <input type="hidden" name="curId" value="<?php echo $mNew['id']; ?>" />
                                        <button name="del" type="submit" class="btn btn-outline-danger" style="padding: .0rem 0.3rem !important;"><small><i class="fas fa-trash-alt"></i></small></button>
                                    </form>
                                </td>
                            </tr>
                            <?php endwhile; ?>
                        </tbody>
                    </table>
                    
                </div>
            </div>
            
    </div>
</div>

<script>
tinymce.init({
  selector: 'textarea',
  plugins: 'advlist autolink lists link image charmap preview anchor pagebreak',
  toolbar_mode: 'floating',
});
</script>


<?php
include_once "./Footer.php";
?>