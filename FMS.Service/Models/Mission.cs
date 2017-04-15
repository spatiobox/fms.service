namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Mission")]
    public partial class Mission
    {
        public Mission()
        {
            MissionDetails = new HashSet<MissionDetail>();
        }

        public Guid ID { get; set; }


        [StringLength(50)]
        public string Title { get; set; }

        public Guid FormularID { get; set; }

        public bool IsTeamwork { get; set; }

        [StringLength(50)]
        public string TeamID { get; set; }

        public bool IsAutomatic { get; set; }

        public DateTime CreateDate { get; set; }

        public int Status { get; set; }

        public virtual Formular Formular { get; set; }

        public virtual ICollection<MissionDetail> MissionDetails { get; set; }

        //public virtual ICollection<Record> Records { get; set; }
    }
}
