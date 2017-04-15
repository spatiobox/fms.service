namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Formular")]
    public partial class Formular
    {
        public Formular()
        {
            Recipes = new HashSet<Recipe>();
            Missions = new HashSet<Mission>();
        }

        public Guid ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        public int OrgID { get; set; }

        [Required]
        [StringLength(128)]
        public string UserID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreateDate { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }

        //public virtual ICollection<Record> Records { get; set; }
        public virtual Organization Organization { get; set; }

        public virtual ICollection<Mission> Missions { get; set; }
    }
}
