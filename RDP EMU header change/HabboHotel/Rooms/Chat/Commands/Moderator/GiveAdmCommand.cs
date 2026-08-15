using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GiveAdmCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give"; }
        }

        public string Parameters
        {
            get { return "%username% %type% %amount%"; }
        }

        public string Description
        {
            get { return "Da recursos a una persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar el nombre de la persona y el tipo de recurso (dinero, saldo, platinos, gotw)", 1);
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            string UpdateVal = Params[2];
            switch (UpdateVal.ToLower())
            {
                case "coins":
                case "credits":
                case "dinero":
                    {
                        // MegaDude
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_coins") && Session.GetHabbo().Id != 9447)
                        {
                            Session.SendWhisper("No tienes permisos para usar este comando.", 1);
                            break;
                        }
                        else
                        {
                            int Amount;
                            if (int.TryParse(Params[3], out Amount))
                            {
                                Target.GetHabbo().Credits = Target.GetHabbo().Credits += Amount;
                                Target.GetPlay().MoneyEarned += Amount;
                                Target.SendMessage(new CreditBalanceComposer(Target.GetHabbo().Credits));

                                if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                                    Target.SendNotification(Session.GetHabbo().Username + " te ha dado $" + Amount.ToString() + ".");
                                Session.SendWhisper("Has entregado $" + Amount + " a " + Target.GetHabbo().Username + ".", 1);

                                Target.GetHabbo().UpdateCreditsBalance();

                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Ingresa una cantidad válida.", 1);
                                break;
                            }
                        }
                    }

                case "pixels":
                case "duckets":
                case "saldo":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                        {
                            Session.SendWhisper("No tienes permisos para usar este comando.", 1);
                            break;
                        }
                        else
                        {
                            int Amount;
                            if (int.TryParse(Params[3], out Amount))
                            {
                                Target.GetHabbo().Duckets += Amount;
                                Target.SendMessage(new HabboActivityPointNotificationComposer(Target.GetHabbo().Duckets, Amount));

                                if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                                    Target.SendNotification(Session.GetHabbo().Username + " te ha dado $" + Amount.ToString() + " de saldo móvil.");
                                Session.SendWhisper("Has entregado $" + Amount + " de saldo móvil a " + Target.GetHabbo().Username + ".", 1);
                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Ingresa una canidad válida.", 1);
                                break;
                            }
                        }
                    }

                case "diamonds":
                case "platinos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                        {
                            Session.SendWhisper("No tienes permisos para usar este comando.", 1);
                            break;
                        }
                        else
                        {
                            int Amount;
                            if (int.TryParse(Params[3], out Amount))
                            {
                                Target.GetHabbo().Diamonds += Amount;
                                //Target.GetPlay().PLEarned += Amount;
                                Target.SendMessage(new HabboActivityPointNotificationComposer(Target.GetHabbo().Diamonds, Amount, 5));

                                if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                                    Target.SendNotification(Session.GetHabbo().Username + " te ha dado " + Amount.ToString() + " platino(s).");
                                Session.SendWhisper("Has entregado " + Amount + " platino(s) a " + Target.GetHabbo().Username + ".", 1);

                                Target.GetHabbo().UpdateDiamondsBalance();
                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Ingresa una cantidad válida.", 1);
                                break;
                            }
                        }
                    }

                case "gotw":
                case "gotwpoints":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                        {
                            Session.SendWhisper("No tienes permisos para usar este comando.", 1);
                            break;
                        }
                        else
                        {
                            int Amount;
                            if (int.TryParse(Params[3], out Amount))
                            {
                                Target.GetHabbo().GOTWPoints = Target.GetHabbo().GOTWPoints + Amount;
                                Target.SendMessage(new HabboActivityPointNotificationComposer(Target.GetHabbo().GOTWPoints, Amount, 103));

                                if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                                    Target.SendNotification(Session.GetHabbo().Username + " te ha dado " + Amount.ToString() + " GOTW Point(s).");
                                Session.SendWhisper("Has entregado " + Amount + " GOTW point(s) a " + Target.GetHabbo().Username + ".", 1);
                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Ingresa una cantidad válida.", 1);
                                break;
                            }
                        }
                    }
                default:
                    Session.SendWhisper("'" + UpdateVal + "' no es un recurso válido.", 1);
                    break;
            }
        }
    }
}