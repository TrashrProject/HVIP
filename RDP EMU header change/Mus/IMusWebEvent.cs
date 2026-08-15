using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;

namespace Plus.Mus
{
    public interface IMusWebEvent
    {
        void Execute(GameClient Client, string Data, IWebSocketConnection Socket);
    }
}
