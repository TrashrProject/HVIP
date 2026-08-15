using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.PhonesApps;
using Plus.HabboHotel.RolePlay.PlayInternet;
using Plus.HabboRoleplay.Comodin;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class UpdateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_update"; }
        }

        public string Parameters
        {
            get { return "%variable%"; }
        }

        public string Description
        {
            get { return "Refresca algo del servidor."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar una variable a refrescar. ((Usa :update list))", 1);
                return;
            }

            string UpdateVariable = Params[1];
            switch (UpdateVariable.ToLower())
            {
                case "list":
                    {
                        StringBuilder List = new StringBuilder();
                        List.Append("---------- Variables para refrescar ----------\n\n");
                        List.Append("catalogue\n");
                        List.Append("items\n");
                        List.Append("playrooms\n");
                        List.Append("playdata\n");
                        List.Append("groups\n");
                        List.Append("gangturfs\n");
                        List.Append("houses\n");
                        List.Append("models\n");
                        List.Append("promotions\n");
                        List.Append("youtube\n");
                        List.Append("filter\n");
                        List.Append("navigator\n");
                        List.Append("ranks\n");
                        List.Append("config\n");
                        List.Append("bans\n");
                        List.Append("quest\n");
                        List.Append("achievements\n");
                        List.Append("moderation\n");
                        List.Append("tickets\n");
                        List.Append("vouchers\n");
                        List.Append("gamecenter\n");
                        List.Append("pet_locale\n");
                        List.Append("locale\n");
                        List.Append("mutant\n");
                        List.Append("bots\n");
                        List.Append("rewards\n");
                        List.Append("chat_styles\n");
                        List.Append("badge_definitions\n");
                        List.Append("offers\n");
                        Session.SendNotification(List.ToString());
                        break;
                    }
                case "cata":
                case "catalog":
                case "catalogue":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
                        PlusEnvironment.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                        Session.SendWhisper("Catalogo actualizado con éxito.", 1);
                        break;
                    }
                case "items":
                case "furni":
                case "furniture":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_furni"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetItemManager().Init();
                        Session.SendWhisper("Items actualizados con éxito.", 1);
                        break;
                    }
                case "playrooms":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_playrooms"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }
                        
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("Play Rooms actualizados con éxito.", 1);
                        break;
                    }
                case "playdata":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_playrooms"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        RoleplayData.Initialize();
                        RoleplayManager.ReloadData();
                        Session.SendWhisper("Play Data actualizada con éxito.", 1);
                        break;
                    }
                case "phoneapps":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_playphones"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PhoneAppManager.Initialize();
                        Session.SendWhisper("Phone Apps actualizadas con éxito.", 1);
                        break;
                    }
                case "comodin":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_comodin"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        ComodinManager.Initialize();
                        Session.SendWhisper("Comodines actualizados con éxito.", 1);
                        break;
                    }
                case "internet":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_playrooms"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlayInternetManager.Init();
                        Session.SendWhisper("Páginas de internter actualizadas con éxito.", 1);
                        break;
                    }
                case "groups":
                    {
                        
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.");
                            break;
                        }
                        
                        PlusEnvironment.GetGame()._groupManager.Init();
                        PlusEnvironment.GetGame()._groupForumManager.Init();
                        //Session.SendWhisper("Desactivado para evitar bugs. Se recomienda reiniciar Emulador. No se hicieron cambios.", 1);
                        break;
                    }
                case "gangturfs":
                    {

                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.");
                            break;
                        }

                        PlusEnvironment.GetGame().GetGangTurfsManager().Init();
                        break;
                    }
                case "houses":
                case "house":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_houses"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetHouseManager().Init();
                        Session.SendWhisper("Casas actualizadas con éxito.", 1);
                        break;
                    }
                case "models":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_models"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetRoomManager().LoadModels();
                        Session.SendWhisper("Room models actualizados con éxito.", 1);
                        break;
                    }
                case "promotions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_promotions"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetLandingManager().LoadPromotions();
                        Session.SendWhisper("Landing view promotions actualizados con éxito.", 1);
                        break;
                    }
                case "youtube":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_youtube"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetTelevisionManager().Init();
                        Session.SendWhisper("Youtube televisions playlist actualizada con éxito.", 1);
                        break;
                    }
                case "filter":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_filter"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
                        Session.SendWhisper("Filtros de chat actualizados con éxito.", 1);
                        break;
                    }
                case "navigator":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_navigator"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetNavigator().Init();
                        Session.SendWhisper("Navegador actualizado con éxito.", 1);
                        break;
                    }
                case "ranks":
                case "rights":
                case "permissions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_rights"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetPermissionManager().Init();

                        foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetPermissions() == null)
                                continue;

                            Client.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }

                        Session.SendWhisper("Rangos actualizados con éxito.", 1);
                        break;
                    }
                case "config":
                case "settings":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_configuration"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.ConfigData = new ConfigData();
                        Session.SendWhisper("Configuración sel servidor actualizada con éxito.", 1);
                        break;
                    }
                case "bans":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bans"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
                        Session.SendWhisper("Baneos actualizados con éxito.", 1);
                        break;
                    }
                case "quests":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_quests"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetQuestManager().Init();
                        Session.SendWhisper("Quest actualizados con éxito.", 1);
                        break;
                    }
                case "achievements":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_achievements"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetAchievementManager().LoadAchievements();
                        Session.SendWhisper("Achievement actualizados con éxito.", 1);
                        break;
                    }
                case "moderation":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_moderation"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetModerationManager().Init();
                        PlusEnvironment.GetGame().GetClientManager().ModAlert("Los presets de la Moderación han sido actualizados. Recarga el client para ver los nuevos cambios.");

                        Session.SendWhisper("Configuración de Moderación actualizada con éxito.", 1);
                        break;
                    }
                case "tickets":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_tickets"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        if (PlusEnvironment.GetGame().GetModerationTool().Tickets.Count > 0)
                            PlusEnvironment.GetGame().GetModerationTool().Tickets.Clear();

                        PlusEnvironment.GetGame().GetClientManager().ModAlert("Los Tickets de Moderación fueron refrescados. Recarga el client.");
                        Session.SendWhisper("Tickets actualizados con éxito.", 1);
                        break;
                    }
                case "vouchers":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_vouchers"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
                        Session.SendWhisper("Códigos Vourcher actualizados con éxito.", 1);
                        break;
                    }
                case "gc":
                case "games":
                case "gamecenter":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_game_center"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetGameDataManager().Init();
                        Session.SendWhisper("Game Center actualizado con éxito.", 1);
                        break;
                    }
                case "pet_locale":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_pet_locale"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetPetLocale().Init();
                        Session.SendWhisper("Pet locale actualizado con éxito.", 1);
                        break;
                    }
                case "locale":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_locale"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetLanguageLocale().Init();
                        Session.SendWhisper("Locale actualizado con éxito.", 1);
                        break;
                    }
                case "mutant":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_anti_mutant"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetAntiMutant().Init();
                        Session.SendWhisper("Anti mutant actualizado con éxito.", 1);
                        break;
                    }
                case "bots":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bots"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetBotManager().Init();
                        Session.SendWhisper("Bot managaer actualizado con éxito.", 1);
                        break;
                    }
                case "rewards":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_rewards"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetRewardManager().Reload();
                        Session.SendWhisper("Rewards managaer actualizado con éxito.", 1);
                        break;
                    }
                case "chat_styles":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_chat_styles"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetChatStyles().Init();
                        Session.SendWhisper("Chat Styles actualizados con éxito.", 1);
                        break;
                    }
                case "badge_definitions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_badge_definitions"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetBadgeManager().Init();
                        Session.SendWhisper("Badge definitions actualizados con éxito.", 1);
                        break;
                    }
                case "offers":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_offers"))
                        {
                            Session.SendWhisper("No tienes permiso para refrescar eso.", 1);
                            break;
                        }

                        PlusEnvironment.ConfigData = new ConfigData();
                        Session.SendWhisper("Configuración de servidor para ofertas actualizada con éxito.", 1);
                        PlusEnvironment.offers = new TargetedOffers();
                        if (PlusEnvironment.GetDBConfig().DBData["targeted_offers_enabled"] == "1")
                        {
                            foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (Client == null || Client.GetHabbo() == null)
                                    continue;

                                Client.SendMessage(new TargetedOffersComposer());
                            }
                        }
                            
                        
                        break;
                    }
                default:
                    Session.SendWhisper("'" + UpdateVariable + "' no es una variable válida.", 1);
                    break;
            }
        }
    }
}
