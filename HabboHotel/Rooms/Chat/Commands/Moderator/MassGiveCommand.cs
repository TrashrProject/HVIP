using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class MassGiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mass_give"; }
        }

        public string Parameters
        {
            get { return "%type% %amount%"; }
        }

        public string Description
        {
            get { return "Da recursos a todos los que estén en línea."; }
        }

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Debes escribir el tipo de recurso (dinero, saldo, platinos, gotw)");
                return;
            }

            var updateVal = Params[1];
            switch (updateVal.ToLower())
            {
                case "coins":
                case "dinero":
                case "credits":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                        {
                            session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                            break;
                        }
                        int amount;
                        if (int.TryParse(Params[2], out amount))
                        {
                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList().Where(client => client?.GetHabbo() != null && client.GetHabbo().Username != session.GetHabbo().Username))
                            {
                                client.GetHabbo().Credits = client.GetHabbo().Credits += amount;
                                client.GetPlay().MoneyEarned += amount;
                                client.SendMessage(new CreditBalanceComposer(client.GetHabbo().Credits));

                                if (client.GetHabbo().Id != session.GetHabbo().Id)
                                    client.SendNotification(session.GetHabbo().Username + " les ha dado $" + amount);
                                session.SendWhisper("Has dado $" + amount + " exitosamente a " + client.GetHabbo().Username + ".", 1);
                            }

                            break;
                        }
                        session.SendWhisper("Ingresa una cantidad válida.", 1);
                        break;
                    }

                case "pixels":
                case "duckets":
                case "saldo":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                        {
                            session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                            break;
                        }
                        int amount;
                        if (int.TryParse(Params[2], out amount))
                        {
                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList().Where(client => client?.GetHabbo() != null && client.GetHabbo().Username != session.GetHabbo().Username))
                            {
                                client.GetHabbo().Duckets += amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(
                                    client.GetHabbo().Duckets, amount));

                                if (client.GetHabbo().Id != session.GetHabbo().Id)
                                    client.SendNotification(session.GetHabbo().Username + " les ha dado $" + amount +
                                                            " de saldo móvil.");
                                session.SendWhisper("Has dado $" + amount + " de saldo móvil a " + client.GetHabbo().Username + ".", 1);
                            }
                            break;
                        }
                        session.SendWhisper("Ingresa una cantidad válida.", 1);
                        break;
                    }

                case "diamonds":
                case "platinos":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                        {
                            session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                            break;
                        }
                        int amount;
                        if (int.TryParse(Params[2], out amount))
                        {
                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList().Where(client => client?.GetHabbo() != null && client.GetHabbo().Username != session.GetHabbo().Username))
                            {
                                client.GetHabbo().Diamonds += amount;
                                client.GetPlay().PLEarned += amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().Diamonds,
                                    amount,
                                    5));

                                if (client.GetHabbo().Id != session.GetHabbo().Id)
                                    client.SendNotification(session.GetHabbo().Username + " les ha dado " + amount +
                                                            " platino(s)");
                                session.SendWhisper("Has dado " + amount + " platino(s) a " + client.GetHabbo().Username + ".", 1);
                            }

                            break;
                        }
                        session.SendWhisper("Ingresa una cantidad válida.", 1);
                        break;
                    }

                case "gotw":
                case "gotwpoints":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                        {
                            session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                            break;
                        }
                        int amount;
                        if (int.TryParse(Params[2], out amount))
                        {
                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList().Where(client => client?.GetHabbo() != null && client.GetHabbo().Username != session.GetHabbo().Username))
                            {
                                client.GetHabbo().GOTWPoints = client.GetHabbo().GOTWPoints + amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().GOTWPoints,
                                    amount, 103));

                                if (client.GetHabbo().Id != session.GetHabbo().Id)
                                    client.SendNotification(session.GetHabbo().Username + " les ha dado " + amount +
                                                            " GOTW Point(s).");
                                session.SendWhisper("Has dado " + amount + " GOTW point(s) a " + client.GetHabbo().Username + ".", 1);
                            }
                            break;
                        }
                        session.SendWhisper("Ingresa una cantidad válida.", 1);
                        break;
                    }
                default:
                    session.SendWhisper("'" + updateVal + "' no es un recurso válido.", 1);
                    break;
            }
        }
    }
}