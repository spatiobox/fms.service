namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BucketData
    { 
        public Guid ID { get; set; }
         
        public string Title { get; set; }
         
        public string Scale { get; set; }
         
        public string Url { get; set; }

        public string Signature { get; set; }

        public string AppID { get; set; }

        public string Type { get; set; }
    }
}
