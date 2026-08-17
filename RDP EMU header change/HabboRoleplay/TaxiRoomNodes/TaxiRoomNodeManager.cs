using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using log4net;
using System.IO;
using System.Text;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.TaxiRoomNodes
{
    public static class TaxiRoomNodeManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.TaxiRoomNodes");

        /// <summary>
        /// Thread-safe dictionary containing all the taxi nodes
        /// </summary>
        public static ConcurrentDictionary<int, TaxiRoomNode> TaxiRoomNodes;

        /// <summary>
        /// Initializes the TaxiRoomNode manager
        /// </summary>
        public static void Initialize()
        {
            if (TaxiRoomNodes == null)
            {
                TaxiRoomNodes = new ConcurrentDictionary<int, TaxiRoomNode>();
            }
            else
            {
                TaxiRoomNodes.Clear();
            }

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * FROM `play_rooms` WHERE taxi_node >= 0");
                DataTable TaxiTable = DB.getTable();

                if (TaxiTable == null)
                    log.Error("¡Error al cargar TaxiRoomNodes de la DB!");
                else
                    ProcessTaxiRoomNodesTable(TaxiTable);
            }
        }

        /// <summary>
        /// Creates an instance of the TaxiRoomNode and stores it in the dictionary
        /// </summary>
        /// <param name="TaxiRoomNodesTable"></param>
        private static void ProcessTaxiRoomNodesTable(DataTable TaxiTable)
        {
            foreach (DataRow Row in TaxiTable.Rows)
            {
                int RoomId = Convert.ToInt32(Row["id"]);
                int NodeId = Convert.ToInt32(Row["taxi_node"]);

                if (TaxiRoomNodes.ContainsKey(RoomId))
                    continue;

                TaxiRoomNode TaxiRoomNode = new TaxiRoomNode(RoomId, NodeId);
                TaxiRoomNodes.TryAdd(RoomId, TaxiRoomNode);
            }

            log.Info("Loaded " + TaxiRoomNodes.Count + " taxi room nodes.");
        }

        /// <summary>
        /// Gets the taxiroomnode based
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TaxiRoomNode getTaxiRoomNode(int Id)
        {
            if (TaxiRoomNodes.ContainsKey(Id))
                return TaxiRoomNodes[Id];
            else
                return null;
        }

        public static List<TaxiRoomNode> getAllTaxiRoomNodes()
        {
            List<TaxiRoomNode> TN = new List<TaxiRoomNode>();

            foreach (var item in TaxiRoomNodes)
            {
                TN.Add(item.Value);
            }

            return TN;
        }

        public static List<TaxiRoomNode> getTaxiRoomNodeByRoom(int RoomId)
        {
            List<TaxiRoomNode> APP = new List<TaxiRoomNode>();

            foreach (var item in TaxiRoomNodes)
            {
                if(item.Value.RoomId == RoomId)
                    APP.Add(item.Value);
            }
            return APP;
        }

        public static List<TaxiRoomNode> getTaxiRoomNodeByNode(int NodeId)
        {
            List<TaxiRoomNode> APP = new List<TaxiRoomNode>();

            foreach (var item in TaxiRoomNodes)
            {
                if (item.Value.NodeId == NodeId)
                    APP.Add(item.Value);
            }
            return APP;
        }
    }
}
