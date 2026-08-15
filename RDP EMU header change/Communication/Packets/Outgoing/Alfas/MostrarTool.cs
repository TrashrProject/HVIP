using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Outgoing.Alfas
{
    class MostrarTool : ServerPacket
    {
        public MostrarTool(GameClient Session) : base(ServerPacketHeader.HelperToolConfigurationMessageComposer)
        {

            Console.WriteLine("hola muestro tool");
            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
            bool onDuty = true;

            if (onDuty)
                guideManager.AddGuide(Session);
            else
                guideManager.RemoveGuide(Session);

            Session.GetHabbo().OnHelperDuty = onDuty;
            base.WriteBoolean(onDuty);
            base.WriteInteger(guideManager.GuidesCount);
            base.WriteInteger(0);
            base.WriteInteger(0);
        }
    }
}
