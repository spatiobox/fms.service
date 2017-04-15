namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Profile")]
    public partial class Profile
    {
        [Key]
        public string UserID { get; set; }

        [StringLength(10)]
        public string Language { get; set; }

        public virtual User User { get; set; }

    }
}
