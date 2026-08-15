using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class LawCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_law"; }
        }

        public string Parameters
        {
            get { return "%user% %level%"; }
        }

        public string Description
        {
            get { return "Asigna nivel de Cargos a una persona [1-5]."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int WantedLevel = 1;
            int NewWantedLevel = 1;
            Wanted NewWanted;
            #endregion

            #region Conditions
            if (Params.Length != 3)
            {
                Session.SendWhisper("Comando inválido. Usa :cargos [usuario] [1-5]", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetPlay() == null)
            {
                Session.SendWhisper("((Ha ocurrido un error al buscar a la persona, probablemente esté desconectada))", 1);
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                    return;
                }
                if(PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "law") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo policías pueden hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes asignarle cargos a una persona que está encarcelada!", 1);
                return;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes asignarle cargos a una persona que está en modo pasivo!", 1);
                return;
            }
            #endregion

            #region Execute
            if (int.TryParse(Params[2], out WantedLevel))
            {
                if (WantedLevel > 5 || WantedLevel == 0)
                {
                    Session.SendWhisper("Debes ingresar un nivel de búsqueda entre [1-5]", 1);
                    return;
                }

                string RoomId = TargetClient.GetHabbo().CurrentRoomId.ToString() != "0" ? TargetClient.GetHabbo().CurrentRoomId.ToString() : "Desconocida";

                NewWanted = new Wanted(Convert.ToUInt32(TargetClient.GetHabbo().Id), RoomId, WantedLevel);

                if (RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    int CurrentWantedLevel = RoleplayManager.WantedList[TargetClient.GetHabbo().Id].WantedLevel;
                    NewWantedLevel = WantedLevel;
                    if (NewWantedLevel > CurrentWantedLevel)
                    {
                        if (TargetClient.GetPlay().TimerManager.ActiveTimers.ContainsKey("wanted"))
                            TargetClient.GetPlay().TimerManager.ActiveTimers["wanted"].EndTimer();
                        

                        TargetClient.GetPlay().IsWanted = true;
                        TargetClient.GetPlay().WantedLevel = WantedLevel;
                        TargetClient.GetPlay().WantedTimeLeft = RoleplayManager.WantedTime;

                        TargetClient.GetPlay().TimerManager.CreateTimer("wanted", 1000, false);
                        RoleplayManager.WantedList.TryUpdate(TargetClient.GetHabbo().Id, NewWanted, RoleplayManager.WantedList[TargetClient.GetHabbo().Id]);
                        RoleplayManager.Shout(Session, "*Asigna el nivel de cargos de " + TargetClient.GetHabbo().Username + " de " + CurrentWantedLevel + " Estrellas a " + WantedLevel + " Estrellas*", 37);
                        PlusEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] El nivel de búsqueda de " + TargetClient.GetHabbo().Username + " se actualizó de " + CurrentWantedLevel + " estrellas a " + WantedLevel + " estrellas.");

                        // WS Wanted Stars
                        if (TargetClient.GetPlay().WebSocketConnection != null)
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "compose_wanted_stars|" + TargetClient.GetPlay().WantedLevel);
                        
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Intenta actualizar los cargos de " + TargetClient.GetHabbo().Username + " pero nota que su nivel de búsqueda se encuentra en " + CurrentWantedLevel + " estrellas*", 37);
                        return;
                    }
                }
                else
                {
                    if (TargetClient.GetPlay().TimerManager.ActiveTimers.ContainsKey("wanted"))
                        TargetClient.GetPlay().TimerManager.ActiveTimers["wanted"].EndTimer();

                    TargetClient.GetPlay().IsWanted = true;
                    TargetClient.GetPlay().WantedLevel = WantedLevel;
                    TargetClient.GetPlay().WantedTimeLeft = RoleplayManager.WantedTime;
                    TargetClient.GetPlay().TimerManager.CreateTimer("wanted", 1000, false);
                    RoleplayManager.WantedList.TryAdd(TargetClient.GetHabbo().Id, NewWanted);
                    RoleplayManager.Shout(Session, "*Agrega a " + TargetClient.GetHabbo().Username + " a la Lista de buscados con " + WantedLevel + " estrellas de búsqueda*", 37);
                    PlusEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] " + TargetClient.GetHabbo().Username + " ha sido añadido a la Lista de buscados con " + WantedLevel + " estrellas de búsqueda.");

                    // WS Wanted Stars
                    if (TargetClient.GetPlay().WebSocketConnection != null)
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "compose_wanted_stars|" + TargetClient.GetPlay().WantedLevel);
                    return;
                }
            }
            else
            {
                Session.SendWhisper("Debes ingresar un nivel de búsqueda entre [1-5]", 1);
                return;
            }
            #endregion
        }
    }
}