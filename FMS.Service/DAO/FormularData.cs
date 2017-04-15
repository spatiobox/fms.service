using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{
    public class FormularData
    {
        public Guid ID { get; set; }
         
        public string Title { get; set; }
         
        public string Code { get; set; }

        public int OrgID { get; set; }

        public string UserID { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual UserData User { get; set; } 

        public virtual ICollection<RecipeData> Recipes { get; set; }

        public virtual OrganizationData Organization { get; set; }
         
    }
}