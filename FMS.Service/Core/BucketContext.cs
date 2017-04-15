using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class BucketContext : Repository<Bucket>
    {
        public BucketContext()
            : this(new OmsContext())
        {
        }

        public BucketContext(OmsContext context)
        {
            this.context = context;
        }
    }
}