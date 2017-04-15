using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class UserContext : Repository<User>
    {
        public UserContext()
            : this(new OmsContext())
        {
        }

        public UserContext(OmsContext context)
        {
            this.context = context;
        }
    }
}