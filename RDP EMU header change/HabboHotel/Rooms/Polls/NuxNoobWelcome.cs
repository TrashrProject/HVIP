using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rewards.Rooms.Polls
{
    class NuxNoobWelcome
    {
        static public void welcome(GameClient Session)
        {
            if(Session.GetHabbo().nuxclickCount == 0)
            {
                Session.GetHabbo().nuxclickCount++;
                Session.SendMessage(new NuxAlertMessageComposer("helpBubble/add/BOTTOM_BAR_INVENTORY/Este es el inventario. Para colocar tus furnis, tan sólo tienes que arrastrarlos hasta el suelo."));
            }
            else if (Session.GetHabbo().nuxclickCount == 1)
            {
                Session.GetHabbo().nuxclickCount++;
                Session.SendMessage(new NuxAlertMessageComposer("helpBubble/add/MEMENU_CLOTHES/Aquí están los ajustes. Puedes cambiarte de ropa y modificar aspectos de tu personaje."));
            }
            else if (Session.GetHabbo().nuxclickCount == 2)
            {
                Session.GetHabbo().nuxclickCount++;
                Session.SendMessage(new NuxAlertMessageComposer("helpBubble/add/BOTTOM_BAR_NAVIGATOR/Este es el navegador. ¡Úsalo para explorar las miles de salas que hay en el hotel!"));
            }
            else if (Session.GetHabbo().nuxclickCount == 3)
            {
                Session.GetHabbo().nuxclickCount++;
                Session.SendMessage(new NuxAlertMessageComposer("helpBubble/add/BOTTOM_BAR_CATALOGUE/Esta es la Tienda. Aquí encontrarás cosas fabulosas con las que divertirte aún más. ¡Pruébalo!"));
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE users SET isNoob = '0' WHERE id = " + Session.GetHabbo().Id + ";");
                }
                Session.GetHabbo().isNoob = false;
            }
        
        }
    }
}
