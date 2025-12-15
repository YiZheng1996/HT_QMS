namespace QMSCientForm.QMS.Models
{
    /// <summary>
    /// 上传进度报告类（替代元组，兼容 .NET 4.5.2）
    /// </summary>
    public class UploadProgressReport
    {
        /// <summary>
        /// 当前进度
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 响应结果
        /// </summary>
        public QmsResponse Response { get; set; }

        public UploadProgressReport(int current, int total, QmsResponse response)
        {
            Current = current;
            Total = total;
            Response = response;
        }
    }
}