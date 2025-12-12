using FreeSql.DataAnnotations;

namespace QMSCientForm.Model
{
    [Table(Name = "TestModel")]
    public class TestModelModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string spec { get; set; }

        /// <summary>
        /// 单元格名称
        /// </summary>
        public string cell_name { get; set; }

        /// <summary>
        /// 参数名称,MR泄漏试验、作用试验、紧急制动、压力开关测试
        /// </summary>
        public string paraname { get; set; }

        /// <summary>
        /// 参数单位
        /// </summary>
        public string paraunit { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public string standmin { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public string standmax { get; set; }
    }
}
