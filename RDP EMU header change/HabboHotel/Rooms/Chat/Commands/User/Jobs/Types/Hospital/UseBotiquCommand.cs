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
    class UseBotiquCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_usebotiqu"; }
        }

        public string Parameters
        {
            get { return "%id%"; }
        }

        public string Description
        {
            get { return "Saca algo del Botiquin para aplicarselo a tu paciente."; }
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
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el ID del remedio. ((Usa :verbotiquin para ver todas las ID))", 1);
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
            int heri;
            if (!int.TryParse(Params[1], out heri))
            {
                Session.SendWhisper("((Ingresa un ID válida))", 1);
                return;
            }
            
            #region Heridas
            if (heri <= 0 || heri > 10)
            {
                Session.SendWhisper("Ese remedio no existe.", 1);
                return;
            }
            if (heri == 1)
            {
                Session.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con anestesia";
                Session.GetPlay().BotiquinDoc = 1;
            }
            else if (heri == 2)
            {
                Session.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con morfina";
                Session.GetPlay().BotiquinDoc = 2;
            }
            else if (heri == 3)
            {
                Session.GetPlay().BotiquinName = "Antiinflamatorios y yeso";
                Session.GetPlay().BotiquinDoc = 3;
            }
            else if (heri == 4)
            {
                Session.GetPlay().BotiquinName = "Antiinflamatorios";
                Session.GetPlay().BotiquinDoc = 4;
            }
            else if (heri == 5)
            {
                Session.GetPlay().BotiquinName = "Hilo, aguja, vendas, y suero fisiológico";
                Session.GetPlay().BotiquinDoc = 5;
            }
            else if (heri == 6)
            {
                Session.GetPlay().BotiquinName = "Antiinflamatorios y hielo";
                Session.GetPlay().BotiquinDoc = 6;
            }
            else if (heri == 7)
            {
                Session.GetPlay().BotiquinName = "Antiinflamatorios hielo y yeso es";
                Session.GetPlay().BotiquinDoc = 7;
            }
            else if (heri == 8)
            {
                Session.GetPlay().BotiquinName = "Bisturí, escalpelo, hilo, aguja y jeringa con morfina";
                Session.GetPlay().BotiquinDoc = 8;
            }
            else if (heri == 9)
            {
                Session.GetPlay().BotiquinName = "Hielo, vendas y jeringa con morfina";
                Session.GetPlay().BotiquinDoc = 9;
            }
            else if (heri == 10)
            {
                Session.GetPlay().BotiquinName = "Yeso, vendas, morfina y antiinflamatorios";
                Session.GetPlay().BotiquinDoc = 10;
            }
            else
            {
                Session.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con anestesia";
                Session.GetPlay().BotiquinDoc = 1;
            }
            #endregion

            RoleplayManager.Shout(Session, "*Toma un remedio del botiquín y corre a atender su paciente*", 5);
            Session.SendWhisper("Tomas " + Session.GetPlay().BotiquinName + " del botiquín.", 1);
            Session.GetPlay().CooldownManager.CreateCooldown("botiq", 1000, 3);
            #endregion
        }
    }
}