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
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.RolePlay.PlayRoom;
namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class KillCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_kill"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Mata a una persona."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                Session.SendWhisper("Debes ingresar el nombre de usuario.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }
            if (TargetClient.GetPlay().IsSanc)
            {
                Session.SendWhisper("No puedes matar a esa persona porque se encuentra sancionada.", 1);
                return;
            }

            if (Session != TargetClient && TargetClient.GetHabbo().Rank >= 6)
            {
                Session.SendWhisper("No puedes hacerle eso a un todo poderoso Dios Developer.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;

            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes matar a una persona encarcelada!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡Esa persona ya está muerta!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(TargetClient);
                TargetClient.GetPlay().IsWorking = false;
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetPlay().Cuffed)
                TargetClient.GetPlay().Cuffed = false;
            
            RoleplayManager.CheckEscort(TargetClient);

            // Pasajero o Manejando
            RoleplayManager.CheckOnCar(TargetClient);

            TargetClient.GetPlay().CurHealth = 0;
            TargetClient.GetPlay().UpdateInteractingUserDialogues();
            TargetClient.GetPlay().RefreshStatDialogue();
            RoleplayManager.Shout(Session, "*Mata a " + TargetClient.GetHabbo().Username + "*", 23);
            return;
        }
    }
}