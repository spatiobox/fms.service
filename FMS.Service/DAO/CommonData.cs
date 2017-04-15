using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{
    public class CommonData
    {
        public IList<Guid> guids { get; set; }

        public IList<int> ids { get; set; }
        
        public IList<string> uids { get; set; }
    }
}