using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMSCientForm.DAL;
using QMSCientForm.Model;
using QMSCientForm.QMS;
using QMSCientForm.QMS.Models;

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
        /// 提交按钮点击事件 - 批量发送到QMS
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

                // 准备批量发送的请求列表
                var requestList = new List<ProdInfoRequest>();

                foreach (var row in selectedRows)
                {
                    var product = row.DataBoundItem as ProductInfoModel;
                    if (product == null) continue;

                    // 获取该产品的测试数据
                    var testDataList = testDataDAL.GetByMfgno(product.mfgno);

                    foreach (var testData in testDataList)
                    {
                        var request = new ProdInfoRequest
                        {
                            projectNo = product.projectno,
                            productNo = product.mfgno,
                            paraName = testData.cell_name,
                            paraUnit = "kPa",
                            standValue = "28-38",
                            actualValue = testData.cell_value,
                            remark = null
                        };

                        requestList.Add(request);
                    }
                }

                if (requestList.Count == 0)
                {
                    MessageBox.Show("没有可发送的数据", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    $"准备发送 {requestList.Count} 条数据到QMS\n" +
                    $"预计耗时约 {requestList.Count * 0.2} 秒\n\n确定继续？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 禁用按钮，防止重复点击
                btnSubmit.Enabled = false;
                btnQuery.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // 创建进度窗口（可选）
                    var progressForm = new Form
                    {
                        Text = "正在发送数据",
                        Width = 400,
                        Height = 120,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        StartPosition = FormStartPosition.CenterParent,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    var lblProgress = new Label
                    {
                        Text = "准备发送...",
                        AutoSize = false,
                        Width = 360,
                        Height = 40,
                        Left = 20,
                        Top = 20,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    progressForm.Controls.Add(lblProgress);

                    // 异步显示进度窗口
                    progressForm.Show(this);
                    Application.DoEvents();

                    // 批量发送（带进度回调）
                    var result = QmsApiClient.SendProdInfoBatch(
                        requestList.ToArray(),
                        delayMilliseconds: 200, // 每次请求间隔200ms
                        progressCallback: (current, total, response) =>
                        {
                    // 更新进度
                    lblProgress.Text = $"正在发送：{current}/{total}\n" +
                                              $"当前结果：{(response.IsSuccess ? "成功" : response.resultMsg)}";
                            Application.DoEvents(); // 刷新界面
                }
                    );

                    // 关闭进度窗口
                    progressForm.Close();

                    // 显示结果
                    string message = $"发送完成！\n\n" +
                                   $"总数量：{result.TotalCount}\n" +
                                   $"成功：{result.SuccessCount}\n" +
                                   $"失败：{result.FailCount}";

                    MessageBox.Show(message, "发送结果",
                        MessageBoxButtons.OK,
                        result.IsAllSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
                finally
                {
                    // 恢复按钮状态
                    btnSubmit.Enabled = true;
                    btnQuery.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量发送失败：{ex.Message}", "错误",
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
