<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

// Initialize MySQL Manager
require_once "Model/DBConManager.php";
require_once "Controller/FunctionsManager.php";
$DB = new DBManager();
require_once "Model/MusManager.php";
// Initialize MusSocket
$M = new MusManager();
require_once "Controller/TokenManager.php";
$TK = new TokenManager();

?>