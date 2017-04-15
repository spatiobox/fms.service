using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FMS.Service.Models
{
    [Table("Bucket")]
    public class Bucket
    {

        [Key]
        public Guid ID { get; set; }

        [StringLength(50)]
        public string Title { get; set; }

        [StringLength(50)]
        public string Scale { get; set; }

        [StringLength(1024)]
        public string Url { get; set; }
         
    }
}