using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class OffersCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_offers_list"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Muestra una lista de ofertas activas que te han hecho."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("--- Ofertas Activas ---\n\n");

            if (Session.GetPlay().OfferManager.ActiveOffers.Count <= 0)
                Message.Append("Actualmente no tienes ninguna oferta.\n");
            else
                Message.Append("Escribe :aceptar [proteccion/reparacion/arma/matrinomnio/banda/, etc.] para aceptar la oferta.\n\n");

            lock (Session.GetPlay().OfferManager.ActiveOffers.Values)
            {
                foreach (var Offer in Session.GetPlay().OfferManager.ActiveOffers.Values)
                {
                    if (Offer == null)
                        continue;

                    string Name = "";
                    UserCache OffererCache = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Offer.OffererId);

                    if (OffererCache == null)
                        continue;

                    Name = OffererCache.Username;
                    
                    if (Offer.Type.ToLower() == "proteccion")
                        Message.Append("Protección: Por parte de " + Name + " por $"+Offer.Cost+" \n\n");
                    else if (Offer.Type.ToLower() == "fix_armero")
                        Message.Append("Reparación Arma: Por parte de " + Name + " por $" + Offer.Cost + " \n\n");
                    else if (Offer.Type.ToLower() == "fix_mecanico")
                        Message.Append("Reparación Vehículo: Por parte de " + Name + " por $" + Offer.Cost + " \n\n");
                    else if (Offer.Type.ToLower() == "medicamentos")
                        Message.Append("Medicamentos: Por parte de " + Name + " "+Offer.Params[0]+" por $" + Offer.Cost + " \n\n");
                    else if (Offer.Type.ToLower() == "crack")
                        Message.Append("Crack: Por parte de " + Name + " " + Offer.Params[0] + " por $" + Offer.Cost + " \n\n");
                    else if (Offer.Type.ToLower() == "piezas")
                        Message.Append("Piezas: Por parte de " + Name + " " + Offer.Params[0] + " por $" + Offer.Cost + " \n\n");
                    else if (Offer.Type.ToLower() == "platinos")
                        Message.Append("Platinos: Por parte de " + Name + " " + Offer.Params[0] + " por $" + Offer.Cost + " \n\n");
                    else if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                    {
                        Weapon weapon = WeaponManager.Weapons[Offer.Type.ToLower()];

                        if (weapon != null)
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                            if (Offerer != null)
                            {
                                if (Offerer.GetPlay().OwnedWeapons.Count > 0)
                                {
                                    var Wepn = Offerer.GetPlay().OwnedWeapons[weapon.Name];
                                    if (Wepn != null)
                                        Message.Append("Arma: Un/a " + weapon.PublicName + " (Estado: " + Wepn.WLife + " / 100) por $" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "\n\n");

                                }
                            }
                        }
                    }
                }
            }
            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}