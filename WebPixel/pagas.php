<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once "app/init.pz.php";
?>
<style>
	table {
	  font-family: arial, sans-serif;
	  border-collapse: collapse;
	  width: 100%;
	}

	td, th {
	  border: 1px solid #dddddd;
	  text-align: left;
	  padding: 8px;
	}

	td{
		color:white;
	}

	tr:nth-child(even) {
	  background-color: #dddddd;
	}
</style>

<table style="width: 30%">
  <tr>
    <th>Nombre</th>
    <th>Horas de actividad</th>
  </tr>
  <?php
	$SQL = $DB->Query("SELECT * FROM users WHERE rank > 3");
	while($Row = mysqli_fetch_assoc($SQL)){
		$SQL2 = $DB->Query("SELECT `user_id`, SUM(`real_total_time`) AS TOTAL  FROM `staff_paysheet` WHERE `user_id` = '" . $Row['id'] . "'");
		while($Row2 = mysqli_fetch_assoc($SQL2)){

			$User_ = $UserMG->GetUserDataByID($Row2["user_id"]);
			$TOTAL = ($Row2["TOTAL"] / 3600);


			echo ($TOTAL >= 5) ? "<tr style='background-color:green'>" : "<tr style='background-color:red'>";
			echo "<td>" . $User_["username"] . "</td>";
			echo "<td>" . number_format($TOTAL, 2, '.', ' ') . " horas</td>";
			echo "</tr>";
		}
	}
	?>
</table>
