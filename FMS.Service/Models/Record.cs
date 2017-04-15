namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Record")]
    public partial class Record
    {
        public Guid ID { get; set; }

        public int OrgID { get; set; }

        public string OrgTitle { get; set; }

        [Required]
        [StringLength(128)]
        public string UserID { get; set; }

        public Guid FormularID { get; set; }

        public string FormularCode { get; set; }

        public string FormularTitle { get; set; }

        public Guid MaterialID { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialTitle { get; set; }

        public Guid RecipeID { get; set; }

        [StringLength(100)]
        public string Device { get; set; }

        public int Copies { get; set; }

        public decimal StandardWeight { get; set; }

        public decimal Weight { get; set; }

        public string BatchNo { get; set; }

        public string Viscosity { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime RecordDate { get; set; }
        
        public virtual User User { get; set; }

        //public virtual Formular Formular { get; set; }

        //public virtual Material Material { get; set; }

        //public virtual Recipe Recipe { get; set; }
    }
}
