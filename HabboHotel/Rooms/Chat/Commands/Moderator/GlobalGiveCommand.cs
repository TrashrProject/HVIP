using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GlobalGiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_global_currency"; }
        }

        public string Parameters
        {
            get { return "%type% %amount%"; }
        }

        public string Description
        {
            get { return "Da recursos a todos."; }
        }

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Debes especificar el tipo de recurso (dinero, saldo, platinos, gotw).", 1);
                return;
            }

            string updateVal = Params[1];
            switch (updateVal.ToLower())
            {
                case "coins":
                case "credits":
                case "dinero":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                        {
                            session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                            break;
                        }
                        int amount;
                        if (int.TryParse(Params[2], out amount))
                        {
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                client.GetHabbo().Credits += amount;
                                client.GetPlay().MoneyEarned += amount;
                                client.SendMessage(new CreditBalanceComposer(client.GetHabbo().Credits));
                            }
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("UPDATE users SET credits = credits + " + amount);
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
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                client.GetHabbo().Duckets += amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(
                                    client.GetHabbo().Duckets, amount));
                            }
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("UPDATE users SET activity_points = activity_points + " + amount);
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
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                client.GetHabbo().Diamonds += amount;
                                client.GetPlay().PLEarned += amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().Diamonds,
                                    amount,
                                    5));
                            }
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("UPDATE users SET vip_points = vip_points + " + amount);
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
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                client.GetHabbo().GOTWPoints = client.GetHabbo().GOTWPoints + amount;
                                client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().GOTWPoints,
                                    amount, 103));
                            }
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("UPDATE users SET gotw_points = gotw_points + " + amount);
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