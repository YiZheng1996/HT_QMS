using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMSCientForm
{
    /// <summary>
    /// 主界面
    /// </summary>
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.IsMdiContainer = true; // 设置为MDI容器


        }

        /// <summary>
        /// 试验信息发送按钮
        /// </summary>
        private void btnTestInfo_Click(object sender, EventArgs e)
        {
            try
            {
                ProductQueryForm form = new ProductQueryForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开窗口失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设备信息发送按钮
        /// </summary>
        private void btnDeviceInfo_Click(object sender, EventArgs e)
        {
            try
            {
                DeviceQueryForm form = new DeviceQueryForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开窗口失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
