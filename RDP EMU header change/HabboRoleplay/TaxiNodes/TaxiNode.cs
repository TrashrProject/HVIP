using System;
using Microsoft.Win32;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.TaxiNodes
{
    /// <summary>
    /// Structure for taxinodes
    /// </summary>
    public class TaxiNode
    {
        #region Variables
        public int Id;
        public int NodeFrom;
        public int NodeTo;
        public int X;
        public int Y;
        #endregion

        /// <summary>
        /// Taxi Nodes constructor
        /// </summary>
        public TaxiNode(int Id, int NodeFrom, int NodeTo, int X, int Y)
        {
            this.Id = Id;
            this.NodeFrom = NodeFrom;
            this.NodeTo = NodeTo;
            this.X = X;
            this.Y = Y;
        }
    }
}
