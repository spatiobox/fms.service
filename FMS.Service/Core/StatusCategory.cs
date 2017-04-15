using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{

    [Flags]
    public enum TaskStatusCategory
    {
        /// <summary>
        /// 未派工
        /// </summary>
        [Display(Name = "未派工")]
        unassigned = 0,

        /// <summary>
        /// 派工中
        /// </summary>
        [Display(Name = "派工中")]
        working = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        [Display(Name = "已完成")]
        accomplished = 2,

        /// <summary>
        /// 取消
        /// </summary>
        [Display(Name = "取消")]
        cancel = 0x08,
    }

    [Flags]
    public enum ScaleStatusCategory
    {
        /// <summary>
        /// 离线
        /// </summary>
        [Display(Name = "离线")]
        offline = 0x0,

        /// <summary>
        /// 空闲
        /// </summary>
        [Display(Name = "空闲")]
        idle = 0x01,

        /// <summary>
        /// 工作中
        /// </summary>
        [Display(Name = "工作中")]
        working = 0x02,

        /// <summary>
        /// 暂停
        /// </summary>
        [Display(Name = "暂停")]
        pause = 0x04,

        /// <summary>
        /// 取消
        /// </summary>
        [Display(Name = "取消")]
        cancel = 0x08,
         
    }
    
    [Flags]
    public enum TaskDetailStatusCategory
    {
        /// <summary>
        /// 未开始
        /// </summary>
        [Display(Name = "未开始")]
        ready = 0,

        /// <summary>
        /// 称重中
        /// </summary>
        [Display(Name = "称重中")]
        weighing = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        [Display(Name = "已完成")]
        accomplished = 2
    }
}