using System.Linq;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Utilities;
using Plus.HabboHotel.Quests;
using System;
using Plus.Core;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Workout timer
    /// </summary>
    public class WorkoutTimer : RoleplayTimer
    {
        public WorkoutTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert 80 seconds to milliseconds
            TimeLeft = RoleplayManager.WorkoutTime * 1000;
        }

        /// <summary>
        /// Executes workout tick
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null || base.Client.GetRoomUser().GetRoom() == null)
                {
                    base.Client.GetPlay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                int EffectId = EffectsList.CrossTrainer;
                int ItemId = (int)Params[0];
                bool Strength = (bool)Params[1];

                if (Strength)
                    EffectId = EffectsList.Treadmill;

                Item Treadmill = base.Client.GetRoomUser().GetRoom().GetRoomItemHandler().GetItem(ItemId);

                if (!base.Client.GetPlay().IsWorkingOut || (Treadmill != null && Treadmill.Coordinate != base.Client.GetRoomUser().Coordinate))
                {
                    RoleplayManager.Shout(base.Client, "*Deja de ejercitarse*", 5);
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetPlay().IsWorkingOut = false;

                    if (Treadmill != null)
                    {
                        Treadmill.ExtraData = "0";
                        Treadmill.InteractingUser = 0;
                        Treadmill.UpdateState(false, true);
                        Treadmill.RequestUpdate(1, true);
                    }

                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetRoomUser().GetRoom().GymEnabled)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetPlay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser().IsAsleep)
                {
                    RoleplayManager.Shout(base.Client, "*Deja de ejercitarse al quedarse dormid@*", 5);
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                    base.Client.GetPlay().IsWorkingOut = false;

                    Treadmill.ExtraData = "0";
                    Treadmill.InteractingUser = 0;
                    Treadmill.UpdateState(false, true);
                    Treadmill.RequestUpdate(1, true);

                    base.EndTimer();
                    return;
                }
                /*
                if (base.Client.GetPlay().CurEnergy <= 0)
                {
                    base.Client.SendWhisper("You ran out of energy! You are too weak to continue working out!", 1);

                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetPlay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }
                */
                if (base.Client.GetRoomUser().CurrentEffect != EffectId)
                    base.Client.GetRoomUser().ApplyEffect(EffectId);
                /*
                if (RoleplayManager.WorkoutCAPTCHABox)
                {
                    if (base.Client.GetPlay().CaptchaSent)
                        return;

                    if (!base.Client.GetPlay().CaptchaSent && base.Client.GetPlay().CaptchaTime >= Convert.ToInt32(RoleplayData.GetData("captcha", "workoutinterval")))
                    {
                        base.Client.GetPlay().CreateCaptcha("Enter the code into the box to continue working out!");
                        return;
                    }
                }
                */
                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;
                /*
                CryptoRandom Random = new CryptoRandom();

                int AmountToAdd;

                if (base.Client.GetHabbo().VIPRank > 0)
                    AmountToAdd = Random.Next(1, 6) * 2;
                else
                    AmountToAdd = Random.Next(1, 6);

                int Exp = AmountToAdd * 4;

                //LevelManager.AddLevelEXP(base.Client, Exp);

                //if (Strength)
                  //  LevelManager.AddStrengthEXP(base.Client, AmountToAdd);
                //else
                  //  LevelManager.AddStaminaEXP(base.Client, AmountToAdd);

                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.WORKOUT);

                int EnergyLoss = Random.Next(RoleplayManager.WorkoutCAPTCHABox ? 2 : 4, RoleplayManager.WorkoutCAPTCHABox ? 4 : 8);

                if (base.Client.GetPlay().CurEnergy - EnergyLoss <= 0)
                    base.Client.GetPlay().CurEnergy = 0;
                else
                    base.Client.GetPlay().CurEnergy -= EnergyLoss;

                base.Client.SendWhisper("You have lost " + EnergyLoss + " energy for working out!", 1);
                */

                // FIN RUTINA
                base.Client.GetPlay().StrengthEXP++;
                base.Client.SendWhisper("Rutina " + base.Client.GetPlay().StrengthEXP + "/50 completada.", 1);

                if(base.Client.GetPlay().StrengthEXP >= 50 && base.Client.GetPlay().Strength < RoleplayManager.StrengthCap)
                {
                    base.Client.GetPlay().Strength++;
                    base.Client.GetPlay().StrengthEXP = 0;
                    base.Client.SendWhisper("¡Has subido tu nivel de Fuerza [+ 1 Fuerza]! Fuerza: " + base.Client.GetPlay().Strength, 1);
                }

                if (/*Strength &&*/ base.Client.GetPlay().Strength >= RoleplayManager.StrengthCap)
                {
                    base.Client.SendWhisper("Ya has alcanzado el máximo nivel de Fuerza: " + RoleplayManager.StrengthCap, 1);
                }

                base.Client.GetPlay().IsWorkingOut = false;

                if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                Treadmill.ExtraData = "0";
                Treadmill.InteractingUser = 0;
                Treadmill.UpdateState(false, true);
                Treadmill.RequestUpdate(1, true);

                base.EndTimer();
                return;
                /*
                if (!Strength && base.Client.GetPlay().Stamina >= RoleplayManager.StaminaCap)
                {
                    base.Client.SendWhisper("You have reached the maximum stamina level of: " + RoleplayManager.StaminaCap + "!", 1);
                    base.Client.GetPlay().IsWorkingOut = false;

                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.EndTimer();
                    return;
                }
                */

                // Convert 80 seconds to milliseconds
                //TimeLeft = 80 * 1000;
                //return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}