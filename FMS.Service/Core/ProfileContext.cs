using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class ProfileContext : Repository<Profile>
    {
        public ProfileContext()
            : this(new OmsContext())
        {
        }

        public ProfileContext(OmsContext context)
        {
            this.context = context;
        }
    }
}