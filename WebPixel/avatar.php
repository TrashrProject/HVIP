<?php
/**
 * Same-origin avatar proxy for the CMS.
 * It uses the hotel's local figure library and starts its renderer silently
 * when needed. Habbo's public imager remains available as a fallback.
 */

$figure = isset($_GET['figure']) ? (string) $_GET['figure'] : '';
if ($figure === '' || !preg_match('/^[A-Za-z0-9._-]+$/', $figure)) {
    http_response_code(400);
    exit;
}

$allowed = array('size', 'direction', 'head_direction', 'gesture', 'action', 'headonly');
$query = array('figure' => $figure);
foreach ($allowed as $key) {
    if (!isset($_GET[$key])) {
        continue;
    }

    $value = (string) $_GET[$key];
    if (preg_match('/^[A-Za-z0-9_-]{1,24}$/', $value)) {
        $query[$key] = $value;
    }
}

$queryString = http_build_query($query, '', '&', PHP_QUERY_RFC3986);
$cacheDirectory = __DIR__ . DIRECTORY_SEPARATOR . 'cache' . DIRECTORY_SEPARATOR . 'avatars';
$cacheKey = hash('sha256', $queryString);
$cacheFile = $cacheDirectory . DIRECTORY_SEPARATOR . $cacheKey . '.png';

function fetchAvatarImage($url, $timeout)
{
    $context = stream_context_create(array('http' => array(
        'timeout' => $timeout,
        'ignore_errors' => true,
        'header' => "User-Agent: ParadiseRP CMS Avatar Service\r\n"
    )));
    $data = @file_get_contents($url, false, $context);
    return ($data !== false && strlen($data) > 32) ? $data : false;
}

$localTarget = 'http://127.0.0.1:5000/habbo-imaging/avatarimage?' . $queryString;
$image = fetchAvatarImage($localTarget, 1);

// Minerva contains the same figure data as the hotel. Start it silently on
// demand so nobody has to keep a command prompt open for CMS avatars.
if ($image === false && PHP_OS_FAMILY === 'Windows') {
    $minervaDirectory = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'tools' . DIRECTORY_SEPARATOR . 'minerva' . DIRECTORY_SEPARATOR . 'win-x64';
    $minervaExecutable = $minervaDirectory . DIRECTORY_SEPARATOR . 'Minerva.exe';
    if (is_file($minervaExecutable)) {
        $psExecutable = str_replace("'", "''", $minervaExecutable);
        $psDirectory = str_replace("'", "''", $minervaDirectory);
        $startScript = "Start-Process -FilePath '" . $psExecutable . "' -WorkingDirectory '" . $psDirectory . "' -WindowStyle Hidden";
        $command = 'powershell.exe -NoProfile -NonInteractive -WindowStyle Hidden -Command ' . escapeshellarg($startScript);
        $process = @popen($command, 'r');
        if (is_resource($process)) @pclose($process);

        for ($attempt = 0; $attempt < 15 && $image === false; $attempt++) {
            usleep(150000);
            $image = fetchAvatarImage($localTarget, 1);
        }
    }
}

// The official renderer is a safe fallback while Minerva starts or if its
// executable is temporarily unavailable.
if ($image === false) {
    $image = fetchAvatarImage('https://www.habbo.fr/habbo-imaging/avatarimage?' . $queryString, 10);
}

if ($image !== false) {
    if (!is_dir($cacheDirectory)) @mkdir($cacheDirectory, 0775, true);
    if (is_dir($cacheDirectory)) @file_put_contents($cacheFile, $image, LOCK_EX);
} elseif (is_file($cacheFile)) {
    $image = @file_get_contents($cacheFile);
}

if ($image === false || strlen($image) < 32) {
    http_response_code(503);
    exit;
}

header('Content-Type: image/png');
header('Cache-Control: no-cache, no-store, must-revalidate');
header('Pragma: no-cache');
header('Expires: 0');
echo $image;
