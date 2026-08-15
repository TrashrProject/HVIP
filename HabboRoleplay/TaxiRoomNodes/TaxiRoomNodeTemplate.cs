using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.TaxiRoomNodes
{
    /// <summary>
    /// Structure for TaxiRoomNodes
    /// </summary>
    public class TaxiRoomNodeTemplate
    {
        #region Variables
        public int Orig;
        public int Dest;
        public List<int> Path;
        #endregion

        /// <summary>
        /// Taxi room Nodes constructor
        /// </summary>
        public TaxiRoomNodeTemplate(int Orig, int Dest, List<int> Path)
        {
            this.Orig = Orig;
            this.Dest = Dest;
            this.Path = Path;
        }
    }
}
