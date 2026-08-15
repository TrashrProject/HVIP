using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Houses;
using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// HousesWebEvent class.
    /// </summary>
    class HousesWebEvent : IWebEvent
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

            if (!Client.GetPlay().ViewHouse)
            {
                // AntiCheat Here
                Client.SendNotification("¡Buen intento, tratando de inyectar el Sistema, abre el Panel de Información de la Propiedad!");
                return;
            }

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        if (Client.GetPlay().TryGetCooldown("viewhouse"))
                            return;

                        #region Get House Info
                        House House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Client.GetPlay().HouseSignId);
                        
                        if(House == null)
                        {
                            Client.SendNotification("((Ha ocurrido un problema al buscar la Información de la Casa o Terreno. Contacte con un Administrador))");
                            return;
                        }
                        #endregion

                        string cost = "";
                        if(House.Cost > 1000) // Dinero Normal
                        {
                            cost = House.Cost.ToString("C");
                        }
                        else // Platinos (PL)
                        {
                            cost = House.Cost + " PL";
                        }
                        string SendData = "";
                        SendData += (Client.GetPlay().HouseOwner.Length > 0 ? Client.GetPlay().HouseOwner + ";" : "Ninguno" + ";");
                        SendData += cost + ";";
                        SendData += House.Level + ";";
                        SendData += House.ForSale + ";";
                        SendData += (Client.GetHabbo().Username == Client.GetPlay().HouseOwner) ? "true;" : "false;";
                        Socket.Send("compose_house|open|" + SendData);
                        
                        Client.GetPlay().CooldownManager.CreateCooldown("viewhouse", 1000, 5);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().HouseOwner = "";
                        Client.GetPlay().HouseSignId = 0;
                        Client.GetPlay().ViewHouse = false;
                        Socket.Send("compose_house|close|");
                        break;
                    }
                #endregion

                #region Buy
                case "buy":
                    {
                        if (Client.GetPlay().TryGetCooldown("buyhouse"))
                            return;

                        #region Get House Info
                        House House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Client.GetPlay().HouseSignId);

                        if (House == null)
                        {
                            Client.SendNotification("((Ha ocurrido un problema al buscar la Información de la Casa o Terreno. Contacte con un Administrador))");
                            return;
                        }
                        #endregion

                        // Comprar
                        if (Client.GetPlay().HouseOwner != Client.GetHabbo().Username)
                        {
                            #region Verify limit houses
                            int MyHouses = 0;
                            List<House> HO = PlusEnvironment.GetGame().GetHouseManager().GetHouseByOwnerId(Client.GetHabbo().Id);
                            if (HO != null)
                                MyHouses = HO.Count;

                            if (MyHouses >= 1 && Client.GetHabbo().VIPRank != 2)
                            {
                                Socket.Send("compose_house|error|¡Ya tienes " + MyHouses + " Casa(s)! Usuarios VIP2 pueden tener más hasta dos propiedades.");
                                return;
                            }
                            else if (MyHouses >= 2)
                            {
                                Socket.Send("compose_house|error|¡Ya tienes " + MyHouses + " Casa(s)! Solo es posible tener hasta 2 propiedades.");
                                return;
                            }
                            #endregion

                            if (!House.ForSale)
                                return;

                            bool pl = false;
                            if (House.Cost > 100) // Dinero Normal
                            {
                                if (Client.GetHabbo().Credits < House.Cost)
                                {
                                    Socket.Send("compose_house|error|No cuentas con dinero suficiente.");
                                    return;
                                }
                            }
                            else // PL
                            {
                                if (Client.GetHabbo().Diamonds < House.Cost)
                                {
                                    Socket.Send("compose_house|error|No cuentas con los Platinos suficientes.");
                                    return;
                                }
                                pl = true;
                            }
                            if (Client.GetPlay().Level < House.Level)
                            {
                                Socket.Send("compose_house|error|No cuentas con el nivel suficiente para comprar la casa.");
                                return;
                            }

                            if (!pl)
                                RoleplayManager.Shout(Client, "*Compra una propiedad con un valor de $" + String.Format("{0:N0}", House.Cost) + "*", 5);
                            else
                                RoleplayManager.Shout(Client, "*Compra una propiedad con un valor de " + String.Format("{0:N0}", House.Cost) + " PL*", 5);

                            string command = (House.Type == 3) ? ":ayuda terrenos" : ":ayuda casas"; // Type = 3 = terrain
                            Client.SendWhisper("¡Enhorabuena! Acabas de comprar una propiedad. Recuerda usar '"+ command +"' para saber sobre su uso.", 1);

                            // Hacemos Pago
                            if (!pl)
                            {
                                Client.GetHabbo().Credits -= House.Cost;
                                Client.GetHabbo().UpdateCreditsBalance();
                            }
                            else
                            {
                                Client.GetHabbo().Diamonds -= House.Cost;
                                Client.GetHabbo().UpdateDiamondsBalance();
                            }
                            
                            House.BuyHouse(Client);
                        }
                        // Vender / No Vender
                        else 
                        {
                            if (!House.ForSale) // Vender
                            {
                                int Pay = 0;
                                bool pl = false;
                                if (House.Cost > 1000) // Dinero Normal
                                {
                                    Pay = House.Cost / 10;
                                }
                                else // PL
                                {
                                    Pay = House.Cost * 1000;
                                    pl = true;
                                }

                                if (Client.GetHabbo().Credits < Pay)
                                {
                                    Socket.Send("compose_house|error|Necesitas $ " + Pay + " para cubrir los gastos del embargo.");
                                    return;
                                }

                                if (!pl)
                                    RoleplayManager.Shout(Client, "*Pone en Venta su casa por un valor de $" + String.Format("{0:N0}", House.Cost) + "*", 5);
                                else
                                    RoleplayManager.Shout(Client, "*Pone en Venta su casa por un valor de " + String.Format("{0:N0}", House.Cost) + " PL*", 5);
                                Client.SendWhisper("Tu propiedad ahora se encuentra en Venta. Cuando alguien la compre, recibirás tu respectivo pago por ella.", 1);

                                Client.GetHabbo().Credits -= Pay;
                                Client.GetHabbo().UpdateCreditsBalance();

                                House.SellHouse(Client);
                            }
                            else // Dejar de Vender
                            {
                                RoleplayManager.Shout(Client, "*Ha quitado la oferta para la Venta de su Propiedad*", 5);
                                Client.SendWhisper("Tu propiedad ya no se encuentra en Venta, pero ya no recibirás de vuelta tu Dinero del Embargo.", 1);

                                House.NoSellHouse(Client);
                            }
                        }
                        
                        Client.GetPlay().CooldownManager.CreateCooldown("buyhouse", 1000, 5);

                        // Cerramos Ventana ws
                        Client.GetPlay().HouseOwner = "";
                        Client.GetPlay().HouseSignId = 0;
                        Client.GetPlay().ViewHouse = false;
                        Socket.Send("compose_house|close|");
                    }
                    break;
                    #endregion
            }
        }
    }
}
