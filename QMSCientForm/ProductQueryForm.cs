using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMSCientForm.DAL;
using QMSCientForm.Model;
using QMSCientForm.QMS;

namespace QMSCientForm
{
    /// <summary>
    /// 项目产品筛选查询弹窗
    /// </summary>
    public partial class ProductQueryForm : Form
    {
        private ProductInfoDAL productDAL = new ProductInfoDAL();
        private ProjectInfoDAL projectDAL = new ProjectInfoDAL();
        private TestDataDAL testDataDAL = new TestDataDAL();

        public ProductQueryForm()
        {
            InitializeComponent();
            LoadProjects();
        }

        /// <summary>
        /// 加载项目下拉框
        /// </summary>
        private void LoadProjects()
        {
            try
            {
                var projects = projectDAL.GetAll();
                
                cmbProject.Items.Clear();
                cmbProject.Items.Add("全选");
                
                foreach (var project in projects)
                {
                    cmbProject.Items.Add(project.projectno);
                }
                
                if (cmbProject.Items.Count > 0)
                    cmbProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载项目失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取查询条件
                string projectNo = cmbProject.SelectedItem?.ToString();
                if (projectNo == "全选") projectNo = null;
                
                string train = txtTrain.Text.Trim();
                string spec = txtSpec.Text.Trim();
                string mfgno = txtMfgno.Text.Trim();

                // 查询产品信息
                var products = productDAL.GetByConditions(
                    projectno: projectNo,
                    train: train,
                    spec: spec,
                    mfgno: mfgno
                );

                // 绑定到DataGridView
                dgvProducts.DataSource = null;
                dgvProducts.DataSource = products;

                // 设置列标题
                if (dgvProducts.Columns["id"] != null)
                    dgvProducts.Columns["id"].Visible = false;

                if (dgvProducts.Columns["projectno"] != null)
                    dgvProducts.Columns["projectno"].HeaderText = "项目编号";

                if (dgvProducts.Columns["train"] != null)
                    dgvProducts.Columns["train"].HeaderText = "列";
                
                if (dgvProducts.Columns["spec"] != null)
                    dgvProducts.Columns["spec"].HeaderText = "型号";
                
                if (dgvProducts.Columns["mfgno"] != null)
                    dgvProducts.Columns["mfgno"].HeaderText = "制造编号";

                if (dgvProducts.Columns["projectname"] != null)
                    dgvProducts.Columns["projectname"].HeaderText = "项目名称";
                
                // 隐藏不需要的列
                if (dgvProducts.Columns["create_time"] != null)
                    dgvProducts.Columns["create_time"].Visible = false;
                
                if (dgvProducts.Columns["qms_status"] != null)
                    dgvProducts.Columns["qms_status"].Visible = false;
                
                if (dgvProducts.Columns["qms_time"] != null)
                    dgvProducts.Columns["qms_time"].Visible = false;
                
                if (dgvProducts.Columns["qms_rem"] != null)
                    dgvProducts.Columns["qms_rem"].Visible = false;
                
                if (dgvProducts.Columns["prdt_code"] != null)
                    dgvProducts.Columns["prdt_code"].Visible = false;
                
                if (dgvProducts.Columns["productname"] != null)
                    dgvProducts.Columns["productname"].Visible = false;
                
                if (dgvProducts.Columns["virsn"] != null)
                    dgvProducts.Columns["virsn"].Visible = false;

                lblCount.Text = $"共 {products.Count} 条记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 提交按钮点击事件 - 发送到QMS
        /// </summary>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中的行
                var selectedRows = dgvProducts.Rows.Cast<DataGridViewRow>()
                    .Where(row => row.Cells[0].Value != null && (bool)row.Cells[0].Value)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("请至少选择一条记录", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show($"确定要发送选中的 {selectedRows.Count} 条产品数据到QMS吗？", 
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var row in selectedRows)
                {
                    var product = row.DataBoundItem as ProductInfoModel;
                    if (product == null) continue;

                    // 获取该产品的测试数据
                    var testDataList = testDataDAL.GetByMfgno(product.mfgno);

                    foreach (var testData in testDataList)
                    {
                        // 发送到QMS
                        var response = QmsApiClient.SendProdInfo(
                            projectNo: product.projectno,
                            productNo: product.mfgno,
                            paraName: testData.cell_name,
                            paraUnit: "kPa", // 从TestModel获取
                            standValue: "0-10", // 从TestModel获取
                            actualValue: testData.cell_value,
                            remark: ""
                        );

                        if (response.IsSuccess)
                        {
                            // 更新QMS状态
                            testDataDAL.UpdateQmsStatus(testData.id, "1", 
                                DateTime.Now, response.resultMsg);
                            successCount++;
                        }
                        else
                        {
                            testDataDAL.UpdateQmsStatus(testData.id, "2", 
                                DateTime.Now, response.resultMsg);
                            failCount++;
                        }
                    }
                }

                MessageBox.Show($"发送完成！\n成功：{successCount} 条\n失败：{failCount} 条", 
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 刷新数据
                btnQuery_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// DataGridView双击事件 - 打开测试详情窗口
        /// </summary>
        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                var product = dgvProducts.Rows[e.RowIndex].DataBoundItem as ProductInfoModel;
                if (product == null) return;

                // 打开试验项点明细查询弹窗
                TestDetailQueryForm detailForm = new TestDetailQueryForm(product);
                detailForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开详情失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
