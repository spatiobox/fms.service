using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Service.Core
{

    [Flags]
    public enum CategoryDictionary : long
    {
        [Display(Name = "无")]
        None = 0x00000,

        /// <summary>
        /// 1: 配方
        /// </summary>
        [Display(Name = "配方")]
        Formular = 0x00001,

        /// <summary>
        /// 2: 原料
        /// </summary>
        [Display(Name = "原料")]
        Material = 0x00002,

        /// <summary>
        /// 4: 配方明细
        /// </summary>
        [Display(Name = "配方明细")]
        Recipe = 0x00004,


        /// <summary>
        /// 8: 配方明细
        /// </summary>
        [Display(Name = "配方明细")]
        Record = 0x00008,


        /// <summary>
        /// 16: 用户
        /// </summary>
        [Display(Name = "用户")]
        User = 0x00010,


        /// <summary>
        /// 32: 角色
        /// </summary>
        [Display(Name = "角色")]
        Role = 0x00020,

        /// <summary>
        /// 64: 权限
        /// </summary>
        [Display(Name = "权限")]
        Permission = 0x00040,

        /// <summary>
        /// 128: 权限
        /// </summary>
        [Display(Name = "个人资料")]
        Profile = 0x00080,

        /// <summary>
        /// 256: 权限
        /// </summary>
        [Display(Name = "字典")]
        Dictionary = 0x0100,

        /// <summary>
        /// 512: 空间
        /// </summary>
        [Display(Name = "空间")]
        Bucket = 0x00200,

        /// <summary>
        /// 1024: 配置
        /// </summary>
        [Display(Name = "配置")]
        Config = 0x00400,

        /// <summary>
        /// 2048: 项目
        /// </summary>
        [Display(Name = "项目")]
        Organization = 0x00800,

        /// <summary>
        /// 4096: 父级对象
        /// </summary>
        [Display(Name = "父级对象")]
        Parent = 0x01000,

        /// <summary>
        /// 8192: 父级对象
        /// </summary>
        [Display(Name = "子级对象")]
        Children = 0x02000,

        /// <summary>
        /// 16384: 台秤
        /// </summary>
        [Display(Name = "台秤")]
        Scale = 0x04000,

        /// <summary>
        /// 32768: 任务
        /// </summary>
        [Display(Name = "任务")]
        Mission = 0x08000,

        /// <summary>
        /// 65536: 任务明细
        /// </summary>
        [Display(Name = "任务明细")]
        MissionDetail = 0x10000, 
          
        /// <summary>
        /// 2048: 签名
        /// </summary>
        [Display(Name = "签名")]
        Signature = 0x20000,

        /// <summary>
        /// 1024: 全部
        /// </summary>
        [Display(Name = "全部")]
        All = 0xFFFFF
    }
}
