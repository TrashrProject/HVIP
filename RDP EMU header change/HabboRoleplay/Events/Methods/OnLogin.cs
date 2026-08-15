using System;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Core;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboRoleplay.Events.Methods
{
    public class OnLogin : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;

            if (Client == null || Client.GetPlay() == null || Client.GetHabbo() == null)
                return;

            if (Client.GetPlay().OriginalOutfit == null)
                Client.GetPlay().OriginalOutfit = Client.GetHabbo().Look;

            DeathCheck(Client);
            DyingCheck(Client);
            CuffCheck(Client);
            NoobCheck(Client);
            JailedCheck(Client);
            SancCheck(Client);
            BanCHnCheck(Client);
            PhoneCheck(Client);
            RoleplayManager.PoliceCMDSCheck(Client);
            CheckWS(Client);
            //SocketConnection(Client);

            // El primer usuario en conectarse se encargará de hacer que el emu precargue todas las salas
            if (!RoleplayManager.PreLoadedRooms)
            {
                RoleplayManager.PreLoadedRooms = true;
                PlusEnvironment.GetGame().GetRoomManager().PreLoadRooms();
            }
        }

        #region Check WS Connect
        public void CheckWS(GameClient Client)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true))
            {
                //Console.WriteLine("Los WebSockets NO conectaron");
                //Client.SendMessage(new BroadcastMessageAlertComposer("¡Los WebSockets NO conectaron!\n\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\n\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee."));
                //Client.SendNotification("¡Los WebSockets NO conectaron!\n\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\n\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee.");
                Logging.WriteLine(Client.GetHabbo().Username + " se ha conectado.", ConsoleColor.DarkGreen);
            }
            else
            {
                //Client.SendMessage(new BroadcastMessageAlertComposer("¡Los WebSockets NO. conectaron!\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee."));
                SocketConnection(Client);
                Logging.WriteLine(Client.GetHabbo().Username + " se ha conectado. (+WS)", ConsoleColor.DarkGreen);
            }
        }
        #endregion

        #region SocketConnection
        public void SocketConnection(GameClient Client)
        {
            Client.GetPlay().InitWSDialogues();
            Client.GetPlay().InitStatDialogue();
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_loader|8");// 8 = Cantidad a sumar al loader.
        }
        #endregion

        #region CuffCheck
        public void CuffCheck(GameClient Client)
        {
            if (!Client.GetPlay().Cuffed)
                return;
            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("cuff"))
                Client.GetPlay().TimerManager.CreateTimer("cuff", 1000, true);
        }
        #endregion

        #region DeathCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void DeathCheck(GameClient Client)
        {
            if (!Client.GetPlay().IsDead)
                return;
            if(!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("death"))
                Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);

        }
        #endregion

        #region DyingCheck
        public void DyingCheck(GameClient Client)
        {
            if (!Client.GetPlay().IsDying)
                return;
            
            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("dying"))
                Client.GetPlay().TimerManager.CreateTimer("dying", 1000, true);
        }
        #endregion

        #region NoobCheck

        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void NoobCheck(GameClient Client)
        {
            if (!Client.GetPlay().IsNoob)
                return;

            Client.GetPlay().TimerManager.CreateTimer("noob", 1000, true);
        }
        #endregion

        #region JailedCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void JailedCheck(GameClient Client)
        {
            // WS Wanted Stars
            if (Client.GetPlay().WebSocketConnection != null)
            {
                if (Client.GetPlay().WantedLevel > 0)
                {
                    Wanted NewWanted = new Wanted(Convert.ToUInt32(Client.GetHabbo().Id), "Desconocida", Client.GetPlay().WantedLevel);
                    Client.GetPlay().TimerManager.CreateTimer("wanted", 1000, false);
                    RoleplayManager.WantedList.TryAdd(Client.GetHabbo().Id, NewWanted);
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_wanted_stars|" + Client.GetPlay().WantedLevel);
                }
            }

            if (!Client.GetPlay().IsJailed)
                return;
            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("jail"))
                Client.GetPlay().TimerManager.CreateTimer("jail", 1000, true);

        }
        #endregion

        #region SancCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void SancCheck(GameClient Client)
        {
            if (!Client.GetPlay().IsSanc)
                return;
            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("sanc"))
                Client.GetPlay().TimerManager.CreateTimer("sanc", 1000, true);
        }
        #endregion

        #region BanCHnCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void BanCHnCheck(GameClient Client)
        {
            if (!Client.GetPlay().ChNBanned)
                return;
            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("chnban"))
                Client.GetPlay().TimerManager.CreateTimer("chnban", 1000, true);
        }
        #endregion

        #region PhoneCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void PhoneCheck(GameClient Client)
        {
            if (Client.GetPlay().Phone <= 0)
                return;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "load_apps");
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "show_button");
        }
        #endregion
    }
}