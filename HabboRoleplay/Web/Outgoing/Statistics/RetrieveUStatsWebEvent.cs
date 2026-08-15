using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Roleplay.Web;


namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// RetrieveUStatsWebEvent class.
    /// </summary>
    class RetrieveUStatsWebEvent : IWebEvent
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

            if (Client.GetPlay().TargetLock)
                return;

            int UserID = Convert.ToInt32(Data);
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserID);

            if (TargetClient == null)
                return;

            string CachedTargetString = GetUserComponent.ReturnUserStatistics(TargetClient);
            
            if (String.IsNullOrEmpty(CachedTargetString))
                return;

            Client.GetPlay().Target = TargetClient.GetHabbo().Username; // Definimos Target si no tiene Fijado uno

            Socket.Send("compose_characterbar|" + CachedTargetString);

            #region Hospital
            Socket.Send("compose_hospital|close_actionbtn|");
            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
            {
                if (Client.GetPlay().IsWorking)
                {
                    // In Hospital
                    if (TargetClient.GetPlay().IsDead && !TargetClient.GetPlay().BeingHealed)
                    {
                        // Revisar
                        if (Client.GetPlay().RevisPaci != TargetClient.GetHabbo().Id)
                        {
                            Socket.Send("compose_hospital|open_actionbtn|Revisar|revisar");
                        }
                        // Atender
                        else
                        {
                            Socket.Send("compose_hospital|open_actionbtn|Atender|atender");
                        }
                    }
                    // Out Hospital
                    else if (TargetClient.GetPlay().IsDying)
                    {
                        // Reanimar
                        if (!TargetClient.GetPlay().TargetReanim)
						{
                            Socket.Send("compose_hospital|open_actionbtn|Reanimar|reanimar");
                        }
						else
						{
                            // Subir paciente
                            if (!TargetClient.GetPlay().Pasajero)
							{
                                Socket.Send("compose_hospital|open_actionbtn|Subir|subir");
                            }
                            // Salvar
							else if (TargetClient.GetPlay().ChoferID == Client.GetHabbo().Id)
							{
                                Socket.Send("compose_hospital|open_actionbtn|Salvar|salvar");
                            }
						}
                    }
                }
            }
            #endregion

            #region BodyGuard
            Socket.Send("compose_bodyguard|hide_sell_button|");
            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "cubrir"))
            {
                Socket.Send("compose_bodyguard|show_sell_button|");
            }
            #endregion
        }
    }
}
