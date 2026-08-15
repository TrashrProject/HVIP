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
using System.Web.ModelBinding;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TargetWebEvent class.
    /// </summary>
    class TutorialWebEvent : IWebEvent
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
                case "step":
                    {
                        string[] ReceivedData = Data.Split(',');
                        int Step = 0;

                        if (!int.TryParse(ReceivedData[1], out Step))
                            return;

                        Client.GetPlay().TutorialStep = Step;

                        #region Check Cases
                        switch (Client.GetPlay().TutorialStep)
                        {
                            case 13:
                                {
                                    #region Check si está en tienda de ropa
                                    if (Client.GetRoomUser() == null)
                                        return;

                                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                                    if (Room.WardrobeEnabled && Room.Type.Equals("public"))
                                    {
                                        Socket.Send("compose_tutorial|13");
                                    }
                                    #endregion
                                }
                                break;
                            case 18:
                                {
                                    #region Check si está en tienda de teléfonos
                                    if (Client.GetRoomUser() == null)
                                        return;

                                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                                    if (Room.PhoneStoreEnabled && Room.Type.Equals("public"))
                                    {
                                        Socket.Send("compose_tutorial|18");
                                    }
                                    #endregion
                                }
                                break;
                            case 23:
                                {
                                    #region Check si está en el concesionario
                                    if (Client.GetRoomUser() == null)
                                        return;

                                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                                    if (Room.BuyCarEnabled && Room.Type.Equals("public"))
                                    {
                                        Socket.Send("compose_tutorial|24");
                                    }
                                    #endregion
                                }
                                break;
                            case 27:
                                {
                                    #region Check si está en la Tienda 24/7
                                    if (Client.GetRoomUser() == null)
                                        return;

                                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                                    if (Room.MallEnabled && Room.Type.Equals("public"))
                                    {
                                        Socket.Send("compose_tutorial|28");
                                    }
                                    #endregion
                                }
                                break;
                            case 36:
                                {
                                    // Last Step
                                    Client.GetPlay().InTutorial = false;
                                }
                                break;
                            default:
                                break;
                        }
                        #endregion
                    }
                    break;
            }
        }
    }
}
