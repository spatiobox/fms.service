using OFMSService.DAO;
using FMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public static class Extensions
    {
        #region Formular
        public static FormularData ToViewData(this Formular node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new FormularData()
            {
                Code = node.Code,
                CreateDate = node.CreateDate,
                ID = node.ID,
                Title = node.Title,
                //User = node.User.ToViewData(),
                OrgID = node.OrgID,
                UserID = node.UserID,
                User = ((suffix & CategoryDictionary.User) == CategoryDictionary.User) ? node.User.ToViewData() : null,
                Recipes = ((suffix & CategoryDictionary.Recipe) == CategoryDictionary.Recipe) ? node.Recipes.ToList().Select(x => x.ToViewData()).ToList() : null,
                Organization = ((suffix & CategoryDictionary.Organization) == CategoryDictionary.Organization) ? node.Organization.ToViewData() : null
            };
        }

        public static Formular ToModel(this FormularData node)
        {
            return new Formular()
            {
                Code = node.Code,
                CreateDate = node.CreateDate,
                ID = node.ID,
                Title = node.Title,
                OrgID = node.OrgID,
                UserID = node.UserID
            };
        }
        #endregion

        #region Material
        public static MaterialData ToViewData(this Material node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new MaterialData()
            {
                Code = node.Code,
                ID = node.ID,
                Title = node.Title,
                UserID = node.UserID,
                User = ((suffix & CategoryDictionary.User) == CategoryDictionary.User) ? node.User.ToViewData() : null,
                Recipes = ((suffix & CategoryDictionary.Recipe) == CategoryDictionary.Recipe) ? node.Recipes.ToList().Select(x => x.ToViewData()).ToList() : null,
            };
        }
        public static Material ToModel(this MaterialData node)
        {
            return new Material()
            {
                Code = node.Code,
                ID = node.ID,
                Title = node.Title,
                UserID = node.UserID
            };
        }
        #endregion

        #region Recipe
        public static RecipeData ToViewData(this Recipe node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new RecipeData()
            {
                Deviation = node.Deviation,
                FormularID = node.FormularID,
                FormularCode = node.Formular == null ? "" : node.Formular.Code,
                FormularTitle = node.Formular == null ? "" : node.Formular.Title,
                ID = node.ID,
                IsRatio = node.IsRatio,
                MaterialID = node.MaterialID,
                MaterialCode = node.Material == null ? "" : node.Material.Code,
                MaterialTitle = node.Material == null ? "" : node.Material.Title,
                Sort = node.Sort,
                UserID = node.UserID,
                Weight = node.Weight,
                DeviationWeight = node.IsRatio ? (node.Deviation * node.Weight / 100) : node.Deviation
            };
        }
        public static Recipe ToModel(this RecipeData node)
        {
            return new Recipe()
            {
                Deviation = node.Deviation,
                FormularID = node.FormularID,
                ID = node.ID,
                IsRatio = node.IsRatio,
                MaterialID = node.MaterialID,
                Sort = node.Sort,
                UserID = node.UserID,
                Weight = node.Weight
            };
        }
        #endregion

        #region User
        public static UserData ToViewData(this User node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new UserData()
            {
                Id = node.Id,
                AccessFailedCount = node.AccessFailedCount,
                Email = node.Email,
                EmailConfirmed = node.EmailConfirmed,
                LockoutEnabled = node.LockoutEnabled,
                LockoutEndDateUtc = node.LockoutEndDateUtc,
                //PasswordHash = node.PasswordHash,
                PhoneNumber = node.PhoneNumber,
                PhoneNumberConfirmed = node.PhoneNumberConfirmed,
                SecurityStamp = node.SecurityStamp,
                TwoFactorEnabled = node.TwoFactorEnabled,
                UserName = node.UserName,
                FullName = node.FullName,
                Company = node.Company,
                Remark = node.Remark,
                Position = node.Position,
                Department = node.Department,
                Status = node.Status,
                Formulars = ((suffix & CategoryDictionary.Formular) == CategoryDictionary.Formular) ? node.Formulars.ToList().Select(x => x.ToViewData()).ToList() : null,
                Materials = ((suffix & CategoryDictionary.Material) == CategoryDictionary.Material) ? node.Materials.ToList().Select(x => x.ToViewData()).ToList() : null,
                Recipes = ((suffix & CategoryDictionary.Recipe) == CategoryDictionary.Recipe) ? node.Recipes.ToList().Select(x => x.ToViewData()).ToList() : null,
                Records = ((suffix & CategoryDictionary.Record) == CategoryDictionary.Record) ? node.Records.ToList().Select(x => x.ToViewData()).ToList() : null,
                Roles = ((suffix & CategoryDictionary.Role) == CategoryDictionary.Role) ? node.Roles.ToList().Select(x => x.ToViewData()).ToList() : null,
                Permissions = ((suffix & CategoryDictionary.Permission) == CategoryDictionary.Permission) ? node.Roles.SelectMany(x => x.Permissions).ToList().Select(x => x.ToViewData()).OrderBy(x => x.Sort).ToList() : null,
                Profile = ((suffix & CategoryDictionary.Profile) == CategoryDictionary.Profile) ? node.Profile.ToViewData() : null,
                Organizations = ((suffix & CategoryDictionary.Organization) == CategoryDictionary.Organization) ? node.Organizations.ToList().Select(x => x.ToViewData()).ToList() : null
            };
        }
        public static User ToModel(this UserData node)
        {
            return new User()
            {
                Id = node.Id,
                AccessFailedCount = node.AccessFailedCount,
                Email = node.Email,
                EmailConfirmed = node.EmailConfirmed,
                LockoutEnabled = node.LockoutEnabled,
                LockoutEndDateUtc = node.LockoutEndDateUtc,
                //PasswordHash = node.PasswordHash,
                PhoneNumber = node.PhoneNumber,
                PhoneNumberConfirmed = node.PhoneNumberConfirmed,
                SecurityStamp = node.SecurityStamp,
                TwoFactorEnabled = node.TwoFactorEnabled,
                UserName = node.UserName,
                FullName = node.FullName ?? "",
                Company = node.Company ?? "",
                Department = node.Department ?? "",
                Position = node.Position ?? "",
                Status = node.Status,
                Remark = node.Remark

            };
        }
        #endregion

        #region Role
        public static RoleData ToViewData(this Role node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new RoleData()
            {
                ID = node.Id,
                Name = node.Name,
                Permissions = ((suffix & CategoryDictionary.Permission) == CategoryDictionary.Permission) ? node.Permissions.ToList().Select(x => x.ToViewData(suffix)).ToList() : null
            };
        }
        public static Role ToModel(this RoleData node)
        {
            return new Role()
            {
                Id = node.ID,
                Name = node.Name
            };
        }
        #endregion

        #region Permission
        public static PermissionData ToViewData(this Permission node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new PermissionData()
            {
                Action = node.Action,
                Actived = node.Actived,
                Category = node.Category,
                Code = node.Code,
                Controller = node.Controller,
                Description = node.Description,
                Icon = node.Icon,
                ID = node.ID,
                IsAPI = node.IsAPI,
                IsNav = node.IsNav,
                IsDefault = node.IsDefault,
                SystemName = node.SystemName,
                Name = node.Name,
                ParentID = node.ParentID,
                Sort = node.Sort,
                Url = node.Url,
                Roles = ((suffix & CategoryDictionary.Role) == CategoryDictionary.Role) ? node.Roles.ToList().Select(x => x.ToViewData(suffix)).ToList() : null
            };
        }
        public static Permission ToModel(this PermissionData node)
        {
            return new Permission()
            {
                Action = node.Action,
                Actived = node.Actived,
                Category = node.Category,
                Code = node.Code,
                Controller = node.Controller,
                Description = node.Description,
                Icon = node.Icon,
                ID = node.ID,
                IsAPI = node.IsAPI,
                IsNav = node.IsNav,
                Name = node.Name,
                ParentID = node.ParentID,
                Sort = node.Sort,
                Url = node.Url
            };
        }
        #endregion

        #region Record
        public static RecordData ToViewData(this Record node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new RecordData()
            {
                ID = node.ID,
                OrgID = node.OrgID,
                OrgTitle = node.OrgTitle,
                Code = "",
                Copies = node.Copies,
                Device = node.Device,
                FormularID = node.FormularID,
                FormularCode = node.FormularCode,
                FormularTitle = node.FormularTitle,
                MaterialID = node.MaterialID,
                MaterialCode = node.MaterialCode,
                MaterialTitle = node.MaterialTitle,
                RecipeID = node.RecipeID,
                RecordDate = node.RecordDate,
                UserID = node.UserID,
                UserName = node.User == null ? "" : node.User.UserName,
                FullName = node.User == null ? "" : node.User.FullName,
                StandardWeight = node.StandardWeight,
                Weight = node.Weight,
                Department = node.User == null ? "" : node.User.Department,
                Position = node.User == null ? "" : node.User.Position,
                Remark = node.User == null ? "" : node.User.Remark,
                BatchNo = node.BatchNo,
                Viscosity = node.Viscosity,
                User = ((suffix & CategoryDictionary.User) == CategoryDictionary.User) ? node.User.ToViewData() : null
            };
        }

        public static Record ToModel(this RecordData node)
        {
            var model = new Record()
            {
                ID = node.ID,
                Copies = node.Copies,
                Device = node.Device ?? "",
                RecipeID = node.RecipeID,
                RecordDate = node.RecordDate,
                UserID = node.UserID,
                Weight = node.Weight,
                BatchNo = node.BatchNo ?? "",
                Viscosity = node.Viscosity ?? ""
            };
            var ctx = new OmsContext();
            try
            {
                var recipe = ctx.Recipes.Find(node.RecipeID);
                if (recipe != null)
                {
                    model.FormularID = recipe.Formular.ID;
                    model.FormularCode = recipe.Formular.Code;
                    model.FormularTitle = recipe.Formular.Title;
                    model.MaterialID = recipe.Material.ID;
                    model.MaterialCode = recipe.Material.Code;
                    model.MaterialTitle = recipe.Material.Title;
                    model.StandardWeight = recipe.Weight;
                    //model.UserID = recipe.UserID;
                }
                var org = ctx.Organizations.Find(node.OrgID);
                if (org != null)
                {
                    model.OrgID = org.ID;
                    model.OrgTitle = org.Title;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return model;
        }
        #endregion

        #region Profile
        public static ProfileData ToViewData(this Profile node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new ProfileData()
            {
                UserID = node.UserID,
                Language = node.Language,
                User = ((suffix & CategoryDictionary.User) == CategoryDictionary.User) ? node.User.ToViewData() : null
            };
        }

        public static Profile ToModel(this ProfileData node)
        {
            return new Profile()
            {
                UserID = node.UserID,
                Language = node.Language
            };
        }
        #endregion

        #region Dictionary
        public static DictionaryData ToViewData(this Dictionary node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new DictionaryData()
            {
                ID = node.ID,
                Code = node.Code,
                Name = node.Name,
                Title = node.Title,
                TitleCN = node.TitleCN,
                TitleTW = node.TitleTW,
                TitleEN = node.TitleEN,
                Description = node.Description
            };
        }

        public static Dictionary ToModel(this DictionaryData node)
        {
            return new Dictionary()
            {
                ID = node.ID,
                Code = node.Code,
                Name = node.Name,
                Title = node.Title,
                TitleCN = node.TitleCN,
                TitleTW = node.TitleTW,
                TitleEN = node.TitleEN
            };
        }
        #endregion

        #region Bucket

        public static BucketData ToViewData(this Bucket node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            var model = new BucketData()
            {
                ID = node.ID,
                Title = node.Title,
                Scale = node.Scale,
                Url = node.Url
            };
            //if ((suffix & CategoryDictionary.Signature) == CategoryDictionary.Signature)
            //{
            //    model.AppID = OmniConsole.Cipher.AppID;
            //    model.Signature = "";
            //}
            return model;
        }

        public static Bucket ToModel(this BucketData node)
        {
            return new Bucket()
            {
                ID = node.ID,
                Title = node.Title,
                Scale = node.Scale,
                Url = node.Url
            };
        }
        #endregion


        #region Organization

        public static OrganizationData ToViewData(this Organization node, CategoryDictionary suffix = CategoryDictionary.None)
        {

            var model = new OrganizationData()
            {
                ID = node.ID,
                ParentID = node.ParentID,
                Title = node.Title,
                Users = ((suffix & CategoryDictionary.User) == CategoryDictionary.User) ? node.Users.ToList().Select(x => x.ToViewData()).ToList() : null,
                Parent = ((suffix & CategoryDictionary.Parent) == CategoryDictionary.Parent) ? node.Parent.ToViewData() : null,
                Children = ((suffix & CategoryDictionary.Children) == CategoryDictionary.Children) ? node.Children.ToList().Select(x => x.ToViewData()).ToList() : null,
                Formulas = ((suffix & CategoryDictionary.Formular) == CategoryDictionary.Formular) ? node.Formulas.ToList().Select(x => x.ToViewData()).ToList() : null
            };
            return model;
        }

        public static Organization ToModel(this OrganizationData node)
        {
            return new Organization()
            {
                ID = node.ID,
                ParentID = node.ParentID,
                Title = node.Title
            };
        }

        #endregion

        #region Config

        public static ConfigData ToViewData(this Config node, CategoryDictionary suffix = CategoryDictionary.None)
        {
            return new ConfigData()
            {
                ID = node.ID,
                AppID = node.AppID,
                SecretID = node.SecretID,
                SecretKey = node.SecretKey
            };
        }

        public static Config ToModel(this ConfigData node)
        {
            return new Config()
            {
                ID = node.ID,
                AppID = node.AppID,
                SecretID = node.SecretID,
                SecretKey = node.SecretKey
            };
        }
        #endregion

        public static object GetReportTitles(string language)
        {
            object json = null;
            var ctx = new DictionaryContext();
            var list = ctx.Filter(x => x.Code == "Region");
            var pm = list.FirstOrDefault(x => x.Name == "position");
            var dm = list.FirstOrDefault(x => x.Name == "department");
            var cm = list.FirstOrDefault(x => x.Name == "company");
            switch (language)
            {
                case "zh-CN":
                    json = new
                    {
                        Code = "编码",
                        Title = "标题",
                        CreateDate = "创建日期",
                        Deviation = "误差量",
                        DeviationWeight = "误差重量",
                        FormularTitle = "配方标题",
                        MaterialTitle = "配料标题",
                        Weight = "重量",
                        Copies = "份数",
                        RecordDate = "记录日期",
                        UserName = "用户名",
                        FullName = "用户姓名",
                        Status = "状态",
                        Department = dm == null ? "部门" : dm.TitleCN,
                        Position = pm == null ? "职位" : pm.TitleCN,
                        Company = cm == null ? "公司" : cm.TitleCN,
                        Remark = "备注",
                        BatchNo = "批号",
                        Viscosity = "粘度",
                        IsRatio = "是否百分比",
                        Ratio = "配比",
                        Organization = "组织",
                        Directory = "目录"
                    };
                    break;
                case "zh-TW":
                    json = new
                    {
                        Code = "編碼",
                        Title = "標題",
                        CreateDate = "創建日期",
                        Deviation = "誤差量",
                        DeviationWeight = "誤差重量",
                        FormularTitle = "配方名稱",
                        MaterialTitle = "配料名稱",
                        Weight = "重量",
                        Copies = "份數",
                        RecordDate = "錄入日期",
                        UserName = "用戶名",
                        FullName = "用戶姓名",
                        Status = "狀態",
                        Department = dm == null ? "部門" : dm.TitleTW,
                        Position = pm == null ? "職位" : pm.TitleTW,
                        Company = cm == null ? "公司" : cm.TitleTW,
                        Remark = "備註",
                        BatchNo = "批號",
                        Viscosity = "黏度",
                        IsRatio = "是否百分比",
                        Ratio = "配比",
                        Organization = "組織",
                        Directory = "目錄"
                    };
                    break;
                case "en-US":
                    json = new
                    {
                        Code = "Code",
                        Title = "Title",
                        CreateDate = "Create Date",
                        Deviation = "Deviation",
                        DeviationWeight = "Deviation Weight",
                        FormularTitle = "Formular Name",
                        MaterialTitle = "Material Name",
                        Weight = "Weight",
                        Copies = "Copies",
                        RecordDate = "Record Date",
                        UserName = "User Name",
                        FullName = "Full Name",
                        Status = "Status",
                        Department = dm == null ? "Department" : dm.TitleEN,
                        Position = pm == null ? "Position" : pm.TitleEN,
                        Company = cm == null ? "Company" : cm.TitleEN,
                        Remark = "Remark",
                        BatchNo = "Batch",
                        Viscosity = "Viscosity",
                        IsRatio = "Is Percentage",
                        Ratio = "Ratio",
                        Organization = "Organization",
                        Directory = "Directory"
                    };
                    break;
                default:
                    break;
            }
            return json;
        }

        public static long ToUnixTime(this DateTime nowTime)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
        }
    }
}