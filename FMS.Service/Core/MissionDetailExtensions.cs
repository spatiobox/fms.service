using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FMS.Service.DAO;
using FMS.Service.Models;

namespace FMS.Service.Core
{
    public static class MissionDetailExtensions
    {

        #region MissionDetail
        public static MissionDetailData ToViewData(this MissionDetail node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new MissionDetailData()
            {
                ID = node.ID,
                MissionID = node.MissionID,
                RecipeID = node.RecipeID,
                MaterialID = node.Recipe.MaterialID,
                MaterialTitle = node.Recipe.Material.Title,
                Weight = node.Weight,
                //Weighing = node.Weighing,
                StandardWeight = node.Recipe.Weight,
                CreateDate = node.CreateDate,
                IsRatio = node.Recipe.IsRatio,
                Deviation = node.Recipe.Deviation,
                DeviationWeight = node.Recipe.IsRatio ? (node.Recipe.Deviation * node.Recipe.Weight / 100) : node.Recipe.Deviation,
                Status = node.Status,
                StatusTitle = GetMissionDetailStatusTitle(node.Status),
                Mission = ((suffix & CategoryDictionary.Mission) == CategoryDictionary.Mission) ? node.Mission.ToViewData() : null,
                Recipe = ((suffix & CategoryDictionary.Recipe) == CategoryDictionary.Recipe) ? node.Recipe.ToViewData() : null,
                //Scales = ((suffix & CategoryDictionary.Scale) == CategoryDictionary.Scale) ? node.Scales.ToList().Select(s => s.ToViewData()).ToList() : null
                Scales = node.Scales.ToList().Select(s => s.ToViewData()).ToList()

            };
        }

        public static IEnumerable<MissionDetailData> ToViewList(this IQueryable<MissionDetail> node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            if (node == null) return null;
            return node.ToList().Select(x => x.ToViewData(suffix));
        }

        public static string GetMissionDetailStatusTitle(int status)
        {
            string result = "ready";
            if (status == 1)
            {
                result = "weighing";
            }
            else if (status == 2)
            {
                result = "accomplished";
            }
            return result;
        }

        public static MissionDetail ToModel(this MissionDetailData node)
        {
            return new MissionDetail()
            {
                ID = node.ID,
                MissionID = node.MissionID,
                RecipeID = node.RecipeID,
                Weight = node.Weight,
                //Weighing = node.Weighing,
                CreateDate = node.CreateDate,
                Status = node.Status
            };
        }
        #endregion
    }
}