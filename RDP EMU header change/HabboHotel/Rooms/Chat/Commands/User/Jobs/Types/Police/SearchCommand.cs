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
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class SearchCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_search"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Revisar a una persona para ver si tiene objetos ilegales."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                    return;
                }
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "search") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
                return;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacerle eso a una persona que va de pasajer@))", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes catear a una persona muerta!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes catear a una persona encarcelada!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes catear a un usuario ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                Random Random = new Random();

                int Chance = Random.Next(1, 101);

                if (Chance <= 8)
                {
                    RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " intentando encontrar alguna droga pero parece que no puede encontrar ninguna*", 37);
                    return;
                }
                else
                {
                    bool HasWeed = TargetClient.GetPlay().Weed > 0;
                    bool HasCocaine = TargetClient.GetPlay().Cocaine > 0;

                    if (!HasWeed && !HasCocaine)
                    {
                        RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " intentando encontrar alguna droga pero parece que no puede encontrar ninguna*", 37);
                        return;
                    }
                    else if (HasWeed && !HasCocaine)
                    {
                        RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " y encuentra " + String.Format("{0:N0}", TargetClient.GetPlay().Weed) + "g de marihuana*", 37);
                        return;
                    }
                    else if (HasCocaine && !HasWeed)
                    {
                        RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " y encuentra " + String.Format("{0:N0}", TargetClient.GetPlay().Cocaine) + "g de cocaína*", 37);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " y encuentra " + String.Format("{0:N0}", TargetClient.GetPlay().Cocaine) + "g de cocaína y " + String.Format("{0:N0}", TargetClient.GetPlay().Weed) + "g de marihuana*", 37);
                        return;
                    }
                }
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a catear.", 1);
                return;
            }
            #endregion
        }
    }
}