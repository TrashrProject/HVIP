<?php
/**
 * PZ API database manager.
 * Local XAMPP defaults: 127.0.0.1:3306, root, no password, database hv_rp.
 */

class DBConManager
{
    protected $host = '127.0.0.1';
    protected $port = 3306;
    protected $user = 'root';
    protected $pass = '';
    protected $db = 'hv_rp';
    protected $pz_conn = null;

    public function connect()
    {
        if ($this->pz_conn instanceof mysqli) {
            return $this->pz_conn;
        }

        $con = mysqli_connect($this->host, $this->user, $this->pass, $this->db, $this->port);
        if (!$con) {
            throw new RuntimeException('Database connection failed: ' . mysqli_connect_error());
        }

        mysqli_set_charset($con, 'utf8mb4');
        $this->pz_conn = $con;
        return $this->pz_conn;
    }

    public function close()
    {
        if ($this->pz_conn instanceof mysqli) {
            mysqli_close($this->pz_conn);
            $this->pz_conn = null;
        }
    }
}

class DBManager extends DBConManager
{
    public function Con()
    {
        return $this->connect();
    }

    public function Select($T, $C = '*', $W = null, $E = null)
    {
        $Q = 'SELECT ' . $C . ' FROM ' . $T;
        if ($W !== null && $W !== '') {
            $Q .= ' WHERE ' . $W;
        }
        if ($E !== null && $E !== '') {
            $Q .= ' ' . $E;
        }

        $R = mysqli_query($this->Con(), $Q);
        if ($R === false) {
            throw new RuntimeException('Database query failed: ' . mysqli_error($this->Con()));
        }

        return mysqli_num_rows($R) > 0 ? mysqli_fetch_assoc($R) : null;
    }

    public function Count($T, $C = '*', $W = null, $E = null)
    {
        $Q = 'SELECT ' . $C . ' FROM ' . $T;
        if ($W !== null && $W !== '') {
            $Q .= ' WHERE ' . $W;
        }
        if ($E !== null && $E !== '') {
            $Q .= ' ' . $E;
        }

        $R = mysqli_query($this->Con(), $Q);
        if ($R === false) {
            throw new RuntimeException('Database query failed: ' . mysqli_error($this->Con()));
        }

        return mysqli_num_rows($R);
    }

    public function Update($T, $P, $W = null, $E = null)
    {
        $Q = 'UPDATE ' . $T . ' SET ' . $P;
        if ($W !== null && $W !== '') {
            $Q .= ' WHERE ' . $W;
        }
        if ($E !== null && $E !== '') {
            $Q .= ' ' . $E;
        }

        $R = mysqli_query($this->Con(), $Q);
        if ($R === false) {
            throw new RuntimeException('Database update failed: ' . mysqli_error($this->Con()));
        }

        return true;
    }
}
