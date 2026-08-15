using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.RolePlay.PlayRoom
{
    public class PlayRoom
    {
        public int Id { get; set; }
        public string CityRP { get; set; }
        public bool  HospitalRP { get; set; }
        public bool PrisonRP { get; set; }
        public bool CourtRP { get; set; }
        public bool CamioneroRP { get; set; }
        public bool MecanicoRP { get; set; }
        public bool BasureroRP { get; set; }
        public bool MineroRP { get; set; }
        public bool ArmeroRP { get; set; }
        public bool PolStationRP { get; set; }
        public bool SancRP { get; set; }

        public PlayRoom(int Id, string CityRP, bool HospitalRP, bool PrisonRP, bool CourtRP, bool CamioneroRP, bool MecanicoRP, bool BasureroRP, bool MineroRP, bool ArmeroRP, bool PolStationRP, bool SancRP)
        {
            this.Id = Id;
            this.CityRP = CityRP;
            this.HospitalRP = HospitalRP;
            this.CourtRP = CourtRP;
            this.CamioneroRP = CamioneroRP;
            this.MecanicoRP = MecanicoRP;
            this.BasureroRP = BasureroRP;
            this.MineroRP = MineroRP;
            this.ArmeroRP = ArmeroRP;
            this.PolStationRP = PolStationRP;
            this.SancRP = SancRP;
        }
    }
}
