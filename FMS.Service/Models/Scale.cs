namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Scale")]
    public partial class Scale
    {
        public Scale()
        { 
        }

        public Guid ID { get; set; }


        [StringLength(50)]
        public string Title { get; set; }

        [StringLength(100)]
        public string Device { get; set; }

        public decimal MaxRange { get; set; }

        public decimal? Weight { get; set; }

        public decimal? DeviationWeight { get; set; }

        public Guid? Salt { get; set; }

        public int Precision { get; set; }

        public int Percent { get; set; }

        public Guid? MissionDetailID { get; set; }

        [StringLength(50)]
        public string Team { get; set; }

        public DateTime LastHeartBeat { get; set; }

        [StringLength(20)]
        public string IPAddress { get; set; }

        public int Status { get; set; }

        public virtual MissionDetail MissionDetail { get; set; }

        //public virtual ICollection<Record> Records { get; set; }
    }
}
