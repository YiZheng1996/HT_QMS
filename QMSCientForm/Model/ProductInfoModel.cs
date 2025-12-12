using FreeSql.DataAnnotations;
using System;

namespace QMSCientForm.Model
{
    [Table(Name = "ProductInfo")]
    public class ProductInfoModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string projectno { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string projectname { get; set; }

        /// <summary>
        /// 车列号
        /// </summary>
        public string train { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string spec { get; set; }
        
        /// <summary>
        /// 编号 
        /// </summary>
        public string mfgno { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// 接口调用情况，0未调，1成功，2失败
        /// </summary>
        public string qms_status { get; set; }
        /// <summary>
        /// 接口调用时间
        /// </summary>
        public DateTime qms_time { get; set; }

        /// <summary>
        /// 接口调用？
        /// </summary>
        public string qms_rem { get; set; }

        /// <summary>
        /// 接口编号
        /// </summary>
        public string prdt_code { get; set; }

        /// <summary>
        /// 接口名称
        /// </summary>
        public string productname { get; set; }

        /// <summary>
        /// ？
        /// </summary>
        public string virsn { get; set; }
    }
}
