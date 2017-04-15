using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class DictionaryContext : Repository<Dictionary>
    {
        public DictionaryContext()
            : this(new OmsContext())
        {
        }

        public DictionaryContext(OmsContext context)
        {
            this.context = context;
        }
    }
}