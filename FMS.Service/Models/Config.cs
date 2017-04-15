namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Config")]
    public partial class Config
    {
        public Config()
        {

        }

        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        [StringLength(256)]
        public string AppID { get; set; }

        [StringLength(1024)]
        public string SecretID { get; set; }

        [StringLength(1024)]
        public string SecretKey { get; set; }
    }
}
