using System;
using Microsoft.Win32;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.TaxiRoomNodes
{
    /// <summary>
    /// Structure for TaxiRoomNodes
    /// </summary>
    public class TaxiRoomNode
    {
        #region Variables
        public int RoomId;
        public int NodeId;
        #endregion

        /// <summary>
        /// Taxi room Nodes constructor
        /// </summary>
        public TaxiRoomNode(int RoomId, int NodeId)
        {
            this.RoomId = RoomId;
            this.NodeId = NodeId;
        }
    }
}
