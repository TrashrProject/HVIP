using System;
using ConnectionManager;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Incoming;
using SharedPacketLib;
using System.Text;
using Plus.Utilities;
using System.IO;
using log4net;
using System.Security.Cryptography;
using Plus.Communication.WebSocket;

namespace Plus.Net
{
    public class GamePacketParser : IDataParser
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Net.GamePacketParser");

        public delegate void HandlePacket(ClientPacket message);

        private readonly GameClient currentClient;
        private ConnectionInformation con;

        private bool _halfDataRecieved = false;
        private byte[] _halfData = null;
        private bool _deciphered = false;
        private bool _isWebSocket = false;
        public GamePacketParser(GameClient me)
        {
            currentClient = me;
        }

        public void handlePacketData(byte[] Data)
        {
            try
            {
                if (Data[0] == 71 && Data[1] == 69)
                {
                    PolicyRequest(Data);

                    this._isWebSocket = true;
                    this.currentClient.GetConnection().IsWebSocket = true;
                    return;
                }

                if (this.currentClient.RC4Client != null && !this._deciphered)
                {
                    this.currentClient.RC4Client.Decrypt(ref Data);
                    this._deciphered = true;
                }

                try
                {
                    if (this._isWebSocket) Data = EncodeDecode.DecodeMessage(Data);
                }
                catch
                {
                    return;
                }


                if (this._halfDataRecieved)
                {
                    byte[] FullDataRcv = new byte[this._halfData.Length + Data.Length];
                    Buffer.BlockCopy(this._halfData, 0, FullDataRcv, 0, this._halfData.Length);
                    Buffer.BlockCopy(Data, 0, FullDataRcv, this._halfData.Length, Data.Length);

                    this._halfDataRecieved = false; // mark done this round
                    handlePacketData(FullDataRcv); // repeat now we have the combined array
                    return;
                }

                using (BinaryReader Reader = new BinaryReader(new MemoryStream(Data)))
                {
                    if (Data.Length < 4)
                        return;

                    int MsgLen = HabboEncoding.DecodeInt32(Reader.ReadBytes(4));
                    if ((Reader.BaseStream.Length - 4) < MsgLen)
                    {
                        this._halfData = Data;
                        this._halfDataRecieved = true;
                        return;
                    }
                    else if (MsgLen < 0 || MsgLen > 5120)//TODO: Const somewhere.
                        return;

                    byte[] Packet = Reader.ReadBytes(MsgLen);

                    using (BinaryReader R = new BinaryReader(new MemoryStream(Packet)))
                    {
                        int Header = HabboEncoding.DecodeInt16(R.ReadBytes(2));

                        byte[] Content = new byte[Packet.Length - 2];
                        Buffer.BlockCopy(Packet, 2, Content, 0, Packet.Length - 2);

                        ClientPacket Message = new ClientPacket(Header, Content);
                        onNewPacket.Invoke(Message);

                        this._deciphered = false;
                    }

                    if (Reader.BaseStream.Length - 4 > MsgLen)
                    {
                        byte[] Extra = new byte[Reader.BaseStream.Length - Reader.BaseStream.Position];
                        Buffer.BlockCopy(Data, (int)Reader.BaseStream.Position, Extra, 0, (int)(Reader.BaseStream.Length - Reader.BaseStream.Position));

                        this._deciphered = true;
                        handlePacketData(Extra);
                    }
                }
            }
            catch (Exception e)
            {
                //log.Error("Packet Error!", e);
            }
        }

        private void PolicyRequest(byte[] packet)
        {
            string data = Encoding.UTF8.GetString(packet);
            /* Handshaking and managing ClientSocket */

            if (!data.Contains("ey:"))
                return;

            string key = data.Replace("ey:", "`")
                      .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                      .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                      .Trim();

            // key should now equal dGhlIHNhbXBsZSBub25jZQ==
            string longKey = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            string test1 = Convert.ToBase64String(hashBytes);

            string newLine = "\r\n";

            string response = "HTTP/1.1 101 Switching Protocols" + newLine
                 + "Upgrade: websocket" + newLine
                 + "Connection: Upgrade" + newLine
                 + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                 ;

            // which one should I use? none of them fires the onopen method
            currentClient.GetConnection().SendData(Encoding.UTF8.GetBytes(response));
        }

        public void Dispose()
        {
            onNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone()
        {
            return new GamePacketParser(currentClient);
        }

        public event HandlePacket onNewPacket;

        public void SetConnection(ConnectionInformation con)
        {
            this.con = con;
            onNewPacket = null;
        }
    }
}