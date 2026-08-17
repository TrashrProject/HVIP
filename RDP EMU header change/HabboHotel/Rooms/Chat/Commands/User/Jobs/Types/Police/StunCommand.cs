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
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class StunCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_stun"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Paraliza a una persona para detenerla."; }
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
                if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
                if (!TargetClient.GetPlay().IsWanted)
                {
                    Session.SendWhisper("No puedes hacer eso a alguien que no está en la lista de buscados.", 1);
                    return;
                }
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3 && TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }
            if(Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
                return;
            }
            if (TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("Esta persona se encuentra esposada. No hace falta paralizarla.", 1);
                return;
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "stun") && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }

            if (Session.GetPlay().DrivingCar || Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas dentro de un vehículo!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona muert@!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona encarcelad@!", 1);
                return;
            }

            if (TargetClient.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona mientras va conduciendo!", 1);
                return;
            }
            if (TargetClient.GetPlay().Pasajero || TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona mientras va de pasajer@!", 1);
                return;
            }

            if (TargetClient.GetRoomUser().Frozen)
            {
                Session.SendWhisper("¡No puedes aturdir a alguien que ya está aturdido, debes esposarlo!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes aturdir a un usuario ausente!", 1);
                return;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona que está en modo pasivo!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("stun"))
                return;
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);
            Point TargetClientPos = new Point(TargetUser.Coordinate.X, TargetUser.Coordinate.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            CryptoRandom Random = new CryptoRandom();
            int Chance = Random.Next(1, 101);

            if (Distance <= RoleplayManager.StunGunRange)
            {
                if (Chance <= 8)
                {
                    RoleplayManager.Shout(Session, "*Dispara su pistola paralizadora hacia " + TargetClient.GetHabbo().Username + ", pero falla*", 37);
                    
                    Session.GetPlay().CooldownManager.CreateCooldown("stun", 1000, 3);
                }
                else
                {
                    RoleplayManager.Shout(Session, "*Dispara su pistola paralizadora hacia " + TargetClient.GetHabbo().Username + " inmovilizándolo inmediatamente*", 37);
                    TargetClient.GetPlay().TimerManager.CreateTimer("stun", 1000, false);
                    //TargetClient.SendMessage(new FloodControlComposer(15));

                    if (TargetClient.GetPlay().InsideTaxi)
                        TargetClient.GetPlay().InsideTaxi = false;

                    TargetClient.GetRoomUser().Frozen = true;
                    TargetClient.GetRoomUser().CanWalk = false;
                    TargetClient.GetRoomUser().ClearMovement(true);

                    #region Desequipar al Convicto
                    if (TargetClient.GetPlay().EquippedWeapon != null)
                    {
                        string UnEquipMessage = TargetClient.GetPlay().EquippedWeapon.UnEquipText;
                        UnEquipMessage = UnEquipMessage.Replace("[NAME]", TargetClient.GetPlay().EquippedWeapon.PublicName);

                        RoleplayManager.Shout(TargetClient, UnEquipMessage, 5);

                        if (TargetClient.GetRoomUser().CurrentEffect == TargetClient.GetPlay().EquippedWeapon.EffectID)
                            TargetClient.GetRoomUser().ApplyEffect(0);

                        if (TargetClient.GetRoomUser().CarryItemID == TargetClient.GetPlay().EquippedWeapon.HandItem)
                            TargetClient.GetRoomUser().CarryItem(0);

                        TargetClient.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                        TargetClient.GetPlay().EquippedWeapon = null;

                        TargetClient.GetPlay().WLife = 0;
                        TargetClient.GetPlay().Bullets = 0;
                    }
                    #endregion

                    Session.GetPlay().CooldownManager.CreateCooldown("stun", 1000, 3);
                }
            }
            else
            {
                RoleplayManager.Shout(Session, "*Dispara su pistola paralizadora hacia " + TargetClient.GetHabbo().Username + ", pero el disparo no lo alcanza*", 37);
                
                Session.GetPlay().CooldownManager.CreateCooldown("stun", 1000, 3);
            }

            #region Sound System
            foreach (RoomUser RoomUsers in Session.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUsers == null || RoomUsers.GetClient() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|paralizer");
            }
            #endregion
            #endregion
        }
    }
}