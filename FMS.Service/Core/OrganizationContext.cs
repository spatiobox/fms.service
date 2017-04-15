using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class OrganizationContext : Repository<Organization>
    {
        public OrganizationContext()
            : this(new OmsContext())
        {
        }

        public OrganizationContext(OmsContext context)
        {
            this.context = context;
        }
    }
}