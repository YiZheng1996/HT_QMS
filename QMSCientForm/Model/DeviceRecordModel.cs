using FreeSql.DataAnnotations;
using System;

namespace QMSCientForm.Model
{
    /// <summary>
    /// 设备记录模型
    /// </summary>
    [Table(Name = "DeviceRecord")]
    public class DeviceRecordModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceno { get; set; }

        /// <summary>
        /// 设备状态变更类型，如正常开机、正常关机、故障、恢复
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// 接口调用状态，0未调，1成功，2失败
        /// </summary>
        public string qms_status { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime qms_time { get; set; }

        /// <summary>
        /// 接口响应内容
        /// </summary>
        public string qms_response { get; set; }
    }
}
