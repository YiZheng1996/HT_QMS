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

            // 添加表头全选 checkbox
            AddHeaderCheckBox();
        }

        /// <summary>
        /// 添加表头全选复选框
        /// </summary>
        private void AddHeaderCheckBox()
        {
            // 创建 checkbox 控件
            CheckBox headerCheckBox = new CheckBox();
            headerCheckBox.Size = new Size(15, 15);
            headerCheckBox.BackColor = Color.Transparent;

            // 添加点击事件
            headerCheckBox.CheckedChanged += (sender, e) =>
            {
                if (dgvProducts.DataSource == null) return;

                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    row.Cells["colSelect"].Value = headerCheckBox.Checked;
                }
                dgvProducts.Refresh();
            };

            // 将 checkbox 添加到表头
            dgvProducts.Controls.Add(headerCheckBox);

            // 自定义绘制表头
            dgvProducts.Paint += (sender, e) =>
            {
                // 获取第一列（checkbox列）的表头位置
                Rectangle rect = dgvProducts.GetCellDisplayRectangle(0, -1, true);

                // 计算checkbox位置（靠左）
                Point checkBoxLocation = new Point(
                    rect.Location.X + 45,  // 左边距5像素
                    rect.Location.Y + (rect.Height - headerCheckBox.Height) / 2 + 1
                );

                headerCheckBox.Location = checkBoxLocation;
                headerCheckBox.BringToFront(); // 确保checkbox在最前面
            };

            // 自定义绘制表头内容（绘制"全选"文字）
            dgvProducts.CellPainting += (sender, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex == 0)
                {
                    e.PaintBackground(e.CellBounds, true);

                    // 在checkbox右侧绘制"全选"文字
                    TextRenderer.DrawText(
                        e.Graphics,
                        "全选",
                        dgvProducts.Font,
                        new Point(e.CellBounds.X + 15, e.CellBounds.Y + 5),  // checkbox宽度约20，所以从25开始
                        dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor
                    );

                    e.Handled = true;
                }
            };

            // 初始化checkbox位置
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvProducts.ColumnHeadersHeight = 23; // 设置表头高度
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

                // 数据绑定后,设置列的只读属性
                foreach (DataGridViewColumn column in dgvProducts.Columns)
                {
                    // checkbox 列保持可编辑
                    if (column.Name == "colSelect")
                    {
                        column.ReadOnly = false;
                    }
                    else
                    {
                        // 其他列设为只读
                        column.ReadOnly = true;
                    }
                }

                // 设置列标题
                if (dgvProducts.Columns["id"] != null)
                    dgvProducts.Columns["id"].Visible = false;

                if (dgvProducts.Columns["projectno"] != null)
                    dgvProducts.Columns["projectno"].HeaderText = "项目编号";

                if (dgvProducts.Columns["projectname"] != null)
                {
                    dgvProducts.Columns["projectname"].HeaderText = "项目名称";
                    dgvProducts.Columns["projectname"].Width = 200;
                }

                if (dgvProducts.Columns["train"] != null)
                    dgvProducts.Columns["train"].HeaderText = "列";

                if (dgvProducts.Columns["spec"] != null)
                    dgvProducts.Columns["spec"].HeaderText = "型号";

                if (dgvProducts.Columns["mfgno"] != null)
                    dgvProducts.Columns["mfgno"].HeaderText = "制造编号";

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
        private async void btnSubmit_Click(object sender, EventArgs e)
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

                    // 使用 mfgno + spec 获取测试数据
                    var testDataList = testDataDAL.GetLatestByMfgnoAndSpec(product.mfgno, product.spec);

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

                // 禁用按钮,防止重复点击
                btnSubmit.Enabled = false;
                btnQuery.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // 创建进度窗口
                    var progressForm = new UploadProgressForm(requestList.Count);
                    progressForm.Show(this);

                    // 创建进度报告器
                    var progress = new Progress<UploadProgressReport>(report =>
                    {
                        string status = report.Response.IsSuccess ?
                            "成功" : string.Format("失败: {0}", report.Response.resultMsg);
                        progressForm.UpdateProgress(report.Current, report.Total,
                            status, report.Response.IsSuccess);
                    });

                    // 批量发送
                    var result = await QmsApiClient.SendProdInfoBatchAsync(
                        requestList.ToArray(),
                        delayMilliseconds: 200,
                        progress: progress,
                        cancellationToken: progressForm.CancellationToken
                    );

                    // 设置完成状态
                    if (progressForm.IsCancelled)
                    {
                        progressForm.SetCancelled(result.TotalCount, requestList.Count);
                    }
                    else
                    {
                        progressForm.SetCompleted(result.SuccessCount, result.FailCount);
                    }
                }
                finally
                {
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

                // 使用 mfgno + spec 检查是否有测试数据
                var testDataList = testDataDAL.GetLatestByMfgnoAndSpec(product.mfgno, product.spec);
                if (testDataList == null || testDataList.Count == 0)
                {
                    MessageBox.Show(this, $"产品型号 {product.spec}\n\n制造编号 {product.mfgno}\n\n暂无测试数据。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

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