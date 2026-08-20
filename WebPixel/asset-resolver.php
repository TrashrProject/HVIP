<?php
/**
 * ParadiseRP asset resolver.
 *
 * Used only when a requested Nitro/SWF asset is missing at its exact URL.
 * It first tries to serve a real matching file from local asset packs. For
 * optional camera overlays / old promo images that are not present in the pack,
 * it returns a transparent image instead of leaving the client with hard 404s.
 */

function pr_plain(int $code, string $message): void {
    http_response_code($code);
    header('Content-Type: text/plain; charset=utf-8');
    header('Cache-Control: no-store');
    echo $message;
    exit;
}

function pr_transparent_png(string $reason): void {
    $png = base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg==');
    header('Content-Type: image/png');
    header('Content-Length: ' . strlen($png));
    header('Cache-Control: public, max-age=86400');
    header('X-Paradise-Optional-Asset-Fallback: ' . str_replace(["\r", "\n"], '', $reason));
    echo $png;
    exit;
}

function pr_empty_font(string $reason): void {
    http_response_code(204);
    header('Cache-Control: public, max-age=86400');
    header('X-Paradise-Optional-Font-Fallback: ' . str_replace(["\r", "\n"], '', $reason));
    exit;
}

function pr_key(string $name): string {
    return strtolower($name);
}

function pr_normalized_key(string $name): string {
    $name = strtolower(rawurldecode($name));
    $name = preg_replace('/[\s\-]+/', '_', $name);
    $name = preg_replace('/_+/', '_', $name);
    return $name ?: '';
}

function pr_is_optional_image(string $requestedPath, string $filename): bool {
    $path = strtolower(str_replace('\\', '/', $requestedPath));
    $base = pr_normalized_key($filename);
    $optionalNames = [
        'hearts_hardlight_02.png', 'shadow_multiply_02.png', 'texture_overlay.png',
        'pinky_nrm.png', 'stars_hardlight_02.png', 'coffee_mpl.png', 'rusty_mpl.png',
        'bluemood_mpl.png', 'security_hardlight.png', 'toxic_hrd.png', 'alien_hrd.png',
        'shiny_hrd.png', 'drops_mpl.png', 'frame_gold.png', 'misty_hrd.png',
        'frame_gray_4.png', 'glitter_hrd.png', 'frame_black_2.png', 'frame_wood_2.png',
        'finger_nrm.png', 'canal_bundle.png', 'spromo_canal_bundle.png'
    ];

    if (in_array($base, $optionalNames, true)) return true;
    if (strpos($path, '/c_images/camera/') !== false) return true;
    if (strpos($path, '/c_images/web_promo_small/') !== false) return true;
    return false;
}

$requested = isset($_GET['u']) ? (string) $_GET['u'] : '';
$requestedPath = parse_url($requested, PHP_URL_PATH);
if (!is_string($requestedPath) || $requestedPath === '') $requestedPath = $requested;
$requestedPath = rawurldecode(str_replace('\\', '/', $requestedPath));
$filename = basename($requestedPath);

if ($filename === '' || $filename === '.' || $filename === '..') pr_plain(400, 'invalid asset');

$extension = strtolower(pathinfo($filename, PATHINFO_EXTENSION));
$allowed = ['png','gif','jpg','jpeg','webp','svg','ico','nitro','json','mp3','ogg','wav','ttf','otf','woff','woff2','eot'];
if (!in_array($extension, $allowed, true)) pr_plain(404, 'unsupported asset');

$fontExtensions = ['ttf','otf','woff','woff2','eot'];
$imageExtensions = ['png','gif','jpg','jpeg','webp','svg','ico'];

$webRoot = __DIR__;
$projectRoot = dirname(__DIR__);
$roots = [];
foreach ([
    $webRoot . DIRECTORY_SEPARATOR . 'swf_pz',
    $projectRoot . DIRECTORY_SEPARATOR . 'swf_pz',
    $webRoot . DIRECTORY_SEPARATOR . 'SWF',
    $projectRoot . DIRECTORY_SEPARATOR . 'SWF',
    $webRoot . DIRECTORY_SEPARATOR . 'nitro-last',
    $webRoot
] as $candidateRoot) {
    $real = realpath($candidateRoot);
    if ($real !== false && is_dir($real)) $roots[] = $real;
}
$roots = array_values(array_unique($roots));

