using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class AcceptDeathCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_accept_death"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Acepta tu muerte para reaparecer en el Hospital."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions           

            if (!Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡Debes estar inconsciente en el suelo para hacer eso!", 1);
                return;
            }
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("Ya te encuentras siendo transportado al Hospital. Por favor espera.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("acceptdeath", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
            
            string MyCity = Room.City;

            PlayRoom Data;
            int ToHosp = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

            if (ToHosp > 0)
            {
                Room Room2 = RoleplayManager.GenerateRoom(ToHosp);
                if (Room2 != null)
                {
                    Session.GetPlay().IsDead = true;
                    Session.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                    Session.GetHabbo().HomeRoom = ToHosp;
                    Session.GetPlay().DeathInOut = Room.DriveEnabled;
                    /*
                    if (Session.GetHabbo().CurrentRoomId != ToHosp)
                        RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                    else
                        Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                    */
                    RoleplayManager.SendUserTimer(Session, ToHosp, "", "death");
                }
                else
                {
                    Session.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                    Session.GetPlay().CurHealth = Session.GetPlay().MaxHealth;
                    Session.GetPlay().RefreshStatDialogue();
                    Session.GetRoomUser().Frozen = false;
                    Session.SendWhisper("Se te ha revivido a causa de que no hay ningún hospital en esta Ciudad", 1);
                }
            }
            else
            {
                Session.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                Session.GetPlay().CurHealth = Session.GetPlay().MaxHealth;
                Session.GetPlay().RefreshStatDialogue();
                Session.GetRoomUser().Frozen = false;
                Session.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
            }
            Session.GetPlay().IsDying = false;
            Session.GetPlay().DyingTimeLeft = 0;
            Session.GetPlay().CooldownManager.CreateCooldown("acceptdeath", 1000, 10);
            #endregion
        }
        
    }
}