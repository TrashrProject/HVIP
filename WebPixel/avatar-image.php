<?php
// Local placeholder endpoint for legacy RP phone avatar URLs.
// The modern phone UI reads the figure query parameter from this URL and
// replaces the placeholder with its own rendered avatar. Returning a valid
// local image prevents dead external imager hosts from spamming the console.
header('Content-Type: image/png');
header('Cache-Control: public, max-age=86400');
header('X-Content-Type-Options: nosniff');

// 1x1 transparent PNG.
echo base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=');
