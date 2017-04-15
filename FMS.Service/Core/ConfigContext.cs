using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class ConfigContext : Repository<Config>
    {
        public ConfigContext()
            : this(new OmsContext())
        {
        }

        public ConfigContext(OmsContext context)
        {
            this.context = context;
        }
    }
}