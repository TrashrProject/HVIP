<?php
/**
 * PixelZone session wrapper.
 * V3 hardening remains compatible with HTTP development while enabling secure
 * cookies automatically on the production HTTPS CMS.
 */
class SessionMG
{
    public function __construct()
    {
        if (session_status() !== PHP_SESSION_ACTIVE) {
            $lifetime = 30 * 60;
            ini_set('session.gc_maxlifetime', (string)$lifetime);
            ini_set('session.use_strict_mode', '1');
            ini_set('session.use_only_cookies', '1');

            $https = (!empty($_SERVER['HTTPS']) && strtolower((string)$_SERVER['HTTPS']) !== 'off')
                || (isset($_SERVER['SERVER_PORT']) && (int)$_SERVER['SERVER_PORT'] === 443)
                || (isset($_SERVER['HTTP_X_FORWARDED_PROTO']) && strtolower((string)$_SERVER['HTTP_X_FORWARDED_PROTO']) === 'https');

            session_set_cookie_params(array(
                'lifetime' => 0,
                'path' => '/',
                'domain' => '',
                'secure' => $https,
                'httponly' => true,
                'samesite' => 'Lax'
            ));
            session_start();
        }
    }

    public function Exist($nome)
    {
        return isset($_SESSION[$nome]);
    }

    public function Save($nome, $valor)
    {
        $_SESSION[$nome] = $valor;
    }

    public function Read($nome)
    {
        return $this->Exist($nome) ? $_SESSION[$nome] : null;
    }

    public function Delete($nome)
    {
        if ($this->Exist($nome)) unset($_SESSION[$nome]);
    }

    public function Destroy()
    {
        if (session_status() !== PHP_SESSION_ACTIVE) return false;
        $_SESSION = array();
        if (ini_get('session.use_cookies')) {
            $params = session_get_cookie_params();
            setcookie(session_name(), '', time() - 42000, $params['path'], $params['domain'], $params['secure'], $params['httponly']);
        }
        return session_destroy();
    }
}
