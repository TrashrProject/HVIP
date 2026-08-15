using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class NCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_chanel"; }
        }

        public string Parameters
        {
            get { return "%msg%"; }
        }

        public string Description
        {
            get { return "Para hablar en el canal dudas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("((Ingresa una duda. Recuerda que este canal es solo para dudas. Mal uso es sancionable))", 1);
                return;
            }
            if (Session.GetPlay().ChNDisabled)
            {
                Session.SendWhisper("((No puedes usar el Canal :n mientras lo tengas desactivado))", 1);
                return;
            }
            if (Session.GetPlay().ChNBanned)
            {
                Session.SendWhisper("((No tienes permitido usar el Canal :n hasta dentro de "+ Session.GetPlay().ChNBannedTimeLeft +" minuto(s) ))", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("ncommand", true))
                return;
            #endregion

            #region Execute
            string Message = CommandManager.MergeParams(Params, 1);

            if (Session.GetHabbo().Translating)
            {
                string LG1 = Session.GetHabbo().FromLanguage.ToLower();
                string LG2 = Session.GetHabbo().ToLanguage.ToLower();

                PlusEnvironment.GetGame().GetClientManager().CanalN(PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", Session);
            }
            else
                PlusEnvironment.GetGame().GetClientManager().CanalN(Message, Session);

            if(Session.GetHabbo().Rank < 3)
                Session.GetPlay().CooldownManager.CreateCooldown("ncommand", 1000, 60);
            else
                Session.GetPlay().CooldownManager.CreateCooldown("ncommand", 1000, 3);
            #endregion
        }
    }
}