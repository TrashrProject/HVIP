using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Users.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class OpenGuideTool : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().Rank >= 3)
            {
                GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
                bool onDuty = Packet.PopBoolean();

                Packet.PopBoolean();
                Packet.PopBoolean();
                Packet.PopBoolean();

                if (onDuty)
                    guideManager.AddGuide(Session);
                else
                    guideManager.RemoveGuide(Session);

                Session.GetHabbo().OnHelperDuty = onDuty;
                ServerPacket Response = new ServerPacket(ServerPacketHeader.HelperToolConfigurationMessageComposer);
                Response.WriteBoolean(onDuty);
                Response.WriteInteger(guideManager.GuidesCount);
                Response.WriteInteger(0);
                Response.WriteInteger(0);
                Session.SendMessage(Response);
            }
            else
            {
                Session.SendNotification("Por razones de rank no tienes permiso para utilizar esta herramienta");
            }


            /*
                        var guideManager = PlusEnvironment.GetGame().GetGuideManager();
                        var onDuty = Packet.PopBoolean();

                        Packet.PopBoolean(); // guide
                        Packet.PopBoolean(); // helper
                        Packet.PopBoolean(); // guardian

                        if (onDuty)
                            guideManager.AddGuide(Session);
                        else
                            guideManager.RemoveGuide(Session);
                        Session.GetHabbo().OnHelperDuty = onDuty;
                        var Response = new ServerPacket(ServerPacketHeader.HelperToolConfigurationMessageComposer);
                        Response.WriteBoolean(onDuty); // on duty
                        Response.WriteInteger(guideManager.GuidesCount); // guides
                        Response.WriteInteger(0); // helpers
                        Response.WriteInteger(0); // guardians
                        Session.SendMessage(Response);
             * */
        }
    }
}
