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
    class ViewBotiquCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_botiqu"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Revisa el Botiquin para sacar el remedio que curará a tu paciente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Item Item = null;
            RoomUser User = Session.GetRoomUser();
            #endregion

            #region Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "botiquin"))
            {
                Session.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                return;
            }
            string MyCity = Room.City;
            int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom Data);//hospital de la cd.
            if (Session.GetHabbo().CurrentRoomId != HospID)
            {
                Session.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
                return;
            }
            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes comer mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes comer mientras estás encarcelad@!", 1);
                return;
            }

            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("¡No puedes comer mientras estás esposad@!", 1);
                return;
            }

            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y)
                {
                    if (item.BaseItem == 10280 || item.BaseItem == 10281)// ID del Botiquín
                    {
                        Item = item;
                    }
                }
            }
            
            if (Item == null)
            {
                Session.SendWhisper("No hay ningún botiquín en frente de ti.", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("botiq", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            string Boti = "";
            Boti += "Lista de remedios para pacientes. Selecciona uno usando :usarbotiquin (ID remedio)\n";
            Boti += "==========================\n               Botiquín\n==========================\n";//4 Tabs
            Boti += "[1] Pinzas, vendas, jeringa con anestesia\n";
            Boti += "[2] Pinzas, vendas, jeringa con morfina\n";
            Boti += "[3] Antiinflamatorios y yeso\n";
            Boti += "[4] Antiinflamatorios\n";
            Boti += "[5] Hilo, aguja, vendas, y suero fisiológico\n";
            Boti += "[6] Antiinflamatorios y hielo\n";
            Boti += "[7] Antiinflamatorios hielo y yeso\n";
            Boti += "[8] Bisturí, escalpelo, hilo, aguja y jeringa con morfina\n";
            Boti += "[9] Hielo, vendas y jeringa con morfina\n";
            Boti += "[10] Yeso, vendas, morfina y antiinflamatorios\n";
            Boti += "====================================================\n";
            Session.SendNotification(Boti);
            Session.GetPlay().CooldownManager.CreateCooldown("botiq", 1000, 3);
            #endregion
        }
    }
}