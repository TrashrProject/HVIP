<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

require_once "app/api.init.php";

// Verify if Token was received
if(!isset($_GET['tkn'])):
    FunctionsManager::ThrowE(1000, "Token no recibido");
    exit;
endif;

// Storage Filtered Token in a Variable
$TKN = FunctionsManager::FilterVar($_GET['tkn']);

// Run Token
$TK->Run($TKN, $DB, $M);
?>