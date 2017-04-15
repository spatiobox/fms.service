namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SignatureData
    { 
        public string AppID { get; set; }
         
        public string Signature { get; set; }
         
        public string Bucket { get; set; }
         
        public string Url { get; set; }
                 
    }

    public partial class SignatureParam
    {
        public string Type { get; set; }
        public string Scale { get; set; }

        public string Path { get; set; }
    }

}
