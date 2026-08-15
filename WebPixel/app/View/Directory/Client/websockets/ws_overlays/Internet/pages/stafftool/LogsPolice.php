<?php
include_once "./Header.php";

$getLogs = $DB->Query("SELECT * FROM command_logs_police ORDER BY id DESC LIMIT 250");

?>

<div class="main_content_iner ">
    <div class="container-fluid p-0">
        <div class="row justify-content-center">
            
            <div class="col-lg-12">
                <div class="white_box mb_30">
                    <div class="box_header ">
                        <div class="main-title">
                            <h3 class="mb-0">Logs de comandos Staff</h3>
                        </div>
                    </div>

                    <table class="table lms_table_active myTable">
                        <thead>
                            <tr>
                                <th scope="col">ID</th>
                                <th scope="col">Usuario</th>
                                <th scope="col">Comando</th>
                                <th scope="col">Fecha</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php while($resultLogs = mysqli_fetch_assoc($getLogs)): ?>
                            <tr>
                                <td><?php echo $resultLogs["id"]; ?></td>
                                <td><?php echo $UserMG->GetUserDataByID($resultLogs["user_id"])["username"]; ?></td>
                                <td><?php echo $resultLogs["data_string"]; ?></td>
                                <td><?php echo date("d/m/Y \a \l\a\s H:m", $resultLogs['timestamp']); ?></td>
                            </tr>
                            <?php endwhile; ?>
                        </tbody>
                    </table>
                </div>
            </div>

    </div>
</div>



<?php
include_once "./Footer.php";
?>