using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class MissionDetailContext : Repository<MissionDetail>
    {
        public MissionDetailContext()
            : this(new OmsContext())
        {
        }

        public MissionDetailContext(OmsContext context)
        {
            this.context = context;
        }
    }
}