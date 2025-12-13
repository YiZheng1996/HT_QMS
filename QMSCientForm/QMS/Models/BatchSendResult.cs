namespace QMSCientForm.QMS.Models
{
    /// <summary>
    /// 批量发送结果
    /// </summary>
    public class BatchSendResult
    {
        /// <summary>
        /// 成功数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败数量
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount
        {
            get { return SuccessCount + FailCount; }
        }

        /// <summary>
        /// 是否全部成功
        /// </summary>
        public bool IsAllSuccess
        {
            get { return FailCount == 0; }
        }

        public BatchSendResult()
        {
            SuccessCount = 0;
            FailCount = 0;
        }
    }
}