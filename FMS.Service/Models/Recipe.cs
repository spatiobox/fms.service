namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Recipe")]
    public partial class Recipe
    {
        public Recipe()
        {
            //Records = new HashSet<Record>();
            MissionDetails = new HashSet<MissionDetail>();
        }

        public Guid ID { get; set; }

        [Required]
        [StringLength(128)]
        public string UserID { get; set; }

        public Guid FormularID { get; set; }

        public Guid MaterialID { get; set; }

        public decimal Weight { get; set; }

        public decimal Deviation { get; set; }

        public bool IsRatio { get; set; }

        public int Sort { get; set; }

        public virtual User User { get; set; }

        public virtual Formular Formular { get; set; }

        public virtual Material Material { get; set; }

        public virtual ICollection<MissionDetail> MissionDetails { get; set; }   
        //public virtual ICollection<Record> Records { get; set; }
    }
}
