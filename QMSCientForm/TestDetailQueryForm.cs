using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMSCientForm.DAL;
using QMSCientForm.Model;

namespace QMSCientForm
{
    /// <summary>
    /// 试验项点明细查询弹窗
    /// </summary>
    public partial class TestDetailQueryForm : Form
    {
        private ProductInfoModel product;
        private TestDataJoinDAL joinDAL = new TestDataJoinDAL();

        public TestDetailQueryForm(ProductInfoModel product)
        {
            InitializeComponent();
            this.product = product;

            // 设置窗体标题为产品信息
            this.Text = $"{product.spec}-{product.mfgno}";

            LoadTestDetails();
        }

        /// <summary>
        /// 加载测试详情
        /// </summary>
        private void LoadTestDetails()
        {
            try
            {
                // 使用 mfgno + spec 获取产品测试详情
                var details = joinDAL.GetProductTestDetail(product.mfgno, product.spec);

                // 创建用于显示的列表
                var displayList = details.Select((d, index) => new
                {
                    序号 = index + 1,
                    参数名称 = d.paraname,
                    描述 = d.test_remark,
                    单元格 = d.cell_name,
                    标准值 = d.standard_range,
                    实测值 = d.cell_value,
                    单位 = d.paraunit,
                    判定 = d.test_result,
                    同步状态 = GetQmsStatusText(d.qms_status),
                    同步时间 = d.qms_time,
                    同步信息 = d.qms_response
                }).ToList();

                // 绑定到DataGridView
                dgvDetails.DataSource = displayList;

                // 根据判定结果设置行颜色
                foreach (DataGridViewRow row in dgvDetails.Rows)
                {
                    var result = row.Cells["判定"].Value?.ToString();
                    if (result == "不合格")
                    {
                        //row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                        row.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (result == "合格")
                    {
                        //row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                        row.DefaultCellStyle.BackColor = Color.Green;
                    }
                }

                lblCount.Text = $"共 {details.Count} 项测试";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载测试详情失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region 辅助方法
        /// <summary>
        /// 获取QMS状态文本
        /// </summary>
        private string GetQmsStatusText(string status)
        {
            switch (status)
            {
                case "0": return "未同步";
                case "1": return "已同步";
                case "2": return "同步失败";
                default: return "未同步";
            }
        }
        #endregion
    }
}