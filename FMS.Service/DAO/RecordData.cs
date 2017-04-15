namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RecordData
    {
        public Guid ID { get; set; }

        public int OrgID { get; set; }

        public string OrgTitle { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Code { get; set; }

        public Guid FormularID { get; set; }

        public string FormularCode { get; set; }

        public string FormularTitle { get; set; }

        public Guid RecipeID { get; set; }

        public Guid MaterialID { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialTitle { get; set; }
        
        public string Device { get; set; }

        public int Copies { get; set; }

        public decimal StandardWeight { get; set; }

        public decimal Weight { get; set; }

        public DateTime RecordDate { get; set; }

        public string FullName { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }

        public string Remark { get; set; }

        public string BatchNo { get; set; }

        public string Viscosity { get; set; }

        public virtual UserData User { get; set; }

    }
}
