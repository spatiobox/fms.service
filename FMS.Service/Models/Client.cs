namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Client")]
    public partial class Client
    {
        public string ID { get; set; }

        [StringLength(128)]
        public string Code { get; set; }

        [StringLength(128)]
        public string Title { get; set; }

        [Required]
        [StringLength(128)]
        public string UserID { get; set; }

        public string Secret { get; set; }

        public virtual User User { get; set; }
    }
}
