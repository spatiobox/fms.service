using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class RecordContext : Repository<Record>
    {
        public RecordContext()
            : this(new OmsContext())
        {
        }

        public RecordContext(OmsContext context)
        {
            this.context = context;
        }
    }
}