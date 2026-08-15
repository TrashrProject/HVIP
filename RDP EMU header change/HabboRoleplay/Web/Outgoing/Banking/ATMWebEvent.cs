using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// ATMWebEvent class.
    /// </summary>
    class ATMWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            if (!Client.GetPlay().UsingAtm)
            {
                // AntiCheat Here
                Client.SendNotification("¡Buen intento, tratando de inyectar al Sistema, ve a un ATM!");
                return;
            }

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        string SendData = "";
                        SendData += Client.GetPlay().Bank.ToString("C");
                        Socket.Send("compose_atm|open|" + SendData);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().UsingAtm = false;
                        Client.SendWhisper("Has dejado de usar el Cajero.", 1);
                        Socket.Send("compose_atm|close|");
                        break;
                    }
                #endregion

                #region Withdraw
                case "withdraw":
                    {
                        string[] ReceivedData = Data.Split(',');
                        int Amount;

                        if (!int.TryParse(ReceivedData[1], out Amount))
                        {
                            Socket.Send("compose_atm|error|Debe ingresar una cantidad válida.");
                            return;
                        }

                        int WithdrawAmount = Convert.ToInt32(ReceivedData[1]);

                        int ActualAmount = Client.GetPlay().Bank;

                        if (Client.GetPlay().TryGetCooldown("withdraw"))
                            return;

                        if (WithdrawAmount <= 0)
                        {
                            Socket.Send("compose_atm|error|Debe ingresar una cantidad válida.");
                            return;
                        }

                        if (WithdrawAmount > ActualAmount || ActualAmount - WithdrawAmount <= -1)
                        {
                            Socket.Send("compose_atm|error|No dispone de esa cantidad de dinero.");
                            return;
                        }

                        //int TaxAmount = Convert.ToInt32((double)WithdrawAmount * 0.05);

                        RoleplayManager.Shout(Client, "*Retira $" + String.Format("{0:N0}", WithdrawAmount) + " del cajero y lo guarda en su cartera*", 5);

                        Client.GetPlay().Bank -= WithdrawAmount;

                        Client.GetHabbo().Credits += WithdrawAmount;
                        Client.GetHabbo().UpdateCreditsBalance();
                        Client.GetHabbo().UpdateBankBalance();
                        Client.GetPlay().CooldownManager.CreateCooldown("withdraw", 1000, 5);

                        Socket.Send("compose_atm|change_balance_1|" + Client.GetPlay().Bank.ToString("C"));
                    }
                    break;
                #endregion

                #region Deposit
                case "deposit":
                    {
                        string[] ReceivedData = Data.Split(',');
                        int Amount;

                        if (!int.TryParse(ReceivedData[1], out Amount))
                        {
                            Socket.Send("compose_atm|error|Debe ingresar una cantidad válida.");
                            return;
                        }

                        int DepositAmount = Convert.ToInt32(ReceivedData[1]);

                        int ActualAmount = Client.GetHabbo().Credits;

                        if (Client.GetPlay().TryGetCooldown("deposit"))
                            return;

                        if (DepositAmount <= 0)
                        {
                            Socket.Send("compose_atm|error|Debe ingresar una cantidad válida.");
                            return;
                        }

                        if (DepositAmount > ActualAmount || ActualAmount - DepositAmount <= -1)
                        {
                            Socket.Send("compose_atm|error|No dispone de esa cantidad de dinero.");
                            return;
                        }

                        RoleplayManager.Shout(Client, "*Saca $" + String.Format("{0:N0}", DepositAmount) + " de su cartera y los deposita en su Cuenta bancaria*", 5);

                        Client.GetHabbo().Credits -= DepositAmount;
                        Client.GetHabbo().UpdateCreditsBalance();

                        Client.GetPlay().Bank += DepositAmount;
                        Client.GetHabbo().UpdateBankBalance();

                        Socket.Send("compose_atm|change_balance_1|" + Client.GetPlay().Bank.ToString("C"));

                        Client.GetPlay().CooldownManager.CreateCooldown("deposit", 1000, 5);
                    }
                    break;
                    #endregion
            }
        }
    }
}
