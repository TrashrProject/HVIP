using System;
using System.Data;
using Plus.HabboHotel.GameClients;
using Plus.Database.Interfaces;
using log4net;
using System.Collections.Generic;
using Plus.HabboRoleplay.ProductOwned;
using Plus.HabboRoleplay.Products;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.RoleplayUsers
{
    public class UserDataHandler
    {
        /// <summary>
        /// Log mechanism
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("UserDataHandler");

        /// <summary>
        /// The users session
        /// </summary>
        GameClient Client;

        /// <summary>
        /// The users roleplay instance
        /// </summary>
        RoleplayUser RoleplayUser;

        /// <summary>
        /// Constructs the class
        /// </summary>
        public UserDataHandler(GameClient Client, RoleplayUser RoleplayUser)
        {
            this.Client = Client;
            this.RoleplayUser = RoleplayUser;
        }

        /// <summary>
        /// Saves all rp data for the user to the db
        /// </summary>
        /// <returns></returns>
        public bool SaveData()
        {
            if (Client == null)
                return false;

            if (Client.GetHabbo() == null)
                return false;

            if (Client.GetPlay() == null)
                return false;

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery(GetQueryString());
                AddParameters(DB);

                DB.RunQuery();
            }
            return true;
        }
        
        private string GetQueryString()
        {
            string Query = @"UPDATE play_stats SET     
                                        level = @level,
                                        curxp = @exp,                                         
                                        curhealth = @curhealth,
                                        maxhealth = @maxhealth,
                                        armor = @armor,
                                        hunger = @hunger,
                                        hygiene = @hygiene,
                                        strength = @strength,
                                        strength_exp = @strength_exp,
                                        bank = @bank,
                                        last_coordinates = @last_coordinates,
                                        is_dying = @is_dying,
                                        dying_time_left = @dying_time_left,
                                        is_dead = @is_dead,
                                        dead_time_left = @dead_time_left,
                                        is_jailed = @is_jailed,
                                        jailed_time_left = @jailed_time_left,
                                        is_wanted = @is_wanted,
                                        wanted_level = @wanted_level,
                                        wanted_time_left = @wanted_time_left,
                                        is_cuffed = @is_cuffed,
                                        cuffed_time_left = @cuffed_time_left,    
                                        is_noob = @is_noob,
                                        noob_time_left = @noob_time_left,
                                        car_limit = @car_limit,
                                        time_worked = @time_worked,
                                        arrests = @arrests,
                                        arrested = @arrested,
                                        CamLvl = @CamLvl,
                                        CamXP = @CamXP,
                                        MecLvl = @MecLvl,
                                        MecXP = @MecXP,
                                        MinerLvl = @MinerLvl,
                                        MinerXP = @MinerXP,
                                        ArmLvl = @ArmLvl,
                                        ArmXP = @ArmXP,
                                        BasuLvl = @BasuLvl,
                                        BasuXP = @BasuXP,
                                        LadronLvl = @LadronLvl,
                                        LadronXP = @LadronXP,
                                        Corp = @Corp,
                                        Gang = @Gang,
                                        chn_disabled = @ChNDisabled,
                                        chn_banned = @ChNBanned,
                                        chn_banned_time_left = @ChNBannedTimeLeft,
                                        send_home_time = @send_home_time,
                                        punches = @punches,
                                        kills = @kills,
                                        hitkills = @hitkills,
                                        gunkills = @gunkills,
                                        copkills = @copkills,
                                        deaths = @deaths,
                                        copdeaths = @copdeaths,
                                        evasions = @evasions,
                                        cocaine_taken = @cocaine_taken,
                                        medicines_taken = @medicines_taken,
                                        weed_taken = @weed_taken,
                                        guns_fab = @guns_fab,
                                        total_shifts = @total_shifts,
                                        money_earned = @money_earned,
                                        pl_earned = @pl_earned,
                                        drugs_taken = @drugs_taken,
                                        tutorial_step = @tutorial_step,
                                        is_sanc = @is_sanc,
                                        sanc_time_left = @sanc_time_left,
                                        sancs = @sancs,
                                        passive_mode = @passive_mode,
                                        changename_count = @changename_count,
                                        is_news_reporter = @is_news_reporter
                            
                                        WHERE id = @userid;";
            /*
            Query += "UPDATE play_products_owned SET extradata = @weed WHERE product_id = '" + RoleplayManager.WeedID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @cocaine WHERE product_id = '" + RoleplayManager.CocaineID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @medicines WHERE product_id = '" + RoleplayManager.MedicinesID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @bidon WHERE product_id = '" + RoleplayManager.BidonID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @MecParts WHERE product_id = '" + RoleplayManager.MecPartsID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @ArmMat WHERE product_id = '" + RoleplayManager.ArmMatID + "' AND user_id = @userid;";
            Query += "UPDATE play_products_owned SET extradata = @ArmPieces WHERE product_id = '" + RoleplayManager.ArmPiecesID + "' AND user_id = @userid;";
            */
            return Query;
        }
        

        /// <summary>
        /// Adds the parameters to the mysql command
        /// </summary>
        private void AddParameters(IQueryAdapter DB)
        {
            #region Play Stats Params
            // User ID
            DB.AddParameter("userid", Client.GetHabbo().Id);
            
            // Basic Info
            DB.AddParameter("level", RoleplayUser.Level);
            DB.AddParameter("exp", RoleplayUser.CurXP);

            // Human Needs   
            DB.AddParameter("curhealth", RoleplayUser.CurHealth);
            DB.AddParameter("maxhealth", RoleplayUser.MaxHealth);
            DB.AddParameter("armor", RoleplayUser.Armor);
            DB.AddParameter("hunger", RoleplayUser.Hunger);
            DB.AddParameter("hygiene", RoleplayUser.Hygiene);

            // Dead/Jailed/Wanted 
            DB.AddParameter("is_dying", PlusEnvironment.BoolToEnum(RoleplayUser.IsDying));
            DB.AddParameter("dying_time_left", RoleplayUser.DyingTimeLeft);
            DB.AddParameter("is_dead", PlusEnvironment.BoolToEnum(RoleplayUser.IsDead));
            DB.AddParameter("dead_time_left", RoleplayUser.DeadTimeLeft);
            DB.AddParameter("is_jailed", PlusEnvironment.BoolToEnum(RoleplayUser.IsJailed));
            DB.AddParameter("jailed_time_left", RoleplayUser.JailedTimeLeft);
            DB.AddParameter("is_wanted", PlusEnvironment.BoolToEnum(RoleplayUser.IsWanted));
            DB.AddParameter("wanted_level", RoleplayUser.WantedLevel);
            DB.AddParameter("wanted_time_left", RoleplayUser.WantedTimeLeft);
            DB.AddParameter("is_cuffed", PlusEnvironment.BoolToEnum(RoleplayUser.Cuffed));
            DB.AddParameter("cuffed_time_left", RoleplayUser.CuffedTimeLeft);
            DB.AddParameter("ChNDisabled", PlusEnvironment.BoolToEnum(RoleplayUser.ChNDisabled));
            DB.AddParameter("ChNBanned", PlusEnvironment.BoolToEnum(RoleplayUser.ChNBanned));
            DB.AddParameter("ChNBannedTimeLeft", RoleplayUser.ChNBannedTimeLeft);
            DB.AddParameter("send_home_time", RoleplayUser.SendHomeTimeLeft);
            DB.AddParameter("is_sanc", PlusEnvironment.BoolToEnum(RoleplayUser.IsSanc));
            DB.AddParameter("sanc_time_left", RoleplayUser.SancTimeLeft);
            DB.AddParameter("sancs", RoleplayUser.Sancs);
            DB.AddParameter("passive_mode", PlusEnvironment.BoolToEnum(RoleplayUser.PassiveMode));
            DB.AddParameter("is_news_reporter", PlusEnvironment.BoolToEnum(RoleplayUser.IsNewsReporter));

            // Levelable
            DB.AddParameter("strength", RoleplayUser.Strength);
            DB.AddParameter("strength_exp", RoleplayUser.StrengthEXP);

            // Banking
            DB.AddParameter("bank", RoleplayUser.Bank);

            // Misc
            DB.AddParameter("last_coordinates", RoleplayUser.LastCoordinates);

            // Newcomer Misc
            DB.AddParameter("is_noob", PlusEnvironment.BoolToEnum(RoleplayUser.IsNoob));
            DB.AddParameter("noob_time_left", RoleplayUser.NoobTimeLeft);

            // Vehicles
            DB.AddParameter("car_limit", RoleplayUser.CarLimit);

            // Statistics
            DB.AddParameter("time_worked", RoleplayUser.TimeWorked);
            DB.AddParameter("punches", RoleplayUser.Punches);
            DB.AddParameter("kills", RoleplayUser.Kills);
            DB.AddParameter("hitkills", RoleplayUser.HitKills);
            DB.AddParameter("gunkills", RoleplayUser.GunKills);
            DB.AddParameter("copkills", RoleplayUser.CopKills);
            DB.AddParameter("deaths", RoleplayUser.Deaths);
            DB.AddParameter("copdeaths", RoleplayUser.CopDeaths);
            DB.AddParameter("arrests", RoleplayUser.Arrests);
            DB.AddParameter("arrested", RoleplayUser.Arrested);
            DB.AddParameter("evasions", RoleplayUser.Evasions);
            DB.AddParameter("cocaine_taken", RoleplayUser.CocaineTaken);
            DB.AddParameter("medicines_taken", RoleplayUser.MedicinesTaken);
            DB.AddParameter("weed_taken", RoleplayUser.WeedTaken);
            DB.AddParameter("guns_fab", RoleplayUser.GunsFab);
            DB.AddParameter("total_shifts", RoleplayUser.TotalShifts);
            DB.AddParameter("money_earned", RoleplayUser.MoneyEarned);
            DB.AddParameter("pl_earned", RoleplayUser.PLEarned);
            DB.AddParameter("drugs_taken", RoleplayUser.DrugsTaken);
            DB.AddParameter("tutorial_step", RoleplayUser.TutorialStep);
            

            // Jobs Lelvels
            DB.AddParameter("CamLvl", RoleplayUser.CamLvl);
            DB.AddParameter("CamXP", RoleplayUser.CamXP);
            DB.AddParameter("MinerLvl", RoleplayUser.MinerLvl);
            DB.AddParameter("MinerXP", RoleplayUser.MinerXP);
            DB.AddParameter("ArmLvl", RoleplayUser.ArmLvl);
            DB.AddParameter("ArmXP", RoleplayUser.ArmXP);
            DB.AddParameter("MecLvl", RoleplayUser.MecLvl);
            DB.AddParameter("MecXP", RoleplayUser.MecXP);
            DB.AddParameter("BasuLvl", RoleplayUser.BasuLvl);
            DB.AddParameter("BasuXP", RoleplayUser.BasuXP);
            DB.AddParameter("LadronLvl", RoleplayUser.LadronLvl);
            DB.AddParameter("LadronXP", RoleplayUser.LadronXP);

            // Groups
            DB.AddParameter("Corp", RoleplayUser.Corp);
            DB.AddParameter("Gang", RoleplayUser.Gang);

            // Change Name
            DB.AddParameter("changename_count", RoleplayUser.ChangeNameCount);
            #endregion

            #region Play Products Params
            /*
            DB.AddParameter("weed", RoleplayUser.Weed);
            DB.AddParameter("cocaine", RoleplayUser.Cocaine);
            DB.AddParameter("medicines", RoleplayUser.Medicines);
            DB.AddParameter("bidon", RoleplayUser.Bidon);
            DB.AddParameter("MecParts", RoleplayUser.MecParts);
            DB.AddParameter("ArmMat", RoleplayUser.ArmMat);
            DB.AddParameter("ArmPieces", RoleplayUser.ArmPieces);
            */
            #endregion

            #region Save Cuenteable products
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.WeedID, RoleplayUser.Weed.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.CocaineID, RoleplayUser.Cocaine.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.MedicinesID, RoleplayUser.Medicines.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.BidonID, RoleplayUser.Bidon.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.MecPartsID, RoleplayUser.MecParts.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.ArmMatID, RoleplayUser.ArmMat.ToString());
            RoleplayManager.UpdateMyProductExtrada(Client, RoleplayManager.ArmPiecesID, RoleplayUser.ArmPieces.ToString());
            #endregion
        }
    }
}