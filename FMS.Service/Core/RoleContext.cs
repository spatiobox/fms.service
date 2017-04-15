using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class RoleContext : Repository<Role>
    {
        public RoleContext()
            : this(new OmsContext())
        {
        }

        public RoleContext(OmsContext context)
        {
            this.context = context;
        }
    }
}