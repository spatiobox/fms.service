using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{
    
    public partial class MissionData
    { 
        public Guid ID { get; set; }

         
        public string Title { get; set; }

        public Guid FormularID { get; set; }

        public string FormularTitle { get; set; }


        public bool IsTeamwork { get; set; }
         
        public string TeamID { get; set; }

        public bool IsAutomatic { get; set; }

        public DateTime CreateDate { get; set; }

        public int Status { get; set; }

        public string StatusTitle { get; set; }

        public virtual FormularData Formular { get; set; }

        public virtual ICollection<MissionDetailData> MissionDetails { get; set; }

        //public virtual ICollection<Record> Records { get; set; }
    }
}
