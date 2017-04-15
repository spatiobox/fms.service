namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MaterialData
    {

        public Guid ID { get; set; }

        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        public virtual UserData User { get; set; }

        public virtual ICollection<RecipeData> Recipes { get; set; }

    }
}
