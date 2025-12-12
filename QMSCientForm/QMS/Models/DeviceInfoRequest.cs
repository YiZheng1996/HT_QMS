using System;

namespace QMSCientForm.QMS.Models
{
    /// <summary>
    /// 设备信息请求模型
    /// </summary>
    public class DeviceInfoRequest
    {
        /// <summary>
        /// 请求流水号（供应商编码+时间）0000088064+年月日时分秒纯数字
        /// </summary>
        public string requestNo { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string productCategory { get; set; }

        /// <summary>
        /// 供方编号
        /// </summary>
        public string suppNo { get; set; }

        /// <summary>
        /// 接口密码
        /// </summary>
        public string suppSecurity { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        public string processName { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string deviceName { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceNo { get; set; }

        /// <summary>
        /// 设备状态（正常开机、正常关机、发生故障、故障恢复）
        /// </summary>
        public string deviceStatus { get; set; }

        /// <summary>
        /// 设备状态改变时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string deviceStatusCtime { get; set; }

        /// <summary>
        /// 检定有效期（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string deviceVld { get; set; }

        /// <summary>
        /// 数据发送时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string sendTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }
}