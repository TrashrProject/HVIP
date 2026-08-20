<?php
/**
 * ParadiseRP asset resolver.
 *
 * Used only when a requested Nitro/SWF asset is missing at its exact URL.
 * It serves a real matching file from the local asset packs, including case,
 * space/underscore and legacy folder differences. It never creates fake images.
 */

function pr_fail(int $code, string $message): void {
    http_response_code($code);
    header('Content-Type: text/plain; charset=utf-8');
    echo $message;
    exit;
}

$webRoot = __DIR__;
$projectRoot = dirname(__DIR__);
$roots = [];
foreach ([
    $webRoot . DIRECTORY_SEPARATOR . 'swf_pz',
    $projectRoot . DIRECTORY_SEPARATOR . 'swf_pz',
    $webRoot . DIRECTORY_SEPARATOR . 'SWF',
    $projectRoot . DIRECTORY_SEPARATOR . 'SWF',
] as $candidateRoot) {
    $real = realpath($candidateRoot);
    if ($real !== false && is_dir($real)) $roots[] = $real;
}
$roots = array_values(array_unique($roots));
if (!$roots) pr_fail(404, 'asset roots not found');

$requested = isset($_GET['u']) ? (string) $_GET['u'] : '';
$requestedPath = parse_url($requested, PHP_URL_PATH);
if (!is_string($requestedPath) || $requestedPath === '') $requestedPath = $requested;
$requestedPath = rawurldecode(str_replace('\\', '/', $requestedPath));
$filename = basename($requestedPath);

if ($filename === '' || $filename === '.' || $filename === '..') pr_fail(400, 'invalid asset');

$extension = strtolower(pathinfo($filename, PATHINFO_EXTENSION));
$allowed = ['png','gif','jpg','jpeg','webp','svg','ico','nitro','json','mp3','ogg','wav','ttf','otf','woff','woff2','eot'];
if (!in_array($extension, $allowed, true)) pr_fail(404, 'unsupported asset');

function pr_key(string $name): string {
    return strtolower($name);
}

function pr_normalized_key(string $name): string {
    $name = strtolower(rawurldecode($name));
    $name = preg_replace('/[\s\-]+/', '_', $name);
    $name = preg_replace('/_+/', '_', $name);
    return $name ?: '';
}

$mtime = 0;
foreach ($roots as $root) $mtime = max($mtime, (int) @filemtime($root));
$cacheFile = rtrim(sys_get_temp_dir(), DIRECTORY_SEPARATOR) . DIRECTORY_SEPARATOR . 'paradiserp_asset_index_v4.json';
$index = null;

if (is_file($cacheFile) && (time() - (int) @filemtime($cacheFile) < 86400)) {
    $decoded = json_decode((string) @file_get_contents($cacheFile), true);
    if (is_array($decoded) && (int) ($decoded['_mtime'] ?? 0) === $mtime && is_array($decoded['files'] ?? null)) {
        $index = $decoded['files'];
    }
}

if (!is_array($index)) {
    $index = [];
    foreach ($roots as $rootIndex => $root) {
        $iterator = new RecursiveIteratorIterator(new RecursiveDirectoryIterator($root, FilesystemIterator::SKIP_DOTS));
        foreach ($iterator as $fileInfo) {
            if (!$fileInfo->isFile()) continue;
            $ext = strtolower($fileInfo->getExtension());
            if (!in_array($ext, $allowed, true)) continue;

            $absolute = $fileInfo->getPathname();
            $relative = ltrim(str_replace('\\', '/', substr($absolute, strlen($root))), '/');
            $entry = ['root' => $rootIndex, 'path' => $relative];

            foreach (array_unique([
                pr_key($fileInfo->getFilename()),
                pr_normalized_key($fileInfo->getFilename())
            ]) as $key) {
                if ($key === '') continue;
                if (!isset($index[$key])) $index[$key] = [];
                $index[$key][] = $entry;
            }
        }
    }
    @file_put_contents($cacheFile, json_encode(['_mtime' => $mtime, 'files' => $index], JSON_UNESCAPED_SLASHES));
}

$lookupKeys = array_values(array_unique([
    pr_key($filename),
    pr_normalized_key($filename),
    pr_key(str_replace(' ', '_', $filename)),
    pr_key(str_replace('_', ' ', $filename))
]));

$candidates = [];
foreach ($lookupKeys as $key) {
    if (isset($index[$key]) && is_array($index[$key])) {
        foreach ($index[$key] as $entry) $candidates[] = $entry;
    }
}

if (!$candidates) pr_fail(404, 'asset not found');

$wantedSegments = array_values(array_filter(explode('/', strtolower($requestedPath)), 'strlen'));
$best = null;
$bestScore = -1;
foreach ($candidates as $entry) {
    $candidate = strtolower(str_replace('\\', '/', (string) ($entry['path'] ?? '')));
    $candidateSegments = array_values(array_filter(explode('/', $candidate), 'strlen'));
    $score = 0;

    foreach ($wantedSegments as $segment) {
        if ($segment === strtolower($filename)) continue;
        if (in_array($segment, $candidateSegments, true)) $score += 2;
    }

    $wantedTail = implode('/', array_slice($wantedSegments, -3));
    if ($wantedTail !== '' && substr($candidate, -strlen($wantedTail)) === $wantedTail) $score += 30;

    // Prefer exact case-insensitive filename before normalized matches.
    if (basename($candidate) === strtolower($filename)) $score += 10;

    if ($score > $bestScore) {
        $bestScore = $score;
        $best = $entry;
    }
}

if ($best === null || !isset($roots[(int) $best['root']])) pr_fail(404, 'asset not found');
$root = $roots[(int) $best['root']];
$file = realpath($root . DIRECTORY_SEPARATOR . str_replace('/', DIRECTORY_SEPARATOR, (string) $best['path']));
if ($file === false || strpos(strtolower($file), strtolower($root)) !== 0 || !is_file($file)) pr_fail(404, 'asset not found');

$mimeMap = [
    'png' => 'image/png', 'gif' => 'image/gif', 'jpg' => 'image/jpeg', 'jpeg' => 'image/jpeg',
    'webp' => 'image/webp', 'svg' => 'image/svg+xml', 'ico' => 'image/x-icon',
    'json' => 'application/json; charset=utf-8', 'mp3' => 'audio/mpeg', 'ogg' => 'audio/ogg', 'wav' => 'audio/wav',
    'ttf' => 'font/ttf', 'otf' => 'font/otf', 'woff' => 'font/woff', 'woff2' => 'font/woff2', 'eot' => 'application/vnd.ms-fontobject',
    'nitro' => 'application/octet-stream'
];

header('Content-Type: ' . ($mimeMap[$extension] ?? 'application/octet-stream'));
header('Content-Length: ' . filesize($file));
header('Cache-Control: public, max-age=86400');
header('X-Paradise-Asset-Fallback: ' . str_replace(["\r", "\n"], '', (string) $best['path']));
readfile($file);
