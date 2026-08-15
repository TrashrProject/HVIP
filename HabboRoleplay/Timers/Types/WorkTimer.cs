using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Work timer
    /// </summary>
    public class WorkTimer : RoleplayTimer
    {
        public WorkTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            TimeCount = 0;
            TimeCount2 = 0;

            // Get Var Time
            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(base.Client.GetPlay().JobId, base.Client.GetPlay().JobRank);
            int RankTimer = Rank.Timer;
            // Convert to milliseconds
            double TimeDivisible = Math.Floor((double)base.Client.GetPlay().TimeWorked / RankTimer);
            int TimeRemaining = base.Client.GetPlay().TimeWorked - Convert.ToInt32(TimeDivisible) * RankTimer;
            TimeLeft = (RankTimer - TimeRemaining) * 60000;
            OriginalTime = RankTimer;//15

            Client.GetPlay().UpdateTimerDialogue("Work-Timer", "add", (TimeLeft / 60000), OriginalTime);            

            base.Client.SendWhisper("Te resta(n) " + (TimeLeft / 60000) + " minuto(s) para recibir tu próximo cheque.", 1);
        }

        /// <summary>
        /// Pays user after shift
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

                //GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
                GroupRank JobRank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(base.Client.GetPlay().JobId, base.Client.GetPlay().JobRank);

                // Actualizamos información de Empresa
                Group GetJob = PlusEnvironment.GetGame().GetGroupManager().GetJobByID(base.Client.GetPlay().JobId);
                bool HasAdmin = false;
                if(GetJob == null)
                {
                    base.EndTimer();
                    return;
                }

                // Pago si es Empresa
                if(GetJob.GetAdministrator.Count > 0)
                {
                    HasAdmin = true;
                    if(GetJob.Bank < JobRank.Pay)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);

                        WorkManager.RemoveWorkerFromList(base.Client);
                        base.Client.GetPlay().IsWorking = false;
                        base.Client.GetHabbo().Poof();
                        RoleplayManager.Shout(base.Client, "*Intenta trabajar pero no puede al ver que la Empresa no cuenta con el Capital suficiente para su Cheque*", 5);
                        base.Client.SendWhisper("No puedes comenzar a Trabajar debido a que la Empresa no cuenta con Capital suficiente para tu Cheque.", 1);
                        base.EndTimer();
                        return;
                    }
                }

                
                if (!base.Client.GetPlay().IsWorking || base.Client.GetPlay().JobId == 0)
                {
                    Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            

                    WorkManager.RemoveWorkerFromList(base.Client);
                    base.Client.GetPlay().IsWorking = false;
                    base.Client.GetHabbo().Poof();

                    /*
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide"))
                    {
                        guideManager.RemoveGuide(base.Client);
                        base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                        #region End Existing Calls
                        if (base.Client.GetPlay().GuideOtherUser != null)
                        {
                            base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                            if (base.Client.GetPlay().GuideOtherUser.GetPlay() != null)
                            {
                                base.Client.GetPlay().GuideOtherUser.GetPlay().Sent911Call = false;
                                base.Client.GetPlay().GuideOtherUser.GetPlay().GuideOtherUser = null;
                            }

                            base.Client.GetPlay().GuideOtherUser = null;
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                        }
                        #endregion
                    }
                    */
                    base.EndTimer();
                    return;
                }
                /*
                if (base.Client.GetPlay().CurEnergy <= 0)
                {
                    Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            

                    RoleplayManager.Shout(base.Client, "*Stops working as they have run out of energy*", 4);

                    WorkManager.RemoveWorkerFromList(base.Client);
                    base.Client.GetPlay().IsWorking = false;
                    base.Client.GetHabbo().Poof();

                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide"))
                    {
                        guideManager.RemoveGuide(base.Client);
                        base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                        #region End Existing Calls
                        if (base.Client.GetPlay().GuideOtherUser != null)
                        {
                            base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                            if (base.Client.GetPlay().GuideOtherUser.GetPlay() != null)
                            {
                                base.Client.GetPlay().GuideOtherUser.GetPlay().Sent911Call = false;
                                base.Client.GetPlay().GuideOtherUser.GetPlay().GuideOtherUser = null;
                            }

                            base.Client.GetPlay().GuideOtherUser = null;
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                        }
                        #endregion
                    }

                    base.EndTimer();
                    return;
                }
                */

                if (base.Client.GetRoomUser() != null)
                {
                    if (base.Client.GetRoomUser().IsAsleep)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            
                        RoleplayManager.Shout(base.Client, "*Se ha quedado dormido y deja trabajar*", 5);

                        WorkManager.RemoveWorkerFromList(base.Client);
                        base.Client.GetPlay().IsWorking = false;
                        base.Client.GetHabbo().Poof();

                        RoleplayManager.CheckCorpCarp(base.Client);
                        base.Client.GetPlay().CamCargId = 0;
                        if (base.Client.GetPlay().TimerManager != null && Client.GetPlay().TimerManager.ActiveTimers != null)
                        {
                            if (Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("vehiclejob"))
                            {
                                base.Client.SendWhisper("¡Tu Vehículo de trabajo ha sido regresado a su sitio por haberlo abandonado mucho tiempo!", 1);
                                Client.GetPlay().TimerManager.ActiveTimers["vehiclejob"].EndTimer();
                            }
                        }

                        /*
                        if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide"))
                        {
                            guideManager.RemoveGuide(base.Client);
                            base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                            #region End Existing Calls
                            if (base.Client.GetPlay().GuideOtherUser != null)
                            {
                                base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                                base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                                if (base.Client.GetPlay().GuideOtherUser.GetPlay() != null)
                                {
                                    base.Client.GetPlay().GuideOtherUser.GetPlay().Sent911Call = false;
                                    base.Client.GetPlay().GuideOtherUser.GetPlay().GuideOtherUser = null;
                                }

                                base.Client.GetPlay().GuideOtherUser = null;
                                base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                                base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                            }
                            #endregion
                        }
                        */
                        base.EndTimer();
                        return;
                    }
                    if(base.Client.GetHabbo().CurrentRoom != null)
                    {
                        if (!JobRank.CanWorkHere(base.Client.GetHabbo().CurrentRoomId))
                        {
                            Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);
                            RoleplayManager.Shout(base.Client, "*Deja de trabajar por abandonar su zona de trabajo*", 5);

                            WorkManager.RemoveWorkerFromList(base.Client);
                            base.Client.GetPlay().IsWorking = false;
                            base.Client.GetHabbo().Poof();

                            RoleplayManager.CheckCorpCarp(base.Client);
                            base.Client.GetPlay().CamCargId = 0;

                            base.EndTimer();
                            return;
                        }
                    }
                    /*
                    if (base.Client.GetHabbo().CurrentRoom != null)
                    {
                        if (base.Client.GetHabbo().CurrentRoom.TurfEnabled)
                        {
                            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide"))
                            {
                                Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);

                                WorkManager.RemoveWorkerFromList(base.Client);
                                base.Client.GetPlay().IsWorking = false;
                                base.Client.GetHabbo().Poof();

                                guideManager.RemoveGuide(base.Client);
                                base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                                #region End Existing Calls
                                if (base.Client.GetPlay().GuideOtherUser != null)
                                {
                                    base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                                    base.Client.GetPlay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                                    if (base.Client.GetPlay().GuideOtherUser.GetPlay() != null)
                                    {
                                        base.Client.GetPlay().GuideOtherUser.GetPlay().Sent911Call = false;
                                        base.Client.GetPlay().GuideOtherUser.GetPlay().GuideOtherUser = null;
                                    }

                                    base.Client.GetPlay().GuideOtherUser = null;
                                    base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                                    base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                                }
                                #endregion

                                base.EndTimer();
                                return;
                            }
                        }
                    }
                    */
                }

                if (base.Client.GetRoomUser() != null)
                    base.Client.GetRoomUser().IdleTime += 3;
                /*
                if (RoleplayManager.JobCAPTCHABox)
                {
                    if (base.Client.GetPlay().CaptchaSent)
                        return;

                    if (!base.Client.GetPlay().CaptchaSent && base.Client.GetPlay().CaptchaTime >= Convert.ToInt32(RoleplayData.GetData("captcha", "jobinterval")))
                    {
                        base.Client.GetPlay().CreateCaptcha("Enter the code into the box to continue collecting paychecks!");
                        return;
                    }
                }
                */
                TimeCount++;
                TimeCount2++;
                TimeLeft -= 1000;

                if (TimeCount == 30 || TimeCount == 60)
                {
                    var Timers = base.Client.GetPlay().TimerManager;

                    if (Timers != null)
                    {
                        if (Timers.ActiveTimers != null)
                        {
                            if (Timers.ActiveTimers.ContainsKey("hunger"))
                            {
                                int hungercount = Random.Next(20, 46);
                                Timers.ActiveTimers["hunger"].TimeCount += hungercount;
                            }
                            if (Timers.ActiveTimers.ContainsKey("hygiene"))
                            {
                                int hygienecount = Random.Next(20, 46);
                                Timers.ActiveTimers["hygiene"].TimeCount += hygienecount;
                            }
                        }
                    }
                }

                if (TimeCount2 == 60)
                {
                    base.Client.GetPlay().TimeWorked++;
                    TimeCount2 = 0;
                }

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        /*
                        int EnergyLoss = Random.Next(2, 6);

                        if (base.Client.GetPlay().CurEnergy - EnergyLoss <= 0)
                            base.Client.GetPlay().CurEnergy = 0;
                        else
                            base.Client.GetPlay().CurEnergy -= EnergyLoss;
                        */
                        Client.GetPlay().UpdateTimerDialogue("Work-Timer", "decrement", (TimeLeft / 60000), OriginalTime);            

                        base.Client.SendWhisper("Te resta(n) " + (TimeLeft / 60000) + " minuto(s) para recibir tu próximo cheque.", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                if (JobRank == null)
                    return;

                int Pay = JobRank.Pay;
                /*
                if (base.Client.GetPlay().Class.ToLower() == "civilian")
                {
                    Random Random = new Random();
                    int ExtraPay = Random.Next(1, 6);

                    Pay += ExtraPay;
                }
                */
                RoleplayManager.Shout(base.Client, "*Recibe su pago por completar un turno laboral*", 5);
                base.Client.SendWhisper("Se han depositado $" + Pay + " en tu cuenta bancaria. ¡Sigue así!", 1);

                /*
                if (base.Client.GetPlay().BankAccount > 0)
                    base.Client.GetPlay().BankChequings += Pay;
                else
                {
                    base.Client.GetHabbo().Credits += Pay;
                    base.Client.GetHabbo().UpdateCreditsBalance();
                }
                */
                base.Client.GetPlay().Bank += Pay;
                base.Client.GetPlay().MoneyEarned += Pay;
                base.Client.GetPlay().TimeWorked++;
                base.Client.GetPlay().TotalShifts++;
                RoleplayManager.UpdateBankBalance(base.Client);
                base.Client.GetPlay().MoneyEarned += Pay;

                // Actualizamos información de Empresa
                if (GetJob != null)
                {
                    GetJob.Shifts++;
                    if (HasAdmin)
                    {
                        GetJob.Bank -= Pay;
                    }

                    GetJob.SetBussines(GetJob.Bank, GetJob.Stock);
                    GetJob.UpdateShifts(GetJob.Shifts);
                }

                //LevelManager.AddLevelEXP(base.Client, GetExp());
                //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.WORK_CYCLE);

                #region Timer Restart Calculation
                // Get Var Time
                GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(base.Client.GetPlay().JobId, base.Client.GetPlay().JobRank);
                int RankTimer = Rank.Timer;

                double TimeDivisible2 = Math.Floor(Convert.ToDouble(base.Client.GetPlay().TimeWorked) / RankTimer);
                int TimeRemaining2 = base.Client.GetPlay().TimeWorked - Convert.ToInt32(TimeDivisible2) * RankTimer;
                TimeLeft = (RankTimer - TimeRemaining2) * 60000;
                TimeCount = 0;
                TimeCount2 = 0;

                #endregion            

                base.Client.SendWhisper("¡Has comenzado un nuevo Turno! Te resta(n) " + (TimeLeft / 60000) + " minuto(s) para recibir tu próximo cheque.", 1);
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        /// <summary>
        /// Calculates the exp earned from finishing a shift
        /// </summary>
        public int GetExp()
        {
            int Multiplier = 1;

            int Chance = Random.Next(1, 101);

            if (Chance <= 42)
            {
                if (Chance <= 8)
                    Multiplier = 4;
                else if (Chance <= 16)
                    Multiplier = 3;
                else if (Chance <= 32)
                    Multiplier = 2;
                else
                    Multiplier = 1;
            }

            int Amount = Random.Next(15, 51) * Multiplier;

            return Amount;
        }
    }
}