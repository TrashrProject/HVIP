using System;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.Utilities;

namespace Plus.HabboRoleplay.Combat.Types
{
    public class Fist : ICombat
    {
        /// <summary>
        /// Executes this type of combat
        /// </summary>
        public void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = true)
        {
            // Stats WebSocket
            Client.GetPlay().OpenUsersDialogue(TargetClient);

            RoomUser User;
            if (HitClosest && TargetClient == null)
            {
                if (!this.TryGetClosestTarget(Client, out User))
                    return;

                if (User == null)
                    return;

                if (User.IsBot)
                {
                    return;
                }
                else
                    TargetClient = User.GetClient();
            }

            if (!this.CanCombat(Client, TargetClient))
                return;

            int Damage = this.GetDamage(Client, TargetClient);

            if (!TargetClient.GetPlay().DrivingCar && !TargetClient.GetPlay().Pasajero)
            {
                // If the user is about to die and the user attacked themself
                if ((TargetClient.GetPlay().CurHealth - Damage) <= 0 && TargetClient == Client)
                {
                    TargetClient.SendWhisper("¡No puedes matarte a ti mism@!", 1);
                    return;
                }

                // If about to die
                if (TargetClient.GetPlay().CurHealth - Damage <= 0)
                {
                    Client.GetPlay().ClearWebSocketDialogue();

                    RoleplayManager.Shout(Client, "*Golpea con fuerza a " + TargetClient.GetHabbo().Username + " causandole " + Damage + " de daño*", 5);
                    RoleplayManager.Shout(Client, "*Noquea a " + TargetClient.GetHabbo().Username + " dejandol@ inconsciente*", 5);

                    /*
                    int Amount = this.GetCoins(TargetClient);
                    //this.GetRewards(Client, TargetClient, null);

                    Client.GetHabbo().Credits += Amount;
                    Client.GetHabbo().UpdateCreditsBalance();

                    TargetClient.GetHabbo().Credits -= Amount;
                    TargetClient.GetHabbo().UpdateCreditsBalance();

                    if (Amount > 0)
                    {
                        RoleplayManager.Shout(Client, "*Swings at " + TargetClient.GetHabbo().Username + ", causing " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Swings at " + TargetClient.GetHabbo().Username + ", knocking them out and stealing $" + String.Format("{0:N0}", Amount) + " from their wallet*", 6);
                    }
                    else
                    {
                        RoleplayManager.Shout(Client, "*Swings at " + TargetClient.GetHabbo().Username + ", causing " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Swings at " + TargetClient.GetHabbo().Username + ", knocking them out*", 6);
                    }
                    */

                    //BountyManager.CheckBounty(Client, TargetClient.GetHabbo().Id);

                    if ((TargetClient.GetPlay().CurHealth - Damage) <= 0)
                        TargetClient.GetPlay().CurHealth = 0;

                    Client.GetPlay().Kills++;
                    Client.GetPlay().HitKills++;
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

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_feedcomposer", "alert|" + Client.GetHabbo().Username + "|" + TargetClient.GetHabbo().Username + "|" + "asesinó a");
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
                else
                {
                    RoleplayManager.Shout(Client, "*Golpea a " + TargetClient.GetHabbo().Username + " causandole " + Damage + " de daño*", 5);

                    // Stats WebSocket
                    Client.GetPlay().OpenUsersDialogue(TargetClient);
                    TargetClient.GetPlay().OpenUsersDialogue(Client);

                    #region Sound System
                    foreach (RoomUser RoomUsers in Client.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
                    {
                        if (RoomUsers == null || RoomUsers.GetClient() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|punch");
                    }
                    #endregion
                }

                if (TargetClient.GetPlay().CurHealth - Damage <= 0)
                    TargetClient.GetPlay().CurHealth = 0;
                else
                    TargetClient.GetPlay().CurHealth -= Damage;
            }
            else
            {
                RoleplayManager.Shout(Client, "*Golpea el auto que " + TargetClient.GetHabbo().Username + " concuce causandole " + Damage + " de daño*", 5);

                // Stats WebSocket
                Client.GetPlay().OpenUsersDialogue(TargetClient);
                TargetClient.GetPlay().OpenUsersDialogue(Client);

                // Hace daño al auto que conduce
                TargetClient.GetPlay().CarLife -= Damage;
            }

            Client.GetPlay().CooldownManager.CreateCooldown("fist", 1000, RoleplayManager.DefaultHitCooldown);
            Client.GetPlay().Punches++;
        }
        

        /// <summary>
        /// Checks if a client can complete this action
        /// </summary>
        public bool CanCombat(GameClient Client, GameClient TargetClient)
        {
            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = TargetClient.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);

            if (RoomUser == null || TargetRoomUser == null)
                return false;
            
            Point ClientPos = RoomUser.Coordinate;
            Point TargetClientPos = TargetRoomUser.Coordinate;

            #region Main Conditions
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            Room Room = null;

            if (Client.GetHabbo().CurrentRoomId > 0)
                Room = Client.GetHabbo().CurrentRoom;

            if (Room != null && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
            {
                if (Room.SafeZoneEnabled && !RoleplayManager.PurgeEvent)
                {
                    Client.SendWhisper("No se puede golpear en esta zona.", 1);
                    return false;
                }                
                else if(Room.IsHospital || (Room.Group != null && Room.Group.GType == 2 && Room.Group.Name.Contains("Hospital")))
                {
                    Client.SendWhisper("No se puede golpear dentro del hospital.", 1);
                    return false;
                }
            }

            if (TargetRoomUser == null)
            {
                Client.SendWhisper("Esa persona no se encuentra aquí.", 1);
                return false;
            }


            if (RoleplayManager.LevelDifference)
            {
                if (!Room.TurfEnabled)
                {
                    int TargetLevel = TargetClient.GetPlay().Level;
                    int LevelDifference = Math.Abs(Client.GetPlay().Level - TargetLevel);

                    if (LevelDifference > 8)
                    {
                        Client.SendWhisper("((No puedes golpear a una persona con 8 niveles de diferencia tuya.))", 1);
                        return false;
                    }

                }
            }
            if (Client.GetPlay().GodMode)
            {
                Client.SendWhisper("¡No puedes golpear mientras estás con inmunidad!", 1);
                return false;
            }
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
                    Client.SendWhisper("((Actualmente tienes activa tu  protección de inmunidad de Usuario nuevo. Si cometes delitos perderás esa protección antes de tiempo. (Advertencia: 2/2)  ))", 1);
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

            if (Client.GetHabbo().TaxiChofer > 0)
                return false;

            if (Client.GetPlay().TryGetCooldown("fist"))
                return false;

            #endregion

            #region Status Conditions
            if (RoomUser.Frozen)
            {
                Client.SendWhisper("No puedes golpear en tu estado actual.", 1);
                return false;
            }

            if (RoomUser.IsAsleep)
            {
                Client.SendWhisper("No puedes golpear mientras estás ausente.", 1);
                return false;
            }

            if (Client.GetPlay().IsDead)
            { 
              Client.SendWhisper("No puedes golpear mientras estás muert@.", 1);
              return false;
            }

            if (Client.GetPlay().IsJailed)
            {
                Client.SendWhisper("No puedes golpear mientras estás encarcelad@.", 1);
                return false;
            }

            if (Client.GetPlay().IsWorking && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
            {
                Client.SendWhisper("No puedes golpear mientras estás trabajando.", 1);
                return false;
            }

            if (Client.GetPlay().PassiveMode)
            {
                Client.SendWhisper("No puedes agredir en modo pasivo.", 1);
                return false;
            }

            if(Client.GetPlay().DrivingCar || Client.GetPlay().DrivingInCar)
            {
                Client.SendWhisper("No puedes agredir mientras vas dentro de un vehículo.", 1);
                return false;
            }

            if (TargetClient != null)
            {
                if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
                {
                    Client.SendWhisper("No puedes golpear a una persona muerta.", 1);
                    return false;
                }

                if (TargetClient.GetPlay().IsJailed)
                {
                    Client.SendWhisper("No puedes golpear a una persona encarcelada.", 1);
                    return false;
                }
                
                if (TargetClient == Client)
                {
                    Client.SendWhisper("No puedes golpearte a ti mism@.", 1);
                    return false;
                }

                if (TargetClient.GetConnection().getIp() == Client.GetConnection().getIp())
                {
                    Client.SendWhisper("No puedes golpear a tus multiples cuentas registradas.", 1);
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
                if(TargetClient.GetHabbo().TaxiChofer > 0)
                {
                    Client.SendWhisper("No puedes hacerle daño a una persona dentro de un taxi.", 1);
                    return false;
                }
                if (TargetClient.LoggingOut || TargetClient.GetPlay().IsDisconnecting)
                {
                    Client.SendWhisper("((Esta persona se encuentra desconectándose.))", 1);
                    return false;
                }
            }
            /*
            if (Client.GetPlay().CurEnergy <= 0 && Client.GetPlay().Game == null)
            {
                Client.SendWhisper("You have run out of energy to hit someone!", 1);
                return false;
            }
            */
            if (Client.GetPlay().Cuffed)
            {
                Client.SendWhisper("No puedes golpear mientras estás esposad@.", 1);
                return false;
            }
            /*
            if (Client.GetPlay().DrivingCar)
            {
                Client.SendWhisper("Please stop driving your vehicle to fight with your fists!", 1);
                return false;
            }
            */
            if (TargetRoomUser.IsAsleep)
            {
                Client.SendWhisper("No puedes golpear a una persona ausente.", 1);
                return false;
            }
            #endregion

            #region Distance

            if (Distance > 1)
            {
                RoleplayManager.Shout(Client, "*Intenta golpear a " + TargetClient.GetHabbo().Username + " pero falla*", 5);
                Client.GetPlay().CooldownManager.CreateCooldown("fist", 1000, RoleplayManager.DefaultHitCooldown);
                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// Selects the closest person to the client
        /// </summary>
        public bool TryGetClosestTarget(GameClient Client, out RoomUser Target)
        {
            Target = null;

            if (Client.GetRoomUser() == null)
                return false;

            if (Client.GetRoomUser().RoomId <= 0)
                return false;

            var Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);

            if (Room == null)
                return false;

            if (Room.GetRoomUserManager().GetRoomUsers().Count <= 1)
            {
                Client.SendWhisper("No hay nadie cerca de ti a quien puedas golpear.", 1);
                return false;
            }

            var Point = new Point(Client.GetRoomUser().Coordinate.X, Client.GetRoomUser().Coordinate.Y);

            ConcurrentDictionary<RoomUser, double> PossibleUsers = new ConcurrentDictionary<RoomUser, double>();
            lock (Room.GetRoomUserManager().GetRoomUsers())
            {
                foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    

                    if (User == Client.GetRoomUser())
                        continue;

                    Point TargetPoint = new Point(User.Coordinate.X, User.Coordinate.Y);
                    double Distance = RoleplayManager.GetDistanceBetweenPoints2D(Point, TargetPoint);

                    
                    if (!PossibleUsers.ContainsKey(User))
                        PossibleUsers.TryAdd(User, Distance);
                }
            }

            var OrderedUsers = PossibleUsers.OrderBy(x => x.Value);

            if (OrderedUsers.ToList().Count < 1)
                return false;

            Target = OrderedUsers.FirstOrDefault().Key;

            if (Target != null)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the damage
        /// </summary>
        private int GetDamage(GameClient Client, GameClient TargetClient)
        {
            //if (Client.GetHabbo().Username == "Jeihden" || Client.GetHabbo().Username == "Tester")
              //  return 50;

            CryptoRandom Randomizer = new CryptoRandom();
            
            int Strength = Client.GetPlay().Strength;
            int MinDamage = (Strength - 6) <= 0 ? 5 : (Strength - 6);
            int MaxDamage = Strength + 6;

            // Lucky shot?
            if (Randomizer.Next(0, 100) < 12)
            {
                MinDamage = Strength + 12;
                MaxDamage = MinDamage + 3;
            }

            int Damage = Randomizer.Next(MinDamage, MaxDamage);

            /*
            if (Client.GetPlay().GangId > 1000)
            {
                if (GroupManager.HasGangCommand(Client, "fighter"))
                {
                    if (RoleplayManager.GenerateRoom(Client.GetHabbo().CurrentRoomId, false).TurfEnabled || GroupManager.HasJobCommand(TargetClient, "guide"))
                        Damage += Randomizer.Next(1, 3);
                }
            }
            
            if (Client.GetPlay().HighOffWeed)
                Damage += Randomizer.Next(1, 3);
            */
            //if (Client.GetHabbo().Rank >= 9)
              //  Damage = 50;
            return Damage;
        }

        /// <summary>
        /// Gets the coins from the users dead body
        /// </summary>
        public int GetCoins(GameClient TargetClient)
        {
            if (TargetClient != null && TargetClient.GetHabbo() != null)
            {
                if (TargetClient.GetHabbo().VIPRank > 1)
                    return 0;

                if (TargetClient.GetHabbo().Credits < 3)
                    return 0;
            }
            

            return TargetClient.GetHabbo().Credits / 3;
        }

        /// <summary>
        /// calculates the amount of exp to give to the client
        /// </summary>
        public int GetEXP(GameClient Client, GameClient TargetClient)
        {
            int TargetLevel = TargetClient.GetPlay().Level;

            CryptoRandom Random = new CryptoRandom();
            int LevelDifference = Math.Abs(Client.GetPlay().Level - TargetLevel);
            int Amount;
            int Bonus;

            if (LevelDifference > 8)
            {
                Amount = 0;
                Bonus = 0;
            }
            else
            {
                if (TargetLevel > Client.GetPlay().Level)
                    Bonus = (10 * (LevelDifference + 1)) + LevelDifference * 2 + 5;
                else if (TargetLevel == Client.GetPlay().Level)
                    Bonus = (10 * 2) + 3 + 5;
                else if (TargetLevel < Client.GetPlay().Level)
                    Bonus = 10 + 5;
                else
                    Bonus = 2 * LevelDifference + 5;

                Amount = Random.Next(20, 20 + (LevelDifference + 9));
            }

            return (Amount + Bonus + 18);
        }

        /// <summary>
        /// Gets the rewards from the dead body
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="TargetClient"></param>
        /// <param name="Bot"></param>
        public void GetRewards(GameClient Client, GameClient TargetClient)
        {
            /*
            if (Client.GetPlay().LastKilled != TargetClient.GetHabbo().Id)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Death", 1);

                Client.GetPlay().LastKilled = TargetClient.GetHabbo().Id;
                Client.GetPlay().Kills++;
                Client.GetPlay().HitKills++;

                if (GroupManager.HasJobCommand(TargetClient, "guide") && TargetClient.GetPlay().IsWorking)
                    TargetClient.GetPlay().CopDeaths++;
                else
                    TargetClient.GetPlay().Deaths++;

                if (!Client.GetPlay().WantedFor.Contains("murder"))
                    Client.GetPlay().WantedFor = Client.GetPlay().WantedFor + "murder, ";

                CryptoRandom Random = new CryptoRandom();
                int Multiplier = 1;

                int Chance = Random.Next(1, 101);

                if (Chance <= 16)
                {
                    if (Chance <= 8)
                        Multiplier = 3;
                    else
                        Multiplier = 2;
                }

                LevelManager.AddLevelEXP(Client, GetEXP(Client, TargetClient) * Multiplier);

                Group Gang = GroupManager.GetGang(Client.GetPlay().GangId);
                Group TarGetGang = GroupManager.GetGang(TargetClient.GetPlay().GangId);

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (Gang != null)
                    {
                        if (Gang.Id > 1000)
                        {
                            int ScoreIncrease = Random.Next(1, 11);

                            Gang.GangKills++;
                            Gang.GangScore += ScoreIncrease;

                            dbClient.RunQuery("UPDATE `rp_gangs` SET `gang_kills` = '" + Gang.GangKills + "', `gang_score` = '" + Gang.GangScore + "' WHERE `id` = '" + Gang.Id + "'");
                        }
                    }
                    if (TarGetGang != null)
                    {
                        if (TarGetGang.Id > 1000)
                        {
                            TarGetGang.GangDeaths++;

                            dbClient.RunQuery("UPDATE `rp_gangs` SET `gang_deaths` = '" + TarGetGang.GangDeaths + "' WHERE `id` = '" + TarGetGang.Id + "'");
                        }
                    }
                }
            }*/
        }
    }
}
