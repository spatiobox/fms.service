namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CipherData
    { 
        public string AppID { get; set; }
         
        public string SecretID { get; set; }
         
        public string SecretKey { get; set; }
                  
    }
}
