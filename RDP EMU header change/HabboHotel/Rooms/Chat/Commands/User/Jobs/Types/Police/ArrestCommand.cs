using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ArrestCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_arrest"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Arresta a una persona según el nivel de búsqueda."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            string MyCity = Room.City;
            int JailRID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJail(MyCity, out PlayRoom Data);//prision de la cd.
            int PolStationID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetPolStation(MyCity, out PlayRoom Data2);//prision

            if (Session.GetHabbo().CurrentRoomId != JailRID && Session.GetHabbo().CurrentRoomId != PolStationID)
            {
                Session.SendWhisper("Debes llevar a la persona dentro de la Prisión o Comisaría para encarcelarla. ((Usa :escoltar [nombre] para llevarlo hasta allá)).", 1);
                return;
            }
            
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

            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if(TargetClient.GetPlay().IsSanc)
            {
                Session.SendWhisper("No puedes encarcelar a esa persona porque se encuentra sancionada.", 1);
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

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "arrest") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (Session.GetHabbo().Escorting != TargetClient.GetHabbo().Id)
            {
                Session.SendWhisper("Debes tener escoltando a tu convicto a arrestar.", 1);
                return;
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes arrestar a una persona muerta!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes arrestar a una persona encarcelada!", 1);
                return;
            }

            if (!TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("¡Primero debes esposar a la persona!", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes arrestar a un usuario ausente!", 1);
                return;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona que está en modo pasivo!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("arrest", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id) ? RoleplayManager.WantedList[TargetClient.GetHabbo().Id] : null;
            int WantedTime = Wanted == null ? RoleplayManager.DefaultJailTime : Wanted.WantedLevel * RoleplayManager.StarsJailTime;
            int ReduceTime = 0;
            string ExtraMsg = "";

            if(TargetClient.GetHabbo().VIPRank == 1)
            {
                ExtraMsg = " con una reducción del 10% de tiempo por ser VIP.";
                ReduceTime = WantedTime / 10;
            }
            if (TargetClient.GetHabbo().VIPRank == 2)
            {
                ExtraMsg = " con una reducción del 25% de tiempo por ser VIP2.";
                ReduceTime = WantedTime / 4;
            }

            if (Distance <= 1)
            {
                if (TargetClient.GetPlay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(TargetClient);
                    TargetClient.GetPlay().IsWorking = false;
                    TargetClient.GetHabbo().Poof();
                }

                RoleplayManager.Shout(Session, "*Libera las manos de " + TargetClient.GetHabbo().Username + " y lo encierra en una celda durante " + WantedTime + " minuto(s)" + ExtraMsg,37);
                TargetClient.GetPlay().Cuffed = false;
                TargetClient.GetRoomUser().ApplyEffect(0);

                if (TargetClient.GetHabbo().Look.Contains("lg-78322"))
                {
                    if (!TargetClient.GetPlay().WantedFor.Contains("exposición indecente"))
                        TargetClient.GetPlay().WantedFor = TargetClient.GetPlay().WantedFor + "exposicion indecente, ";
                }

                if (TargetUser.Frozen)
                    TargetUser.Frozen = false;

                if (!TargetClient.GetPlay().IsJailed)
                {
                    TargetClient.GetPlay().IsJailed = true;
                    TargetClient.GetPlay().JailedTimeLeft = WantedTime - ReduceTime;
                    TargetClient.GetPlay().TimerManager.CreateTimer("jail", 1000, false);
                }                

                if (TargetClient.GetHabbo().CurrentRoomId == JailRID)
                {
                    RoleplayManager.GetLookAndMotto(TargetClient);
                    RoleplayManager.SpawnBeds(TargetClient, "bed_silo_one");
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Has sido arrestad@ por " + Session.GetHabbo().Username + " por " + WantedTime + " minuto(s)" + ExtraMsg));
                }
                else
                {
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Has sido arrestad@ por " + Session.GetHabbo().Username + " por " + WantedTime + " minuto(s)" + ExtraMsg));
                    RoleplayManager.SendUser(TargetClient, JailRID);
                }
                if (RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    Wanted Junk;
                    RoleplayManager.WantedList.TryRemove(TargetClient.GetHabbo().Id, out Junk);
                }

                PlusEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] ¡" + TargetClient.GetHabbo().Username + " ha sido arrestad@ por " + Session.GetHabbo().Username + "! Buen trabajo chicos.");
                //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Arrests", 1);
                Session.GetPlay().Arrests++;
                //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Arrested", 1);
                TargetClient.GetPlay().Arrested++;
                if(TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().CanWalk = true;

                // UnEscort
                if (TargetClient.GetRoomUser() != null)
                {
                    TargetClient.GetRoomUser().ClearMovement(true);
                    TargetClient.GetRoomUser().CanWalk = true;
                }
                TargetClient.GetHabbo().EscortID = 0;
                Session.GetHabbo().Escorting = 0;

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

                #region Business Police
                Room GenRoom = RoleplayManager.GenerateRoom(PolStationID);
                if (GenRoom != null && GenRoom.Group != null)
                {
                    GenRoom.Group.Spend += 10;
                    GenRoom.Group.UpdateSpend(10);
                }
                #endregion

                Session.GetPlay().CooldownManager.CreateCooldown("arrest", 1000, 3);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona para hacer eso.", 1);
                return;
            }
            #endregion
        }
        
    }
}