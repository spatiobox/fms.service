namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MissionDetail")]
    public partial class MissionDetail
    {
        public MissionDetail()
        {
            Scales = new HashSet<Scale>();
        }

        public Guid ID { get; set; }
        
        public Guid MissionID { get; set; }

        public Guid RecipeID { get; set; }

        public decimal Weight { get; set; }

        /// <summary>
        /// �����е�����
        /// </summary>
        //public decimal Weighing { get; set; }

        public DateTime CreateDate { get; set; }

        public int Status { get; set; }

        public virtual Mission Mission { get; set; }

        public virtual Recipe Recipe { get; set; }

        public virtual ICollection<Scale> Scales { get; set; }
    }
}
