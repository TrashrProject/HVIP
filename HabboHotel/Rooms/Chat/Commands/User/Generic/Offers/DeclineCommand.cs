using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class DeclineCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_decline"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Rechaza la oferta del tipo que desees."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa el tipo de oferta, ':rechazar [oferta]'. Usa :ofertas para ver tus ofertas Activas.", 1);
                return;
            }

            string Type = Params[1];

            if (Session.GetPlay().OfferManager.ActiveOffers.Count <= 0)
            {
                Session.SendWhisper("No tienes ninguna oferta para rechazar.", 1);
                return;
            }

            Weapon weapon = null;
            if (Type.ToLower() == "arma")
            {
                if (Session.GetPlay().OfferManager.ActiveOffers.Values.Where(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).ToList().Count > 0)
                    weapon = WeaponManager.getWeapon(Session.GetPlay().OfferManager.ActiveOffers.Values.FirstOrDefault(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).Type.ToLower());
            }
            /* Inecesario
            if (Type.ToLower() == "checkings")
                Type = "chequings";
            */
            if (Session.GetPlay().OfferManager.ActiveOffers.ContainsKey(Type.ToLower()) || weapon != null)
            {
                RoleplayOffer Offer;
                if (weapon == null)
                    Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                else
                    Offer = Session.GetPlay().OfferManager.ActiveOffers[weapon.Name.ToLower()];

                if (Offer.Params != null && Offer.Params.Length > 0)
                {
                    if (Offer.Type.ToLower() == "semillas")
                    {
                        if (Offer.Params.Length > 1)
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                            RoleplayManager.Shout(Session, "*Rechaza la oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + Offerer.GetHabbo().Username + "*", 5);
                        }
                        else
                            RoleplayManager.Shout(Session, "*Rechaza la oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de" + PlusEnvironment.GetHabboById(Offer.OffererId).Username + "*", 5);
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        RoleplayManager.Shout(Session, "*Rechaza la oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + Offerer.GetHabbo().Username + "*", 5);
                    }
                }
                else
                    RoleplayManager.Shout(Session, "*Rechaza la oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + PlusEnvironment.GetHabboById(Offer.OffererId).Username + "*", 5);

                RoleplayOffer Junk;
                Session.GetPlay().OfferManager.ActiveOffers.TryRemove(Offer.Type.ToLower(), out Junk);
            }
            else
            {
                Session.SendWhisper("No tienes ninguna oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + ". Usa ':ofertas' para ver tus ofertas activas.", 1);
                return;
            }
        }
    }
}