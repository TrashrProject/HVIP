using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun 
{
    class PetCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_pet"; }
        }

        public string Parameters
        {
            get { return "%PetId%"; }
        }

        public string Description
        {
            get { return "Transformate en una mascota."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            RoomUser RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (RoomUser == null)
                return;

            if (!Room.PetMorphsAllowed)
            {
                Session.SendWhisper("Esta zona tiene desactivada la habilidad de convertirse en mascota.", 1);
                if (Session.GetHabbo().PetId > 0)
                {
                    Session.SendWhisper("Destransformandote.", 1);
                    //Change the users Pet Id.
                    Session.GetHabbo().PetId = 0;

                    //Quickly remove the old user instance.
                    Room.SendMessage(new UserRemoveComposer(RoomUser.VirtualId));

                    //Add the new one, they won't even notice a thing!!11 8-)
                    Room.SendMessage(new UsersComposer(RoomUser));
                }
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("¡Oops, se te ha olvidado escribir cuál mascota quieres ser! Usa :pet list para ver las mascotas disponibles.", 1);
                return;
            }

            if (Params[1].ToString().ToLower() == "list")
            {
                StringBuilder List = new StringBuilder();
                List.Append("---------- Mascotas disponbiles ----------\n\n");
                List.Append("Habbo\n");
                List.Append("Perro\n");
                List.Append("Gato\n");
                List.Append("Terrier\n");
                List.Append("Croc\n");
                List.Append("Oso\n");
                List.Append("Cerdo\n");
                List.Append("Leon\n");
                List.Append("Rhino\n");
                List.Append("Araña\n");
                List.Append("Tortuga\n");
                List.Append("Pollo\n");
                List.Append("Rana\n");
                List.Append("Dragon\n");
                List.Append("Mono\n");
                List.Append("Caballo\n");
                List.Append("Conejo\n");
                List.Append("Pajaro\n");
                List.Append("Demonio\n");
                List.Append("bebeoso\n");
                List.Append("bebeterrier\n");
                List.Append("perrito\n");
                List.Append("gatito\n");
                List.Append("cerdito\n");
                List.Append("piedra\n");
                List.Append("velociraptor\n");
                List.Append("pterosaur\n");
                List.Append("Haloompa\n");
                List.Append("Gnomo\n");
                List.Append("vaca\n");
                List.Append("Mario\n");
                List.Append("Pikachu\n");
                List.Append("lobo\n");
                List.Append("pinguino\n");
                List.Append("elefante\n");
                List.Append("bebeguapo\n");
                List.Append("bebefeo\n");
                List.Append("lobo\n");
                Session.SendNotification(List.ToString());
                return;
            }

            int TargetPetId = GetPetIdByString(Params[1].ToString());
            if (TargetPetId == 0)
            {
                Session.SendWhisper("¡Oops, esa mascota no fue encontrada!", 1);
                return;
            }

            //Change the users Pet Id.
            Session.GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

            //Quickly remove the old user instance.
            Room.SendMessage(new UserRemoveComposer(RoomUser.VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            Room.SendMessage(new UsersComposer(RoomUser));

            //Tell them a quick message.
            if (Session.GetHabbo().PetId > 0)
                Session.SendWhisper("((Usa ':pet habbo' para volver a la normalidad))", 1);
        }

        private int GetPetIdByString(string Pet)
        {
            switch (Pet.ToLower())
            {
                default:
                    return 0;
                case "habbo":
                    return -1;
                case "perro":
                    return 60;//This should be 0.
                case "gato":
                case "1":
                    return 1;
                case "terrier":
                case "2":
                    return 2;
                case "croc":
                case "croco":
                case "3":
                    return 3;
                case "oso":
                case "4":
                    return 4;
                case "liz":
                case "cerdo":
                case "kill":
                case "5":
                    return 5;
                case "leon":
                case "rawr":
                case "6":
                    return 6;
                case "rhino":
                case "7":
                    return 7;
                case "spider":
                case "arana":
                case "araña":
                case "8":
                    return 8;
                case "tortuga":
                case "9":
                    return 9;
                case "chick":
                case "chicken":
                case "pollo":
                case "10":
                    return 10;
                case "frog":
                case "rana":
                case "11":
                    return 11;
                case "drag":
                case "dragon":
                case "12":
                    return 12;
                case "monkey":
                case "mono":
                case "14":
                    return 14;
                case "horse":
                case "caballo":
                case "15":
                    return 15;
                case "bunny":
                case "conejo":
                case "17":
                    return 17;
                case "pigeon":
                case "pajaro":
                case "21":
                    return 21;
                case "demon":
                case "demonio":
                case "23":
                    return 23;
                case "babybear":
                case "bebeoso":
                case "24":
                    return 24;
                case "babyterrier":
                case "bebeterrier":
                case "25":
                    return 25;
                case "gnome":
                case "gnomo":
                case "26":
                    return 26;
                case "kitten":
                case "gatito":
                case "28":
                    return 28;
                case "puppy":
                case "perrito":
                case "29":
                    return 29;
                case "piglet":
                case "cerdito":
                case "30":
                    return 30;
                case "haloompa":
                case "31":
                    return 31;
                case "rock":
                case "piedra":
                case "32":
                    return 32;
                case "pterosaur":
                case "33":
                    return 33;
                case "velociraptor":
                case "34":
                    return 34;
                case "vaca":
                case "35":
                    return 35;
                case "pinguino":
                case "36":
                    return 36;
                case "elefante":
                case "37":
                    return 37;
                case "bebeguapo":
                case "38":
                    return 38;
                case "bebefeo":
                case "39":
                    return 39;
                case "mario":
                case "40":
                    return 40;
                case "pikachu":
                case "41":
                    return 41;
                case "lobo":
                case "42":
                    return 42;
            }
        }
    }
}