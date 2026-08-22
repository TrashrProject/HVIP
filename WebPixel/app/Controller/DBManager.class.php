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
        if ($this->pz_conn instanceof mysqli) return $this->pz_conn;

        $this->host = self::$DBHOST;
        $this->user = self::$DBUser;
        $this->pass = self::$DBPass;
        $this->db = self::$DBName;

        $con = mysqli_connect($this->host, $this->user, $this->pass, $this->db, 3306);
        if (!$con) throw new RuntimeException('Could not connect to database: ' . mysqli_connect_error());

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
        if ($R === false) throw new RuntimeException('Database query failed: ' . mysqli_error($this->Con()));
        return $R;
    }

    /**
     * Prepared-query primitive used by the Control Center. Legacy helpers stay
     * untouched so the existing CMS/EMU integration keeps working.
     */
    public function Prepared($sql, $types = '', array $params = array())
    {
        $stmt = mysqli_prepare($this->Con(), $sql);
        if (!$stmt) throw new RuntimeException('Database prepare failed: ' . mysqli_error($this->Con()));

        if ($types !== '') {
            if (strlen($types) !== count($params)) {
                mysqli_stmt_close($stmt);
                throw new InvalidArgumentException('Prepared parameter count mismatch.');
            }
            $bind = array($stmt, $types);
            foreach ($params as $key => $value) $bind[] = &$params[$key];
            if (!call_user_func_array('mysqli_stmt_bind_param', $bind)) {
                $error = mysqli_stmt_error($stmt);
                mysqli_stmt_close($stmt);
                throw new RuntimeException('Database bind failed: ' . $error);
            }
        }

        if (!mysqli_stmt_execute($stmt)) {
            $error = mysqli_stmt_error($stmt);
            mysqli_stmt_close($stmt);
            throw new RuntimeException('Database execute failed: ' . $error);
        }
        return $stmt;
    }

    public function PreparedResult($sql, $types = '', array $params = array())
    {
        $stmt = $this->Prepared($sql, $types, $params);
        $result = mysqli_stmt_get_result($stmt);
        if ($result === false) {
            $error = mysqli_stmt_error($stmt);
            mysqli_stmt_close($stmt);
            throw new RuntimeException('Database result failed: ' . $error);
        }
        mysqli_stmt_close($stmt);
        return $result;
    }

    public function PreparedRow($sql, $types = '', array $params = array())
    {
        $result = $this->PreparedResult($sql, $types, $params);
        $row = mysqli_fetch_assoc($result);
        mysqli_free_result($result);
        return $row ?: null;
    }

    public function PreparedAll($sql, $types = '', array $params = array())
    {
        $result = $this->PreparedResult($sql, $types, $params);
        $rows = array();
        while ($row = mysqli_fetch_assoc($result)) $rows[] = $row;
        mysqli_free_result($result);
        return $rows;
    }

    public function PreparedAffect($sql, $types = '', array $params = array())
    {
        $stmt = $this->Prepared($sql, $types, $params);
        $affected = mysqli_stmt_affected_rows($stmt);
        mysqli_stmt_close($stmt);
        return $affected;
    }

    public function LastInsertId()
    {
        return (int)mysqli_insert_id($this->Con());
    }

    public function Begin()
    {
        if (!mysqli_begin_transaction($this->Con())) throw new RuntimeException('Unable to start database transaction.');
    }

    public function Commit()
    {
        if (!mysqli_commit($this->Con())) throw new RuntimeException('Unable to commit database transaction.');
    }

    public function Rollback()
    {
        @mysqli_rollback($this->Con());
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
