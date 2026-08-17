<?php
/**
 * ParadiseRP local Nitro avatar imager proxy.
 * Renders with the exact FigureData/FigureMap/.nitro assets used by the client.
 *
 * Nitro Imager peut mettre plusieurs secondes à charger un asset custom au
 * premier rendu. Le proxy laisse donc assez de temps au renderer et met en
 * cache les images côté navigateur afin d'éviter de recalculer 50/80 previews.
 */
header('Cache-Control: public, max-age=86400, stale-while-revalidate=604800');
header('X-Content-Type-Options: nosniff');

$figure = trim((string)($_GET['figure'] ?? ''));
if ($figure === '' || strlen($figure) > 2000 || !preg_match('/^[a-z0-9.\-]+$/i', $figure)) {
    http_response_code(422);
    header('Content-Type: image/png');
    echo base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=');
    exit;
}

$allowed = [
    'action' => '/^[a-z0-9,=_-]{0,80}$/i',
    'gesture' => '/^[a-z]{2,5}$/i',
    'direction' => '/^[0-7]$/',
    'head_direction' => '/^[0-7]$/',
    'headonly' => '/^[01]$/',
    'dance' => '/^[0-4]$/',
    'effect' => '/^\d{1,5}$/',
    'size' => '/^[snl]$/i',
    'frame_num' => '/^\d{1,3}$/',
    'img_format' => '/^(png|gif)$/i',
];
$params = ['figure' => $figure];
foreach ($allowed as $key => $pattern) {
    if (!isset($_GET[$key])) continue;
    $value = trim((string)$_GET[$key]);
    if (preg_match($pattern, $value)) $params[$key] = $value;
}
if (!isset($params['size'])) $params['size'] = 'l';
if (!isset($params['direction'])) $params['direction'] = '2';
if (!isset($params['head_direction'])) $params['head_direction'] = '2';
if (!isset($params['gesture'])) $params['gesture'] = 'std';
if (!isset($params['action'])) $params['action'] = 'std';

$url = 'http://127.0.0.1:3030/?' . http_build_query($params, '', '&', PHP_QUERY_RFC3986);

function paradise_fetch_nitro_image(string $url, int $timeout = 60): array {
    $ctx = stream_context_create([
        'http' => [
            'method' => 'GET',
            'timeout' => $timeout,
            'ignore_errors' => true,
            'header' => "Connection: close\r\n",
        ]
    ]);

    $body = @file_get_contents($url, false, $ctx);
    $status = 0;
    $contentType = '';
    foreach (($http_response_header ?? []) as $line) {
        if (preg_match('#^HTTP/\S+\s+(\d+)#i', $line, $m)) $status = (int)$m[1];
        if (stripos($line, 'Content-Type:') === 0) $contentType = trim(substr($line, 13));
    }
    return [$body, $status, $contentType];
}

// Le premier appel peut être lent car Nitro Imager charge le .nitro custom.
[$body, $status, $contentType] = paradise_fetch_nitro_image($url, 60);

// Une seule seconde tentative courte si le renderer était momentanément occupé.
if ($body === false || $status < 200 || $status >= 300 || strlen((string)$body) <= 50) {
    usleep(250000);
    [$body, $status, $contentType] = paradise_fetch_nitro_image($url, 20);
}

if ($body !== false && $status >= 200 && $status < 300 && strlen($body) > 50) {
    header('Content-Type: ' . ($contentType ?: 'image/png'));
    header('X-Paradise-Imager: local-nitro');
    echo $body;
    exit;
}

// Imager indisponible : image valide pour ne pas casser toute la fenêtre.
http_response_code(503);
header('Content-Type: image/png');
header('X-Paradise-Imager: offline');
echo base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=');
