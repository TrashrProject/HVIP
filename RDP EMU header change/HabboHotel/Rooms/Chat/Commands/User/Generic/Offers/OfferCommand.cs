using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class OfferCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_offer"; }
        }

        public string Parameters
        {
            get { return "%user% %type%"; }
        }

        public string Description
        {
            get { return "Ofrece algo a un usuario."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido, escribe :ofrecer [usuario] [tipo] [precio].", 1);
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("No se ha podido encontrar al usuario.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en la Zona.", 1);
                return;
            }

            string Type = Params[2];
            
            #region Weapon Check
            Weapon weapon = null;
            foreach (Weapon Weapon in WeaponManager.Weapons.Values)
            {
                if (Type.ToLower() == Weapon.Name.ToLower())
                {
                    Type = "weapon";
                    weapon = Weapon;
                }
            }
            #endregion
           
            switch (Type.ToLower())
            {
                #region Weapon
                case "weapon":
                    {
                        if (weapon == null)
                        {
                            Session.SendWhisper("'" + Type + "' no es un arma válida.");
                            break;
                        }

                        if (weapon.Stock < 1)
                        {
                            Session.SendWhisper("No quedan " + weapon.PublicName + " en Stock. Por favor usa el comando :pedido para traer más inventario.", 1);
                            return;
                        }

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "weapon") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything") && Session.GetHabbo().VIPRank < 2)
                        {
                            Session.SendWhisper("Debes ser Fabricante de armas.", 1);
                            break;
                        }

                        if (Target.GetPlay().OwnedWeapons.ContainsKey(weapon.Name) && Target.GetPlay().OwnedWeapons[weapon.Name].CanUse)
                        {
                            Session.SendWhisper("Esta persona ya tiene un/a " + weapon.PublicName, 1);
                            break;
                        }

                        if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything") && Session.GetHabbo().VIPRank < 2)
                        {
                            Session.SendWhisper("Debes estar trabajando para ofrecer un/a " + weapon.PublicName, 1);
                            break;
                        }

                        else
                        {
                            int Cost = (!Target.GetPlay().OwnedWeapons.ContainsKey(weapon.Name) ? weapon.Cost : weapon.CostFine);
                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                                {
                                    if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                                        HasOffer = true;
                                }
                                if (!HasOffer)
                                {
                                    RoleplayManager.Shout(Session, "*Ofrece un/a " + weapon.PublicName + " a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", 5);
                                    Target.GetPlay().OfferManager.CreateOffer(weapon.Name.ToLower(), Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Te han ofrecido un/a " + weapon.PublicName + " por $" + String.Format("{0:N0}", Cost) + " Escribe ':aceptar arma' para comprarla.", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("¡Este usuario ya recibió un arma!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este usuario no puede permitirse un/a " + weapon.PublicName, 1);
                                break;
                            }
                        }
                    }
                #endregion            
            }
        }
    }
}