using System.Collections.Generic;
namespace FMS.Service.DAO
{
    public partial class IDaoData<T>
    {
        public IEnumerable<T> list { get; set; }
        public Pagination pagination { get; set; }
    }
}
