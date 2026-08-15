using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.GameClients;
using System.Linq;

namespace Plus.HabboRoleplay.Misc
{
    public static class PayDayManager
    {
        
        /// <summary>
        /// Set the payday checks
        /// </summary>
        public static void SetTime()
        {
            DateTime TimeNow = DateTime.Now;
            TimeSpan TimeOfDay = TimeNow.TimeOfDay;

            // Entra Cada Hora:02 minutos
            if (TimeOfDay.Minutes == 02 && TimeOfDay.Seconds == 00)
            {
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    #region CheckVIPStatus
                    if (client.GetHabbo().VIPRank > 0)
                    {
                        if (PlusEnvironment.GetUnixTimestamp() >= client.GetHabbo().VIPExpire)
                        {
                            client.SendWhisper("¡Tu suscripción VIP"+ client.GetHabbo().VIPRank + " ha terminado! No perderás tus vehículos ni propiedades adquiridos con ella.", 1);
                            client.GetHabbo().VIPRank = 0;
                        }
                    }
                    #endregion

                    if (client.GetRoomUser() != null && client.GetRoomUser().IsAsleep)
                        continue;

                    //double Minutes = client.GetHabbo().GetStats().OnlineTime / 60;
                    int Minutes = Convert.ToInt32(Math.Ceiling((PlusEnvironment.GetUnixTimestamp() - Convert.ToDouble(client.GetHabbo().LastOnline)) / 60));
                    if (Minutes < 45)
                    {
                        client.SendWhisper("No has jugado lo suficiente para recibir tu Pago Diario (PD).", 1);
                        continue;
                    }

                    #region Vars
                    int CurLevel = client.GetPlay().Level;
                    int CurXP = client.GetPlay().CurXP;
                    int NeedXP = RoleplayManager.GetInfoPD(CurLevel, "NeedXP");
                    int PD = RoleplayManager.GetInfoPD(CurLevel, "PD");
                    int Cost = RoleplayManager.GetInfoPD((CurLevel), "Cost");
                    int VIP = client.GetHabbo().VIPRank;
                    int Inter = Convert.ToInt32(client.GetPlay().Bank * 0.05);
                    string ExtraMsg = "";
                    #endregion

                    #region Interes Límite
                    if (VIP == 1)
                    {
                        if (Inter > 1500) { Inter = 1500; }
                    }
                    else if (VIP == 2)
                    {
                        if (Inter > 3000) { Inter = 3000; }
                    }
                    else
                    {
                        if (Inter > 1000) { Inter = 1000; }
                    }
                    #endregion

                    // Pagamos
                    client.GetPlay().Bank += (PD + Inter);
                    client.GetPlay().MoneyEarned += (PD + Inter);
                    if (CurXP < NeedXP)
                    {
                        CurXP++;
                        client.GetPlay().CurXP = CurXP;
                        ExtraMsg = " [+ 1 Reputación]";
                    }
                    // Guardamos en DB (OFF)
                    //RoleplayManager.UpdateBankBalance(client);
                    //RoleplayManager.SaveQuickStat(client, "curxp", CurXP+"");

                    client.SendWhisper("¡Has Recibido tu Pago Diario!  -> [+ $"+ PD +"]" + ExtraMsg, 1);
                    client.SendWhisper("Saldo Bancario: $" + client.GetPlay().Bank + "  -  Intereses (Ganancias): $" + Inter + "  -  Costo para el Nuevo Nivel: $" + Cost + "  -  Reputación: " + CurXP + " / " + NeedXP, 1);
                    
                    // Refrescamos WS
                    client.GetPlay().UpdateInteractingUserDialogues();
                    client.GetPlay().RefreshStatDialogue();
                }
            }
        }
    }
}