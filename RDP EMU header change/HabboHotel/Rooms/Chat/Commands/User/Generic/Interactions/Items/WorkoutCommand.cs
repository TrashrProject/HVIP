using System;
using System.Linq;
using System.Text;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class WorkoutCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_workout"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Comienzas a hacer ejercicio en el gimnasio."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetRoomUser() == null)
                return;

            if (Session.GetPlay().IsWorkingOut)
            {
                Session.SendWhisper("¡Ya estás ejercitandote!", 1);
                return;
            }

            if (Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras trabajas!", 1);
                return;
            }

            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                return;
            }

            if (!Room.GymEnabled)
            {
                Session.SendWhisper("Debes estar dentro del Gimnasio para ejercitarte.", 1);
                return;
            }
            /*
            if (Session.GetPlay().CurEnergy <= 0)
            {
                Session.SendWhisper("No tienes suficiente energía para ejercitarte.", 1);
                return;
            }
            */
            if (Session.GetPlay().TryGetCooldown("cansancio"))
                return;

            bool HasTreadmill = Room.GetRoomItemHandler().GetFloor.Where(x => (x.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || x.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer") && x.Coordinate == Session.GetRoomUser().Coordinate).ToList().Count > 0;

            if (!HasTreadmill)
            {
                Session.SendWhisper("¡Debes estar en una caminadora o un crosstrainer para entrenar!", 1);
                return;
            }

            Item Treadmill = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => (x.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || x.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer") && x.Coordinate == Session.GetRoomUser().Coordinate);

            if (Treadmill == null)
            {
                Session.SendWhisper("¡Debes estar en una caminadora o un crosstrainer para entrenar!", 1);
                return;
            }

            bool Strength = false;//always true
            if (Treadmill.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill")
                Strength = true;

            if (Strength && Session.GetPlay().Strength >= RoleplayManager.StrengthCap)
            {
                Session.SendWhisper("Has alcanzado el nivel de fuerza máximo de: " + RoleplayManager.StrengthCap, 1);
                return;
            }
            /*No Stamina
            if (!Strength && Session.GetPlay().Stamina >= RoleplayManager.StaminaCap)
            {
                Session.SendWhisper("Has alcanzado el nivel máximo de resistencia de: " + RoleplayManager.StaminaCap, 1);
                return;
            }
            */
            #endregion

            #region Execute
            if (Session.GetRoomUser().isSitting)
            {
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().RemoveStatus("sit");
                Session.GetRoomUser().isSitting = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }
            else if (Session.GetRoomUser().isLying)
            {
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().RemoveStatus("lay");
                Session.GetRoomUser().isLying = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }

            Treadmill.ExtraData = "1";
            Treadmill.InteractingUser = Session.GetHabbo().Id;
            Treadmill.UpdateState(false, true);
            Treadmill.RequestUpdate(1, true);

            Session.GetRoomUser().SetRot(Treadmill.Rotation,false);

            if (!Strength)
                Session.GetRoomUser().ApplyEffect(195);
            else
                Session.GetRoomUser().ApplyEffect(194);

            object[] Data = { Treadmill.Id, Strength };

            Session.GetRoomUser().SetRot(Treadmill.Rotation, false);
            Session.GetPlay().IsWorkingOut = true;

            Session.GetPlay().TimerManager.CreateTimer("workout", 1000, true, Data);

            if (Strength)
            {
                RoleplayManager.Shout(Session, "*Comienza a correr en la caminadora comenzando a entrenar su fuerza*", 5);
            }
            else
            {
                RoleplayManager.Shout(Session, "*Comienza a ejercitarse en el crosstrainer comenzando a entrenar su fuerza*", 5);
            }
            Session.SendWhisper("Rutina: " + String.Format("{0:N0}", Session.GetPlay().StrengthEXP) + "/50", 1);

            // Cansancio
            if (Session.GetPlay().StrengthEXP == 3 || Session.GetPlay().StrengthEXP == 5 || Session.GetPlay().StrengthEXP == 8 || Session.GetPlay().StrengthEXP == 13 || Session.GetPlay().StrengthEXP == 21 || Session.GetPlay().StrengthEXP == 34 || Session.GetPlay().StrengthEXP == 45)
			{
                int coolDown = 300;
                Session.GetPlay().CooldownManager.CreateCooldown("cansancio", 1000, RoleplayManager.WorkoutTime + coolDown);
            }
            #endregion
        }
    }
}