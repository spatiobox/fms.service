using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class ScaleContext : Repository<Scale>
    {
        public ScaleContext()
            : this(new OmsContext())
        {
        }

        public ScaleContext(OmsContext context)
        {
            this.context = context;
        }
    }
}