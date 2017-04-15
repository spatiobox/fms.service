using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{

    public partial class ScaleData
    {
        public Guid ID { get; set; }

        public string Title { get; set; }

        public string Device { get; set; }

        public decimal MaxRange { get; set; }

        public decimal? Weight { get; set; }

        //public decimal DispatchedWeight { }

        public decimal? DeviationWeight { get; set; }

        public Guid? Salt { get; set; }

        public int Precision { get; set; }

        public int Percent { get; set; }

        public Guid? MissionID { get; set; }

        public Guid? MissionDetailID { get; set; }

        public string MaterialTitle { get; set; }

        public Guid RecipeID { get; set; }

        public string Team { get; set; }

        public DateTime LastHeartBeat { get; set; }

        public string IPAddress { get; set; }

        public int Status { get; set; }

        public string StatusTitle { get; set; }

        public virtual MissionDetailData MissionDetail { get; set; }

        //public virtual ICollection<Record> Records { get; set; }
    }
}
