using FreeSql.DataAnnotations;
using System;

namespace QMSCientForm.Model
{
    [Table(Name = "TestData")]
    public class TestDataModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 单元格名称
        /// </summary>
        public string cell_name { get; set; }

        /// <summary>
        /// 单元格名称对应测试值
        /// </summary>
        public string cell_value { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string spec { get; set; }

        /// <summary>
        /// 测试人员
        /// </summary>
        public string tester { get; set; }

        /// <summary>
        /// 制造编号
        /// </summary>
        public string mfgno { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceno { get; set; }

        /// <summary>
        /// 接口调用状态，0未调，1成功，2失败
        /// </summary>
        public string qms_status { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime qms_time { get; set; }

        /// <summary>
        /// 接口反馈
        /// </summary>
        public string qms_response { get; set; }
    }
}
