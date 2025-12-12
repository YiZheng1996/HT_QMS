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
                // 获取产品测试详情
                var details = joinDAL.GetProductTestDetail(product.mfgno);

                // 创建用于显示的列表
                var displayList = details.Select((d, index) => new
                {
                    序号 = index + 1,
                    参数名称 = d.paraname,
                    描述 = d.cell_name,
                    标准值 = d.standard_range,
                    实测值 = d.cell_value,
                    单位 = d.paraunit,
                    判定 = d.test_result
                }).ToList();

                // 绑定到DataGridView
                dgvDetails.DataSource = displayList;

                // 设置列宽
                if (dgvDetails.Columns["序号"] != null)
                    dgvDetails.Columns["序号"].Width = 60;
                
                if (dgvDetails.Columns["参数名称"] != null)
                    dgvDetails.Columns["参数名称"].Width = 150;
                
                if (dgvDetails.Columns["描述"] != null)
                    dgvDetails.Columns["描述"].Width = 200;
                
                if (dgvDetails.Columns["标准值"] != null)
                    dgvDetails.Columns["标准值"].Width = 100;
                
                if (dgvDetails.Columns["实测值"] != null)
                    dgvDetails.Columns["实测值"].Width = 100;
                
                if (dgvDetails.Columns["单位"] != null)
                    dgvDetails.Columns["单位"].Width = 80;
                
                if (dgvDetails.Columns["判定"] != null)
                    dgvDetails.Columns["判定"].Width = 80;

                // 根据判定结果设置行颜色
                foreach (DataGridViewRow row in dgvDetails.Rows)
                {
                    var result = row.Cells["判定"].Value?.ToString();
                    if (result == "不合格")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    }
                    else if (result == "合格")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
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
    }
}
