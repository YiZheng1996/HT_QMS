using QMSCientForm.DAL;
using QMSCientForm.Model;
using QMSCientForm.QMS;
using QMSCientForm.QMS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMSCientForm
{
    /// <summary>
    /// 项目产品筛选查询弹窗
    /// </summary>
    public partial class ProductQueryForm : Form
    {
        #region Win32 API，禁用关闭按钮
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_ENABLED = 0x00000000;
        private const uint SC_CLOSE = 0xF060;

        // 禁用关闭按钮
        private void DisableCloseButton()
        {
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }

        // 启用关闭按钮
        private void EnableCloseButton()
        {
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
            }
        }
        #endregion

        private ProductInfoDAL productDAL = new ProductInfoDAL();
        private ProjectInfoDAL projectDAL = new ProjectInfoDAL();
        private TestDataDAL testDataDAL = new TestDataDAL();

        public ProductQueryForm()
        {
            InitializeComponent();
            // 加载项目下拉框
            LoadProjects();
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
        private async void btnQuery_Click(object sender, EventArgs e)
        {
            // 记录开始时间
            var startTime = DateTime.Now;

            try
            {
                // 显示加载提示
                lblCount.Text = "正在查询，请稍候...";
                lblCount.ForeColor = Color.Blue;
                dgvProducts.DataSource = null;

                // 禁用控件，防止重复点击
                btnQuery.Enabled = false;
                btnSubmit.Enabled = false;
                panelTop.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // 获取查询条件
                string projectNo = cmbProject.SelectedItem?.ToString();
                if (projectNo == "全选") projectNo = null;

                string train = txtTrain.Text.Trim();
                string spec = txtSpec.Text.Trim();
                string mfgno = txtMfgno.Text.Trim();

                // 使用 Task.Run 异步执行查询，避免阻塞UI线程
                var products = await Task.Run(() =>
                    productDAL.GetProductsWithSyncStatus(
                        projectno: projectNo,
                        train: train,
                        spec: spec,
                        mfgno: mfgno
                    )
                );

                // 计算查询耗时
                var elapsed = DateTime.Now - startTime;

                // 绑定到DataGridView
                dgvProducts.DataSource = products;

                // 设置列的只读属性
                foreach (DataGridViewColumn column in dgvProducts.Columns)
                {
                    if (column.Name == "colSelect")
                    {
                        column.ReadOnly = false;
                    }
                    else
                    {
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

                if (dgvProducts.Columns["create_time"] != null)
                    dgvProducts.Columns["create_time"].Visible = false;

                // 更新统计信息（显示耗时）
                lblCount.Text = $"共 {products.Count} 条记录 (查询耗时: {elapsed.TotalSeconds:F2}秒)";
                lblCount.ForeColor = elapsed.TotalSeconds > 2 ? Color.Red : Color.Green;

                // 如果查询很慢，给出提示
                if (elapsed.TotalSeconds > 3)
                {
                    MessageBox.Show(
                        $"查询耗时较长({elapsed.TotalSeconds:F2}秒)！\n\n" +
                        $"可能原因：\n" +
                        $"1. 数据库索引未创建\n" +
                        $"2. 数据量过大\n" +
                        $"3. 网络延迟\n\n" +
                        $"建议：\n" +
                        $"1. 执行 PerformanceDiagnosis.sql 诊断问题\n" +
                        $"2. 执行 CreateIndexes.sql 创建索引\n" +
                        $"3. 缩小查询范围",
                        "性能提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                lblCount.Text = "查询失败";
                lblCount.ForeColor = Color.Red;
                MessageBox.Show($"查询失败：{ex.Message}\n\n详细信息：{ex.StackTrace}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复控件状态
                btnQuery.Enabled = true;
                btnSubmit.Enabled = true;
                panelTop.Enabled = true;
                this.Cursor = Cursors.Default;
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
                // TestData表记录Id列表，修改接口调用信息
                var recordIdList = new List<int>();
                var testModelDAL = new TestModelDAL();

                // 统计有测试数据的产品数量
                int productCountWithData = 0;

                // 无测试数据的产品数
                int productCountNoData = 0;

                // 跳过的数量
                int skippedCount = 0;

                foreach (var row in selectedRows)
                {
                    // 从 ProductWithSyncStatus 获取
                    if (!(row.DataBoundItem is ProductInfoDAL.ProductWithSyncStatus productWithStatus)) continue;

                    // 跳过无测试数据的产品
                    if (productWithStatus.total_count == 0)
                    {
                        productCountNoData++;
                        continue;
                    }

                    // 有测试数据的产品
                    productCountWithData++;

                    // 使用 mfgno + spec 获取测试数据
                    var testDataList = testDataDAL.GetLatestByMfgnoAndSpec(
                        productWithStatus.mfgno,
                        productWithStatus.spec);

                    foreach (var testData in testDataList)
                    {
                        // 跳过已同步的数据
                        if (testData.qms_status == "1")
                        {
                            skippedCount++;
                            continue;
                        }

                        // 根据 spec + cell_name 查询 TestModel
                        var testModel = testModelDAL.GetBySpecAndParaname(testData.spec, testData.cell_name);
                        var request = new ProdInfoRequest
                        {
                            projectNo = productWithStatus.projectno,
                            productNo = productWithStatus.mfgno,
                            paraName = testModel.paraname,
                            paraUnit = testModel.paraunit,
                            standValue = $"{testModel.standmin}-{testModel.standmax}",
                            actualValue = testData.cell_value,
                            remark = testModel.remark
                        };

                        requestList.Add(request);
                        recordIdList.Add(testData.id);
                    }
                }

                // 情况1：全部产品都没有测试数据
                if (productCountNoData == selectedRows.Count)
                {
                    MessageBox.Show(
                        $"选中的 {selectedRows.Count} 个产品都没有测试数据，无法发送。",
                        "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 情况2：有测试数据，但全部都已同步
                if (skippedCount > 0 && requestList.Count == 0)
                {
                    string message = $"选中的测试数据都已同步完成，无需重复上传。\n\n" +
                                     $"统计信息：\n" +
                                     $"  • 有测试数据的产品：{productCountWithData} 个\n" +
                                     $"  • 已同步的测试项：{skippedCount} 条";

                    if (productCountNoData > 0)
                    {
                        message += $"\n  • 无测试数据的产品：{productCountNoData} 个";
                    }

                    MessageBox.Show(message, "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 情况3：其他导致没有可发送数据的情况
                if (requestList.Count == 0)
                {
                    MessageBox.Show("没有可发送的数据", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 显示更详细的确认信息（包含跳过数量）
                string confirmMessage = $"准备发送:\n" +
                    $"  产品数量: {productCountWithData} 个\n" +
                    $"  测试项数: {requestList.Count} 条\n";

                if (skippedCount > 0)
                {
                    confirmMessage += $"  已跳过: {skippedCount} 条（已同步）\n";
                }

                confirmMessage += $"  预计耗时: 约 {requestList.Count * 0.2:F1} 秒\n\n确定继续？";

                if (MessageBox.Show(confirmMessage, "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 禁用按钮和面板（实现模态效果）
                btnSubmit.Enabled = false;
                btnQuery.Enabled = false;
                panelTop.Enabled = false;
                dgvProducts.Enabled = false;
                DisableCloseButton(); // 禁用关闭按钮
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // 创建进度窗口
                    var progressForm = new UploadProgressForm(requestList.Count);
                    progressForm.Owner = this; // 设置父窗体
                    progressForm.Show();

                    // 创建进度报告器
                    var progress = new Progress<UploadProgressReport>(report =>
                    {
                        // 更新数据库状态
                        int testId = recordIdList[report.Current - 1];
                        if (report.Response.IsSuccess)
                        {
                            testDataDAL.UpdateQmsStatus(testId, "1",
                                DateTime.Now, report.Response.resultMsg);
                        }
                        else
                        {
                            testDataDAL.UpdateQmsStatus(testId, "2",
                                DateTime.Now, report.Response.resultMsg);
                        }

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

                    // 新增: 上传完成后自动刷新查询（更新同步状态）
                    if (result.SuccessCount > 0 || result.FailCount > 0)
                    {
                        // 等待进度窗口关闭后再刷新
                        await System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                         {
                             if (this.InvokeRequired)
                             {
                                 this.Invoke(new Action(() => btnQuery_Click(null, null)));
                             }
                             else
                             {
                                 btnQuery_Click(null, null);
                             }
                         });
                    }
                }
                finally
                {
                    // 恢复控件状态
                    btnSubmit.Enabled = true;
                    btnQuery.Enabled = true;
                    panelTop.Enabled = true;
                    dgvProducts.Enabled = true;
                    EnableCloseButton(); // 启用关闭按钮
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

                // 从 ProductWithSyncStatus 获取数据
                if (!(dgvProducts.Rows[e.RowIndex].DataBoundItem is ProductInfoDAL.ProductWithSyncStatus productWithStatus)) return;

                // 检查是否有测试数据
                if (productWithStatus.total_count == 0)
                {
                    MessageBox.Show(
                        $"产品型号: {productWithStatus.spec}\n" +
                        $"制造编号: {productWithStatus.mfgno}\n\n" +
                        $"暂无测试数据。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // 创建临时 ProductInfoModel 对象传递给详情窗口
                var product = new ProductInfoModel
                {
                    id = productWithStatus.id,
                    projectno = productWithStatus.projectno,
                    projectname = productWithStatus.projectname,
                    train = productWithStatus.train,
                    spec = productWithStatus.spec,
                    mfgno = productWithStatus.mfgno,
                    create_time = productWithStatus.create_time
                };

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