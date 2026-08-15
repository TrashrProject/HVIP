using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using log4net;
using System.IO;
using System.Text;
using Plus.HabboRoleplay.Misc;
using Plus.Utilities;

namespace Plus.HabboRoleplay.TaxiRoomNodes
{
    public static class TaxiRoomNodeTemplateManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.TaxiRoomNodeTemplateManager");

        /// <summary>
        /// Thread-safe dictionary containing all the taxi nodes
        /// </summary>
        public static ConcurrentDictionary<string, TaxiRoomNodeTemplate> TaxiRoomNodeTemplates;

        /// <summary>
        /// Initializes the TaxiRoomNode manager
        /// </summary>
        public static void Initialize(Dijkstra Dijkstra)
        {
            if (TaxiRoomNodeTemplates == null)
            {
                TaxiRoomNodeTemplates = new ConcurrentDictionary<string, TaxiRoomNodeTemplate>();
            }
            else
            {
                TaxiRoomNodeTemplates.Clear();
            }

            PreLoadRoomNodes(Dijkstra);
        }

        /// <summary>
        /// Creates an instance of the TaxiRoomNode and stores it in the dictionary
        /// </summary>
        /// <param name="TaxiRoomNodeTemplatesTable"></param>
        private static void PreLoadRoomNodes(Dijkstra Dijkstra)
        {
            List<TaxiRoomNode> TN = TaxiRoomNodeManager.getAllTaxiRoomNodes();
            if (TN.Count > 0 && TN != null)
            {
                foreach (var N in TN)
                {
                    foreach(var N2 in TN)
                    {
                        List<int> ruta = Dijkstra.RunDijkstra(N.NodeId, N2.NodeId);

                        string Id = N.NodeId + "," + N2.NodeId;
                        if (TaxiRoomNodeTemplates.ContainsKey(Id))
                            continue;

                        TaxiRoomNodeTemplate TaxiNode = new TaxiRoomNodeTemplate(N.NodeId, N2.NodeId, ruta);
                        TaxiRoomNodeTemplates.TryAdd(Id, TaxiNode);
                    }
                }
            }

            log.Info("Loaded " + TaxiRoomNodeTemplates.Count + " taxi room paths.");
        }

        /// <summary>
        /// Gets the taxiroomnode based
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TaxiRoomNodeTemplate getTaxiRoomNodeTemplate(string Id)
        {
            if (TaxiRoomNodeTemplates.ContainsKey(Id))
                return TaxiRoomNodeTemplates[Id];
            else
                return null;
        }
    }
}
