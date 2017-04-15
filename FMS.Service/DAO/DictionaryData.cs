namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
     
    public partial class DictionaryData
    {  
        public int ID { get; set; }
         
        public string Code { get; set; }
        
        public string Name { get; set; }

        public string Title { get; set; }
         
        public string TitleCN { get; set; }

        public string TitleTW { get; set; }
         
        public string TitleEN { get; set; }

        public string Description { get; set; }

    }
}
