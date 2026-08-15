using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboRoleplay.Weapons;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class AcceptCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_accept"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Acepta la oferta del tipo que ingreses."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa el tipo de oferta a aceptar. ':aceptar [oferta]'. Usa :ofertas para ver las ofertas Activas que tienes.", 1);
                return;
            }

            if (Session.GetPlay().OfferManager.ActiveOffers.Count <= 0)
            {
                Session.SendWhisper("No tienes ninguna oferta para aceptar.", 1);
                return;
            }

            string Type = Params[1];

            #region Basic Conditions
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            #endregion

            #region Check Weapon
            Weapon weapon = null;
            if (Type.ToLower() == "arma")
            {
                if (Session.GetPlay().OfferManager.ActiveOffers.Values.Where(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).ToList().Count > 0)
                    weapon = WeaponManager.getWeapon(Session.GetPlay().OfferManager.ActiveOffers.Values.FirstOrDefault(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).Type.ToLower());
            }
            #endregion

            if (Session.GetPlay().OfferManager.ActiveOffers.ContainsKey(Type.ToLower()) || Type.ToLower() == "arma" || Type.ToLower() == "reparacion")
            {
                #region Weapons
                if (Type.ToLower() == "arma" || WeaponManager.Weapons.ContainsKey(Type.ToLower()))
                {
                    if (weapon != null)
                    {

                        var Offer = Session.GetPlay().OfferManager.ActiveOffers[weapon.Name.ToLower()];

                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Session.SendWhisper("Al parecer el Ofertante se ha ido. Oferta cancelada.", 1);
                            return;
                        }
                        if (Offerer.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Session.SendWhisper("Lo sentimos, no te encuentras en el mismo lugar que " + Offerer.GetHabbo().Username + " para aceptar la oferta del arma.", 1);
                            return;
                        }
                        if (Session.GetPlay().Level < 2)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Session.SendWhisper("((Debes ser al menos Nivel 2 para poder portar armas))", 1);
                            return;
                        }

                        if (Session.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                            return;
                        }
                        if (Offerer.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                            return;
                        }

                        if (Session.GetPlay().OwnedWeapons.ContainsKey(weapon.Name))
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Session.SendWhisper("Ya tienes esa arma en tu inventario.", 1);
                            return;
                        }
                        if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Session.SendWhisper("No tienes $" + Offer.Cost + " para aceptar la oferta", 1);
                            return;
                        }
                        if (Session.GetPlay().EquippedWeapon != null)
                        {
                            Session.SendWhisper("No debes tener ningún arma equipada mientras aceptas la oferta.", 1);
                            return;
                        }
                        if (!Offerer.GetPlay().OwnedWeapons.ContainsKey(weapon.Name))
                        {
                            Session.SendWhisper("Esta persona ya no tiene un/a " + weapon.PublicName, 1);
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            return;
                        }
                        if (Offerer.GetPlay().EquippedWeapon == null || Offerer.GetPlay().EquippedWeapon.Name.ToLower() != weapon.Name.ToLower())
                        {
                            Session.SendWhisper("El vendedor no tiene el arma equipada.", 1);
                            return;
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Acepta la oferta de " + Offerer.GetHabbo().Username + " y compra su " + weapon.PublicName + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Offerer.GetHabbo().Credits += Offer.Cost;
                            Offerer.GetPlay().MoneyEarned += Offer.Cost;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            // Traspaso del dato WLife & Bullets
                            Session.GetPlay().WLife = Offerer.GetPlay().WLife;
                            Session.GetPlay().Bullets = Offerer.GetPlay().Bullets;

                            #region Desequipar al Ofertante
                            if (Offerer.GetPlay().EquippedWeapon != null)
                            {
                                if (Offerer.GetPlay().EquippedWeapon.Name == weapon.Name)
                                {
                                    string UnEquipMessage = Offerer.GetPlay().EquippedWeapon.UnEquipText;
                                    UnEquipMessage = UnEquipMessage.Replace("[NAME]", Offerer.GetPlay().EquippedWeapon.PublicName);

                                    RoleplayManager.Shout(Offerer, UnEquipMessage, 5);

                                    if (Offerer.GetRoomUser().CurrentEffect == Offerer.GetPlay().EquippedWeapon.EffectID)
                                        Offerer.GetRoomUser().ApplyEffect(0);

                                    if (Offerer.GetRoomUser().CarryItemID == Offerer.GetPlay().EquippedWeapon.HandItem)
                                        Offerer.GetRoomUser().CarryItem(0);

                                    Offerer.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                                    Offerer.GetPlay().EquippedWeapon = null;

                                    Offerer.GetPlay().WLife = 0;
                                    Offerer.GetPlay().Bullets = 0;
                                }
                            }
                            #endregion

                            // Cambio
                            RoleplayManager.AddWeapon(Session, weapon);
                            Session.GetPlay().EquippedWeapon = null;
                            Session.GetPlay().OwnedWeapons = null;
                            Session.GetPlay().OwnedWeapons = Session.GetPlay().LoadAndReturnWeapons();

                            RoleplayManager.DropMyWeapon(Offerer, weapon.Name);
                            Offerer.GetPlay().EquippedWeapon = null;
                            Offerer.GetPlay().OwnedWeapons = null;
                            Offerer.GetPlay().OwnedWeapons = Offerer.GetPlay().LoadAndReturnWeapons();

                            #region Equipar al comprador
                            string GunName = weapon.Name;
                            Weapon BaseWeapon = WeaponManager.getWeapon(GunName);
                            if (!Session.GetPlay().OwnedWeapons.ContainsKey(GunName))
                            {
                                Session.SendWhisper("Algo ha pasado y no has recibido el arma. ((Contacta con un Administrador))", 1);
                                return;
                            }

                            RoleplayManager.UpdateMyWeaponStats(Session, "bullets", Session.GetPlay().Bullets, GunName);
                            RoleplayManager.UpdateMyWeaponStats(Session, "life", Session.GetPlay().WLife, GunName);
                            Session.GetPlay().OwnedWeapons = null;
                            Session.GetPlay().OwnedWeapons = Session.GetPlay().LoadAndReturnWeapons();

                            var Weapon = Session.GetPlay().OwnedWeapons[GunName];

                            string EquipMessage = Weapon.EquipText;
                            EquipMessage = EquipMessage.Replace("[NAME]", Weapon.PublicName);

                            RoleplayManager.Shout(Session, EquipMessage, 5);
                            Session.SendWhisper("Has recibido un/a " + Weapon.PublicName + " con " + Session.GetPlay().Bullets + "/" + Weapon.ClipSize + " balas y un estado de " + Session.GetPlay().WLife + "/100", 1);

                            Session.GetPlay().EquippedWeapon = Weapon;
                            Session.GetPlay().Bullets = Weapon.TotalBullets;
                            Session.GetPlay().WLife = Weapon.WLife;

                            Session.GetPlay().CooldownManager.CreateCooldown("equip", 1000, 3);

                            if (Session.GetRoomUser().CurrentEffect != Weapon.EffectID)
                                Session.GetRoomUser().ApplyEffect(Weapon.EffectID);

                            if (Session.GetRoomUser().CarryItemID != Weapon.HandItem)
                                Session.GetRoomUser().CarryItem(Weapon.HandItem);
                            #endregion

                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                            Offerer.SendWhisper(Session.GetHabbo().Username + " acepta tu " + weapon.PublicName + " y recibes $" + Offer.Cost, 1);
                            return;
                        }
                    }
                    else
                    {
                        Session.SendWhisper("No tienes ninguna oferta de Arma.", 1);
                        return;
                    }
                }
                #endregion

                #region Reparación
                else if (Type.ToLower() == "reparacion")
                {
                    #region FixArma
                    if (Session.GetPlay().OfferManager.ActiveOffers.ContainsKey("fix_armero"))
                    {
                        var Offer = Session.GetPlay().OfferManager.ActiveOffers["fix_armero"];
                        if (Offer != null)
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                            if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);
                                Session.SendWhisper("Lo sentimos, el armero no está en la misma zona que tú.", 1);
                                return;
                            }
                            else if (Offerer.GetPlay().ArmUserTo != Session.GetHabbo().Id)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);
                                Session.SendWhisper("El Armero ya no recuerda cuantas piezas usar. Dile que vuelva a revisar.", 1);
                                return;
                            }
                            else if (Offerer.GetPlay().ArmPieces < Offerer.GetPlay().ArmPiecesTo)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);
                                Session.SendWhisper("El Armero ya no tiene las piezas suficientes para la reparación.", 1);
                                return;
                            }
                            else if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                return;
                            }
                            else if (Offerer.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                                return;
                            }
                            else if (Session.GetHabbo().Credits < Offer.Cost)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);
                                Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar la reparación!", 1);
                                return;
                            }
                            if (Session.GetPlay().EquippedWeapon == null)
                            {
                                Session.SendWhisper("Debes tener Equipada el arma a ser reparada.", 1);
                                return;
                            }
                            if (Session.GetPlay().WLife > 0)
                            {
                                Session.SendWhisper("Esa arma no necesita una reparación.", 1);
                                return;
                            }
                            if ((Session.GetPlay().EquippedWeapon.Cost / 4) != Offerer.GetPlay().ArmPiecesTo)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);
                                Session.SendWhisper("Esta no es la misma arma que el Armero examinó.", 1);
                                return;
                            }
                            else
                            {
                                RoleplayManager.Shout(Session, "*Acepta la oferta de Reparación de arma a " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_armero", out Junk);

                                RoleplayManager.Shout(Offerer, "*Usa su destornillador para reparar el arma de " + Session.GetHabbo().Username + "*", 5);
                                Offerer.SendWhisper("¡Buen trabajo! Has usado " + Offerer.GetPlay().ArmPiecesTo + " pieza(s) por reparar el arma y tus ganancias son: $" + Offer.Cost, 1);
                                Session.SendWhisper("Has pagado $" + Offer.Cost + " a " + Offerer.GetHabbo().Username + " por reparar tu arma.", 1);
                                Offerer.GetPlay().ArmPieces -= Offerer.GetPlay().ArmPiecesTo;
                                RoleplayManager.UpdateMyWeaponStats(Session, "life", 100, Session.GetPlay().EquippedWeapon.Name);
                                Session.GetPlay().WLife = 100;
                                RoleplayManager.UpdateMyWeaponStats(Session, "life", Session.GetPlay().WLife, Session.GetPlay().EquippedWeapon.Name);
                                Session.GetPlay().OwnedWeapons = null;
                                Session.GetPlay().OwnedWeapons = Session.GetPlay().LoadAndReturnWeapons();
                                Session.GetHabbo().Credits -= Offer.Cost;
                                Session.GetHabbo().UpdateCreditsBalance();
                                Offerer.GetHabbo().Credits += Offer.Cost;
                                Offerer.GetPlay().MoneyEarned += Offer.Cost;
                                Offerer.GetHabbo().UpdateCreditsBalance();
                            }
                        }
                    }
                    #endregion

                    #region FixCar
                    else if(Session.GetPlay().OfferManager.ActiveOffers.ContainsKey("fix_mecanico"))
                    {
                        var Offer2 = Session.GetPlay().OfferManager.ActiveOffers["fix_mecanico"];
                        if (Offer2 != null)
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer2.OffererId);
                            if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk);
                                Session.SendWhisper("Lo sentimos, el ofertante no está en la misma zona que tú.", 1);
                                return;
                            }
                            else if(Session.GetPlay().Level < 2)
							{
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk);
                                Session.SendWhisper("((Lo sentimos, no puedes aceptar los servicios de un mecánico hasta el nivel 2.))", 1);
                                return;
                            }
                            else if (Session.GetHabbo().Credits < Offer2.Cost)
                            {
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk);
                                Session.SendWhisper("¡Necesitas $" + Offer2.Cost + " para aceptar la reparación!", 1);
                                return;
                            }
                            else if (Offerer.GetRoomUser().Coordinate != Offerer.GetPlay().MecCordinates)
                            {
                                Session.SendWhisper("¡El mecánico no se encuentra frente a tu Vehículo! Pídele que vuelva a la posición donde te ofreció sus servicios para poder aceptar o usa ':rechazar reparacion'", 1);
                                return;
                            }
                            else
                            {
                                #region Check Vehicle InFront
                                int FuelSize = 0;
                                Vehicle vehicle = null;
                                bool found = false;
                                int itemfurni = 0, corp = 0;
                                Item BTile = null;
                                string itemnm = null;
                                foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                                {
                                    if (!found)
                                    {
                                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == Vehicle.ItemName && x.Coordinate == Offerer.GetRoomUser().SquareInFront);
                                        if (BTile != null)
                                        {
                                            vehicle = Vehicle;
                                            itemfurni = BTile.Id;
                                            itemnm = Vehicle.ItemName;
                                            corp = Convert.ToInt32(Vehicle.CarCorp);
                                            found = true;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    RoleplayOffer Junk2;
                                    Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk2);
                                    Session.SendWhisper("¡El Mecánico debe estar frente al vehículo a reparar!", 1);
                                    return;
                                }

                                #endregion

                                #region Select Vehicle State
                                int state = 0;
                                List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(itemfurni);
                                if (VO == null || VO.Count <= 0)
                                {
                                    RoleplayOffer Junk3;
                                    Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk3);
                                    Session.SendWhisper("¡El Mecánico debe estar frente a un vehículo que requiera reparacion!", 1);
                                    return;
                                }
                                state = VO[0].State;
                                #endregion

                                #region Check Vehicle State
                                if (state == 2 || state == 3 || VO[0].CarLife <= 0)
                                {
                                    if (state == 2)
                                        Offerer.GetPlay().MecNewState = 0;// óptimo y sin traba
                                    else
                                        Offerer.GetPlay().MecNewState = 1;// óptimo y con traba
                                }
                                else
                                {
                                    RoleplayOffer Junk4;
                                    Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk4);
                                    Session.SendWhisper("Este vehículo no necesita ser reparado.", 1);
                                    return;
                                }
                                #endregion

                                #region Calc Repair Kit Cant
                                if (FuelSize <= 90)// Tanque Pequeño
                                {
                                    Offerer.GetPlay().MecPartsTo = 3;
                                }
                                else if (FuelSize > 90 && FuelSize <= 100)// Tanque Mediano
                                {
                                    Offerer.GetPlay().MecPartsTo = 6;
                                }
                                else // Tanque Grande
                                {
                                    Offerer.GetPlay().MecPartsTo = 9;
                                }
                                #endregion

                                if (Offerer.GetPlay().MecParts < Offerer.GetPlay().MecPartsTo)
                                {
                                    RoleplayOffer Junk4;
                                    Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk4);
                                    Session.SendWhisper("El mecánico ya no cuenta con los repuestos suficientes.", 1);
                                    return;
                                }

                                RoleplayManager.Shout(Session, "*Acepta la oferta de Reparación de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer2.Cost) + "*", 5);
                                RoleplayOffer Junk;
                                Session.GetPlay().OfferManager.ActiveOffers.TryRemove("fix_mecanico", out Junk);

                                // Iniciar Timer al Mecánico
                                Offerer.GetPlay().MecUserToRepair = Session.GetHabbo().Id;
                                Offerer.GetPlay().MecPriceTo = Offer2.Cost;
                                Offerer.GetPlay().IsMecLoading = true;
                                Offerer.GetPlay().LoadingTimeLeft = RoleplayManager.GetTimerByMyJob(Offerer, "mecanico"); // Depende nivel del Mecánico

                                #region In State Mecánico
                                if (!Offerer.GetRoomUser().isSitting)
                                {
                                    Offerer.GetRoomUser().SetRot(Offerer.GetPlay().MecRotPosition, false);
                                    #region Sit
                                    if (!Offerer.GetRoomUser().Statusses.ContainsKey("sit"))
                                    {
                                        if ((Offerer.GetRoomUser().RotBody % 2) == 0)
                                        {
                                            try
                                            {
                                                Offerer.GetRoomUser().Statusses.Add("sit", "1.0");
                                                Offerer.GetRoomUser().Z -= 0.35;
                                                Offerer.GetRoomUser().isSitting = true;
                                                Offerer.GetRoomUser().UpdateNeeded = true;
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            Offerer.GetRoomUser().RotBody--;
                                            Offerer.GetRoomUser().Statusses.Add("sit", "1.0");
                                            Offerer.GetRoomUser().Z -= 0.35;
                                            Offerer.GetRoomUser().isSitting = true;
                                            Offerer.GetRoomUser().UpdateNeeded = true;
                                        }
                                    }
                                    else if (Offerer.GetRoomUser().isSitting == true)
                                    {
                                        Offerer.GetRoomUser().Z += 0.35;
                                        Offerer.GetRoomUser().Statusses.Remove("sit");
                                        Offerer.GetRoomUser().Statusses.Remove("1.0");
                                        Offerer.GetRoomUser().isSitting = false;
                                        Offerer.GetRoomUser().UpdateNeeded = true;
                                    }
                                    #endregion
                                }
                                #endregion

                                RoleplayManager.Shout(Offerer, "*Saca sus herramientas y comienza a reparar el vehículo de " + Session.GetHabbo().Username + "*", 5);
                                Offerer.SendWhisper("Debes esperar " + Offerer.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                                Offerer.GetPlay().TimerManager.CreateTimer("general", 1000, true);
                                return;
                            }
                        }
                    }
                    #endregion

                    else
                        Session.SendWhisper("No tienes ninguna oferta de reparación pendiente.", 1);
                    return;
                }
                #endregion

                #region Protección
                else if (Type.ToLower() == "proteccion")
                {
                    var Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        GameClient Bot = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Bot.GetHabbo().Username + " para aceptar su oferta de protección.", 1);
                            return;
                        }
                        else if (Session.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                            return;
                        }
                        else if (Bot.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar la protección!", 1);
                            return;
                        }
                        else if (Session.GetPlay().Level == 1 && Session.GetPlay().CurXP < 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("¡Necesitas al menos 3 puntos de reputación (XP) para aceptar la protección!", 1);
                            return;
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Acepta la oferta de Protección de " + Bot.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);

                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            if (Bot.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Bot.GetHabbo().Credits += Offer.Cost;
                                Bot.GetPlay().MoneyEarned += Offer.Cost;
                                Bot.GetHabbo().UpdateCreditsBalance();
                            }
                            if (Session.GetPlay().Armor < 50)
                                Session.GetPlay().Armor = 50;
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Offerer.GetHabbo().Username + " para aceptar su oferta de protección.", 1);
                            return;
                        }
                        else if (Session.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                            return;
                        }
                        else if (Offerer.GetPlay().PassiveMode)
                        {
                            Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar la protección!", 1);
                            return;
                        }
                        else if (Session.GetPlay().Level == 1 && Session.GetPlay().CurXP < 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.SendWhisper("¡Necesitas al menos 3 puntos de reputación (XP) para aceptar la protección!", 1);
                            return;
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Acepta la oferta de Protección de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                            RoleplayOffer Junk;
                            Session.GetPlay().OfferManager.ActiveOffers.TryRemove("proteccion", out Junk);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            if (Offerer.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Offerer.GetHabbo().Credits += Offer.Cost;
                                Offerer.GetPlay().MoneyEarned += Offer.Cost;
                                Offerer.GetHabbo().UpdateCreditsBalance();
                            }

                            if (Session.GetPlay().Armor < 50)
                                Session.GetPlay().Armor = 50;
                            return;
                        }
                    }
                }
                #endregion

                #region Medicamentos
                else if (Type.ToLower() == "medicamentos")
                {
                    var Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    int Cant = Convert.ToInt32(Offer.Params[0]);

                    if (Offerer.GetRoomUser().RoomId != Room.Id)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("medicamentos", out Junk);
                        Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Offerer.GetHabbo().Username + " para aceptar su oferta de medicamentos.", 1);
                        return;
                    }
                    else if (Session.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                        return;
                    }
                    else if (Session.GetPlay().Level < 2)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("medicamentos", out Junk);
                        Session.SendWhisper("((Debes ser al menos Nivel 2 para poder recibir medicamentos de alguien más))", 1);
                        return;
                    }
                    else if (Session.GetHabbo().Credits < Offer.Cost)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("medicamentos", out Junk);
                        Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar los medicamentos!", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().Medicines < Cant)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("medicamentos", out Junk);
                        Session.SendWhisper("¡El ofertante ya no cuenta con los medicamentos!", 1);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Acepta la oferta de Medicamentos de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                        Session.SendWhisper("Has recibido "+Cant+" medicamento(s) por " + Offerer.GetHabbo().Username +".", 1);
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("medicamentos", out Junk);
                        Session.GetHabbo().Credits -= Offer.Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetHabbo().Credits += Offer.Cost;
                        Offerer.GetPlay().MoneyEarned += Offer.Cost;
                        Offerer.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetPlay().Medicines -= Cant;
                        Session.GetPlay().Medicines += Cant;
                        Session.GetPlay().MedicinesTaken += Cant;
                        return;
                    }
                }
                #endregion

                #region Crack
                else if (Type.ToLower() == "crack")
                {
                    var Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    int Cant = Convert.ToInt32(Offer.Params[0]);

                    if (Offerer.GetRoomUser().RoomId != Room.Id)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("crack", out Junk);
                        Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Offerer.GetHabbo().Username + " para aceptar su oferta de crack.", 1);
                        return;
                    }
                    else if (Session.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                        return;
                    }
                    else if (Session.GetPlay().Level < 2)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("crack", out Junk);
                        Session.SendWhisper("((Debes ser al menos Nivel 2 para poder recibir crack de alguien más))", 1);
                        return;
                    }
                    else if (Session.GetHabbo().Credits < Offer.Cost)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("crack", out Junk);
                        Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar el crack!", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().Cocaine < Cant)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("crack", out Junk);
                        Session.SendWhisper("¡El ofertante ya no cuenta con los gramos de Crack ofrecidos!", 1);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Acepta la oferta de Crack de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                        Session.SendWhisper("Has recibido " + Cant + " g. de crack por " + Offerer.GetHabbo().Username + ".", 1);
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("crack", out Junk);
                        Session.GetHabbo().Credits -= Offer.Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetHabbo().Credits += Offer.Cost;
                        Offerer.GetPlay().MoneyEarned += Offer.Cost;
                        Offerer.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetPlay().Cocaine -= Cant;
                        Session.GetPlay().Cocaine += Cant;
                        Session.GetPlay().CocaineTaken += Cant;
                        return;
                    }
                }
                #endregion

                #region Piezas
                else if (Type.ToLower() == "piezas")
                {
                    var Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    int Cant = Convert.ToInt32(Offer.Params[0]);

                    if (Offerer.GetRoomUser() == null || Offerer.GetRoomUser().RoomId != Room.Id)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("piezas", out Junk);
                        Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Offerer.GetHabbo().Username + " para aceptar su oferta de piezas.", 1);
                        return;
                    }
                    else if (Session.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().PassiveMode)
                    {
                        Session.SendWhisper("No puedes aceptarle eso a una persona en modo pasivo.", 1);
                        return;
                    }
                    else if (Session.GetPlay().Level < 2)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("piezas", out Junk);
                        Session.SendWhisper("((Debes ser al menos Nivel 2 para poder recibir piezas de armas de alguien más))", 1);
                        return;
                    }
                    else if (Session.GetHabbo().Credits < Offer.Cost)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("piezas", out Junk);
                        Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar las piezas!", 1);
                        return;
                    }
                    else if (Offerer.GetPlay().ArmPieces < Cant)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("piezas", out Junk);
                        Session.SendWhisper("¡El ofertante ya no cuenta con las piezas ofrecidas!", 1);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Acepta la oferta de Piezas de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                        Session.SendWhisper("Has recibido " + Cant + " Pieza(s) por " + Offerer.GetHabbo().Username + ".", 1);
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("piezas", out Junk);
                        Session.GetHabbo().Credits -= Offer.Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetHabbo().Credits += Offer.Cost;
                        Offerer.GetPlay().MoneyEarned += Offer.Cost;
                        Offerer.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetPlay().ArmPieces -= Cant;
                        Session.GetPlay().ArmPieces += Cant;
                        return;
                    }
                }
                #endregion

                #region Platinos
                else if (Type.ToLower() == "platinos")
                {
                    var Offer = Session.GetPlay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    int Cant = Convert.ToInt32(Offer.Params[0]);

                    if (Offerer.GetRoomUser().RoomId != Room.Id)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("platinos", out Junk);
                        Session.SendWhisper("Lo sentimos, no te encuentras en la misma Zona que " + Offerer.GetHabbo().Username + " para aceptar su oferta de platinos.", 1);
                        return;
                    }
                    else if (Session.GetHabbo().Credits < Offer.Cost)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("platinos", out Junk);
                        Session.SendWhisper("¡Necesitas $" + Offer.Cost + " para aceptar los platinos!", 1);
                        return;
                    }
                    else if (Offerer.GetHabbo().Diamonds < Cant)
                    {
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("platinos", out Junk);
                        Session.SendWhisper("¡El ofertante ya no cuenta con los platinos ofrecidos!", 1);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Acepta la oferta de Platinos de " + Offerer.GetHabbo().Username + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 5);
                        Session.SendWhisper("Has recibido " + Cant + " Platino(s) por " + Offerer.GetHabbo().Username + ".", 1);
                        RoleplayOffer Junk;
                        Session.GetPlay().OfferManager.ActiveOffers.TryRemove("platinos", out Junk);
                        Session.GetHabbo().Credits -= Offer.Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetHabbo().Credits += Offer.Cost;
                        Offerer.GetPlay().MoneyEarned += Offer.Cost;
                        Offerer.GetHabbo().UpdateCreditsBalance();
                        Offerer.GetHabbo().Diamonds -= Cant;
                        Session.GetHabbo().UpdateDiamondsBalance();
                        Session.GetHabbo().Diamonds += Cant;
                        Session.GetPlay().PLEarned += Cant;
                        Offerer.GetHabbo().UpdateDiamondsBalance();
                        return;
                    }
                }
                #endregion
            }
            else
            {
                Session.SendWhisper("No tienes ninguna oferta de " + Type, 1);
                return;
            }
        }
    }
}