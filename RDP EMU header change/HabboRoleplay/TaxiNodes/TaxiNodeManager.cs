using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using log4net;
using System.IO;
using System.Text;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.TaxiNodes
{
    public static class TaxiNodeManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.TaxiNodes");

        /// <summary>
        /// Thread-safe dictionary containing all the taxi nodes
        /// </summary>
        public static ConcurrentDictionary<int, TaxiNode> TaxiNodes;

        /// <summary>
        /// Initializes the taxinode manager
        /// </summary>
        public static void Initialize()
        {
            if (TaxiNodes == null)
            {
                TaxiNodes = new ConcurrentDictionary<int, TaxiNode>();
            }
            else
            {
                TaxiNodes.Clear();
            }

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * FROM `play_taxi_nodes`");
                DataTable TaxiTable = DB.getTable();

                if (TaxiTable == null)
                    log.Error("¡Error al cargar TaxiNodes de la DB!");
                else
                    ProcessTaxiNodesTable(TaxiTable);
            }
        }

        /// <summary>
        /// Creates an instance of the taxinode and stores it in the dictionary
        /// </summary>
        /// <param name="TaxiNodesTable"></param>
        private static void ProcessTaxiNodesTable(DataTable TaxiTable)
        {
            foreach (DataRow Row in TaxiTable.Rows)
            {
                int Id = Convert.ToInt32(Row["id"]);
                int NodeFrom = Convert.ToInt32(Row["node_from"]);
                int NodeTo = Convert.ToInt32(Row["node_to"]);
                int X = Convert.ToInt32(Row["x"]);
                int Y = Convert.ToInt32(Row["y"]);

                if (TaxiNodes.ContainsKey(Id))
                    continue;

                TaxiNode TaxiNode = new TaxiNode(Id, NodeFrom, NodeTo, X, Y);
                TaxiNodes.TryAdd(Id, TaxiNode);
            }

            log.Info("Loaded " + TaxiNodes.Count + " taxi nodes.");
        }

        /// <summary>
        /// Gets the taxinode based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TaxiNode getTaxiNode(int Id)
        {
            if (TaxiNodes.ContainsKey(Id))
                return TaxiNodes[Id];
            else
                return null;
        }

        public static List<TaxiNode> getAllTaxiNodes()
        {
            List<TaxiNode> TN = new List<TaxiNode>();

            foreach (var item in TaxiNodes)
            {
                TN.Add(item.Value);
            }

            return TN;
        }

        public static List<TaxiNode> getTaxiNodeByFrom(int FromNode)
        {
            List<TaxiNode> APP = new List<TaxiNode>();

            foreach (var item in TaxiNodes)
            {
                if(item.Value.NodeFrom == FromNode)
                    APP.Add(item.Value);
            }
            return APP;
        }

        public static List<TaxiNode> getTaxiNodeByTo(int ToNode)
        {
            List<TaxiNode> APP = new List<TaxiNode>();

            foreach (var item in TaxiNodes)
            {
                if (item.Value.NodeTo == ToNode)
                    APP.Add(item.Value);
            }
            return APP;
        }

        public static List<TaxiNode> getTaxiNodeByFromTo(int FromNode, int ToNode)
        {
            List<TaxiNode> APP = new List<TaxiNode>();

            foreach (var item in TaxiNodes)
            {
                if (item.Value.NodeFrom == FromNode && item.Value.NodeTo == ToNode)
                    APP.Add(item.Value);
            }
            return APP;
        }
    }
}