if (!$roots) {
    if (in_array($extension, $fontExtensions, true)) pr_empty_font('no-asset-root:' . $filename);
    if (in_array($extension, $imageExtensions, true) && pr_is_optional_image($requestedPath, $filename)) pr_transparent_png('no-asset-root:' . $filename);
    pr_plain(404, 'asset roots not found');
}

$mtime = 0;
foreach ($roots as $root) $mtime = max($mtime, (int) @filemtime($root));
$cacheFile = rtrim(sys_get_temp_dir(), DIRECTORY_SEPARATOR) . DIRECTORY_SEPARATOR . 'paradiserp_asset_index_v5.json';
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
        try {
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
        } catch (Throwable $error) {
            // Keep the resolver usable even if one optional asset folder is unreadable.
            error_log('[ParadiseRP asset resolver] index failed for ' . $root . ': ' . $error->getMessage());
        }
    }
    @file_put_contents($cacheFile, json_encode(['_mtime' => $mtime, 'files' => $index], JSON_UNESCAPED_SLASHES));
}

$lookupKeys = array_values(array_unique([
    pr_key($filename),
    pr_normalized_key($filename),
    pr_key(str_replace(' ', '_', $filename)),
    pr_key(str_replace('_', ' ', $filename)),
    pr_normalized_key(str_replace('_', ' ', $filename))
]));

$candidates = [];
foreach ($lookupKeys as $key) {
    if (isset($index[$key]) && is_array($index[$key])) {
        foreach ($index[$key] as $entry) $candidates[] = $entry;
    }
}

if (!$candidates) {
    if (in_array($extension, $fontExtensions, true)) pr_empty_font('missing-font:' . $filename);
    if (in_array($extension, $imageExtensions, true) && pr_is_optional_image($requestedPath, $filename)) pr_transparent_png('missing-optional-image:' . $filename);
    pr_plain(404, 'asset not found');
}

$wantedSegments = array_values(array_filter(explode('/', strtolower($requestedPath)), 'strlen'));
$best = null;
$bestScore = -1;
foreach ($candidates as $entry) {
    $candidate = strtolower(str_replace('\\', '/', (string) ($entry['path'] ?? '')));
    $candidateSegments = array_values(array_filter(explode('/', $candidate), 'strlen'));
    $score = 0;

    foreach ($wantedSegments as $segment) {
        if ($segment === strtolower($filename)) continue;
        $segmentNorm = pr_normalized_key($segment);
        if (in_array($segment, $candidateSegments, true)) $score += 2;
        foreach ($candidateSegments as $candidateSegment) {
            if ($segmentNorm !== '' && pr_normalized_key($candidateSegment) === $segmentNorm) $score += 1;
        }
    }

    $wantedTail = implode('/', array_slice($wantedSegments, -3));
    if ($wantedTail !== '' && substr($candidate, -strlen($wantedTail)) === $wantedTail) $score += 30;
    if (basename($candidate) === strtolower($filename)) $score += 10;
    if (pr_normalized_key(basename($candidate)) === pr_normalized_key($filename)) $score += 8;

    if ($score > $bestScore) {
        $bestScore = $score;
        $best = $entry;
    }
}

if ($best === null || !isset($roots[(int) $best['root']])) {
    if (in_array($extension, $fontExtensions, true)) pr_empty_font('missing-font:' . $filename);
    if (in_array($extension, $imageExtensions, true) && pr_is_optional_image($requestedPath, $filename)) pr_transparent_png('missing-optional-image:' . $filename);
    pr_plain(404, 'asset not found');
}

$root = $roots[(int) $best['root']];
$file = realpath($root . DIRECTORY_SEPARATOR . str_replace('/', DIRECTORY_SEPARATOR, (string) $best['path']));
if ($file === false || strpos(strtolower($file), strtolower($root)) !== 0 || !is_file($file)) {
    if (in_array($extension, $fontExtensions, true)) pr_empty_font('missing-font:' . $filename);
    if (in_array($extension, $imageExtensions, true) && pr_is_optional_image($requestedPath, $filename)) pr_transparent_png('missing-optional-image:' . $filename);
    pr_plain(404, 'asset not found');
}

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
