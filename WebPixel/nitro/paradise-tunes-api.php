<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function tunes_json(array $data, int $status = 200): void { http_response_code($status); echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES); exit; }
function tunes_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql); if (!$stmt) throw new RuntimeException('Service indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Opération impossible.');
    return $stmt;
}
function tunes_body(): array { $data = json_decode(file_get_contents('php://input') ?: '{}', true); return is_array($data) ? $data : []; }
function tunes_text($value, int $max, bool $required = false): string {
    $value = trim((string)$value);
    if (($required && $value === '') || mb_strlen($value) > $max) throw new InvalidArgumentException('Donnée musicale invalide.');
    return $value;
}
function tunes_url($value, bool $required = false): ?string {
    $url = trim((string)$value); if ($url === '') { if ($required) throw new InvalidArgumentException('URL audio requise.'); return null; }
    if (strlen($url) > 500 || filter_var($url, FILTER_VALIDATE_URL) === false || strtolower((string)parse_url($url, PHP_URL_SCHEME)) !== 'https') throw new InvalidArgumentException('Utilisez une URL HTTPS valide.');
    $host = strtolower((string)parse_url($url, PHP_URL_HOST));
    if ($required && $host === '') throw new InvalidArgumentException('Source audio invalide.');
    return $url;
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) tunes_json(['ok' => false, 'error' => 'Session expirée.'], 401);
    $db = (new DBManager())->Con();
    $username = trim((string)$session->Read(Config::$SessionName));
    $user = tunes_stmt($db, 'SELECT id, username FROM users WHERE username=? LIMIT 1', 's', [$username])->get_result()->fetch_assoc();
    if (!$user) tunes_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);
    $userId = (int)$user['id'];
    if (!isset($_SESSION['paradise_tunes_csrf'])) $_SESSION['paradise_tunes_csrf'] = bin2hex(random_bytes(24));
    $csrf = (string)$_SESSION['paradise_tunes_csrf'];

    if ($_SERVER['REQUEST_METHOD'] === 'GET') {
        $tracks = [];
        $result = tunes_stmt($db, 'SELECT t.id,t.owner_id,t.title,t.artist,t.audio_url,t.cover_url,t.genre,t.description,t.created_at,EXISTS(SELECT 1 FROM phone_music_favorites f WHERE f.track_id=t.id AND f.user_id=?) favorite FROM phone_music_tracks t WHERE t.owner_id=? ORDER BY t.created_at DESC,t.id DESC LIMIT 250', 'ii', [$userId, $userId])->get_result();
        while ($row = $result->fetch_assoc()) $tracks[] = ['id'=>(int)$row['id'],'ownerId'=>(int)$row['owner_id'],'title'=>$row['title'],'artist'=>$row['artist'],'audioUrl'=>$row['audio_url'],'coverUrl'=>$row['cover_url'],'genre'=>$row['genre'],'description'=>$row['description'],'createdAt'=>(int)$row['created_at'],'favorite'=>(bool)$row['favorite']];
        $playlists = [];
        $result = tunes_stmt($db, 'SELECT id,name,cover_url,created_at,updated_at FROM phone_music_playlists WHERE user_id=? ORDER BY updated_at DESC,id DESC LIMIT 100', 'i', [$userId])->get_result();
        while ($row = $result->fetch_assoc()) {
            $ids=[]; $items=tunes_stmt($db, 'SELECT track_id FROM phone_music_playlist_tracks WHERE playlist_id=? ORDER BY position,track_id', 'i', [(int)$row['id']])->get_result();
            while ($item=$items->fetch_assoc()) $ids[]=(int)$item['track_id'];
            $playlists[]=['id'=>(int)$row['id'],'name'=>$row['name'],'coverUrl'=>$row['cover_url'],'trackIds'=>$ids,'createdAt'=>(int)$row['created_at']];
        }
        tunes_json(['ok'=>true,'csrf'=>$csrf,'user'=>['id'=>$userId,'username'=>$user['username']],'tracks'=>$tracks,'playlists'=>$playlists]);
    }

    if ($_SERVER['REQUEST_METHOD'] !== 'POST') tunes_json(['ok'=>false,'error'=>'Méthode refusée.'],405);
    $data=tunes_body(); if (!hash_equals($csrf,(string)($data['csrf']??''))) tunes_json(['ok'=>false,'error'=>'Session de sécurité expirée.'],403);
    $action=strtolower(trim((string)($data['action']??''))); $now=time();
    if ($action==='track.create') {
        $title=tunes_text($data['title']??'',100,true); $artist=tunes_text($data['artist']??'',80); $audio=tunes_url($data['audioUrl']??'',true); $cover=tunes_url($data['coverUrl']??''); $genre=tunes_text($data['genre']??'',40); $description=tunes_text($data['description']??'',500);
        tunes_stmt($db,'INSERT INTO phone_music_tracks(owner_id,title,artist,audio_url,cover_url,genre,description,created_at) VALUES(?,?,?,?,?,?,?,?)','issssssi',[$userId,$title,$artist,$audio,$cover,$genre,$description,$now]);
        tunes_json(['ok'=>true,'message'=>'Musique ajoutée à votre bibliothèque.']);
    }
    $trackId=max(0,(int)($data['trackId']??0));
    if ($action==='track.delete') { $stmt=tunes_stmt($db,'DELETE FROM phone_music_tracks WHERE id=? AND owner_id=?','ii',[$trackId,$userId]); if(!$stmt->affected_rows)tunes_json(['ok'=>false,'error'=>'Suppression refusée.'],403); tunes_json(['ok'=>true]); }
    if ($action==='favorite.toggle') { $exists=tunes_stmt($db,'SELECT 1 FROM phone_music_tracks WHERE id=? AND owner_id=?','ii',[$trackId,$userId])->get_result()->fetch_row(); if(!$exists)tunes_json(['ok'=>false,'error'=>'Morceau introuvable.'],404); $fav=tunes_stmt($db,'SELECT 1 FROM phone_music_favorites WHERE user_id=? AND track_id=?','ii',[$userId,$trackId])->get_result()->fetch_row(); if($fav)tunes_stmt($db,'DELETE FROM phone_music_favorites WHERE user_id=? AND track_id=?','ii',[$userId,$trackId]);else tunes_stmt($db,'INSERT INTO phone_music_favorites(user_id,track_id,created_at) VALUES(?,?,?)','iii',[$userId,$trackId,$now]); tunes_json(['ok'=>true,'favorite'=>!$fav]); }
    if ($action==='playlist.create') { $name=tunes_text($data['name']??'',80,true); $cover=tunes_url($data['coverUrl']??''); tunes_stmt($db,'INSERT INTO phone_music_playlists(user_id,name,cover_url,created_at,updated_at) VALUES(?,?,?,?,?)','issii',[$userId,$name,$cover,$now,$now]); tunes_json(['ok'=>true]); }
    $playlistId=max(0,(int)($data['playlistId']??0));
    $owned=tunes_stmt($db,'SELECT id FROM phone_music_playlists WHERE id=? AND user_id=?','ii',[$playlistId,$userId])->get_result()->fetch_assoc();
    if (!$owned) tunes_json(['ok'=>false,'error'=>'Playlist introuvable.'],404);
    if ($action==='playlist.delete') { tunes_stmt($db,'DELETE FROM phone_music_playlists WHERE id=? AND user_id=?','ii',[$playlistId,$userId]); tunes_json(['ok'=>true]); }
    if ($action==='playlist.rename') { $name=tunes_text($data['name']??'',80,true); tunes_stmt($db,'UPDATE phone_music_playlists SET name=?,updated_at=? WHERE id=? AND user_id=?','siii',[$name,$now,$playlistId,$userId]); tunes_json(['ok'=>true]); }
    if ($action==='playlist.add') { $track=tunes_stmt($db,'SELECT id FROM phone_music_tracks WHERE id=? AND owner_id=?','ii',[$trackId,$userId])->get_result()->fetch_assoc(); if(!$track)tunes_json(['ok'=>false,'error'=>'Morceau introuvable.'],404); tunes_stmt($db,'INSERT IGNORE INTO phone_music_playlist_tracks(playlist_id,track_id,position,created_at) SELECT ?,?,COALESCE(MAX(position),-1)+1,? FROM phone_music_playlist_tracks WHERE playlist_id=?','iiii',[$playlistId,$trackId,$now,$playlistId]); tunes_stmt($db,'UPDATE phone_music_playlists SET updated_at=? WHERE id=?','ii',[$now,$playlistId]); tunes_json(['ok'=>true]); }
    if ($action==='playlist.remove') { tunes_stmt($db,'DELETE FROM phone_music_playlist_tracks WHERE playlist_id=? AND track_id=?','ii',[$playlistId,$trackId]); tunes_json(['ok'=>true]); }
    tunes_json(['ok'=>false,'error'=>'Action inconnue.'],404);
} catch (InvalidArgumentException $e) { tunes_json(['ok'=>false,'error'=>$e->getMessage()],422); }
catch (mysqli_sql_exception $e) { error_log('[Paradise Tunes] '.$e->getMessage()); tunes_json(['ok'=>false,'error'=>$e->getCode()===1062?'Cette musique existe déjà.':'Impossible de modifier Paradise Tunes.'],409); }
catch (Throwable $e) { error_log('[Paradise Tunes] '.$e->getMessage()); tunes_json(['ok'=>false,'error'=>'Impossible de charger Paradise Tunes.'],500); }
