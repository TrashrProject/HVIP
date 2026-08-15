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
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class BailCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_bail"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Libera a un convicto de la cárcel."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }
            if (Session.GetPlay().Level == 1 && Session.GetPlay().CurXP < 3)
            {
                Session.SendWhisper("Necesitas al menos 3 puntos de reputación para pagar Fianzas.", 1);
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
            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }
            if (!TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes liberar a una persona que no está encarcelada!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes liberar a un usuario que está ausente!", 1);
                return;
            }

            if (TargetClient.GetRoomUser().RoomId != Session.GetRoomUser().RoomId)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " no se encuentra aquí.", 1);
                return;
            }
            #endregion

            #region Execute
            int Cost = RoleplayManager.BailCost;
            if (TargetClient.GetPlay().WantedLevel > 0)
                Cost = TargetClient.GetPlay().WantedLevel * RoleplayManager.BailCost;

            if(Session.GetHabbo().Credits < Cost)
            {
                Session.SendWhisper("Necesitas $"+Cost+" para pagar la fianza de " + TargetClient.GetHabbo().Username, 1);
                return;
            }
            Session.GetHabbo().Credits -= Cost;
            Session.GetHabbo().UpdateCreditsBalance();
            RoleplayManager.Shout(Session, "*Ha pagado la fianza de " + TargetClient.GetHabbo().Username + " por $"+Cost+"*", 5);
            TargetClient.GetPlay().IsJailed = false;
            TargetClient.GetPlay().JailedTimeLeft = 0;
            TargetClient.GetHabbo().Poof(true);

            #region Business Police
            string MyCity = Room.City;
            int Comisaria = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetPolStation(MyCity, out PlayRoom Data);
            Room GenRoom = RoleplayManager.GenerateRoom(Comisaria);
            if (GenRoom != null && GenRoom.Group != null)
            {
                GenRoom.Group.Sells += Cost;
                GenRoom.Group.UpdateSells(Cost);
            }
            #endregion
            #endregion
        }
    }
}