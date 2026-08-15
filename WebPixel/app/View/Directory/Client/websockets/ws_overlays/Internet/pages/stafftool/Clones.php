<?php
include_once "./Header.php";

$errorMsg = "";
function Alert($msg){
    echo "<script>alert('$msg');</script>";
}

$username = "";
$resultUser = null;
if (isset($_POST["search"])) {
    $username = AppFunctions::InjectionCleaner($_POST["user_clone"]);

    if (empty($username)) {
        $errorMsg = "Debes ingresar un nombre de usuario";
    }
    else {
        $getUser = $DB->Query("SELECT * FROM users WHERE username = '".$username."'");
        $resultUser = mysqli_fetch_assoc($getUser);

        if ($resultUser == null) {
            $errorMsg = "No se encontró ningún usuario con nombre " . $username;
        }
    }
}

?>

<div class="main_content_iner ">
    <div class="container-fluid p-0">
        <div class="row justify-content-center">
            
            <div class="col-lg-12">
                <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Buscador de Clones</h3>
                            <b style="color:red;"><?php echo $errorMsg; ?></b>
                        </div>
                    </div>
                    <form method="POST" action="">
                        <div class="form-group">
                            <label for="userCloneInput">Usuario</label>
                            <input type="text" name="user_clone" class="form-control" id="userCloneInput" placeholder="Nombre de usuario" value="<?php echo $username; ?>">
                        </div>
                        <div class="form-group">
                            <button name="search" type="submit" class="btn btn-primary">Buscar</button>
                        </div>
                    </form>

                    <!-- Results -->
                    <?php if ($resultUser != null): ?>
                        <label>Resultados: </label>
                        <table class="table lms_table_active">
                            <thead>
                                <tr>
                                    <th scope="col">ID</th>
                                    <th scope="col">Usuario</th>
                                    <th scope="col">Registro</th>
                                    <th scope="col">IP de Registro</th>
                                    <th scope="col">IP Actual</th>
                                    <th scope="col">Últ. conexión</th>
                                </tr>
                            </thead>
                            <tbody>
                                <?php
                                $SQL2 = $DB->Query("SELECT * FROM users WHERE ip_reg = '".$resultUser['ip_reg']."' OR ip_last = '".$resultUser['ip_reg']."' OR ip_reg = '".$resultUser['ip_last']."' OR ip_last = '".$resultUser['ip_last']."' ORDER BY id DESC;");
                                while($mNew = mysqli_fetch_assoc($SQL2)): ?>
                                <tr>
                                    <td><?php echo $mNew["id"]; ?></td>
                                    <td><?php echo $mNew["username"]; ?></td>
                                    <td><?php echo date("d/m/Y", $mNew['account_created']); ?></td>
                                    <td><?php echo $mNew["ip_reg"]; ?></td>
                                    <td><?php echo $mNew["ip_last"]; ?></td>
                                    <td><?php echo date("d/m/Y \a \l\a\s H:m", $mNew['last_online']); ?></td>
                                </tr>
                                <?php endwhile; ?>
                            </tbody>
                        </table>
                    <?php endif; ?>
                    <!-- End Results -->
                </div>
            </div>

    </div>
</div>



<?php
include_once "./Footer.php";
?>