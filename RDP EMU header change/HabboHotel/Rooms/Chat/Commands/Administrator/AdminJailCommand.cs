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
    class AdminJailCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_jail"; }
        }

        public string Parameters
        {
            get { return "%user% %stars%"; }
        }

        public string Description
        {
            get { return "Encarcela a una persona colocando su nivel de Búsqueda."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Debes ingresar el nombre de usuario y número de estrellas [1-5] ó [0 = 2 minutos].", 1);
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
                Session.SendWhisper("No puedes encarcelar a esa persona porque se encuentra sancionada.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;
            if (Session != TargetClient && TargetClient.GetHabbo().Rank >= 6)
            {
                Session.SendWhisper("No puedes hacerle eso a un todo poderoso Dios Developer.", 1);
                return;
            }
            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡Esa persona ya está encarcelada!", 1);
                return;
            }

            int Stars;
            if (!int.TryParse(Params[2], out Stars))
            {
                Session.SendWhisper("Ingresa un número de estrellas válido. [1-5] ó [0 = 2 minutos].", 1);
                return;
            }
            else
            {
                if (Stars < 0 || Stars > 5)
                {
                    Session.SendWhisper("Ingresa un número de estrellas válido. [1-5] ó [0 = 2 minutos].", 1);
                    return;
                }
            }

            int WantedTime = Stars * RoleplayManager.StarsJailTime;

            if (WantedTime <= 0)
                WantedTime = RoleplayManager.Star0Time; // 2 minutos en estrella 0.

            int ReduceTime = 0;
            string ExtraMsg = "";

            if (TargetClient.GetHabbo().VIPRank == 1)
            {
                ExtraMsg = " con una reducción del 10% de tiempo por ser VIP.";
                ReduceTime = WantedTime / 10;
            }
            if (TargetClient.GetHabbo().VIPRank == 2)
            {
                ExtraMsg = " con una reducción del 25% de tiempo por ser VIP2.";
                ReduceTime = WantedTime / 4;
            }

            TargetClient.GetPlay().WantedLevel = Stars;
            if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
            {
                TargetClient.GetPlay().IsDead = false;
                TargetClient.GetPlay().IsDying = false;
                TargetClient.GetPlay().ReplenishStats(true);
                TargetClient.GetHabbo().Poof();
                // Refrescamos WS
                TargetClient.GetPlay().UpdateInteractingUserDialogues();
                TargetClient.GetPlay().RefreshStatDialogue();
            }

            if (TargetClient.GetPlay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(TargetClient);
                TargetClient.GetPlay().IsWorking = false;
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetPlay().Cuffed)
                TargetClient.GetPlay().Cuffed = false;

            if (!TargetClient.GetRoomUser().CanWalk)
                TargetClient.GetRoomUser().CanWalk = true;

            if (TargetRoomUser.Frozen)
                TargetRoomUser.Frozen = false;

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

            RoleplayManager.CheckEscort(TargetClient);

            RoleplayManager.CheckOnCar(TargetClient);

            TargetClient.GetPlay().IsJailed = true;
            TargetClient.GetPlay().JailedTimeLeft = WantedTime - ReduceTime;
            TargetClient.GetPlay().TimerManager.CreateTimer("jail", 1000, false);

            string MyCity = Room.City;
            int JailRID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJail(MyCity, out PlayRoom Data);//prision de la cd.

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
                PlusEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] " + TargetClient.GetHabbo().Username + " ha arrestado a " + Session.GetHabbo().Username + ".");
            }
            Session.GetPlay().Arrests++;
            TargetClient.GetPlay().Arrested++;
            
            RoleplayManager.Shout(Session, "*Encarcela a " + TargetClient.GetHabbo().Username + " durante " + WantedTime + " minuto(s)" + ExtraMsg + "*", 23);
            return;
        }
    }
}