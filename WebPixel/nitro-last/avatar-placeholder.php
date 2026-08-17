<?php
header('Content-Type: image/png');
header('Cache-Control: public, max-age=3600');
$png = base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg==');
header('Content-Length: ' . strlen($png));
echo $png;
