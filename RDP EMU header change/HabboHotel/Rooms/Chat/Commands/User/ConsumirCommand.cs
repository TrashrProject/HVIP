using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;


using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Polls;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Polls.Questions;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ConsumirCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_consumir"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Consume algún estupefaciente para ganas stats."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {

            if (Params.Length != 2)
            {
                Session.SendWhisper("Comando inválido, debes especificar el consumible. ((:consumir [crack/medicamentos]))", 1);
                return;
            }
            string Type = Params[1].ToLower();

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes consumir ningún tipo de droga mientras estás en modo pasivo.", 1);
                return;
            }

            switch (Type)
            {
                #region Crack
                case "crack":
                    {
                        if (Session.GetPlay().TryGetCooldown("crack"))
                            return;

                        if (Session.GetPlay().Cocaine <= 0)
                        {
                            Session.SendWhisper("No tienes crack para consumir.", 1);
                            return;
                        }

                        if (Session.GetPlay().Level == 1 && Session.GetPlay().CurXP < 3)
                        {
                            Session.SendWhisper("¡Necesitas al menos 3 puntos de reputación (XP) para consumir crack!", 1);
                            return;
                        }

                        Session.GetPlay().Cocaine--;

                        if ((Session.GetPlay().Armor + 10) < 100)
                            Session.GetPlay().Armor += 10;
                        else
                            Session.GetPlay().Armor = 100;

                        RoleplayManager.Shout(Session, "*Consume 1 g. de Crack*", 13);
                        Session.SendWhisper("Tu Armadura ha incrementado por los efectos del Crack.", 1);

                        // Refrescamos WS
                        Session.GetPlay().UpdateInteractingUserDialogues();
                        Session.GetPlay().RefreshStatDialogue();
                        Session.GetPlay().DrugsTaken++;
                        Session.GetPlay().CooldownManager.CreateCooldown("crack", 1000, 30);
                    }
                    break;
                #endregion

                #region Medicamentos
                case "medicamentos":
                case "medicamento":
                    {
                        if (Session.GetPlay().TryGetCooldown("medicamentos"))
                            return;

                        if (Session.GetPlay().Medicines <= 0)
                        {
                            Session.SendWhisper("No tienes medicamentos para consumir.", 1);
                            return;
                        }

                        Session.GetPlay().Medicines--;

                        if ((Session.GetPlay().CurHealth + 10) < 100)
                            Session.GetPlay().CurHealth += 10;
                        else
                            Session.GetPlay().CurHealth = 100;

                        RoleplayManager.Shout(Session, "*Consume 1 medicamento*", 13);
                        Session.SendWhisper("Tu Salud ha incrementado por los efectos del medicamento.", 1);

                        // Refrescamos WS
                        Session.GetPlay().UpdateInteractingUserDialogues();
                        Session.GetPlay().RefreshStatDialogue();
                        Session.GetPlay().DrugsTaken++;
                        Session.GetPlay().CooldownManager.CreateCooldown("medicamentos", 1000, 30);
                    }
                    break;
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("'"+ Type +"' no es un consumible válido.", 1);
                    }
                    break;
                #endregion
            }

        }
    }
}
