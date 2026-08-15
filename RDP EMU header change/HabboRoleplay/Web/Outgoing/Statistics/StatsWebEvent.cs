using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Roleplay.Web;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TargetWebEvent class.
    /// </summary>
    class StatsWebEvent : IWebEvent
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
                case "open":
                    {
                        string[] ReceivedData = Data.Split(',');
                        switch (ReceivedData[1])
                        {
                            #region WantedList
                            case "WantedList":
                                {
                                    string html = "";

                                    #region HTML
                                    if (RoleplayManager.WantedList.Count <= 0)
                                        html += "<center>Nadie está siendo buscado ahora mismo.</center>";

                                    lock (RoleplayManager.WantedList.Values)
                                    {
                                        foreach (var Wanted in RoleplayManager.WantedList.Values)
                                        {
                                            StringBuilder WantedStar = new StringBuilder();
                                            if (Wanted.WantedLevel == 1) WantedStar.Append("✪");
                                            if (Wanted.WantedLevel == 2) WantedStar.Append("✪✪");
                                            if (Wanted.WantedLevel == 3) WantedStar.Append("✪✪✪");
                                            if (Wanted.WantedLevel == 4) WantedStar.Append("✪✪✪✪");
                                            if (Wanted.WantedLevel == 5) WantedStar.Append("✪✪✪✪✪");
                                            if (Wanted.WantedLevel == 6) WantedStar.Append("✪✪✪✪✪✪");

                                            html += "<div class=\"flex flex-wrap -m-1 justify-center\">";
                                            //<!-- User info -->
                                            html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\" style=\"width: 17%;\">";
                                            html += "<div class=\"m-px relative\">";
                                            html += "<div class=\"overflow-hidden bg-light-05 rounded-t\">";
                                            html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + PlusEnvironment.GetHabboById(Convert.ToInt32(Wanted.UserId)).Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                            html += "</div>";
                                            html += "</div>";
                                            html += "</div>";
                                            html += "<div class=\"bg-dark-4 rounded m-1 group\" style=\"width: 69%;padding: 5px;\">";
                                            html += "<div style=\"max-height: 108px;overflow: auto;\">";
                                            html += "<b>Nombre:</b> ";
                                            html += "<div class=\"bsn_s_country\" style=\"display: inline-block;\">" + PlusEnvironment.GetHabboById(Convert.ToInt32(Wanted.UserId)).Username + "</div>";
                                            html += "<br>";
                                            html += "<b>Nivel de b&uacute;squeda:</b> ";
                                            html += "<div class=\"bsn_s_hours\" style=\"display: inline-block;\">" + WantedStar + "</div>";
                                            if (PlusEnvironment.GetHabboById(Convert.ToInt32(Wanted.UserId)).GetClient() != null)
                                            {
                                                html += "<br>";
                                                html += "<b>Buscad@ por:</b> ";
                                                html += "<div class=\"bsn_s_info\" style=\"display: inline-block;\">" + PlusEnvironment.GetHabboById(Convert.ToInt32(Wanted.UserId)).GetClient().GetPlay().WantedFor + "</div>";
                                            }

                                            string RoomName = "Desconocida";
                                            if (!Wanted.LastSeenRoom.Contains("Desconocida"))
                                            {
                                                Room RoomSeen = RoleplayManager.GenerateRoom(Convert.ToInt32(Wanted.LastSeenRoom));
                                                if (RoomSeen != null)
                                                    RoomName = RoomSeen.Name;
                                            }
                                            html += "<br>";
                                            html += "<b>&Uacute;lt. vez visto:</b> ";
                                            html += "<div class=\"bsn_s_info\" style=\"display: inline-block;\">" + RoomName + "</div>";

                                            html += "</div>";
                                            html += "</div>";
                                            //<!-- End User info -->
                                            html += "</div>";
                                        }
                                    }
                                    #endregion

                                    Socket.Send("compose_stats|wanted|" + html);
                                }
                                break;
                            #endregion

                            default:
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
