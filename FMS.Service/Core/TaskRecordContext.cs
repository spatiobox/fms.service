using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class TaskRecordContext : Repository<TaskRecord>
    {
        public TaskRecordContext()
            : this(new OmsContext())
        {
        }

        public TaskRecordContext(OmsContext context)
        {
            this.context = context;
        }
    }
}