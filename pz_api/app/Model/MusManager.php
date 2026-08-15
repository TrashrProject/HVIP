<?php
/**
 * PZ API MUS socket manager.
 * Local emulator defaults: 127.0.0.1:3043.
 */

class MusManager
{
    protected static $H = '127.0.0.1';
    protected static $P = 3043;

    protected $Host;
    protected $Port;

    protected function Con()
    {
        $this->Host = self::$H;
        $this->Port = self::$P;

        $socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
        if ($socket === false) {
            throw new RuntimeException('Could not create MUS socket: ' . socket_strerror(socket_last_error()));
        }

        socket_set_option($socket, SOL_SOCKET, SO_SNDTIMEO, ['sec' => 3, 'usec' => 0]);
        socket_set_option($socket, SOL_SOCKET, SO_RCVTIMEO, ['sec' => 3, 'usec' => 0]);

        if (@socket_connect($socket, $this->Host, $this->Port) === false) {
            $error = socket_last_error($socket);
            socket_close($socket);
            throw new RuntimeException('Could not connect to MUS server at ' . $this->Host . ':' . $this->Port . ' - ' . socket_strerror($error));
        }

        return $socket;
    }

    public function Ready($C, $P = null)
    {
        $packet = $C . ':';

        if ($P !== null) {
            $values = [];
            foreach ($P as $V) {
                $values[] = $V;
            }
            $packet .= implode('|', $values);
        }

        return $this->Send($packet);
    }

    public function Send($S, $return = false)
    {
        $socket = $this->Con();

        $length = strlen($S);
        $written = 0;
        while ($written < $length) {
            $result = socket_write($socket, substr($S, $written));
            if ($result === false) {
                $error = socket_last_error($socket);
                socket_close($socket);
                throw new RuntimeException('Could not send data to MUS server: ' . socket_strerror($error));
            }
            $written += $result;
        }

        $response = '';
        while (($out = @socket_read($socket, 8192)) !== false && $out !== '') {
            $response .= $out;
            if (strlen($out) < 8192) {
                break;
            }
        }

        socket_close($socket);

        if ($response !== '') {
            $parts = explode(':', $response, 2);
            $payload = isset($parts[1]) ? $parts[1] : $response;
            if (!$return) {
                echo $payload;
            }
            return $return ? $payload : true;
        }

        return $return ? null : true;
    }
}
