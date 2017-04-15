namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ProfileData
    { 
        public string UserID { get; set; }
         
        public string Language { get; set; }

        public UserData User { get; set; }

         
    }
}
