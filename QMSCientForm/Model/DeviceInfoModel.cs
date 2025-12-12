using FreeSql.DataAnnotations;
using System;

namespace QMSCientForm.Model
{
    [Table(Name = "DeviceInfo")]
    public class DeviceInfoModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceno { get; set; } = "C739-150";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.Now;

        /// <summary>
        /// 校准有效期
        /// </summary>
        public string checkdate { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string devicename { get; set; } = "通用制动控制元件试验台-气动类元件试验台";
    }
}
