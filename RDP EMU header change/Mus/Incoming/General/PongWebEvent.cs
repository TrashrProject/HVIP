using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Cache;
using log4net;

namespace Plus.Mus.Incoming.General
{
    /// <summary>
    /// PongWebEvent class.
    /// </summary>
    class PongWebEvent : IMusWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (PlusEnvironment.GetGame().GetMusWebEventManager().SocketReady(Socket))
                return;

            Socket.Send("compose_ping|true");
        }
    }
}
