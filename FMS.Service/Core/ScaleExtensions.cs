using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FMS.Service.DAO;
using FMS.Service.Models;

namespace FMS.Service.Core
{
    public static class ScaleExtensions
    {

        #region Scale
        public static ScaleData ToViewData(this Scale node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            var bean = new ScaleData()
            {
                ID = node.ID,
                Title = node.Title,
                Device = node.Device,
                MaxRange = node.MaxRange,
                Precision = node.Precision,
                MissionID = node.MissionDetail == null ? Guid.Empty : node.MissionDetail.MissionID,
                MissionDetailID = node.MissionDetailID,
                MaterialTitle = node.MissionDetail == null ? "" : node.MissionDetail.Recipe.Material.Title,
                RecipeID = node.MissionDetail == null ? Guid.Empty : node.MissionDetail.RecipeID,
                Team = node.Team,
                LastHeartBeat = node.LastHeartBeat,
                IPAddress = node.IPAddress,
                Percent = node.Percent,
                Weight = node.Weight,
                DeviationWeight = node.MissionDetailID.HasValue ? (node.MissionDetail.Recipe.IsRatio ? (node.MissionDetail.Recipe.Deviation * node.Weight / 100) : node.MissionDetail.Recipe.Deviation) : null,
                Salt = node.Salt,
                Status = ((DateTime.Now - node.LastHeartBeat).TotalMinutes > 30) ? (int)ScaleStatusCategory.offline : node.Status,
                StatusTitle = GetScaleStatusTitle(((DateTime.Now - node.LastHeartBeat).TotalMinutes > 30) ? (int)ScaleStatusCategory.offline : node.Status),
                MissionDetail = ((suffix & CategoryDictionary.MissionDetail) == CategoryDictionary.MissionDetail && node.MissionDetailID.HasValue) ? node.MissionDetail.ToViewData() : null
            };
            return bean;
        }


        public static IEnumerable<ScaleData> ToViewList(this IQueryable<Scale> node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            if (node == null) return null;
            return node.ToList().Select(x => x.ToViewData(suffix));
        }

        public static string GetScaleStatusTitle(int status)
        {
            string result = "offline";
            if (status == 1)
            {
                result = "idle";
            }
            else if (status == 2)
            {
                result = "working";
            }
            else if (status == 4)
            {
                result = "pause";
            }
            else if (status == 8)
            {
                result = "cancel";
            }
            return result;
        }

        public static Scale ToModel(this ScaleData node)
        {
            return new Scale()
            {
                ID = node.ID,
                Title = node.Title,
                Device = node.Device,
                MaxRange = node.MaxRange,
                Precision = node.Precision,
                Percent = node.Percent,
                Weight = node.Weight,
                DeviationWeight = node.DeviationWeight,
                Salt = node.Salt,
                MissionDetailID = node.MissionDetailID,
                Team = node.Team,
                LastHeartBeat = node.LastHeartBeat,
                IPAddress = node.IPAddress,
                Status = node.Status
            };
        }
        #endregion
    }
}