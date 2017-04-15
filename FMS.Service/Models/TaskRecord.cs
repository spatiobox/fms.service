namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskRecord")]
    public partial class TaskRecord
    { 

        public int ID { get; set; }
        
        public Guid FormulaID { get; set; }

        public Guid RecipeID { get; set; }

        public Guid ScaleID { get; set; }

        public decimal Weight { get; set; }
         
        public decimal DeviationWeight { get; set; }

    }
}
