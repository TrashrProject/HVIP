using System;
using System.Drawing;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using Plus.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboRoleplay.Combat.Types
{
    public class Gun : ICombat
    {
        /// <summary>
        /// Executes this type of combat
        /// </summary>
        public void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = true)
        {
            if (!CanCombat(Client, TargetClient))
                return;

            #region Variables

            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = TargetClient.GetRoomUser();
            int Damage = GetDamage(Client, TargetClient);
            Weapon Weapon = Client.GetPlay().EquippedWeapon;
            Point ClientPos = RoomUser.Coordinate;
            Point TargetClientPos = TargetRoomUser.Coordinate;

            #endregion

            #region Ammo Check
            if (Client.GetPlay().GunShots >= Weapon.ClipSize)
            {
                Weapon.Reload(Client, TargetClient);
                Client.GetPlay().CooldownManager.CreateCooldown("reload", 1000, Weapon.ReloadTime);
                return;
            }
            #endregion

            #region Distance Check
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);
            bool chance = true;

            #region Nerfeo para pasajeros que disparan
            if (Client.GetPlay().DrivingInCar)
            {
                var randomNumber = new Random().Next(0, 100);
                if (randomNumber < 50)
                    chance = false;
            }
            #endregion

            if (Distance > Weapon.Range || !chance)
            {
                RoleplayManager.Shout(Client, "*Intenta dispararle a " + TargetClient.GetHabbo().Username + " pero falla*", 5);
                Client.GetPlay().GunShots++;

                //if (Client.GetPlay().Game == null)
                    Client.GetPlay().Bullets--;

                #region Sound System
                foreach (RoomUser RoomUsers in Client.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
                {
                    if (RoomUsers == null || RoomUsers.GetClient() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|shoot|" + Client.GetPlay().EquippedWeapon.Name);
                }
                #endregion
                return;
            }
            #endregion

            if (!TargetClient.GetPlay().DrivingCar && !TargetClient.GetPlay().Pasajero)
            {
                #region Target Death Procedure
                if (TargetClient.GetPlay().CurHealth - Damage <= 0)
                {
                    Client.GetPlay().ClearWebSocketDialogue();

                    string Text = Weapon.FiringText.Split(':')[1];
                    string GunName = Weapon.PublicName;

                    RoleplayManager.Shout(Client, FormatFiringText(Text, GunName, TargetClient.GetHabbo().Username, Damage, Weapon.Energy), 3);

                    Client.GetPlay().Kills++;
                    Client.GetPlay().GunKills++;
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                        Client.GetPlay().CopKills++;

                    //TargetClient.GetPlay().Deaths++; <= Se suma en el DyingTimer
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
                        TargetClient.GetPlay().CopDeaths++;

                    #region Check Gangs
                    List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                    List<Group> EnemyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(TargetClient.GetHabbo().Id);

                    if (MyGang != null && MyGang.Count > 0)
                    {
                        if (MyGang[0].BankRuptcy)
                        {
                            Client.SendWhisper("Tu banda está en bancarota y no podrás gozar de los beneficios de ella.", 1);
                        }
                        else
                        {
                            if (EnemyGang != null && EnemyGang.Count > 0 && EnemyGang[0] != MyGang[0])
                            {
                                int NewTurfsCount = 0;
                                List<Plus.HabboRoleplay.GangTurfs.GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(MyGang[0].Id);
                                if (TF != null && TF.Count > 0)
                                    NewTurfsCount = TF.Count;

                                int Bonif = NewTurfsCount * RoleplayManager.GangsTurfBonif;

                                if (Bonif > 0)
                                {
                                    if (!EnemyGang[0].BankRuptcy)
                                    {
                                        MyGang[0].Bank += (Bonif / 2);
                                        MyGang[0].SetBussines(MyGang[0].Bank, MyGang[0].Stock);
                                        EnemyGang[0].Bank -= Bonif;
                                        EnemyGang[0].SetBussines(EnemyGang[0].Bank, EnemyGang[0].Stock);

                                        MyGang[0].AddLog(Client.GetHabbo().Id, Client.GetHabbo().Username + " mata a " + TargetClient.GetHabbo().Username + ", integrante de la banda " + EnemyGang[0].Name + ", ganando $ " + String.Format("{0:N0}", (Bonif / 2)), (Bonif / 2));

                                        Client.GetHabbo().Credits += (Bonif / 2);
                                        Client.GetPlay().MoneyEarned += (Bonif / 2);
                                        Client.GetHabbo().UpdateCreditsBalance();
                                        Client.SendWhisper("¡Has ganado $ " + String.Format("{0:N0}", (Bonif / 2)) + " de bonificación por matar a un miembro de la banda " + EnemyGang[0].Name + "!", 1);

                                        TargetClient.SendWhisper("¡Tu banda pierde $ " + String.Format("{0:N0}", Bonif) + " por haber sido asesinad@ por un miembro de la banda " + MyGang[0].Name + "!", 1);
                                    }
                                    else
                                        Client.SendWhisper("¡Has matado a un miembro de la banda " + EnemyGang[0].Name + "! Pero no has ganado ninguna bonificación porque su banda está en bancarota.", 1);
                                }
                                else
                                {
                                    Client.SendWhisper("¡Has matado a un miembro de la banda " + EnemyGang[0].Name + "! Pero no has ganado ninguna bonificación porque tu banda no tienen ningún barrio controlado.", 1);
                                }

                                MyGang[0].GangKills++;
                                MyGang[0].UpdateStat("gang_kills", MyGang[0].GangKills);
                            }

                            if (EnemyGang == null || EnemyGang.Count <= 0)
                            {
                                MyGang[0].GangKills++;
                                MyGang[0].UpdateStat("gang_kills", MyGang[0].GangKills);
                            }

                            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                            {
                                MyGang[0].GangCopKills++;
                                MyGang[0].UpdateStat("gang_cop_kills", MyGang[0].GangCopKills);
                            }
                        }
                    }

                    if (EnemyGang != null && EnemyGang.Count > 0)
                    {
                        EnemyGang[0].GangDeaths++;
                        EnemyGang[0].UpdateStat("gang_deaths", EnemyGang[0].GangDeaths);
                    }
                    #endregion

                    #region Check Purge
                    if (RoleplayManager.PurgeEvent)
                    {
                        Client.GetHabbo().Credits += RoleplayManager.PurgeBonif;
                        Client.GetPlay().MoneyEarned += RoleplayManager.PurgeBonif;
                        Client.GetHabbo().UpdateCreditsBalance();
                        Client.SendWhisper("¡Has ganado $ " + String.Format("{0:N0}", RoleplayManager.PurgeBonif) + " de bonificación por matar durante la purga!", 1);
                    }
                    #endregion

                    #region Live Feed
                    foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_feedcomposer", "alert|" + Client.GetHabbo().Username + "|" + TargetClient.GetHabbo().Username + "|" + "acribilló a");
                    }
                    #endregion

                    #region Sound System
                    foreach (RoomUser RoomUsers in Client.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
                    {
                        if (RoomUsers == null || RoomUsers.GetClient() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|dead");
                    }
                    #endregion
                }
                #endregion

                #region Target Damage Procedure (Did not die)
                else
                {
                    string Text = Weapon.FiringText.Split(':')[0];
                    string GunName = Weapon.PublicName;
                    Client.GetPlay().OpenUsersDialogue(TargetClient);
                    TargetClient.GetPlay().OpenUsersDialogue(Client);

                    //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.SHOOT_USER);
                    RoleplayManager.Shout(Client, FormatFiringText(Text, GunName, TargetClient.GetHabbo().Username, Damage, Weapon.Energy), 3);
                }
                #endregion

                #region Sound System
                foreach (RoomUser RoomUsers in Client.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
                {
                    if (RoomUsers == null || RoomUsers.GetClient() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|bullet");
                }
                #endregion

                //TargetClient.GetPlay().CurHealth -= Damage;
                if (TargetClient.GetPlay().Armor <= 0)
                {
                    if (TargetClient.GetPlay().CurHealth - Damage <= 0)
                        TargetClient.GetPlay().CurHealth = 0;
                    else
                        TargetClient.GetPlay().CurHealth -= Damage;
                }
                else // Si tiene chaleco...
                {
                    if (TargetClient.GetPlay().Armor - Damage <= 0)
                        TargetClient.GetPlay().Armor = 0;
                    else
                        TargetClient.GetPlay().Armor -= Damage;
                }
            }
            else
            {
                Client.GetPlay().OpenUsersDialogue(TargetClient);
                TargetClient.GetPlay().OpenUsersDialogue(Client);

                RoleplayManager.Shout(Client, "*Dispara con su "+ Weapon.PublicName +" al auto que " + TargetClient.GetHabbo().Username + " concuce causandole " + Damage + " de daño*", 5);

                // Hace daño al auto que conduce
                TargetClient.GetPlay().CarLife -= Damage;
            }
            //if (Client.GetPlay().Game == null)
            Client.GetPlay().Bullets--;

            //if (Client.GetPlay().Game == null)
              //  Client.GetPlay().CurEnergy -= Weapon.Energy;

            Client.GetPlay().GunShots++;

            // if (!Client.GetPlay().WantedFor.Contains("attempt assault"))
            //   Client.GetPlay().WantedFor = Client.GetPlay().WantedFor + "attempt assault, ";

            #region Sound System
            foreach (RoomUser RoomUsers in Client.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUsers == null || RoomUsers.GetClient() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|shoot|" + Client.GetPlay().EquippedWeapon.Name);
            }
            #endregion
        }

        /// <summary>
        /// Executes this type of combat on a Bot
        /// </summary>
        public void ExecuteBot(GameClient Client)
        {

        }

        /// <summary>
        /// Checks if a client can complete this action
        /// </summary>
        public bool CanCombat(GameClient Client, GameClient TargetClient)
        {
            #region Variables
            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = TargetClient.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            #endregion

            #region Cooldown Conditions
            if (Client.GetPlay().TryGetCooldown("reload", false))
                return false;

            if (Client.GetPlay().TryGetCooldown("gun", false))
                return false;
            #endregion

            #region Main Conditions
            Weapon Weapon = Client.GetPlay().EquippedWeapon;
            Room Room = null;

            if (Weapon == null)
                return false;

            if (Client.GetHabbo().CurrentRoomId > 0)
                Room = Client.GetHabbo().CurrentRoom;

            if (Room != null && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
            {
                if (Room.SafeZoneEnabled && !RoleplayManager.PurgeEvent)
                {
                    Client.SendWhisper("¡No se puede disparar en esta Zona!", 1);
                    return false;
                }
                else if (Room.IsHospital || (Room.Group != null && Room.Group.GType == 2 && Room.Group.Name.Contains("Hospital")))
                {
                    Client.SendWhisper("No se puede golpear dentro del hospital.", 1);
                    return false;
                }
            }

            if (TargetRoomUser == null)
            {
                Client.SendWhisper("¡Esa persona no se encuentra aquí!", 1);
                return false;
            }

            //if (Client.GetPlay().Game == null)
            //{
            if (RoleplayManager.LevelDifference)
            {
                if (!Room.TurfEnabled)
                {
                    int LevelDifference = Math.Abs(Client.GetPlay().Level - TargetClient.GetPlay().Level);

                    if (LevelDifference > 8)
                    {
                        Client.SendWhisper("((¡No puedes dispararle a alguien con 8 niveles de diferencia mayor a ti!))", 1);
                        return false;
                    }
                }
            }
            //}
            /*
            else
            {
                if (Client.GetPlay().Game.GetGameMode() != Games.GameMode.SoloQueueGuns)
                {
                    Client.SendWhisper("You cannot shoot inside a " + Client.GetPlay().Game.GetName() + " event!", 1);
                    return false;
                }

                if (!Client.GetPlay().Game.HasGameStarted())
                {
                    Client.SendWhisper("The event hasn't even started yet!", 1);
                    return false;
                }

                if (TargetClient.GetPlay().Game != Client.GetPlay().Game)
                {
                    Client.SendWhisper("Your target is not part of this event!", 1);
                    return false;
                }
            }
            */

            if (Weapon == null)
            { 
                Client.SendWhisper("No tienes ningún arma Equipada.", 1);
                return false; 
            }
            #endregion

            #region User Conditions

            if (RoomUser == null)
                return false;

            if (RoomUser.Frozen)
            {
                Client.SendWhisper("¡No puedes disparar en tu estado actual!", 1);
                return false;
            }

            if (RoomUser.IsAsleep)
            {
                Client.SendWhisper("¡No puedes disparar mientras estás ausente!", 1);
                return false;
            }

            if (Client.GetPlay().IsDead)
            {
                Client.SendWhisper("¡No puedes disparar mientras estás muert@!", 1);
                return false;
            }

            if (Client.GetPlay().IsWorking && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))// CHECK THIS SHIT -> != Police
            {
                Client.SendWhisper("¡No puedes disparar mientras estás trabajando!", 1);
                return false;
            }
            /*
            if (Client.GetPlay().StaffOnDuty || Client.GetPlay().AmbassadorOnDuty)
            {
                Client.SendWhisper("You cannot shoot someone while you are on-duty!", 1);
                return false;
            }
            */
            if (Client.GetPlay().Cuffed)
            {
                Client.SendWhisper("¡No puedes disparar mientras estás esposad@!", 1);
                return false;
            }

            //if (Client.GetPlay().DrivingCar)
            //{
            //Client.SendWhisper("Please stop driving your vehicle to shoot your gun!", 1);
            //  return false;
            //}

            if (Client.GetHabbo().TaxiChofer > 0)
                return false;

            if (Client.GetPlay().DrivingCar)
            {
                Client.SendWhisper("No puedes disparar mientras vas dentro de un vehículo.", 1);
                return false;
            }

            if (Client.GetPlay().IsJailed)
            {
                Client.SendWhisper("¡No puedes disparar mientras estás encarcelad@!", 1);
                return false;
            }

            if (Client.GetPlay().GodMode)
            {
                Client.SendWhisper("¡No puedes disparar mientras estás con inmunidad!", 1);
                return false;
            }

            if (Client.GetPlay().IsNoob)
            {
                if (Client.GetPlay().IsNoob)
                {
                    if (!Client.GetPlay().NoobWarned)
                    {
                        Client.SendWhisper("((Actualmente tienes activa tu protección de inmunidad de Usuario nuevo. Si cometes delitos perderás esa protección antes de tiempo. (Advertencia: 1/2)  ))", 1);
                        Client.GetPlay().NoobWarned = true;
                        return false;
                    }
                    else if (!Client.GetPlay().NoobWarned2)
                    {
                        Client.SendWhisper("((Actualmente tienes activa tu protección de inmunidad de Usuario nuevo. Si cometes delitos perderás esa protección antes de tiempo. (Advertencia: 2/2)  ))", 1);
                        Client.GetPlay().NoobWarned2 = true;
                        return false;
                    }
                    else
                    {
                        Client.SendWhisper("((¡Tu protección de inmunidad se ha terminado!))", 1);

                        if (Client.GetPlay().TimerManager != null && Client.GetPlay().TimerManager.ActiveTimers != null)
                        {
                            if (Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("noob"))
                                Client.GetPlay().TimerManager.ActiveTimers["noob"].EndTimer();
                        }

                        Client.GetPlay().IsNoob = false;
                        Client.GetPlay().NoobTimeLeft = 0;
                        return true;
                    }
                }
            }
            /* OFF para auto recarga
            if (Client.GetPlay().Bullets <= 0)
            {
                Client.SendWhisper("¡No puedes disparar debido a que no tienes balas!", 1);
                return false;
            }
            */
            /*
            if (Client.GetPlay().CurEnergy <= 0 && Client.GetPlay().Game == null)
            {
                Client.SendWhisper("You cannot complete this action as you ran out of energy!", 1);
                return false;
            }
            */
            #endregion

            #region Target Conditions
            if (TargetClient == Client)
            {
                Client.SendWhisper("¡No puedes dispararte a ti mism@!", 1);
                return false;
            }

            if (TargetClient.GetRoomUser().RoomId != Client.GetRoomUser().RoomId)
                return false;

            if (TargetClient.GetPlay().IsDead)
            {
                Client.SendWhisper("¡No puedes dispararle a una persona muerta!", 1);
                return false;
            }

            if (TargetClient.GetPlay().IsJailed)
            {
                Client.SendWhisper("¡No puedes dispararle a una persona encarcelada!", 1);
                return false;
            }
            /*
            if (TargetClient.GetPlay().StaffOnDuty)
            {
                Client.SendWhisper("You cannot shoot a staff member who is on duty!", 1);
                return false;
            }

            if (TargetClient.GetPlay().AmbassadorOnDuty)
            {
                Client.SendWhisper("You cannot shoot an ambassador who is on duty!", 1);
                return false;
            }
            */
            if (TargetClient.GetRoomUser().IsAsleep)
            {
                Client.SendWhisper("¡No puedes dispararle a una persona ausente!", 1);
                return false;
            }

            if (TargetClient.GetPlay().IsNoob)
            {
                Client.SendWhisper("((Esta persona tiene protección de inmunidad de usuario nuevo. ¡Mejor ayudale a empezar! :) ))", 1);
                return false;
            }

            if (TargetClient.GetPlay().GodMode)
            {
                Client.SendWhisper("((Esta persona tiene protección de inmunidad de usuario.))", 1);
                return false;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Client.SendWhisper("((Esta persona se encuentra en modo pasivo.))", 1);
                return false;
            }
            if (TargetClient.GetPlay().HospReanim > 0)
            {
                Client.SendWhisper("No puedes hacerle daño a un médico en servicio.", 1);
                return false;
            }
            if (TargetClient.GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "botiquin"))
            {
                Client.SendWhisper("No puedes hacerle daño a un médico en servicio.", 1);
                return false;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Client.SendWhisper("No puedes hacerle daño a una persona dentro de un taxi.", 1);
                return false;
            }
            if (TargetClient.LoggingOut || TargetClient.GetPlay().IsDisconnecting)
            {
                Client.SendWhisper("((Esta persona se encuentra desconectándose.))", 1);
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Gets the damage
        /// </summary>
        private int GetDamage(GameClient Client, GameClient TargetClient)
        {
            CryptoRandom Randomizer = new CryptoRandom();
            Weapon Weapon = Client.GetPlay().EquippedWeapon;

            int MinDamage = Weapon.MinDamage;
            int MaxDamage = Weapon.MaxDamage;

            int Damage = Randomizer.Next(MinDamage, MaxDamage);

            if (TargetClient.GetHabbo().Username == "Jeihden")
                return 1;

            /*
            if (Client.GetPlay().Class.ToLower() == "gunner")
                Damage += Randomizer.Next(1, 3);

            if (Client.GetPlay().GangId > 1000)
            {
                if (GroupManager.HasGangCommand(Client, "gunner"))
                {
                    if (RoleplayManager.GenerateRoom(Client.GetHabbo().CurrentRoomId, false).TurfEnabled || GroupManager.HasJobCommand(TargetClient, "guide"))
                        Damage += Randomizer.Next(0, 2);
                }
            }
            */
            return Damage;
        }

        /// <summary>
        /// Formats the string
        /// </summary>
        private string FormatFiringText(string Text, string GunName, string TargetName, int Damage, int Energy)
        {
            Text = Text.Replace("[NAME]", GunName);
            Text = Text.Replace("[TARGET]", TargetName);
            Text = Text.Replace("[DAMAGE]", Damage.ToString());
            Text = Text.Replace("[ENERGY]", Energy.ToString());

            return Text;
        }

        /// <summary>
        /// calculates the amount of exp to give to the client
        /// </summary>
        public int GetEXP(GameClient Client, GameClient TargetClient)
        {
            CryptoRandom Random = new CryptoRandom();
            int LevelDifference = Math.Abs(Client.GetPlay().Level - TargetClient.GetPlay().Level);
            int Amount;
            int Bonus;

            if (LevelDifference > 8)
            {
                Amount = 0;
                Bonus = 0;
            }
            else
            {
                if (TargetClient.GetPlay().Level > Client.GetPlay().Level)
                    Bonus = (10 * (LevelDifference + 1)) + LevelDifference * 2;
                else if (TargetClient.GetPlay().Level == Client.GetPlay().Level)
                    Bonus = (10 * 2) + 3;
                else if (TargetClient.GetPlay().Level < Client.GetPlay().Level)
                    Bonus = 10;
                else
                    Bonus = 2 * LevelDifference;

                Amount = Random.Next(10, 10 + (LevelDifference + 5));
            }

            return (Amount + Bonus + 15);
        }

        /// <summary>
        /// Gets the coins from the users dead body
        /// </summary>
        public int GetCoins(GameClient TargetClient)
        {
            return 0;
        }

        /// <summary>
        /// Gets the rewards from the dead body
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="TargetClient"></param>
        /// <param name="Bot"></param>
        public void GetRewards(GameClient Client, GameClient TargetClient)
        {
            
        }

    }
}
