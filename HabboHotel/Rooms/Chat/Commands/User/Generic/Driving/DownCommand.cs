using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using System.Drawing;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class DownCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_driving_down"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return ":bajar Permite bajarte del Vehículo donde vas de pasajero. | :bajar [pasajero] Permite bajar a alguien de tu vehículo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            // :bajar
            if (Params.Length == 1)
            {
                #region Conditions

                if (!Session.GetPlay().Pasajero)
                {
                    Session.SendWhisper("¡No eres pasajero de nadie!", 1);
                    return;
                }
                if (Session.GetPlay().IsEscorted || Session.GetPlay().Cuffed)
                {
                    Session.SendWhisper("¡No puedes bajarte del vehículo siendo Escoltad@ o Esposad@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDead)
                {
                    Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsJailed)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDying)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetPlay().ChoferName);
                if (TargetClient == null)
                {
                    Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                    return;
                }

                if (Session.GetPlay().TryGetCooldown("pasajero"))
                    return;
                #endregion

                #region Execute

                // PASAJERO
                Session.GetPlay().Pasajero = false;
                Session.GetPlay().ChoferName = "";
                Session.GetPlay().ChoferID = 0;
                Session.GetRoomUser().CanWalk = true;
                Session.GetRoomUser().FastWalking = false;
                Session.GetRoomUser().TeleportEnabled = false;
                Session.GetRoomUser().AllowOverride = false;

                // Descontamos Pasajero
                TargetClient.GetPlay().PasajerosCount--;
                if (TargetClient.GetPlay().PasajerosCount <= 0)
                    TargetClient.GetPlay().Pasajeros = "";
                else
                    TargetClient.GetPlay().Pasajeros.Replace(Session.GetHabbo().Username + ";", "");

                // CHOFER 
                TargetClient.GetPlay().Chofer = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;
                if(TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().AllowOverride = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;

                if(TargetClient.GetHabbo() != null)
                    RoleplayManager.Shout(Session, "*Baja del vehículo de " + TargetClient.GetHabbo().Username + "*", 5);
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
                Session.GetPlay().CooldownManager.CreateCooldown("pasajero", 1000, 5);
                #endregion
            }
            // :bajar [pasajero]
            else if (Params.Length == 2)
            {
                bool MyPasaj = false;
                #region Conditions
                if (!Session.GetPlay().DrivingCar)
                {
                    Session.SendWhisper("¡No te encuentras conduciendo para llevar a algún pasajero!", 1);
                    return;
                }
                if (!Session.GetPlay().Chofer)
                {
                    Session.SendWhisper("¡No llevas a ningún pasajero!", 1);
                    return;
                }

                if (Session.GetPlay().IsDead)
                {
                    Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsJailed)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDying)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                if (TargetClient == null)
                {
                    Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                    return;
                }
                if (TargetClient == Session)
                {
                    Session.SendWhisper("No puedes bajarte a ti mismo como si fueses un pasajero.", 1);
                    return;
                }
                // Verificamos si la persona es mi pasajero
                //Vars
                string Pasajeros = Session.GetPlay().Pasajeros;
                string[] stringSeparators = new string[] { ";" };
                string[] result;
                result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string psjs in result)
                {
                    GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                    if (PJ != null)
                    {
                        if (PJ.GetHabbo().Username == TargetClient.GetHabbo().Username)
                            MyPasaj = true;

                    }
                }
                if (!MyPasaj)
                {
                    Session.SendWhisper("¡Esa persona no es tu pasajero!", 1);
                    return;
                }
                if (TargetClient.GetPlay().IsEscorted || TargetClient.GetPlay().Cuffed)
                {
                    Session.SendWhisper("No puedes bajar a un Convicto escoltado del Vehículo. Detén el motor para bajar ambos de él.", 1);
                    return;
                }
                if (Session.GetPlay().TryGetCooldown("pasajero"))
                    return;
                #endregion

                #region Execute

                // PASAJERO
                TargetClient.GetPlay().Pasajero = false;
                TargetClient.GetPlay().ChoferName = "";
                TargetClient.GetPlay().ChoferID = 0;
                TargetClient.GetRoomUser().CanWalk = true;
                TargetClient.GetRoomUser().FastWalking = false;
                TargetClient.GetRoomUser().TeleportEnabled = false;
                TargetClient.GetRoomUser().AllowOverride = false;

                // Descontamos Pasajero
                Session.GetPlay().PasajerosCount--;
                if (Session.GetPlay().PasajerosCount <= 0)
                    Session.GetPlay().Pasajeros = "";
                else
                {
                    StringBuilder builder = new StringBuilder(Session.GetPlay().Pasajeros);
                    builder.Replace(TargetClient.GetHabbo().Username + ";", "");
                    Session.GetPlay().Pasajeros = builder.ToString();
                }

                // CHOFER 
                Session.GetPlay().Chofer = (Session.GetPlay().PasajerosCount <= 0) ? false : true;
                Session.GetRoomUser().AllowOverride = (Session.GetPlay().PasajerosCount <= 0) ? false : true;

                // SI EL PASAJERO ES UN HERIDO
                if (TargetClient.GetPlay().IsDying)
                {
                    #region Send To Hospital
                    RoleplayManager.Shout(Session, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                    Room Room2 = RoleplayManager.GenerateRoom(Session.GetRoomUser().RoomId);
                    string MyCity2 = Room2.City;

                    PlayRoom Data2;
                    int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                    if (ToHosp2 > 0)
                    {
                        Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                        if (Room3 != null)
                        {
                            Session.GetPlay().IsDead = true;
                            Session.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                            Session.GetHabbo().HomeRoom = ToHosp2;

                            /*
                            if (Session.GetHabbo().CurrentRoomId != ToHosp)
                                RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                            else
                                Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                            */
                            RoleplayManager.SendUserTimer(Session, ToHosp2, "", "death");
                        }
                        else
                        {
                            Session.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                            Session.GetPlay().CurHealth = Session.GetPlay().MaxHealth;
                            Session.GetPlay().RefreshStatDialogue();
                            Session.GetRoomUser().Frozen = false;
                            Session.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad.", 1);
                        }
                    }
                    else
                    {
                        Session.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                        Session.GetPlay().CurHealth = Session.GetPlay().MaxHealth;
                        Session.GetPlay().RefreshStatDialogue();
                        Session.GetRoomUser().Frozen = false;
                        Session.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad.", 1);
                    }
                    Session.GetPlay().IsDying = false;
                    Session.GetPlay().DyingTimeLeft = 0;
                    #endregion
                }

                // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                if (TargetClient.GetPlay().IsBasuPasaj)
                    TargetClient.GetPlay().IsBasuPasaj = false;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_vehicle", "close");// WS FUEL

                RoleplayManager.Shout(Session, "*Baja a " + TargetClient.GetHabbo().Username + " de su Vehículo*", 5);
                Session.GetPlay().CooldownManager.CreateCooldown("pasajero", 1000, 5);
                TargetClient.GetPlay().CooldownManager.CreateCooldown("pasajero", 1000, 5);                
                #endregion
            }
        }
    }
}
