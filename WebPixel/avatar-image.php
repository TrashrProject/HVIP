<?php
/**
 * ParadiseRP local Nitro avatar imager proxy.
 * Renders with the exact FigureData/FigureMap/.nitro assets used by the client.
 */
header('Cache-Control: public, max-age=300');
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
$ctx = stream_context_create([
    'http' => [
        'method' => 'GET',
        'timeout' => 8,
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

if ($body !== false && $status >= 200 && $status < 300 && strlen($body) > 50) {
    header('Content-Type: ' . ($contentType ?: 'image/png'));
    header('X-Paradise-Imager: local-nitro');
    echo $body;
    exit;
}

// Imager offline: return a valid transparent image instead of breaking the UI.
http_response_code(503);
header('Content-Type: image/png');
header('X-Paradise-Imager: offline');
echo base64_decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=');
