using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;



using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RoomExtCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room_ext"; }
        }

        public string Parameters
        {
            get { return "push/pull/enables/respect"; }
        }

        public string Description
        {
            get { return "Activa o desactiva un permiso extra de tu zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes espcificar el tipo de permiso. ((Usa :roomext list))", 1);
                return;
            }

            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Solo gente con permisos puede hacer eso.", 1);
                return;
            }

            string Option = Params[1];
            switch (Option)
            {
                case "list":
                {
                    StringBuilder List = new StringBuilder("");
                    List.AppendLine("Lista de permisos extra");
                    List.AppendLine("-------------------------");
                    List.AppendLine("Transformarse en mascota: " + (Room.PetMorphsAllowed == true ? "activado." : "desactivado."));
                    List.AppendLine("Pull: " + (Room.PullEnabled == true ? "activado." : "desactivado."));
                    List.AppendLine("Push: " + (Room.PushEnabled == true ? "activado." : "desactivado."));
                    List.AppendLine("Super Pull: " + (Room.SPullEnabled == true ? "activado." : "desactivado."));
                    List.AppendLine("Super Push: " + (Room.SPushEnabled == true ? "activado." : "desactivado."));
                    List.AppendLine("Respetos: " + (Room.RespectNotificationsEnabled == true ? "activado." : "desactivado."));
                    List.AppendLine("Efectos: " + (Room.EnablesEnabled == true ? "activado." : "desactivado."));
                    Session.SendNotification(List.ToString());
                    break;
                }

                case "push":
                    {
                        Room.PushEnabled = !Room.PushEnabled;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + Room.Id +"' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.PushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Push' está ahora " + (Room.PushEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "spush":
                    {
                        Room.SPushEnabled = !Room.SPushEnabled;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.SPushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Super Push' está ahora " + (Room.SPushEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "spull":
                    {
                        Room.SPullEnabled = !Room.SPullEnabled;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.SPullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Super Pull' está ahora " + (Room.SPullEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "pull":
                    {
                        Room.PullEnabled = !Room.PullEnabled;
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
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnablesEnabled", PlusEnvironment.BoolToEnum(Room.EnablesEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Efectos' está ahora " + (Room.EnablesEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "respect":
                    {
                        Room.RespectNotificationsEnabled = !Room.RespectNotificationsEnabled;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RespectNotificationsEnabled", PlusEnvironment.BoolToEnum(Room.RespectNotificationsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Respetos' está ahora " + (Room.RespectNotificationsEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }

                case "pets":
                case "morphs":
                    {
                        Room.PetMorphsAllowed = !Room.PetMorphsAllowed;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PetMorphsAllowed", PlusEnvironment.BoolToEnum(Room.PetMorphsAllowed));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Transformaciones' está ahora " + (Room.PetMorphsAllowed == true ? "activado." : "desactivado."), 1);
                        
                        if (!Room.PetMorphsAllowed)
                        {
                            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                    continue;

                                User.GetClient().SendWhisper("El propietario de esta zona ha desactivado la habildad de transformarse en mascotas.", 1);
                                if (User.GetClient().GetHabbo().PetId > 0)
                                {
                                    //Tell the user what is going on.
                                    User.GetClient().SendWhisper("El propietario de esta zona ha desactivado la habildad de transformarse en mascotas. Destransformandote...", 1);
                                    
                                    //Change the users Pet Id.
                                    User.GetClient().GetHabbo().PetId = 0;

                                    //Quickly remove the old user instance.
                                    Room.SendMessage(new UserRemoveComposer(User.VirtualId));

                                    //Add the new one, they won't even notice a thing!!11 8-)
                                    Room.SendMessage(new UsersComposer(User));
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
