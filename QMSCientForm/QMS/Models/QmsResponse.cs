using System;

namespace QMSCientForm.QMS.Models
{
    /// <summary>
    /// QMS接口响应模型
    /// </summary>
    public class QmsResponse
    {
        /// <summary>
        /// 返回码（200=成功，500=失败）
        /// </summary>
        public string resultCode { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string resultMsg { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get { return resultCode == "200"; }
        }
    }
}