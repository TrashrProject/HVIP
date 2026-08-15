<?php
class DBConManager extends Config
{
    protected $host;
    protected $user;
    protected $pass;
    protected $db;
    protected $pz_conn = null;

    public function connect()
    {
        if ($this->pz_conn instanceof mysqli) {
            return $this->pz_conn;
        }

        $this->host = self::$DBHOST;
        $this->user = self::$DBUser;
        $this->pass = self::$DBPass;
        $this->db = self::$DBName;

        $con = mysqli_connect($this->host, $this->user, $this->pass, $this->db, 3306);
        if (!$con) {
            throw new RuntimeException('Could not connect to database: ' . mysqli_connect_error());
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

    public function Query($Q)
    {
        $R = mysqli_query($this->Con(), $Q);
        if ($R === false) {
            throw new RuntimeException('Database query failed: ' . mysqli_error($this->Con()));
        }
        return $R;
    }

    public function Select($T, $C = '*', $W = null, $E = null, $NoFetch = false)
    {
        $Q = 'SELECT ' . $C . ' FROM ' . $T;
        if ($W !== null && $W !== '') $Q .= ' WHERE ' . $W;
        if ($E !== null && $E !== '') $Q .= ' ' . $E;

        $R = $this->Query($Q);
        if (mysqli_num_rows($R) <= 0) return null;
        return $NoFetch ? $R : mysqli_fetch_assoc($R);
    }

    public function Count($T, $C = '*', $W = null, $E = null)
    {
        $Q = 'SELECT ' . $C . ' FROM ' . $T;
        if ($W !== null && $W !== '') $Q .= ' WHERE ' . $W;
        if ($E !== null && $E !== '') $Q .= ' ' . $E;
        return mysqli_num_rows($this->Query($Q));
    }

    public function Update($T, $P, $W = null, $E = null)
    {
        $Q = 'UPDATE ' . $T . ' SET ' . $P;
        if ($W !== null && $W !== '') $Q .= ' WHERE ' . $W;
        if ($E !== null && $E !== '') $Q .= ' ' . $E;
        $this->Query($Q);
        return true;
    }

    public function Insert($T, $P, $V)
    {
        $Q = 'INSERT INTO ' . $T . ' (' . implode(',', $P) . ') VALUES (' . implode(',', $V) . ')';
        $this->Query($Q);
        return true;
    }
}
