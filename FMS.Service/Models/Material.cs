namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Material")]
    public partial class Material
    {
        public Material()
        {
            Recipes = new HashSet<Recipe>();
            //Records = new HashSet<Record>();
        }

        public Guid ID { get; set; }

        [Required]
        [StringLength(128)]
        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }
    
        //public virtual ICollection<Record> Records { get; set; }
    }
}
