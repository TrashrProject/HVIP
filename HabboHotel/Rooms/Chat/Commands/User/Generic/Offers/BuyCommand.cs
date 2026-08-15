using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class BuyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_buy"; }
        }

        public string Parameters
        {
            get { return "%objeto% %cant%"; }
        }

        public string Description
        {
            get { return "Permite comprar objetos en su sitio respectivo: EJ: (repuestos,palanca,balde,martillo,materiales,semillas,etc)"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            string MyCity = Room.City;
            int Cant;
            bool OverrideConditions = false;
            #endregion

            #region Conditions
            if (Params.Length == 2)//:comprar [objeto] (Con cantidad predefinida)
            {
                if (Params[1].ToLower() == "materiales")
                    Cant = 50;
                else if (Params[1].ToLower() == "bidon")
                    Cant = 1;
                else if (Params[1].ToLower() == "vehiculo")
                    Cant = 1;
                else if (Params[1].ToLower() == "productos")// 24/7
                    Cant = 1;
                else if (Params[1].ToLower() == "herramientas")// Ferretería
                    Cant = 1;
                else if (Params[1].ToLower() == "semillas")// Granja
                    Cant = 1;
                else if (Params[1].ToLower() == "nivel")
                {
                    Cant = 1;
                    OverrideConditions = true;
                }
                else if (Params[1].ToLower() == "telefono" || Params[1].ToLower() == "celular")
                    Cant = 1;
                else if(Params[1].ToLower() == "repuestos")
                {
                    Session.SendWhisper("Debes ingresar la cantidad a comprar. ((:comprar [objeto] [cantidad]))", 1);
                    return;
                }
                else
                {
                    Session.SendWhisper("'" + Params[1].ToLower() + "' no es un objeto válido a comprar.", 1);
                    return;
                }
            }
            else
            {
                if (Params.Length != 3)
                {
                    Session.SendWhisper("Debes ingresar el nombre del objeto y cantidad. :comprar [objeto] [cantidad]", 1);
                    return;
                }
                if (!int.TryParse(Params[2], out Cant))
                {
                    Session.SendWhisper("¡Cantidad Inválida!", 1);
                    return;
                }
                else if (Cant < 1)
                {
                    Session.SendWhisper("Al menos debes de comprar un Objeto.", 1);
                    return;
                }

            }

            #region Basic Conditions
            if (!OverrideConditions)
            {
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
            }
            #endregion
            
            if (Session.GetPlay().TryGetCooldown("comprar", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            string Type = Params[1].ToLower();
            switch (Type)
            {
                #region Repuestos
                case "repuestos":
                    {
                        #region Conditions
                        int MecID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetMecanicos(MyCity, out PlayRoom Data);//mecanicos de la cd.
                        if (Session.GetHabbo().CurrentRoomId != MecID)
                        {
                            Session.SendWhisper("Debes ir al Taller de Mecánicos de la ciudad para comprar repuestos", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint02" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al punto de Venta de Repuestos del Taller.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        int RepairKitPrice = RoleplayManager.RepairKitPrice;
                        int Price = RepairKitPrice * Cant;

                        if((Session.GetPlay().MecParts + Cant) > 100)
                        {
                            Session.SendWhisper("¡No puedes llevar más de 100 repuestos en tu inventario!", 1);
                            return;
                        }
                        if(Session.GetHabbo().Credits < Price)
                        {
                            Session.SendWhisper("No cuentas con $"+ Price +" para comprar esos repuestos.", 1);
                            return;
                        }

                        RoleplayManager.Shout(Session, "*Compra "+ Cant + " repuestos y paga $"+ Price + " por ellos*", 5);
                        Session.SendWhisper("Has comprado " + Cant + " repuestos y pagaste $"+ Price, 1);
                        Session.GetHabbo().Credits -= Price;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.GetPlay().MecParts += Cant;
                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        #endregion
                    }
                    break;
                #endregion

                #region Materiales (only Gunners)
                case "materiales":
                    {
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

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "armero"))
                        {
                            Session.SendWhisper("Debes tener el trabajo de Armero para usar ese comando.", 1);
                            return;
                        }
                        // Puede trabajar aquí?
                        Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);
                        GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Job.Id, Session.GetPlay().JobRank);
                        if (!Rank.CanWorkHere(Room.Id))
                        {
                            //String.Join(",", Rank.WorkRooms)
                            Session.SendWhisper("¡Debes buscar el Punto de Venta de materiales! Podrías buscar cerca de los Barrios de la Ciudad.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al punto de Venta de Materiales.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        int Price = Cant * RoleplayManager.ArmMatPrice;
                        if((Session.GetPlay().ArmMat + Cant) > RoleplayManager.ArmMatLimit)
                        {
                            Session.SendWhisper("Solo puedes llevar " + RoleplayManager.ArmMatLimit + " materiales en tu inventario.", 1);
                            return;
                        }
                        // Does user has more credits than Price
                        if(Session.GetHabbo().Credits < Price)
                        {
                            Session.SendWhisper("Necesitas al menos $" + Price + " para comprar 50 materiales.", 1);
                            return;
                        }

                        RoleplayManager.Shout(Session, "*Compra 50 materiales y paga $" + Price + " por ellos*", 5);
                        Session.SendWhisper("Ahora dirígete a la Fábrica para preparar las piezas usando ':crear priezas'.", 1);
                        Session.GetHabbo().Credits -= Price;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.GetPlay().ArmMat += Cant;
                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        #endregion
                    }
                    break;
                #endregion

                #region Vehicles
                case "vehiculo":
                    {
                        #region Conditions
                        if (!Room.BuyCarEnabled)
                        {
                            Session.SendWhisper("Debes ir a un concesionario para comprar vehículos.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte a un despacho para comprar un vehículo.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "openshop");
                        break;
                        #endregion
                    }
                #endregion

                #region Productos
                case "productos":
                    {
                        #region Conditions
                        if (!Room.SupermarketEnabled)
                        {
                            Session.SendWhisper("Debes ir a un supermercado para comprar productos.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte a la caja para comprar productos.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "open_mall");
                        break;
                        #endregion
                    }
                #endregion

                #region Herramientas
                case "herramientas":
                    {
                        #region Conditions
                        if (!Room.FerreteriaEnabled)
                        {
                            Session.SendWhisper("Debes ir a una ferretería para comprar herramientas.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al mostrador para comprar herramientas.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "open_ferre");
                        break;
                        #endregion
                    }
                #endregion

                #region Bidon
                case "bidon":
                    {
                        #region Conditions
                        if (!Room.GasEnabled)
                        {
                            Session.SendWhisper("Debes ir a una Gasolinera para comprar Bidones.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al la despachadora de combustible.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        int L = 5;
                        int Price = (L * RoleplayManager.FuelPrice) + 10;// $20
                        
                        if (Session.GetHabbo().Credits < Price)
                        {
                            Session.SendWhisper("No cuentas con $" + Price + " para comprar un Bidón.", 1);
                            return;
                        }

                        RoleplayManager.Shout(Session, "*Compra un Bidón de 5 L de Combustible y paga $" + Price + " por él*", 5);
                        Session.SendWhisper("Has comprado un Bidón de 5 L y pagaste $" + Price, 1);
                        Session.GetHabbo().Credits -= Price;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.GetPlay().Bidon += Cant;
                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        #endregion
                    }
                    break;
                #endregion

                #region Teléfono
                case "telefono":
                case "celular":
                    {
                        #region V1 OFF
                        /*
                        #region Conditions
                        if (Session.GetPlay().Phone > 0)
                        {
                            Session.SendWhisper("Ya cuentas con un teléfono móvil.", 1);
                            return;
                        }
                        if (!Room.PhoneStoreEnabled)
                        {
                            Session.SendWhisper("Debes ir a una Tienda de Teléfonos.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al mostrador para comprar un teléfono.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        int Price = 1500;

                        if (Session.GetHabbo().Credits < Price)
                        {
                            Session.SendWhisper("No cuentas con $" + Price + " para comprar un Teléfono.", 1);
                            return;
                        }

                        RoleplayManager.Shout(Session, "*Compra un Teléfono nuevo y paga $" + Price + " por él*", 5);
                        Session.SendWhisper("Has comprado un Teléfono y pagaste $" + Price, 1);
                        Session.SendWhisper("Ahora podrás agregar contactos, enviar mensajes y realizar llamadas.", 1);
                        Session.GetHabbo().Credits -= Price;
                        Session.GetHabbo().UpdateCreditsBalance();

                        // Obtenemos Numero Random con Formato (xxx)-xxx-xxxx
                        String NewNumber = RoleplayManager.GeneratePhoneNumber(Session.GetHabbo().Id);
                        PlusEnvironment.GetGame().GetClientManager().RegisterClientPhone(Session.GetRoomUser().GetClient(), Session.GetHabbo().Id, NewNumber);
                        Session.SendWhisper("Tu número es: " + NewNumber + ". ((Para volverlo a consultar usa :minumero))", 1);
                        // Actualizamos información del teléfono.
                        Session.GetPlay().Phone = 1;
                        Session.GetPlay().PhoneNumber = NewNumber;
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "show_button");
                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        #endregion
                        */
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al despacho para comprar un teléfono.", 1);
                            return;
                        }
                        #endregion

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "open_shop_phone");
                    }
                    break;
                #endregion

                #region Nivel
                case "nivel":
                    {
                        #region Vars
                        int CurLevel = Session.GetPlay().Level;
                        int CurXP = Session.GetPlay().CurXP;
                        int NeedXP = RoleplayManager.GetInfoPD(CurLevel, "NeedXP");
                        int Cost = RoleplayManager.GetInfoPD((CurLevel), "Cost");
                        #endregion

                        #region Conditions
                        if (CurXP < NeedXP)
                        {
                            Session.SendWhisper("Aún no cuentas con la reputación suficiente para subir de nivel. ["+ CurXP +"/"+ NeedXP +"]", 1);
                            return;
                        }
                        if (Session.GetHabbo().Credits < Cost)
                        {
                            Session.SendWhisper("Necesitas $" + Cost + " para comprar el nuevo nivel.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        CurLevel++;
                        CurXP = 0;
                        Session.GetPlay().Level = CurLevel;
                        Session.GetPlay().CurXP = CurXP;

                        RoleplayManager.Shout(Session, "((Ha subido a nivel " + CurLevel + "))", 7);
                        Session.GetHabbo().Credits -= Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        RoleplayManager.SaveQuickStat(Session, "curxp", CurXP + "");
                        RoleplayManager.SaveQuickStat(Session, "level", CurLevel + "");
                        RoleplayManager.SaveQuickStat(Session, "needxp", RoleplayManager.GetInfoPD(CurLevel, "NeedXP") + "");
                        // Refrescamos WS
                        Session.GetPlay().UpdateInteractingUserDialogues();
                        Session.GetPlay().RefreshStatDialogue();
                        Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        #endregion
                    }
                    break;
                #endregion

                #region Semillas
                case "semillas":
                    {
                        #region Conditions
                        if (!Room.GrangeEnabled)
                        {
                            Session.SendWhisper("Debes ir a la Granja para comprar semillas.", 1);
                            return;
                        }
                        if (Session.GetPlay().FarmSeeds)
                        {
                            Session.SendWhisper("Ya tienes un puñado se semillas en tu inventario. ((Ahora ve a un surco y usa :sembrar))", 1);
                            return;
                        }
                        #endregion

                        bool WeedSeeds = false;

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint02" && x.Coordinate == Session.GetRoomUser().Coordinate);
                            if (BTile != null)
                                WeedSeeds = true;
                        }
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes acercarte al punto de venta de semillas.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        #region Semillas Cosechador
                        if (!WeedSeeds)
                        {
                            int Price = RoleplayManager.SeedsPrice;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("No cuentas con $" + Price + " para comprar semillas.", 1);
                                return;
                            }

                            RoleplayManager.Shout(Session, "*Compra un puñado de semillas y paga $" + Price + "*", 5);
                            Session.SendWhisper("Has comprado unas semillas y pagaste $" + Price + ". ((Ahora ve a un surco y usa :sembrar))", 1);
                            Session.GetHabbo().Credits -= Price;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetPlay().FarmSeeds = true;
                            Session.GetPlay().CooldownManager.CreateCooldown("comprar", 1000, 3);
                        }
                        #endregion

                        #region Semillas Weed
                        else
                        {

                        }
                        #endregion
                        #endregion
                    }
                    break;
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("'" + Type + "' no es un objeto válido a comprar.", 1);
                        break;
                    }
                    #endregion
            }
            #endregion
        }
    }
}