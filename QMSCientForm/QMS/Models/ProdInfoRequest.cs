using System;

namespace QMSCientForm.QMS.Models
{
    /// <summary>
    /// 生产参数请求模型
    /// </summary>
    public class ProdInfoRequest
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
        /// 信息类别
        /// </summary>
        public string infoCategory { get; set; }

        /// <summary>
        /// 项目编号（必须为四方股份项目编号，如"GT-C11"）
        /// </summary>
        public string projectNo { get; set; }

        /// <summary>
        /// 生产编号
        /// </summary>
        public string productNo { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string batchNo { get; set; }

        /// <summary>
        /// 生产工序
        /// </summary>
        public string prodName { get; set; }

        /// <summary>
        /// 产品部位
        /// </summary>
        public string productPosition { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string paraName { get; set; }

        /// <summary>
        /// 参数单位
        /// </summary>
        public string paraUnit { get; set; }

        /// <summary>
        /// 标准值（标准位为-分隔的两个数值，如"28-38"）
        /// </summary>
        public string standValue { get; set; }

        /// <summary>
        /// 实测值
        /// </summary>
        public string actualValue { get; set; }

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