using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.DAO
{
    public partial class MissionDetailData
    {
        public Guid ID { get; set; }

        public string Title { get; set; }

        public Guid MissionID { get; set; }

        public Guid RecipeID { get; set; }

        public Guid MaterialID { get; set; }

        public string MaterialTitle { get; set; }

        public decimal Weight { get; set; }

        /// <summary>
        /// �����е�����
        /// </summary>
        public decimal Weighing { get; set; }

        public decimal StandardWeight { get; set; }

        public DateTime CreateDate { get; set; }

        public decimal Deviation { get; set; }

        public decimal DeviationWeight { get; set; }

        public bool IsRatio { get; set; }

        public int Status { get; set; }

        public string StatusTitle { get; set; }

        public virtual MissionData Mission { get; set; }

        public virtual RecipeData Recipe { get; set; }

        public virtual ICollection<ScaleData> Scales { get; set; }

    }
}
