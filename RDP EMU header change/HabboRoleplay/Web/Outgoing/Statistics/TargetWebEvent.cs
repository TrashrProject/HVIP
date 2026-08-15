using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Roleplay.Web;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TargetWebEvent class.
    /// </summary>
    class TargetWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;
            
            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                #region Combat Mode
                case "open":
                    {
                        if (Client.GetPlay().TryGetCooldown("combatmode", true))
                            return;

                        Client.GetPlay().CombatMode = !Client.GetPlay().CombatMode;

                        if (Client.GetPlay().CombatMode)
                        {
                            Client.SendMessage(new RoomNotificationComposer("combat-icon", 3, "Modo Combate: Activado", ""));
                            Socket.Send("compose_combat_mode|active");
                        }
                        else
                        {
                            Client.SendMessage(new RoomNotificationComposer("combat-icon", 3, "Modo Combate: Desactivado", ""));
                            Socket.Send("compose_combat_mode|desactive");
                        }

                        Client.GetPlay().CooldownManager.CreateCooldown("combatmode", 1000, 3);
                    }
                    break;
                #endregion

                #region lock
                case "lock":
                    {
                        Client.GetPlay().TargetLock = true;
                    }
                    break;
                #endregion

                #region unlock
                case "unlock":
                    {
                        Client.GetPlay().TargetLock = false;
                    }
                    break;
                #endregion

                #region close
                case "close":
                    {
                        Client.GetPlay().TargetLock = false;
                        Client.GetPlay().Target = "";
                    }
                    break;
                #endregion

                #region Default
                default:
                    break;
                #endregion
            }
        }
    }
}
