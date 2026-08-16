<?php
header('Content-Type: application/javascript; charset=UTF-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');

$source = @file_get_contents(__DIR__ . '/rdp.c.js');
if ($source === false) {
    http_response_code(500);
    echo "console.error('[RDP] Unable to load local module source');";
    exit;
}

$source = str_ireplace(
    array(
        'https://nitro-imager.kubbo.ch/?figure=',
        'http://nitro-imager.kubbo.ch/?figure='
    ),
    array(
        '/WebPixel/avatar-image.php?figure=',
        '/WebPixel/avatar-image.php?figure='
    ),
    $source
);

// Rank generator still contains the obsolete .city host. Do not let it hit DNS.
$source = preg_replace(
    '~https?://nitro-imager\.kubbo\.city/\?head_direction=4&direction=4&figure=~i',
    '/WebPixel/avatar-image.php?head_direction=4&direction=4&figure=',
    $source
);

echo $source;
