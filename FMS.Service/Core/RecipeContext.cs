using OFMSService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public class RecipeContext : Repository<Recipe>
    {
        public RecipeContext()
            : this(new OmsContext())
        {
        }

        public RecipeContext(OmsContext context)
        {
            this.context = context;
        }
    }
}