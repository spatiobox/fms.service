using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class FormularContext : Repository<Formular>
    {
        public FormularContext()
            : this(new OmsContext())
        {
        }

        public FormularContext(OmsContext context)
        {
            this.context = context;
        }
    }
}