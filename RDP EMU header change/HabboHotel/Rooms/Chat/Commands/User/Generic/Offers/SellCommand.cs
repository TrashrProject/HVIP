using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using System.Data;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class SellCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_sell"; }
        }
        // :reparar [x] [precio]
        // :vender [x] [objeto] [precio]
        // :vender [x] [objeto] [cantidad] [precio] (consumibles)
        public string Parameters
        {
            get { return "%user% %obj% %cant% %price%"; }
        }

        public string Description
        {
            get { return "Vende algo a otra persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Vars
            int Dir = 0;
            string Params2 = "";
            string command = Params[0];
            #endregion

            #region Conditions

            #region Params Conditions
            if (Params.Length == 3)
            {
                Dir = 1;
                Params2 = "reparacion";
            }
            else if (Params.Length == 4)
            {
                Dir = 1;
                Params2 = Params[2];
            }
            else if (Params.Length == 5)
            {
                Dir = 2;
                Params2 = Params[2];
            }
            else if (Params.Length == 2)
            {
                Dir = 3;
                Params2 = Params[1];

                /*
                if(Params2.ToLower() != "objeto")
                {
                    Session.SendWhisper("Comando inválido, escribe ':vender objeto'", 1);
                    return;
                }
                */
            }
            else
            {
                if(command == "reparar")
                    Session.SendWhisper("Comando inválido, escribe :reparar [cliente] [precio]", 1);
                else
                    Session.SendWhisper("Comando inválido, escribe :vender [usuario] [objeto] [precio] o :vender [usuario] [objeto] [cantidad] [precio]", 1);
                return;
            }
            #endregion

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

            #region GetTarget (Si no es :vender objeto => Targer es el Params[1])
            GameClient Target = null;
            RoomUser TargetUser = null;
            if (Dir != 3)
            {
                Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                if (Target == null)
                {
                    Session.SendWhisper("No se ha podido encontrar al usuario.", 1);
                    return;
                }

                TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
                if (TargetUser == null)
                {
                    Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en la Zona.", 1);
                    return;
                }

                if (TargetUser.GetClient() == Session)
                {
                    Session.SendWhisper("¡No puedes venderte cosas a ti mism@!", 1);
                    return;
                }

                if (TargetUser.GetClient().GetConnection().getIp() == Session.GetConnection().getIp())
                {
                    Session.SendWhisper("¡No puedes vender/ofrecer servicios entre tus cuentas!", 1);
                    return;
                }
            }
            #endregion

            if (Session.GetPlay().TryGetCooldown("sell", true))
                return;
            #endregion

            if (Dir == 1)
            {
                #region :vender [x] [objeto] [precio]
                string Type = Params2;
                int Price = 0;

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
                            if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                            }
                            if (Target.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes venderle eso a una persona en modo pasivo.", 1);
                            }
                            if (Session.GetPlay().EquippedWeapon == null || Session.GetPlay().EquippedWeapon.Name.ToLower() != weapon.Name.ToLower())
                            {
                                Session.SendWhisper("Debes tener equipada el arma a vender.", 1);
                                break;
                            }
                            if (Target.GetPlay().OwnedWeapons.ContainsKey(weapon.Name))
                            {
                                Session.SendWhisper("Esta persona ya tiene un/a " + weapon.PublicName, 1);
                                break;
                            }
                            else
                            {
                                if (int.TryParse(Params[3], out Price))
                                {
                                    #region Conditions Price
                                    if (Price < 0)
                                    {
                                        Session.SendWhisper("El precio no puede ser negativo.", 1);
                                        return;
                                    }
                                    #endregion

                                    bool HasOffer = false;
                                    if (Target.GetHabbo().Credits >= Price)
                                    {
                                        foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                                        {
                                            if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                                                HasOffer = true;
                                        }
                                        if (!HasOffer)
                                        {
                                            RoleplayManager.Shout(Session, "*Ofrece un/a " + weapon.PublicName + " a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 5);
                                            Target.GetPlay().OfferManager.CreateOffer(weapon.Name.ToLower(), Session.GetHabbo().Id, Price);
                                            Target.SendWhisper("Te han ofrecido un/a " + weapon.PublicName + " por $" + String.Format("{0:N0}", Price) + " Escribe ':aceptar arma' para comprarla ó en su defecto ':rechazar arma'.", 1);
                                            Target.SendWhisper("Detalles del Arma: Uso: " + Session.GetPlay().WLife + " / " + weapon.WLife, 1);
                                            break;
                                        }
                                        else
                                        {
                                            Session.SendWhisper("¡Esta persona tiene una oferta de Arma pendiente!", 1);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Session.SendWhisper("Esta persona no tiene dinero suficiente para comprar tu " + weapon.PublicName, 1);
                                        break;
                                    }
                                }
                                else
                                {
                                    Session.SendWhisper("El precio es inválido", 1);
                                    return;
                                }

                            }
                        }
                    #endregion

                    #region Reparación
                    case "reparacion":
                        {
                            string Job = "";
                            if (command != "reparar")
                            {
                                Session.SendWhisper("Para ofrecer reparaciones de vehículos o armas (depende tu trabajo), usa: ':reparar [cliente] [precio]'", 1);
                                return;
                            }

                            if (int.TryParse(Params[2], out Price))
                            {
                                #region Conditions Price
                                if (Price < 0)
                                {
                                    Session.SendWhisper("El precio no puede ser negativo.", 1);
                                    return;
                                }
                                #endregion

                                #region Group Conditions
                                List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

                                if (Groups.Count <= 0)
                                {
                                    Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                                    return;
                                }

                                int GroupNumber = -1;

                                if (Groups[0].GType != 2)
                                {
                                    if (Groups.Count > 1)
                                    {
                                        if (Groups[1].GType != 2)
                                        {
                                            Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                                            return;
                                        }
                                        GroupNumber = 1; // Segundo indicie de variable
                                    }
                                    else
                                    {
                                        Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                                        return;
                                    }
                                }
                                else
                                {
                                    GroupNumber = 0; // Primer indice de Variable Group
                                }

                                Session.GetPlay().JobId = Groups[GroupNumber].Id;
                                Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
                                #endregion

                                #region Extra Conditions            
                                // Existe el trabajo?
                                if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
                                {
                                    Session.GetPlay().TimeWorked = 0;
                                    Session.GetPlay().JobId = 0; // Desempleado
                                    Session.GetPlay().JobRank = 0;

                                    //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                                    Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                                    return;
                                }

                                if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "reparar"))
                                {
                                    Session.SendWhisper("Tu trabajo actual no te permite hacer uso de ese comando. ¡Solo Mecánicos o Armeros!", 1);
                                    return;
                                }

                                if (!Session.GetPlay().IsWorking)
                                {
                                    if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "armero"))
                                    {
                                        Session.SendWhisper("Debes tener el uniforme de Mecánico para hacer eso.", 1);
                                        return;
                                    }
                                    Job = "armero";
                                }
                                else if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "mecanico"))
                                {
                                    Session.SendWhisper("Tu trabajo actual no te permite hacer uso de ese comando. ¡Solo Mecánicos o Armeros!", 1);
                                    return;
                                }
                                #endregion

                                if (Job != "armero")
                                {
                                    #region Mecánico
                                    if (RoleplayManager.PurgeEvent)
                                    {
                                        Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                                        return;
                                    }

                                    #region Conditions Mec
                                    foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "fix_mecanico")
                                        {
                                            Session.SendWhisper("Esa persona ya tiene una reparación de vehículo pendiente.", 1);
                                            return;
                                        }
                                    }
                                    if (!Target.GetPlay().PediMec)
                                    {
                                        Session.SendWhisper("Esa persona no ha solicitado los servicios de un Mecánico.", 1);
                                        return;
                                    }

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
                                            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == Vehicle.ItemName && x.Coordinate == Session.GetRoomUser().SquareInFront);
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
                                        Session.SendWhisper("¡Debes estar frente al vehículo a reparar!", 1);
                                        return;
                                    }

                                    #endregion

                                    #region Select Vehicle State
                                    int state = 0;
                                    List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(itemfurni);
                                    if (VO == null || VO.Count <= 0)
                                    {
                                        RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo que " + Session.GetHabbo().Username + " intentaba reparar.");
                                        RoleplayManager.PickItem(Session, itemfurni);
                                        return;
                                    }
                                    state = VO[0].State;
                                    #endregion

                                    #region Check Vehicle State
                                    if (state == 2 || state == 3 || VO[0].CarLife <= 0)
                                    {
                                        if (state == 3)
                                            Session.GetPlay().MecNewState = 1;// óptimo y con traba
                                        else
                                            Session.GetPlay().MecNewState = 0;// óptimo y sin traba
                                    }
                                    else
                                    {
                                        Session.SendWhisper("Este vehículo no necesita ser reparado.", 1);
                                        return;
                                    }
                                    #endregion

                                    #region Calc Repair Kit Cant
                                    if (FuelSize <= 90)// Tanque Pequeño
                                    {
                                        Session.GetPlay().MecPartsTo = 3;
                                    }
                                    else if (FuelSize > 90 && FuelSize <= 100)// Tanque Mediano
                                    {
                                        Session.GetPlay().MecPartsTo = 6;
                                    }
                                    else // Tanque Grande
                                    {
                                        Session.GetPlay().MecPartsTo = 9;
                                    }
                                    #endregion
                                    #endregion

                                    #region Execute
                                    if (Session.GetPlay().MecParts >= Session.GetPlay().MecPartsTo)
                                    {
                                        Session.GetPlay().MecCarToRepair = itemfurni;
                                        Session.GetPlay().MecRotPosition = Session.GetRoomUser().RotBody;
                                        Session.GetPlay().MecCordinates = Session.GetRoomUser().Coordinate;
                                        RoleplayManager.Shout(Session, "*Ofrece una reparación a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 5);
                                        Target.GetPlay().OfferManager.CreateOffer("fix_mecanico", Session.GetHabbo().Id, Price);
                                        Target.SendWhisper("Te han ofrecido una reparación de Vehículo por $" + String.Format("{0:N0}", Price) + ". Escribe ':aceptar reparacion' para aceptarla ó en su defecto ':rechazar reparacion'.", 1);
                                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                                    }
                                    else
                                        Session.SendWhisper("Necesitas " + Session.GetPlay().MecPartsTo + " repuestos para reparar este vehículo.", 1);
                                    #endregion
                                    #endregion
                                }
                                else {
                                    #region Armero
                                    #region Conditions Arm
                                    if (!Target.GetPlay().PediArm)
                                    {
                                        Session.SendWhisper("Esa persona no ha solicitado los servicios de un Armero.", 1);
                                        return;
                                    }
                                    foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "fix_armero")
                                        {
                                            Session.SendWhisper("Esa persona ya tiene una reparación de arma pendiente.", 1);
                                            return;
                                        }
                                    }
                                    if (Target.GetPlay().EquippedWeapon == null)
                                    {
                                        Session.SendWhisper("Esa persona no lleva ningún arma Equipada a ser reparada.", 1);
                                        return;
                                    }
                                    if (Target.GetPlay().WLife > 0)
                                    {
                                        Session.SendWhisper("Esa arma no necesita una reparación.", 1);
                                        return;
                                    }
                                    if (Session.GetPlay().ArmPiecesTo <= 0 || Session.GetPlay().ArmUserTo != Target.GetHabbo().Id)
                                    {
                                        Session.SendWhisper("Primero debes :revisar el arma de " + Target.GetHabbo().Username, 1);
                                        return;
                                    }
                                    if (Session.GetPlay().ArmPieces < Session.GetPlay().ArmPiecesTo)
                                    {
                                        Session.SendWhisper("Necesitas " + Session.GetPlay().ArmPiecesTo + " pieza(s) para reparar esa arma.", 1);
                                        return;
                                    }
                                    #endregion

                                    #region Execute
                                    RoleplayManager.Shout(Session, "*Ofrece una reparación de Arma a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 5);
                                    Target.GetPlay().OfferManager.CreateOffer("fix_armero", Session.GetHabbo().Id, Price);
                                    Target.SendWhisper("Te han ofrecido una reparación de Arma por $" + String.Format("{0:N0}", Price) + ". Escribe ':aceptar reparacion' para aceptarla o en su defecto ':rechazar reparacion'.", 1);
                                    Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                                    #endregion
                                    #endregion
                                }
                            }
                            else
                                Session.SendWhisper("El precio es inválido", 1);
                        }
                        break;
                    #endregion

                    #region Default
                    default:
                        {
                            Session.SendWhisper("'" + Type + "' no es una oferta válida.", 1);
                            break;
                        }
                        #endregion
                }
                #endregion
            }
            else if (Dir == 2)
            {
                #region Consumibles :vender [x] [objetos] [cantidad] [precio]
                string Type = Params2;
                int Cant = 0;
                int Cost = 0;

                #region Conditions
                if (!int.TryParse(Params[3], out Cant))
                {
                    Session.SendWhisper("La cantidad no es válida.", 1);
                    return;
                }
                if (!int.TryParse(Params[4], out Cost))
                {
                    Session.SendWhisper("El precio no es válido.", 1);
                    return;
                }
                if (Cant <= 0)
                {
                    Session.SendWhisper("La cantidad debe ser mayor a 0.", 1);
                    return;
                }
                if (Cost < 0)
                {
                    Session.SendWhisper("El precio no puede ser negativo.", 1);
                    return;
                }
                #endregion

                switch (Type.ToLower())
                {
                    #region Medicamentos
                    case "medicamentos":
                    case "medicamento":
                        {
                            if (Session.GetPlay().Medicines < Cant)
                            {
                                Session.SendWhisper("No tienes " + Cant + " medicamento(s) para vender.", 1);
                                return;
                            }

                            if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                return;
                            }
                            if (Target.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes venderle eso a una persona en modo pasivo.", 1);
                                return;
                            }

                            #region Offer Conditions
                            foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == "medicamentos")
                                {
                                    Session.SendWhisper("Esa persona ya tiene una oferta de medicamentos pendiente.", 1);
                                    return;
                                }
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Session, "*Ofrece " + Cant + " medicamento(s) a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", 5);
                            Target.GetPlay().OfferManager.CreateOffer("medicamentos", Session.GetHabbo().Id, Cost, Cant);
                            Target.SendWhisper("Te han ofrecido " + Cant + " medicamento(s) por $" + String.Format("{0:N0}", Cost) + ". Escribe ':aceptar medicamentos' para aceptarlos en su defecto ':rechazar medicamentos'.", 1);
                            Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                            #endregion
                        }
                        break;
                    #endregion

                    #region Crack
                    case "crack":
                        {
                            if (Session.GetPlay().Cocaine < Cant)
                            {
                                Session.SendWhisper("No tienes " + Cant + " g. de crack para vender.", 1);
                                return;
                            }

                            if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                return;
                            }
                            if (Target.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes venderle eso a una persona en modo pasivo.", 1);
                                return;
                            }

                            #region Offer Conditions
                            foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == "crack")
                                {
                                    Session.SendWhisper("Esa persona ya tiene una oferta de crack pendiente.", 1);
                                    return;
                                }
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Session, "*Ofrece " + Cant + " g. de crack a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", 5);
                            Target.GetPlay().OfferManager.CreateOffer("crack", Session.GetHabbo().Id, Cost, Cant);
                            Target.SendWhisper("Te han ofrecido " + Cant + " g. de crack por $" + String.Format("{0:N0}", Cost) + ". Escribe ':aceptar crack' para aceptarlos en su defecto ':rechazar crack'.", 1);
                            Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                            #endregion
                        }
                        break;
                    #endregion

                    #region Piezas
                    case "piezas":
                    case "pieza":
                        {
                            if (Session.GetPlay().ArmPieces < Cant)
                            {
                                Session.SendWhisper("No tienes " + Cant + " piezas de armas para vender.", 1);
                                return;
                            }

                            if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                return;
                            }
                            if (Target.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes venderle eso a una persona en modo pasivo.", 1);
                                return;
                            }

                            #region Offer Conditions
                            foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == "piezas")
                                {
                                    Session.SendWhisper("Esa persona ya tiene una oferta de piezas de armas pendiente.", 1);
                                    return;
                                }
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Session, "*Ofrece " + Cant + " piezas de armas a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", 5);
                            Target.GetPlay().OfferManager.CreateOffer("piezas", Session.GetHabbo().Id, Cost, Cant);
                            Target.SendWhisper("Te han ofrecido " + Cant + " piezas de armas por $" + String.Format("{0:N0}", Cost) + ". Escribe ':aceptar piezas' para aceptarlos en su defecto ':rechazar piezas'.", 1);
                            Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                            #endregion
                        }
                        break;
                    #endregion

                    #region Platinos
                    case "platinos":
                    case "platino":
                        {
                            if (Session.GetHabbo().Diamonds < Cant)
                            {
                                Session.SendWhisper("No tienes " + Cant + " platino(s) para vender.", 1);
                                return;
                            }

                            #region Offer Conditions
                            foreach (var Offer in Target.GetPlay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == "platinos")
                                {
                                    Session.SendWhisper("Esa persona ya tiene una oferta de platinos pendiente.", 1);
                                    return;
                                }
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Session, "*Ofrece " + Cant + " platino(s) a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", 5);
                            Target.GetPlay().OfferManager.CreateOffer("platinos", Session.GetHabbo().Id, Cost, Cant);
                            Target.SendWhisper("Te han ofrecido " + Cant + " platino(s) por $" + String.Format("{0:N0}", Cost) + ". Escribe ':aceptar platinos' para aceptarlos o en su defecto ':rechazar platinos'.", 1);
                            Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                            #endregion
                        }
                        break;
                    #endregion

                    #region Default
                    default:
                        {
                            Session.SendWhisper("'" + Type + "' no es una oferta válida.", 1);
                            break;
                        }
                        #endregion
                }
                #endregion
            }
            else
            {
                string Type = Params2;
                switch (Type.ToLower())
                {
                    #region :vender objeto
                    case "objeto":
                        {
                            if (!Room.RobStoreEnabled)
                            {
                                Session.SendWhisper("Debes estar en una Tienda de Venta de Objetos Robados para hacer eso.", 1);
                                return;
                            }

                            #region Comodin Conditions
                            Item BTile = null;
                            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                            if (BTile == null)
                            {
                                Session.SendWhisper("Debes estar cerca del Mostrador para vender objetos.", 1);
                                return;
                            }
                            #endregion

                            if (Session.GetPlay().Object.Length <= 0)
                            {
                                Session.SendWhisper("No tienes ningún objeto en el Inventario para vender.", 1);
                                return;
                            }

                            if (Session.GetPlay().PassiveMode)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                return;
                            }

                            RoleplayManager.Shout(Session, "*Vende un/a " + Session.GetPlay().Object + "*", 5);
                            Session.SendWhisper("Has vendido un/a " + Session.GetPlay().Object + " y recibes $ " + String.Format("{0:N0}", Session.GetPlay().ObjectPrice), 1);
                            Session.GetHabbo().Credits += Session.GetPlay().ObjectPrice;
                            Session.GetPlay().MoneyEarned += Session.GetPlay().ObjectPrice;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetPlay().Object = "";
                            Session.GetPlay().ObjectPrice = 0;
                        }
                        break;
                    #endregion

                    #region :vender vehículo
                    case "vehiculo":
                        {
                            if (!Room.MunicipalidadEnabled)
                            {
                                Session.SendWhisper("Debes estar en la Municipalidad de la ciudad para hacer eso.", 1);
                                return;
                            }
                            if (Session.GetPlay().DrivingInCar)
                            {
                                Session.SendWhisper("¡No puedes hacer eso mientras tengas un vehículo en marcha afuera!", 1);
                                return;
                            }

                            #region Action Point Conditions
                            Item BTile = null;
                            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                            if (BTile == null)
                            {
                                Session.SendWhisper("Debes acercarte al mostrador para vender vehículos.", 1);
                                return;
                            }
                            #endregion

                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "close");
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "open_sellcar");
                        }
                        break;
                    #endregion

                    #region Default
                    default:
                        {
                            Session.SendWhisper("'"+Type+"' no es un elemento válido a vender.", 1);
                        }
                        break;
                    #endregion
                }
            }
        }
    }
}