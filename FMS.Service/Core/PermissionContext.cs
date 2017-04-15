using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class PermissionContext : Repository<Permission>
    {
        public PermissionContext()
            : this(new OmsContext())
        {
        }

        public PermissionContext(OmsContext context)
        {
            this.context = context;
        }
    }
}