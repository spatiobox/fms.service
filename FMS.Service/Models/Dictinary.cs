namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Dictionary")]
    public partial class Dictionary
    { 

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }


        [Required]
        [StringLength(50)]
        public string Name { get; set; }


        [Required]
        [StringLength(50)]
        public string Title { get; set; }


        [StringLength(50)]
        public string TitleCN { get; set; }


        [StringLength(50)]
        public string TitleTW { get; set; }


        [StringLength(50)]
        public string TitleEN { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

    }
}
