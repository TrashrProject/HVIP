using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Combat;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Drawing;
using Plus.HabboRoleplay.Misc;
using Plus.Utilities;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Users
{
    class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int UserId = Packet.PopInt();
            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
                return;

            Session.SendMessage(new HabboUserBadgesComposer(Habbo));

            // NEW RP
            #region Open user statistics dialogue (sockets)

            if (Habbo.GetClient() != null)
            {
                if (Habbo.GetClient() != Session)
                {
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_characterbar", "" + Habbo.Id);
                }
            }

            #endregion

            #region Combat Mode
            if (Session.GetPlay().CombatMode && !Session.GetPlay().IsSanc && Habbo.GetClient() != null)
            {
                #region Basic Conditions
                if (Session.GetPlay().Cuffed)
                {
                    Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                    return;
                }
                if (Session.GetRoomUser() != null && !Session.GetRoomUser().CanWalk)
                {
                    Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                    return;
                }
                if (Session.GetPlay().IsDead)
                {
                    Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsJailed)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDying)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                #endregion

                GameClient TargetClient = Habbo.GetClient();

                if (Session.GetPlay().EquippedWeapon == null)
                {
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "stun") && (Session.GetPlay().IsWorking || Session.GetPlay().PoliceTrial) && !TargetClient.GetPlay().Cuffed)
                    {
                        // Paralizar / Desparalizar
                        #region Execute
                        RoomUser RoomUser = Session.GetRoomUser();
                        RoomUser TargetUser = TargetClient.GetRoomUser();
                        if (TargetUser == null)
                        {
                            Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en esta Zona.", 1);
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
                        }
                        if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3)
                        {
                            Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                            return;
                        }
                        if (Session.GetPlay().DrivingCar)
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
                        if (TargetUser.IsAsleep)
                        {
                            Session.SendWhisper("¡No puedes aturdir a un usuario ausente!", 1);
                            return;
                        }
                        if (Session.GetPlay().TryGetCooldown("stun"))
                            return;

                        Point ClientPos = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);
                        Point TargetClientPos = new Point(TargetUser.Coordinate.X, TargetUser.Coordinate.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (!TargetClient.GetRoomUser().Frozen)
                        {
                            // Paralizar
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

                                    #region Desequipar al Concito
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
                        }
                        else
                        {
                            // Desparalizar
                            if (Distance <= 1)
                            {
                                RoleplayManager.Shout(Session, "*Ayuda a " + TargetClient.GetHabbo().Username + ", dándole tiempo para recuperarse de su aturdimiento*", 37);

                                TargetClient.GetRoomUser().Frozen = false;
                                TargetClient.GetRoomUser().CanWalk = true;
                                TargetClient.GetRoomUser().ClearMovement(true);
                                return;
                            }
                            else
                            {
                                Session.SendWhisper("Debes estar más cerca de la persona.", 1);
                                return;
                            }
                        }
                        #endregion
                    }
                    else
                    {

                        // Golpear
                        #region Execute
                        RoomUser TargetUser = TargetClient.GetRoomUser();
                        if (TargetUser == null)
                        {
                            Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en esta Zona.", 1);
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
                        if (TargetClient.GetConnection().getIp() == Session.GetConnection().getIp())
                        {
                            Session.SendWhisper("¡No puedes golpear a tus propias cuentas!", 1);
                            return;
                        }

                        // New Target System
                        Session.GetPlay().Target = TargetClient.GetHabbo().Username;

                        Session.GetPlay().LastCommand = ":hit " + Habbo.Username;
                        CombatManager.GetCombatType("fist").Execute(Session, TargetClient);
                        #endregion
                    }
                }
                else
                {
                    // Disparar
                    #region Execute
                    RoomUser TargetUser = TargetClient.GetRoomUser();
                    if (TargetUser == null)
                    {
                        Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en esta Zona.", 1);
                        return;
                    }
                    if (TargetClient.GetHabbo().EscortID > 0)
                    {
                        Session.SendWhisper("¡No puedes dispararle a una persona que va siendo escoltada!", 1);
                        return;
                    }
                    if (TargetClient.GetHabbo().TaxiChofer > 0)
                    {
                        Session.SendWhisper("¡No puedes dispararle a una persona que va en Taxi!", 1);
                        return;
                    }
                    if (TargetClient.GetPlay().Cuffed)
                    {
                        Session.SendWhisper("¡No puedes dispararle a una persona esposada!", 1);
                        return;
                    }
                    if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
                    {
                        Session.SendWhisper("¡No puedes dispararle a una persona muerta!", 1);
                        return;
                    }
                    if (TargetClient.GetPlay().IsJailed)
                    {
                        Session.SendWhisper("¡No puedes dispararle a una persona encarcelada!", 1);
                        return;
                    }
                    if (TargetClient.GetConnection().getIp() == Session.GetConnection().getIp())
                    {
                        Session.SendWhisper("¡No puedes dispararle a tus propias cuentas!", 1);
                        return;
                    }
                    if (Session.GetPlay().WLife <= 0)
                    {
                        Session.SendWhisper("¡Tu arma está dañada! Busca a un Armero para que la repare.", 1);
                        return;
                    }

                    if ((Session.GetPlay().DrivingCar))
                    {
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "law") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                        {
                            Session.SendWhisper("¡No puedes hacer eso mientras vas dentro de un vehículo!", 1);
                            return;
                        }
                    }

                    #region Fuego amigo entre bandas
                    List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);
                    List<Group> EnemyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(TargetClient.GetHabbo().Id);

                    if (MyGang != null && MyGang.Count > 0)
                    {
                        if (EnemyGang != null && EnemyGang.Count > 0 && EnemyGang[0] == MyGang[0])
                        {
                            Session.SendWhisper("¡No puedes dispararle a tus compañeros de banda!", 1);
                            return;
                        }
                    }
                    #endregion

                    #region Tipo de armas

                    #endregion



                    // New Target System
                    Session.GetPlay().Target = Habbo.Username;

                    Session.GetPlay().LastCommand = ":shoot " + Habbo.Username;
                    CombatManager.GetCombatType("gun").Execute(Session, TargetClient);
                    Session.GetPlay().CooldownManager.CreateCooldown("gun", 250, 1);
                    #endregion
                }
            }
            #endregion
        }
    }
}