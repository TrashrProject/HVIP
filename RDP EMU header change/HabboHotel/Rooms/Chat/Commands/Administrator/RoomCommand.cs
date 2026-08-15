using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class RoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Activa o desactiva el tipo de configuración de una zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar un tipo. ((Usa :room list para verlos)).", 1);
                return;
            }

            string Option = Params[1];
            switch (Option)
            {
                case "list":
                    {
                        StringBuilder List = new StringBuilder();
                        List.Append("---------- Configuración de Zona----------\n\n");
                        List.Append("Pet Morphs: " + (Room.PetMorphsAllowed == true ? "activado" : "desactivado") + "\n");
                        List.Append("Pull: " + (Room.PullEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Push: " + (Room.PushEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Super Pull: " + (Room.SPullEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Super Push: " + (Room.SPushEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Respect: " + (Room.RespectNotificationsEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Enables: " + (Room.EnablesEnabled == true ? "activado" : "desactivado") + "\n\n");
                        List.Append("---------- Configuraciones Roleplay ----------\n\n");
                        List.Append("Bank: " + (Room.BankEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Shooting: " + (Room.ShootEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Hitting: " + (Room.HitEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Safezone: " + (Room.SafeZoneEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Robbing: " + (Room.RobEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Sex Commands: " + (Room.SexCommandsEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Turf: " + (Room.TurfEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Gym: " + (Room.GymEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Drive: " + (Room.DriveEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Mensaje de entrada: " + Room.EnterRoomMessage + "\n");
                        List.Append("Buy Car: " + (Room.BuyCarEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Supermercado: " + (Room.SupermarketEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Municipalidad: " + (Room.MunicipalidadEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("No Hunger Timer: " + (Room.NoHungerEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Hospital: " + (Room.IsHospital == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Prisión: " + (Room.IsPrison == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Comisaría: " + (Room.IsPolStation == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Camioneros: " + (Room.IsCamionero == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Mecanicos: " + (Room.IsMecanico == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Basureros: " + (Room.IsBasurero == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Mineros: " + (Room.IsMinero == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Armeros: " + (Room.IsArmero == true ? "activado" : "desactivado") + "\n");
                        List.Append("24/7: " + (Room.MallEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Tienda Telefónica: " + (Room.PhoneStoreEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Ferretería: " + (Room.FerreteriaEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Granja: " + (Room.GrangeEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Gasolinera: " + (Room.GasEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Tienda de Robo: " + (Room.RobStoreEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Wardrobe (Cambiar Ropa): " + (Room.WardrobeEnabled == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona de sanciones (sanc): " + (Room.IsSanc == true ? "activado" : "desactivado") + "\n");
                        Session.SendNotification(List.ToString());
                        break;
                    }

                case "push":
                    {
                        Room.PushEnabled = !Room.PushEnabled;
                        Room.RoomData.PushEnabled = Room.PushEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.PushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Push' está ahora " + (Room.PushEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "spush":
                    {
                        Room.SPushEnabled = !Room.SPushEnabled;
                        Room.RoomData.SPushEnabled = Room.SPushEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.SPushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Spush' está ahora  " + (Room.SPushEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "spull":
                    {
                        Room.SPullEnabled = !Room.SPullEnabled;
                        Room.RoomData.SPullEnabled = Room.SPullEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.SPullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Spull' está ahora " + (Room.SPullEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "pull":
                    {
                        Room.PullEnabled = !Room.PullEnabled;
                        Room.RoomData.PullEnabled = Room.PullEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.PullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Pull' está ahora " + (Room.PullEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "enable":
                case "enables":
                    {
                        Room.EnablesEnabled = !Room.EnablesEnabled;
                        Room.RoomData.EnablesEnabled = Room.EnablesEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnablesEnabled", PlusEnvironment.BoolToEnum(Room.EnablesEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Enables' está ahora " + (Room.EnablesEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "respect":
                    {
                        Room.RespectNotificationsEnabled = !Room.RespectNotificationsEnabled;
                        Room.RoomData.RespectNotificationsEnabled = Room.RespectNotificationsEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RespectNotificationsEnabled", PlusEnvironment.BoolToEnum(Room.RespectNotificationsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Respetos' está ahora " + (Room.RespectNotificationsEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "bank":
                    {
                        Room.BankEnabled = !Room.BankEnabled;
                        Room.RoomData.BankEnabled = Room.BankEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `bank_enabled` = @BankEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BankEnabled", PlusEnvironment.BoolToEnum(Room.BankEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Banco' está ahora " + (Room.BankEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "phonestore":
                    {
                        Room.PhoneStoreEnabled = !Room.PhoneStoreEnabled;
                        Room.RoomData.PhoneStoreEnabled = Room.PhoneStoreEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `phonestore_enabled` = @PhoneStoreEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PhoneStoreEnabled", PlusEnvironment.BoolToEnum(Room.PhoneStoreEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Tienda de Teléfonos' está ahora " + (Room.PhoneStoreEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "buycar":
                    {
                        Room.BuyCarEnabled = !Room.BuyCarEnabled;
                        Room.RoomData.BuyCarEnabled = Room.BuyCarEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `buycar_enabled` = @BuyCarEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BuyCarEnabled", PlusEnvironment.BoolToEnum(Room.BuyCarEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Concesionario' está ahora " + (Room.BuyCarEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "municip":
                    {
                        Room.MunicipalidadEnabled = !Room.MunicipalidadEnabled;
                        Room.RoomData.MunicipalidadEnabled = Room.MunicipalidadEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `municip_enabled` = @MunicipalidadEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("MunicipalidadEnabled", PlusEnvironment.BoolToEnum(Room.MunicipalidadEnabled));
                            dbClient.RunQuery();
                        }
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Municipalidad' está ahora " + (Room.MunicipalidadEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "hospital":
                    {
                        Room.IsHospital = !Room.IsHospital;
                        Room.RoomData.IsHospital = Room.IsHospital;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_hospital` = @IsHospital WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsHospital", PlusEnvironment.BoolToEnum(Room.IsHospital));
                            dbClient.RunQuery();
                        }
                        Room.IsPrison = false;
                        Room.IsPolStation = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Hospital' está ahora " + (Room.IsHospital == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "prision":
                    {
                        Room.IsPrison = !Room.IsPrison;
                        Room.RoomData.IsPrison = Room.IsPrison;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_prison` = @IsPrison WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsPrison", PlusEnvironment.BoolToEnum(Room.IsPrison));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPolStation = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Prisión' está ahora " + (Room.IsPrison == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "sanc":
                    {
                        Room.IsSanc = !Room.IsSanc;
                        Room.RoomData.IsSanc = Room.IsSanc;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_sanc` = @IsSanc WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsSanc", PlusEnvironment.BoolToEnum(Room.IsSanc));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPolStation = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsPrison = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Sanción' está ahora " + (Room.IsSanc == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "comisaria":
                case "polstation":
                    {
                        Room.IsPolStation = !Room.IsPolStation;
                        Room.RoomData.IsPolStation = Room.IsPolStation;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_polstation` = @IsPolStation WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsPolStation", PlusEnvironment.BoolToEnum(Room.IsPolStation));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Estación de Policía' está ahora " + (Room.IsPolStation == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "camionero":
                    {
                        Room.IsCamionero = !Room.IsCamionero;
                        Room.RoomData.IsCamionero = Room.IsCamionero;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_camionero` = @IsCamionero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsCamionero", PlusEnvironment.BoolToEnum(Room.IsCamionero));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsCourt = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsPolStation = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Camionero' está ahora " + (Room.IsCamionero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "mecanico":
                    {
                        Room.IsMecanico = !Room.IsMecanico;
                        Room.RoomData.IsMecanico = Room.IsMecanico;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_mecanico` = @IsMecanico WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsMecanico", PlusEnvironment.BoolToEnum(Room.IsMecanico));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsPolStation = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Mecánico' está ahora " + (Room.IsMecanico == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "basurero":
                    {
                        Room.IsBasurero = !Room.IsBasurero;
                        Room.RoomData.IsBasurero = Room.IsBasurero;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_basurero` = @IsBasurero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsBasurero", PlusEnvironment.BoolToEnum(Room.IsBasurero));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsMinero = false;
                        Room.IsArmero = false;
                        Room.IsPolStation = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Basurero' está ahora " + (Room.IsBasurero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "minero":
                    {
                        Room.IsMinero = !Room.IsMinero;
                        Room.RoomData.IsMinero = Room.IsMinero;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_minero` = @IsMinero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsMinero", PlusEnvironment.BoolToEnum(Room.IsMinero));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsBasurero = false;
                        Room.IsMecanico = false;
                        Room.IsArmero = false;
                        Room.IsPolStation = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Minero' está ahora " + (Room.IsMinero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "armero":
                    {
                        Room.IsArmero = !Room.IsArmero;
                        Room.RoomData.IsArmero = Room.IsArmero;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_armero` = @IsArmero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsArmero", PlusEnvironment.BoolToEnum(Room.IsArmero));
                            dbClient.RunQuery();
                        }
                        Room.IsPrison = false;
                        Room.IsPolStation = false;
                        Room.IsCourt = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsHospital = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Armero' está ahora " + (Room.IsArmero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "corte":
                    {
                        Room.IsCourt = !Room.IsCourt;
                        Room.RoomData.IsCourt = Room.IsCourt;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `is_court` = @Iscorte WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsCorte", PlusEnvironment.BoolToEnum(Room.IsCourt));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsArmero = false;
                        Room.IsCamionero = false;
                        Room.IsBasurero = false;
                        Room.IsMinero = false;
                        Room.IsMecanico = false;
                        Room.IsPolStation = false;
                        Room.IsSanc = false;
                        PlusEnvironment.GetGame()._playroomManager.Init();
                        Session.SendWhisper("El modo 'Corte' está ahora " + (Room.IsCourt == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "shoot":
                    {
                        Room.ShootEnabled = !Room.ShootEnabled;
                        Room.RoomData.ShootEnabled = Room.ShootEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `shoot_enabled` = @ShootEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("ShootEnabled", PlusEnvironment.BoolToEnum(Room.ShootEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Disparos' está ahora " + (Room.ShootEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "hit":
                    {
                        Room.HitEnabled = !Room.HitEnabled;
                        Room.RoomData.HitEnabled = Room.HitEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `hit_enabled` = @HitEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("HitEnabled", PlusEnvironment.BoolToEnum(Room.HitEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Golpes' está ahora " + (Room.HitEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "drive":
                case "car":
                    {
                        Room.DriveEnabled = !Room.DriveEnabled;
                        Room.RoomData.DriveEnabled = Room.DriveEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `drive_enabled` = @DriveEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("DriveEnabled", PlusEnvironment.BoolToEnum(Room.DriveEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Conducir' está ahora " + (Room.DriveEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "mall":
                    {
                        Room.MallEnabled = !Room.MallEnabled;
                        Room.RoomData.MallEnabled = Room.MallEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `mall_enabled` = @MallEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("MallEnabled", PlusEnvironment.BoolToEnum(Room.MallEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Centro comercial' está ahora " + (Room.MallEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "24/7":
                case "supermarket":
                case "super":
                    {
                        Room.SupermarketEnabled = !Room.SupermarketEnabled;
                        Room.RoomData.SupermarketEnabled = Room.SupermarketEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `supermarket_enabled` = @SupermarketEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SupermarketEnabled", PlusEnvironment.BoolToEnum(Room.SupermarketEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo '24/7' está ahora " + (Room.SupermarketEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "ferreteria":
                    {
                        Room.FerreteriaEnabled = !Room.FerreteriaEnabled;
                        Room.RoomData.FerreteriaEnabled = Room.FerreteriaEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `ferreteria_enabled` = @FerreteriaEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("FerreteriaEnabled", PlusEnvironment.BoolToEnum(Room.FerreteriaEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Ferretería' está ahora " + (Room.FerreteriaEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "granja":
                    {
                        Room.GrangeEnabled = !Room.GrangeEnabled;
                        Room.RoomData.GrangeEnabled = Room.GrangeEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `grange_enabled` = @GrangeEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("GrangeEnabled", PlusEnvironment.BoolToEnum(Room.GrangeEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Granja' está ahora " + (Room.GrangeEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "gas":
                    {
                        Room.GasEnabled = !Room.GasEnabled;
                        Room.RoomData.GasEnabled = Room.GasEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `gas_enabled` = @GasEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("GasEnabled", PlusEnvironment.BoolToEnum(Room.GasEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Gasolinera' está ahora " + (Room.GasEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "robstore":
                    {
                        Room.RobStoreEnabled = !Room.RobStoreEnabled;
                        Room.RoomData.RobStoreEnabled = Room.RobStoreEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `robstore_enabled` = @RobStoreEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RobStoreEnabled", PlusEnvironment.BoolToEnum(Room.RobStoreEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Tienda de Objetos robados' está ahora " + (Room.RobStoreEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "wardrobe":
                    {
                        Room.WardrobeEnabled = !Room.WardrobeEnabled;
                        Room.RoomData.WardrobeEnabled = Room.WardrobeEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `wardrobe_enabled` = @WardrobeEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("WardrobeEnabled", PlusEnvironment.BoolToEnum(Room.WardrobeEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Cambio de ropa' está ahora " + (Room.WardrobeEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "rob":
                    {
                        Room.RobEnabled = !Room.RobEnabled;
                        Room.RoomData.RobEnabled = Room.RobEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `rob_enabled` = @RobEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RobEnabled", PlusEnvironment.BoolToEnum(Room.RobEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Robar' está ahora " + (Room.RobEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "sex":
                case "sexcommands":
                    {
                        Room.SexCommandsEnabled = !Room.SexCommandsEnabled;
                        Room.RoomData.SexCommandsEnabled = Room.SexCommandsEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `sexcommands_enabled` = @SexCommandsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SexCommandsEnabled", PlusEnvironment.BoolToEnum(Room.SexCommandsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Comandos sexuales' está ahora " + (Room.SexCommandsEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "turf":
                    {
                        Room.TurfEnabled = !Room.TurfEnabled;
                        Room.RoomData.TurfEnabled = Room.TurfEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `turf_enabled` = @TurfEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TurfEnabled", PlusEnvironment.BoolToEnum(Room.TurfEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Barrio' está ahora " + (Room.TurfEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "gym":
                    {
                        Room.GymEnabled = !Room.GymEnabled;
                        Room.RoomData.GymEnabled = Room.GymEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `gym_enabled` = @GymEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("GymEnabled", PlusEnvironment.BoolToEnum(Room.GymEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Gimnasio' está ahora " + (Room.GymEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                    
                case "message":
                    {
                        if (Params.Length < 3)
                        {
                            Session.SendWhisper("Escribe el mensaje de entrada en esta zona.", 1);
                            break;
                        }

                        string Message = CommandManager.MergeParams(Params, 2);

                        Room.EnterRoomMessage = Message;
                        Room.RoomData.EnterRoomMessage = Room.EnterRoomMessage;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `enter_message` = @EnterMessage WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnterMessage", Room.EnterRoomMessage);
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El mensaje de entrada es ahora: " + Room.EnterRoomMessage, 1);
                        break;
                    }

                case "safezone":
                    {
                        Room.SafeZoneEnabled = !Room.SafeZoneEnabled;
                        Room.RoomData.SafeZoneEnabled = Room.SafeZoneEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `safezone_enabled` = @SafeZoneEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SafeZoneEnabled", PlusEnvironment.BoolToEnum(Room.SafeZoneEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Zona segura' está ahora " + (Room.SafeZoneEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "nohunger":
                    {
                        Room.NoHungerEnabled = !Room.NoHungerEnabled;
                        Room.RoomData.NoHungerEnabled = Room.NoHungerEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `play_rooms` SET `nohunger_enabled` = @NoHungerEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("NoHungerEnabled", PlusEnvironment.BoolToEnum(Room.NoHungerEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Sin Hambre' está ahora " + (Room.NoHungerEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "pets":
                case "morphs":
                    {
                        Room.PetMorphsAllowed = !Room.PetMorphsAllowed;
                        Room.RoomData.PetMorphsAllowed = Room.PetMorphsAllowed;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PetMorphsAllowed", PlusEnvironment.BoolToEnum(Room.PetMorphsAllowed));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Permitir transformaciones a mascotas' está ahora " + (Room.PetMorphsAllowed == true ? "activado." : "desactivado."), 1);

                        if (!Room.PetMorphsAllowed)
                        {
                            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                    continue;

                                User.GetClient().SendWhisper("Se ha desactivado la habilidad de transformación en mascota en esta zona.", 1);
                                if (User.GetClient().GetHabbo().PetId > 0)
                                {
                                    // Tell the user what is going on.
                                    User.GetClient().SendWhisper("Se ha desactivado la habilidad de transformación en mascota en esta zona. Se te destransformará.", 1);

                                    // Change the users Pet Id.
                                    User.GetClient().GetHabbo().PetId = 0;

                                    // Quickly remove the old user instance.
                                    Room.SendMessage(new UserRemoveComposer(User.VirtualId));

                                    // Add the new one, they won't even notice a thing.
                                    Room.SendMessage(new UsersComposer(User));
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        Session.SendWhisper(Option + " no es una configuración de Zona válida. ((Usa :room list))", 1);
                    }
                    break;
            }
        }
    }
}
