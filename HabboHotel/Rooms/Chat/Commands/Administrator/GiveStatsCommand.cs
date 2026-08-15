using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class GiveStatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_stats"; }
        }

        public string Parameters
        {
            get { return "%user% %stat% %level%"; }
        }

        public string Description
        {
            get { return "Da un nivel específico de stats a otro usuario."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length < 4)
            {
                Session.SendWhisper("Comando inválido, escribe ':givestats [usuario] [Stat] [Porcentaje]'", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            if (TargetClient.GetPlay().IsSanc)
            {
                Session.SendWhisper("No puedes ajustar las estadísticas de esa persona porque se encuentra sancionada.", 1);
                return;
            }

            int lvl;
            if (!int.TryParse(Params[3], out lvl))
            {
                Session.SendWhisper("¡Debes ingresar un nivel de porcentaje válido!", 1);
                return;
            }

            if (lvl < 0 || lvl > 100)
            {
                Session.SendWhisper("¡El porcentaje debe ser entre 0 y 100!", 1);
                return;
            }
            #endregion

            #region Execute
            switch (Params[2].ToLower())
            {
                #region Vida
                case "vida":
                    {
                        TargetClient.GetPlay().CurHealth = lvl;
                    }
                    break;
                #endregion

                #region Chaleco
                case "chaleco":
                    {
                        TargetClient.GetPlay().Armor = lvl;
                    }
                    break;
                #endregion

                #region Hambre
                case "hambre":
                    {
                        TargetClient.GetPlay().Hunger = lvl;
                    }
                    break;
                #endregion

                #region Higiene
                case "higiene":
                    {
                        TargetClient.GetPlay().Hygiene = lvl;
                    }
                    break;
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("Stat no válido. (vida/chaleco/hambre/higiene)", 1);
                        return;
                    }
                #endregion
            }


            Session.SendWhisper("((Has establecido el Stat de '" + Params[2].ToLower() + "' de "+ TargetClient.GetHabbo().Username + " a " + lvl + "%))", 1);

            TargetClient.SendWhisper("((Te han establecido tu Stat de '"+ Params[2].ToLower() +"' a " + lvl + "%))", 1);
            // Refrescamos WS
            TargetClient.GetPlay().UpdateInteractingUserDialogues();
            TargetClient.GetPlay().RefreshStatDialogue();
            #endregion
        }
    }
}