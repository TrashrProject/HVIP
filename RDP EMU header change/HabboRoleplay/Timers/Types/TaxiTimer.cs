using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class TaxiTimer : RoleplayTimer
    {
        public TaxiTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            TimeLeft = base.Client.GetPlay().TaxiTimeLeft * 1000; // 15 segs
        }
 
        /// <summary>
        /// Increases the users hunger
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null || base.Client.GetPlay().IsDying || base.Client.GetPlay().IsDead || base.Client.GetPlay().IsJailed || !base.Client.GetPlay().Pasajero || base.Client.GetPlay().ChoferName.Length <= 0 || base.Client.GetRoomUser().IsWalking)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;
                                
                TimeCount++;
                
                TimeLeft -= 1000;

                base.Client.GetPlay().TaxiTimeLeft--;
                
                if (TimeLeft > 0 )
                    return;

                // Reiniciamos Timer
                base.Client.GetPlay().TaxiTimeLeft = RoleplayManager.TaxiTime;
                TimeLeft = base.Client.GetPlay().TaxiTimeLeft * 1000;

                #region Cumple el Timer
                GameClient Taxista = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(base.Client.GetPlay().ChoferName);
                if (Taxista != null)
                {
                    if (Taxista.GetPlay().Ficha > 0)
                    {
                        if (base.Client.GetHabbo().Credits < Taxista.GetPlay().Ficha)
                        {
                            Taxista.SendWhisper("El pasajero " + base.Client.GetHabbo().Username + " no tiene dinero suficiente para pagar la Tarifa. ¡Tú decides si llevarlo o no!", 1);
                            Taxista.SendWhisper("Usa ':bajar [nombre-pasajero]' para bajar a alguien de tu vehículo.", 1);
                        }
                        else
                        {
                            //Chofer
                            Taxista.GetHabbo().Credits += Taxista.GetPlay().Ficha;
                            Taxista.GetPlay().MoneyEarned += Taxista.GetPlay().Ficha;
                            Taxista.GetHabbo().UpdateCreditsBalance();
                            Taxista.SendWhisper("Recibes $" + Taxista.GetPlay().Ficha + " de " + base.Client.GetHabbo().Username + " por la Tarifa tu Taxi.", 1);
                            //base.Client(s)
                            base.Client.GetHabbo().Credits -= Taxista.GetPlay().Ficha;
                            base.Client.GetHabbo().UpdateCreditsBalance();
                            base.Client.SendWhisper("Pagas [-$" + Taxista.GetPlay().Ficha + "] por la Tarifa del Taxi.", 1);
                        }
                    }
                }
                #endregion                
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}