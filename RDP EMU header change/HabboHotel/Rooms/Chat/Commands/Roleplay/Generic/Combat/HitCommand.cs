using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class HitCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_hit"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Golpea a otra persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room == null)
                return;

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes agredir en modo pasivo.", 1);
                return;
            }

            if (Params.Length == 1)
            {
                //Session.GetPlay().LastCommand = ":hit";
                CombatManager.GetCombatType("fist").Execute(Session, null, true);
                return;
            }

            // New Target System
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(( (Session.GetPlay().Target != "" && Params[1] == "x") || Session.GetPlay().TargetLock) ? Session.GetPlay().Target : Params[1]);

            if (TargetClient == null)
            {
                RoomUser Bot = Room.GetRoomUserManager().GetBotByName(Params[1]);

                if (Bot != null)
                {
                    return;
                }

                //Session.GetPlay().LastCommand = ":hit " + Params[1];
                Session.SendWhisper("((Ha ocurrido un error con el usuario, probablemente esté desconectado))", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                //Session.GetPlay().LastCommand = ":hit " + Params[1];
                Session.SendWhisper("((Ha ocurrido un error con el usuario, probablemente esté desconectado o no está en esta Zona))", 1);
                return;
            }
            if (TargetClient.GetRoomUser().RoomId != Room.RoomId)
			{
                Session.SendWhisper("¡Esa persona no se encuentra aquí para poderla golpear!", 1);
                return;
            }
            if (TargetClient.GetHabbo().EscortID > 0)
            {
                Session.SendWhisper("¡No puedes golpear a una persona que va siendo escoltada!", 1);
                return;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("¡No puedes golpear a una persona que va en Taxi!", 1);
                return;
            }
            if (TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("¡No puedes golpear a una persona esposada!", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes golpear a una persona muerta!", 1);
                return;
            }
            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes golpear a una persona encarcelada!", 1);
                return;
            }
            // New Target System
            Session.GetPlay().Target = TargetClient.GetHabbo().Username;

            Session.GetPlay().LastCommand = ":hit " + Params[1];
            CombatManager.GetCombatType("fist").Execute(Session, TargetClient);
        }
    }
}