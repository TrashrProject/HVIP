<?php
/**
 * Localhost asset fallback for the legacy Habbo/RDP pack.
 *
 * Nitro and the old UI reference thousands of historical image paths. The
 * files are present in swf_pz, but some legacy configs point to a different
 * subdirectory. This endpoint resolves a missing asset by filename and serves
 * the best matching local copy.
 */

$htdocs = dirname(__DIR__);
$root = realpath($htdocs . DIRECTORY_SEPARATOR . 'swf_pz');

if ($root === false || !is_dir($root)) {
    http_response_code(404);
    exit('swf_pz not found');
}

$requested = isset($_GET['u']) ? (string) $_GET['u'] : '';
$requestedPath = parse_url($requested, PHP_URL_PATH);
if (!is_string($requestedPath) || $requestedPath === '') {
    $requestedPath = $requested;
}

$requestedPath = str_replace('\\', '/', $requestedPath);
$filename = basename($requestedPath);

if ($filename === '' || $filename === '.' || $filename === '..') {
    http_response_code(400);
    exit('invalid asset');
}

$extension = strtolower(pathinfo($filename, PATHINFO_EXTENSION));
$allowed = array('png','gif','jpg','jpeg','webp','svg','ico','nitro','json','mp3','ogg','wav');
if (!in_array($extension, $allowed, true)) {
    http_response_code(404);
    exit('unsupported asset');
}

// Keep the index outside the web root. Rebuild it when swf_pz changes or when
// the cache is older than a day. The index contains only relative paths.
$cacheFile = rtrim(sys_get_temp_dir(), DIRECTORY_SEPARATOR) . DIRECTORY_SEPARATOR . 'hvip_swf_asset_index_v2.json';
$rootMtime = @filemtime($root);
$index = null;

if (is_file($cacheFile) && (time() - @filemtime($cacheFile) < 86400)) {
    $decoded = json_decode(@file_get_contents($cacheFile), true);
    if (is_array($decoded) && isset($decoded['_root_mtime'], $decoded['files']) && (int)$decoded['_root_mtime'] === (int)$rootMtime) {
        $index = $decoded['files'];
    }
}

if (!is_array($index)) {
    $index = array();
    $iterator = new RecursiveIteratorIterator(
        new RecursiveDirectoryIterator($root, FilesystemIterator::SKIP_DOTS)
    );

    foreach ($iterator as $fileInfo) {
        if (!$fileInfo->isFile()) continue;
        $name = strtolower($fileInfo->getFilename());
        $ext = strtolower($fileInfo->getExtension());
        if (!in_array($ext, $allowed, true)) continue;

        $absolute = $fileInfo->getPathname();
        $relative = ltrim(str_replace('\\', '/', substr($absolute, strlen($root))), '/');
        if (!isset($index[$name])) $index[$name] = array();
        $index[$name][] = $relative;
    }

    @file_put_contents($cacheFile, json_encode(array(
        '_root_mtime' => (int)$rootMtime,
        'files' => $index
    ), JSON_UNESCAPED_SLASHES));
}

$key = strtolower($filename);
if (!isset($index[$key]) || !is_array($index[$key]) || count($index[$key]) === 0) {
    http_response_code(404);
    exit('asset not found');
}

// Score duplicate filenames by how many path segments match the original URL.
$wantedSegments = array_values(array_filter(explode('/', strtolower($requestedPath)), 'strlen'));
$best = null;
$bestScore = -1;
foreach ($index[$key] as $candidate) {
    $candidateLower = strtolower(str_replace('\\', '/', $candidate));
    $candidateSegments = array_values(array_filter(explode('/', $candidateLower), 'strlen'));
    $score = 0;

    foreach ($wantedSegments as $segment) {
        if ($segment === strtolower($filename)) continue;
        if (in_array($segment, $candidateSegments, true)) $score += 2;
    }

    $wantedTail = implode('/', array_slice($wantedSegments, -3));
    if ($wantedTail !== '' && substr($candidateLower, -strlen($wantedTail)) === $wantedTail) $score += 20;

    if ($score > $bestScore) {
        $bestScore = $score;
        $best = $candidate;
    }
}

if ($best === null) {
    http_response_code(404);
    exit('asset not found');
}

$file = realpath($root . DIRECTORY_SEPARATOR . str_replace('/', DIRECTORY_SEPARATOR, $best));
if ($file === false || strpos(strtolower($file), strtolower($root)) !== 0 || !is_file($file)) {
    http_response_code(404);
    exit('asset not found');
}

$mimeMap = array(
    'png' => 'image/png', 'gif' => 'image/gif', 'jpg' => 'image/jpeg', 'jpeg' => 'image/jpeg',
    'webp' => 'image/webp', 'svg' => 'image/svg+xml', 'ico' => 'image/x-icon',
    'json' => 'application/json', 'mp3' => 'audio/mpeg', 'ogg' => 'audio/ogg', 'wav' => 'audio/wav',
    'nitro' => 'application/octet-stream'
);

header('Content-Type: ' . (isset($mimeMap[$extension]) ? $mimeMap[$extension] : 'application/octet-stream'));
header('Content-Length: ' . filesize($file));
header('Cache-Control: public, max-age=86400');
header('X-HVIP-Asset-Fallback: ' . str_replace(array("\r", "\n"), '', $best));
readfile($file);
