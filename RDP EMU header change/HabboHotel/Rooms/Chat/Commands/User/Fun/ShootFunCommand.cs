using System;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class ShootFunCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_shoot"; }
        }
        public string Parameters
        {
            get { return "%nombre de usuario%"; }
        }
        public string Description
        {
            get { return "Disparar a otro usuario"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir el nombre del usuario!");
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ese usuario no fue encontrado, tal vez no estan en la sala o no estan conectados.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                Session.SendWhisper("Ese usuario no fue encontrado, tal vez no estan en la sala o no estan conectados.");
                return;
            }
            //RoomUser User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar ese usuario, tal vez no esta conectado.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("* Me estoy suicidando *");
                return;
            }
            if (TargetUser == null)
                return;

            Task.Run(async delegate
            {
                if (!((Math.Abs(User.X - TargetUser.X) >= 2 && (Math.Abs(User.Y - TargetUser.Y) >= 2))))
                {

                    Room.SendMessage(new ChatComposer(User.VirtualId, "* Disparando a " + Params[1] + " *", 0, 1));
                    User.ApplyEffect(101);
                    TargetUser.ApplyEffect(133);

                    await Task.Delay(5000);

                    User.ApplyEffect(0);
                    TargetUser.ApplyEffect(0);
                }
                else
                {
                    Session.SendWhisper(Params[1] + " esta muy lejos, intenta acercarte.");
                    return;
                }
            });
        }
    }
}
