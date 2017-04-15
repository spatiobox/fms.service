using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{
    public class OrganizationData
    {

        public int ID { get; set; }

        public int? ParentID { get; set; }

        public string Title { get; set; }

        public virtual ICollection<UserData> Users { get; set; }

        public virtual OrganizationData Parent { get; set; }

        public virtual ICollection<OrganizationData> Children { get; set; }

        public virtual ICollection<FormularData> Formulas { get; set; }
    }
}