using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace QMSCientForm
{
    /// <summary>
    /// 上传进度窗口
    /// </summary>
    public partial class UploadProgressForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;

        public CancellationToken CancellationToken
        {
            get { return cancellationTokenSource.Token; }
        }

        public bool IsCancelled
        {
            get { return cancellationTokenSource.IsCancellationRequested; }
        }

        public UploadProgressForm(int totalCount)
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();

            // 设置进度条最大值
            progressBar.Maximum = totalCount;
            lblProgress.Text = "0 / " + totalCount;
        }

        /// <summary>
        /// 更新进度
        /// </summary>
        public void UpdateProgress(int current, int total, string status, bool isSuccess)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, int, string, bool>(UpdateProgress),
                    current, total, status, isSuccess);
                return;
            }

            progressBar.Value = current;
            lblProgress.Text = string.Format("{0} / {1}", current, total);

            // 根据成功/失败显示不同颜色
            lblStatus.ForeColor = isSuccess ? Color.Green : Color.Red;
            lblStatus.Text = string.Format("[{0}/{1}] {2}", current, total, status);
        }

        /// <summary>
        /// 设置完成状态
        /// </summary>
        public void SetCompleted(int successCount, int failCount)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, int>(SetCompleted), successCount, failCount);
                return;
            }

            btnCancel.Text = "关闭";
            btnCancel.BackColor = Color.FromArgb(52, 152, 219);

            string message = string.Format("上传完成！成功 {0} 条，失败 {1} 条",
                successCount, failCount);
            lblStatus.Text = message;
            lblStatus.ForeColor = failCount == 0 ? Color.Green : Color.Orange;
        }

        /// <summary>
        /// 设置取消状态
        /// </summary>
        public void SetCancelled(int processedCount, int totalCount)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, int>(SetCancelled), processedCount, totalCount);
                return;
            }

            btnCancel.Text = "关闭";
            btnCancel.BackColor = Color.FromArgb(52, 152, 219);

            lblStatus.Text = string.Format("上传已取消！已处理 {0}/{1} 条",
                processedCount, totalCount);
            lblStatus.ForeColor = Color.Orange;
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (btnCancel.Text == "关闭")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            if (MessageBox.Show("确定要取消上传吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnCancel.Enabled = false;
                btnCancel.Text = "正在取消...";
                lblStatus.Text = "正在取消上传，请稍候...";
                lblStatus.ForeColor = Color.Orange;

                cancellationTokenSource.Cancel();
                this.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}