using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;

namespace Plus.HabboHotel.Roleplay.Web.Incoming.General
{
    /// <summary>
    /// RetrieveStatsWebEvent class.
    /// </summary>
    class InitWSDialogues : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            Socket.Send("compose_ws_dialogues|");
        }
    }
}
