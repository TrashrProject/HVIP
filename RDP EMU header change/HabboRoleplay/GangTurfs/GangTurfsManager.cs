using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using log4net;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.GangTurfs
{
    public class GangTurfsManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.GangTurfs.GangTurfsManager");
        

        /// <summary>
        /// Thread-safe dictionary containing all turfs
        /// </summary>
        public static ConcurrentDictionary<int, GangTurfs> _GangTurfs = new ConcurrentDictionary<int, GangTurfs>();

        /// <summary>
        /// Initializes the house list dictionary
        /// </summary>
        public void Init()
        {
            _GangTurfs.Clear();
            
            DataTable VhOwn;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * FROM `play_rooms`, `rooms` WHERE play_rooms.id = rooms.id AND play_rooms.turf_enabled = '1'");
                VhOwn = DB.getTable();

                if (VhOwn != null)
                {
                    foreach (DataRow Row in VhOwn.Rows)
                    {
                        int RoomId = Convert.ToInt32(Row["id"]);
                        int GangIdOwner = Convert.ToInt32(Row["group_id"]);

                        GangTurfs newVhOwn = new GangTurfs(RoomId, GangIdOwner);
                        _GangTurfs.TryAdd(RoomId, newVhOwn);
                    }
                }
            }
            log.Info("Loaded "+ _GangTurfs.Count + " Gang Turfs");
        }

        // Obtener el elemento con el key del diccionario
        public GangTurfs getTurfbyRoom(int RoomId)
        {
            if (_GangTurfs.ContainsKey(RoomId))
                return _GangTurfs[RoomId];
            else
                return null;
        }

        public List<GangTurfs> getTurfbyRoomList(int Id)
        {
            List<GangTurfs> VO = new List<GangTurfs>();

            lock (_GangTurfs)
            {
                if (_GangTurfs.Values.Where(x => x.RoomId == Id).ToList().Count > 0)
                    VO.Add(_GangTurfs.Values.FirstOrDefault(x => x.RoomId == Id));
            }
            return VO;
        }

        // Obtener barrios de X Banda
        public List<GangTurfs> getTurfsbyGang(int GangId)
        {
            List<GangTurfs> VO = new List<GangTurfs>();

            foreach (var item in _GangTurfs)
            {
                if (item.Value.GangIdOwner == GangId)
                    VO.Add(item.Value);
            }
            return VO;
        }
    }
}
