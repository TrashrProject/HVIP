<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class SessionMG
{
    // Constructs the Class
    public function __construct() {

        if (!isset($_SESSION)) {
            ini_set('session.gc_maxlifetime', 30*60);
            ini_set('session.use_strict_mode', '1');
            ini_set('session.cookie_httponly', '1');
            ini_set('session.cookie_samesite', 'Lax');
            session_start();
        }
    }

    // Verifies if Session Exists
    public function Exist($nome) {

        return isset($_SESSION[$nome]);
    }

    // Saves the Session
    public function Save($nome, $valor) {

        $_SESSION[$nome] = $valor;
    }

    // Reads the session
    public function Read($nome) {

        if ($this->Exist($nome))
            return $_SESSION[$nome];

        return null;
    }
    // Deletes the session
    public function Delete($nome) {

        if ($this->Exist($nome))
            unset($_SESSION[$nome]);
    }
    // Destroy
    public function Destroy() {

        if (isset($_SESSION))
            return session_destroy();

        return false;
    }
}
