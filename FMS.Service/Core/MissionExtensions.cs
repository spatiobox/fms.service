using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FMS.Service.DAO;
using FMS.Service.Models;

namespace FMS.Service.Core
{
    public static class MissionExtensions
    {

        #region Mission
        public static MissionData ToViewData(this Mission node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new MissionData()
            {
                ID = node.ID,
                Title = node.Title,
                FormularID = node.FormularID,
                FormularTitle = node.Formular == null ? null : node.Formular.Title,
                IsTeamwork = node.IsTeamwork,
                TeamID = node.TeamID,
                IsAutomatic = node.IsAutomatic,
                CreateDate = node.CreateDate,
                Status = node.Status,
                StatusTitle = GetMissionStatusTitle(node.Status),
                Formular = ((suffix & CategoryDictionary.Formular) == CategoryDictionary.Formular) ? node.Formular.ToViewData() : null,
                MissionDetails = ((suffix & CategoryDictionary.MissionDetail) == CategoryDictionary.MissionDetail) ? node.MissionDetails.ToList().Select(x => x.ToViewData()).ToList() : null
            };
        }

        public static string GetMissionStatusTitle(int status)
        {
            string result = "unassigned";
            if (status == 1)
            {
                result = "working";
            }
            else if (status == 2)
            {
                result = "accomplished";
            }
            else if (status == 8)
            {
                result = "cancel";
            }
            return result;
        }

        public static Mission ToModel(this MissionData node)
        {
            return new Mission()
            {
                ID = node.ID,
                Title = node.Title,
                FormularID = node.FormularID,
                IsTeamwork = node.IsTeamwork,
                TeamID = node.TeamID,
                IsAutomatic = node.IsAutomatic,
                CreateDate = node.CreateDate,
                Status = node.Status
                //MissionDetails = node.MissionDetails.Select(x => x.ToModel()).ToList()
            };
        }
        #endregion
    }
}