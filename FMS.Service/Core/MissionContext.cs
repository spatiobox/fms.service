using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class MissionContext : Repository<Mission>
    {
        public MissionContext()
            : this(new OmsContext())
        {
        }

        public MissionContext(OmsContext context)
        {
            this.context = context;
        }
    }
}