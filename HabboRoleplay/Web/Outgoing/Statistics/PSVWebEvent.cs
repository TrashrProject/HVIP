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
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TargetWebEvent class.
    /// </summary>
    class PSVWebEvent : IWebEvent
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
                #region Passive Mode
                case "open":
                    {
                        if (Client.GetPlay().TryGetCooldown("psvmode", true))
                            return;

                        if (Client.GetPlay().TogglingPSV)
                        {
                            Client.SendWhisper("Ya te encuentras cambiando el estado de tu modo pasivo.", 1);
                            return;
                        }
                        if(Client.GetPlay().GodMode)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras tengas protección de inmunidad.", 1);
                            return;
                        }

                        #region Conditions

                        #region Basic Conditions
                        if (Client.GetPlay().Cuffed)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                            return;
                        }
                        if (Client.GetRoomUser() == null || !Client.GetRoomUser().CanWalk)
                        {
                            Client.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsRobATM)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás Robando Cajero!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDying)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                            return;
                        }
                        #endregion

                        if (Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("general"))
                        {
                            Client.SendWhisper("Primero debes de terminar de realizar tu actividad actual.", 1);
                            return;
                        }

                        #region Conditions Checks
                        if (!Client.GetRoomUser().GetRoom().RoomData.SafeZoneEnabled)
                        {
                            Client.SendWhisper("Debes estar en una zona segura para hacer eso.", 1);
                            return;
                        }
                        if (RoleplayManager.PurgeEvent)
                        {
                            Client.SendWhisper("¡No puedes hacer eso durante la purga!", 1);
                            return;
                        }
                        if (!Client.GetPlay().PassiveMode)
                        {
                            if (Client.GetPlay().CurHealth < Client.GetPlay().MaxHealth)
                            {
                                Client.SendWhisper("Debes tener el 100% de vida para hacer eso.", 1);
                                return;
                            }
                        }
                        if (Client.GetPlay().IsWanted)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras estás en la lista de buscados.", 1);
                            return;
                        }
                        if (Client.GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
                        {
                            Client.SendWhisper("No puedes hacer eso mientras estás trabajando de policía", 1);
                            return;
                        }
                        if (Client.GetPlay().CamCargId == 3 || Client.GetPlay().CamCargId == 4)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras transportas cargamentos ilegales.", 1);
                            return;
                        }
                        if (Client.GetPlay().EquippedWeapon != null)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras lleves un arma equipada.", 1);
                            return;
                        }
                        #endregion

                        #endregion
                        if (Client.GetRoomUser().IsWalking)
                            Client.GetRoomUser().PathStep = Client.GetRoomUser().Path.Count;

                        string word = (Client.GetPlay().PassiveMode) ? "salir del" : "entrar en";

                        Client.GetPlay().TogglingPSV = true;
                        Client.GetPlay().LoadingTimeLeft = RoleplayManager.PSVTime;

                        RoleplayManager.Shout(Client, "((Comienza a "+word+" modo pasivo))", 7);
                        Client.SendWhisper("Debes esperar " + Client.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                        Client.GetPlay().TimerManager.CreateTimer("general", 1000, true);
                        Client.GetPlay().CooldownManager.CreateCooldown("psvmode", 1000, 3);
                    }
                    break;
                #endregion

                #region Force Desactive Passive Mode
                case "forceoff":
                    {
                        if (Client.GetPlay().PassiveMode)
                        {
                            Client.GetPlay().PassiveMode = false;
                            Client.SendMessage(new RoomNotificationComposer("psv-icon", 3, "Modo Pasivo: Desactivado", ""));
                            Socket.Send("compose_psv_mode|desactive");
                            Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
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
