namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
     
    public partial class RecipeData
    {  
        public Guid ID { get; set; }
         
        public string UserID { get; set; }
         
        public Guid FormularID { get; set; }

        public string FormularCode { get; set; }

        public string FormularTitle { get; set; }

        public Guid MaterialID { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialTitle { get; set; }
         
         
        public decimal Weight { get; set; }
         
        public decimal Deviation { get; set; }

        public decimal DeviationWeight { get; set; }

        public bool IsRatio { get; set; }
         
        public int Sort { get; set; }
                 
    }
}
