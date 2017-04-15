using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class MaterialContext : Repository<Material>
    {
        public MaterialContext()
            : this(new OmsContext())
        {
        }

        public MaterialContext(OmsContext context)
        {
            this.context = context;
        }
    }
}