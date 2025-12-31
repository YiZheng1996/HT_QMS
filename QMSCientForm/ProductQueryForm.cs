using QMSCientForm.DAL;
using QMSCientForm.Model;
using QMSCientForm.QMS;
using QMSCientForm.QMS.Models;
using QMSCientForm.Utils;  // 引入日志工具类
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
    /// 项目产品筛选查询窗体
    /// 功能：
    /// 1. 查询产品信息（支持项目、车列、型号、制造编号筛选）
    /// 2. 显示产品同步状态统计
    /// 3. 批量提交测试数据到QMS系统
    /// 4. 双击查看产品测试详情
    /// </summary>
    public partial class ProductQueryForm : Form
    {
        #region Win32 API - 控制窗体关闭按钮

        /// <summary>
        /// 获取系统菜单句柄
        /// </summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>
        /// 启用或禁用菜单项
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        /// <summary>
        /// 按命令方式操作菜单
        /// </summary>
        private const uint MF_BYCOMMAND = 0x00000000;

        /// <summary>
        /// 菜单项灰色不可用
        /// </summary>
        private const uint MF_GRAYED = 0x00000001;

        /// <summary>
        /// 菜单项可用
        /// </summary>
        private const uint MF_ENABLED = 0x00000000;

        /// <summary>
        /// 关闭按钮的系统命令ID
        /// </summary>
        private const uint SC_CLOSE = 0xF060;

        /// <summary>
        /// 禁用窗体关闭按钮
        /// 用于防止用户在上传过程中意外关闭窗口
        /// </summary>
        private void DisableCloseButton()
        {
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                Logger.Debug("窗体关闭按钮已禁用");
            }
        }

        /// <summary>
        /// 启用窗体关闭按钮
        /// 在上传完成或取消后恢复关闭功能
        /// </summary>
        private void EnableCloseButton()
        {
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
                Logger.Debug("窗体关闭按钮已启用");
            }
        }

        #endregion

        #region 私有字段

        /// <summary>
        /// 产品信息数据访问层对象
        /// </summary>
        private ProductInfoDAL productDAL = new ProductInfoDAL();

        /// <summary>
        /// 项目信息数据访问层对象
        /// </summary>
        private ProjectInfoDAL projectDAL = new ProjectInfoDAL();

        /// <summary>
        /// 测试数据访问层对象
        /// </summary>
        private TestDataDAL testDataDAL = new TestDataDAL();

        #endregion

        #region 构造函数和初始化

        /// <summary>
        /// 构造函数
        /// 初始化窗体组件、加载项目列表、添加表头复选框
        /// </summary>
        public ProductQueryForm()
        {
            InitializeComponent();
            Logger.Info("=== ProductQueryForm 初始化开始 ===");

            // 加载项目下拉列表
            LoadProjects();

            // 添加DataGridView表头的全选复选框
            AddHeaderCheckBox();

            Logger.Info("ProductQueryForm 初始化完成");
        }

        /// <summary>
        /// 添加DataGridView表头的全选复选框
        /// 实现功能：
        /// 1. 在第一列表头添加复选框控件
        /// 2. 点击复选框时切换所有行的选中状态
        /// 3. 自定义绘制表头显示"全选"文字
        /// </summary>
        private void AddHeaderCheckBox()
        {
            Logger.Debug("开始添加表头全选复选框");

            // 创建复选框控件
            CheckBox headerCheckBox = new CheckBox();
            headerCheckBox.Size = new Size(15, 15);
            headerCheckBox.BackColor = Color.Transparent;

            // 复选框状态改变事件 - 切换所有行的选中状态
            headerCheckBox.CheckedChanged += (sender, e) =>
            {
                // 防护：检查数据源是否为空
                if (dgvProducts.DataSource == null)
                {
                    Logger.Warning("数据源为空，无法切换全选状态");
                    return;
                }

                // 遍历所有行，设置复选框列的值
                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    row.Cells["colSelect"].Value = headerCheckBox.Checked;
                }
                dgvProducts.Refresh();

                Logger.DebugFormat("全选状态切换为: {0}", headerCheckBox.Checked);
            };

            // 将复选框添加到DataGridView控件集合
            dgvProducts.Controls.Add(headerCheckBox);

            // 监听Paint事件，动态调整复选框位置
            dgvProducts.Paint += (sender, e) =>
            {
                // 获取第一列（复选框列）表头的显示区域
                Rectangle rect = dgvProducts.GetCellDisplayRectangle(0, -1, true);

                // 计算复选框位置（水平居中，垂直居中）
                Point checkBoxLocation = new Point(
                    rect.Location.X + 45,  // 左边距45像素（为"全选"文字留空间）
                    rect.Location.Y + (rect.Height - headerCheckBox.Height) / 2 + 1  // 垂直居中
                );

                headerCheckBox.Location = checkBoxLocation;
                headerCheckBox.BringToFront();  // 确保复选框在最前面显示
            };

            // 监听CellPainting事件，自定义绘制表头内容
            dgvProducts.CellPainting += (sender, e) =>
            {
                // 只处理第一列的表头单元格（RowIndex = -1 表示表头）
                if (e.RowIndex == -1 && e.ColumnIndex == 0)
                {
                    // 绘制背景
                    e.PaintBackground(e.CellBounds, true);

                    // 在复选框左侧绘制"全选"文字
                    TextRenderer.DrawText(
                        e.Graphics,
                        "全选",
                        dgvProducts.Font,
                        new Point(e.CellBounds.X + 15, e.CellBounds.Y + 5),
                        dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor
                    );

                    e.Handled = true;  // 标记为已处理，不再执行默认绘制
                }
            };

            // 设置表头高度模式为可调整
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvProducts.ColumnHeadersHeight = 23;  // 设置表头高度为23像素

            Logger.Debug("表头全选复选框添加完成");
        }

        /// <summary>
        /// 加载项目下拉列表
        /// 从数据库读取所有项目，添加到ComboBox中
        /// 第一项为"全选"，用于查询所有项目
        /// </summary>
        private void LoadProjects()
        {
            try
            {
                Logger.Info("开始加载项目列表");

                // 从数据库获取所有项目
                var projects = projectDAL.GetAll();
                Logger.InfoFormat("从数据库获取到 {0} 个项目", projects.Count);

                // 清空现有项
                cmbProject.Items.Clear();

                // 添加"全选"选项
                cmbProject.Items.Add("全选");

                // 添加所有项目编号
                foreach (var project in projects)
                {
                    cmbProject.Items.Add(project.projectno);
                }

                // 默认选中第一项（全选）
                if (cmbProject.Items.Count > 0)
                {
                    cmbProject.SelectedIndex = 0;
                    Logger.Debug("默认选中第一项");
                }

                Logger.Info("项目列表加载完成");
            }
            catch (Exception ex)
            {
                Logger.Error("加载项目列表失败", ex);
                MessageBox.Show($"加载项目失败:{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 查询按钮事件

        /// <summary>
        /// 查询按钮点击事件
        /// 功能：
        /// 1. 根据用户输入的条件查询产品信息
        /// 2. 显示产品的同步状态统计
        /// 3. 统计查询耗时并给出性能提示
        /// </summary>
        private async void btnQuery_Click(object sender, EventArgs e)
        {
            // 记录查询开始时间（用于统计耗时）
            var startTime = DateTime.Now;

            try
            {
                Logger.Info("=== 开始查询产品信息 ===");

                // ===== 步骤1: 初始化UI状态 =====
                lblCount.Text = "正在查询,请稍候...";
                lblCount.ForeColor = Color.Blue;
                dgvProducts.DataSource = null;  // 清空现有数据

                // 禁用控件，防止重复点击
                btnQuery.Enabled = false;
                btnSubmit.Enabled = false;
                panelTop.Enabled = false;
                this.Cursor = Cursors.WaitCursor;  // 显示等待光标

                // ===== 步骤2: 获取查询条件 =====
                string projectNo = cmbProject.SelectedItem?.ToString();
                if (projectNo == "全选")
                {
                    projectNo = null;  // 全选时不限制项目
                    Logger.Debug("查询条件: 所有项目");
                }
                else
                {
                    Logger.DebugFormat("查询条件: 项目编号 = {0}", projectNo);
                }

                string train = txtTrain.Text.Trim();
                string spec = txtSpec.Text.Trim();
                string mfgno = txtMfgno.Text.Trim();

                if (!string.IsNullOrEmpty(train))
                    Logger.DebugFormat("查询条件: 车列 = {0}", train);
                if (!string.IsNullOrEmpty(spec))
                    Logger.DebugFormat("查询条件: 型号 = {0}", spec);
                if (!string.IsNullOrEmpty(mfgno))
                    Logger.DebugFormat("查询条件: 制造编号 = {0}", mfgno);

                // ===== 步骤3: 异步执行数据库查询 =====
                // 使用 Task.Run 在后台线程执行查询，避免阻塞UI线程
                var products = await Task.Run(() =>
                    productDAL.GetProductsWithSyncStatus(
                        projectno: projectNo,
                        train: train,
                        spec: spec,
                        mfgno: mfgno
                    )
                );

                // ===== 步骤4: 计算查询耗时 =====
                var elapsed = DateTime.Now - startTime;
                Logger.InfoFormat("查询完成，耗时: {0:F2}秒，结果数量: {1}",
                    elapsed.TotalSeconds, products.Count);

                // ===== 步骤5: 绑定数据到DataGridView =====
                dgvProducts.DataSource = products;

                // 设置列的只读属性（只有复选框列可编辑）
                foreach (DataGridViewColumn column in dgvProducts.Columns)
                {
                    if (column.Name == "colSelect")
                    {
                        column.ReadOnly = false;  // 复选框列可编辑
                    }
                    else
                    {
                        column.ReadOnly = true;  // 其他列只读
                    }
                }

                // ===== 步骤6: 设置列显示属性 =====
                // 隐藏ID列
                if (dgvProducts.Columns["id"] != null)
                    dgvProducts.Columns["id"].Visible = false;

                // 设置列标题和宽度
                if (dgvProducts.Columns["projectno"] != null)
                    dgvProducts.Columns["projectno"].HeaderText = "项目编号";

                if (dgvProducts.Columns["projectname"] != null)
                {
                    dgvProducts.Columns["projectname"].HeaderText = "项目名称";
                    dgvProducts.Columns["projectname"].Width = 400;
                }

                if (dgvProducts.Columns["train"] != null)
                    dgvProducts.Columns["train"].HeaderText = "列";

                if (dgvProducts.Columns["spec"] != null)
                    dgvProducts.Columns["spec"].HeaderText = "型号";

                if (dgvProducts.Columns["mfgno"] != null)
                    dgvProducts.Columns["mfgno"].HeaderText = "制造编号";

                if (dgvProducts.Columns["create_time"] != null)
                    dgvProducts.Columns["create_time"].Visible = false;

                if (dgvProducts.Columns["total_count"] != null)
                    dgvProducts.Columns["total_count"].HeaderText = "测试项总数";

                if (dgvProducts.Columns["synced_count"] != null)
                    dgvProducts.Columns["synced_count"].HeaderText = "已同步";

                if (dgvProducts.Columns["failed_count"] != null)
                    dgvProducts.Columns["failed_count"].HeaderText = "同步失败";

                if (dgvProducts.Columns["sync_status"] != null)
                    dgvProducts.Columns["sync_status"].HeaderText = "同步状态";

                // ===== 步骤7: 更新统计信息 =====
                lblCount.Text = $"共 {products.Count} 条记录 (查询耗时: {elapsed.TotalSeconds:F2}秒)";
                lblCount.ForeColor = elapsed.TotalSeconds > 2 ? Color.Red : Color.Green;

                // ===== 步骤8: 性能提示 =====
                // 如果查询耗时超过3秒，提示用户优化
                if (elapsed.TotalSeconds > 3)
                {
                    Logger.Warning($"查询性能较差，耗时: {elapsed.TotalSeconds:F2}秒");
                    MessageBox.Show(
                        $"查询耗时较长({elapsed.TotalSeconds:F2}秒)!\n\n" +
                        $"可能原因:\n" +
                        $"1. 数据库索引未创建\n" +
                        $"2. 数据量过大\n" +
                        $"3. 网络延迟\n\n" +
                        $"建议:\n" +
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
                Logger.Error("查询产品信息失败", ex);
                lblCount.Text = "查询失败";
                lblCount.ForeColor = Color.Red;
                MessageBox.Show($"查询失败:{ex.Message}\n\n详细信息:{ex.StackTrace}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复控件状态
                btnQuery.Enabled = true;
                btnSubmit.Enabled = true;
                panelTop.Enabled = true;
                this.Cursor = Cursors.Default;

                Logger.Info("=== 查询操作结束 ===");
            }
        }

        #endregion

        #region 提交按钮事件

        /// <summary>
        /// 提交按钮点击事件 - 批量发送测试数据到QMS系统
        /// 
        /// 实现的防护措施（共13层）：
        /// 防护1: 验证选中行不为空
        /// 防护2: 验证DAL对象初始化
        /// 防护3: 验证DataBoundItem不为null
        /// 防护4: 验证产品关键字段（mfgno, spec, projectno）
        /// 防护5: 验证测试数据列表不为空
        /// 防护6: 验证testData对象不为null
        /// 防护7: 验证testData关键字段（spec, cell_name）
        /// 防护8: 查询TestModel配置
        /// 防护9: 验证testModel对象不为null
        /// 防护10: 验证testModel关键字段（paraname, paraunit, standmin, standmax）
        /// 防护11: 使用??运算符防止cell_value和remark为null
        /// 防护12: Progress回调异常处理（验证report、索引范围、Response）
        /// 防护13: 异步刷新前检查窗体状态
        /// 
        /// 错误处理策略：
        /// - 无效数据：跳过并计数，在确认对话框中显示统计信息
        /// - 已同步数据：跳过并计数
        /// - 缺少配置：跳过并计数
        /// - 异常：记录日志并继续处理下一条
        /// </summary>
        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("=== 开始批量提交测试数据 ===");

                // ===== 防护1: 验证选中行 =====
                Logger.Debug("防护1: 验证选中行");
                var selectedRows = dgvProducts.Rows.Cast<DataGridViewRow>()
                    .Where(row => row.Cells[0].Value != null && (bool)row.Cells[0].Value)
                    .ToList();

                if (selectedRows == null || selectedRows.Count == 0)
                {
                    Logger.Warning("用户未选择任何记录");
                    MessageBox.Show("请至少选择一条记录", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Logger.InfoFormat("用户选择了 {0} 条记录", selectedRows.Count);

                // ===== 防护2: 验证 DAL 初始化 =====
                Logger.Debug("防护2: 验证DAL对象初始化");
                if (testDataDAL == null || productDAL == null)
                {
                    Logger.Fatal("DAL对象未初始化");
                    MessageBox.Show("系统初始化异常,请重新打开窗口", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 准备请求列表和记录ID列表
                var requestList = new List<ProdInfoRequest>();
                var recordIdList = new List<int>();
                var testModelDAL = new TestModelDAL();

                // 统计计数器
                int productCountWithData = 0;      // 有测试数据的产品数
                int productCountNoData = 0;        // 无测试数据的产品数
                int skippedCount = 0;              // 跳过的测试项总数
                int missingConfigCount = 0;        // 缺少TestModel配置的数量
                int invalidDataCount = 0;          // 数据无效的数量

                Logger.Info("开始遍历选中的产品并收集测试数据");

                // ===== 遍历选中的产品 =====
                foreach (var row in selectedRows)
                {
                    try
                    {
                        // ===== 防护3: 验证 DataBoundItem =====
                        if (row.DataBoundItem == null)
                        {
                            Logger.Warning("DataBoundItem为null，跳过该行");
                            continue;
                        }

                        // 尝试转换为 ProductWithSyncStatus 类型
                        if (!(row.DataBoundItem is ProductInfoDAL.ProductWithSyncStatus productWithStatus))
                        {
                            Logger.Warning("DataBoundItem类型转换失败");
                            continue;
                        }

                        // ===== 防护4: 验证产品关键字段 =====
                        // 检查制造编号
                        if (string.IsNullOrWhiteSpace(productWithStatus.mfgno))
                        {
                            Logger.WarningFormat("产品制造编号为空 - ID:{0}", productWithStatus.id);
                            invalidDataCount++;
                            continue;
                        }

                        // 检查型号
                        if (string.IsNullOrWhiteSpace(productWithStatus.spec))
                        {
                            Logger.WarningFormat("产品型号为空 - 制造编号:{0}", productWithStatus.mfgno);
                            invalidDataCount++;
                            continue;
                        }

                        // 检查项目编号
                        if (string.IsNullOrWhiteSpace(productWithStatus.projectno))
                        {
                            Logger.WarningFormat("项目编号为空 - 制造编号:{0}", productWithStatus.mfgno);
                            invalidDataCount++;
                            continue;
                        }

                        // 跳过无测试数据的产品
                        if (productWithStatus.total_count == 0)
                        {
                            Logger.DebugFormat("产品无测试数据 - 制造编号:{0}", productWithStatus.mfgno);
                            productCountNoData++;
                            continue;
                        }

                        productCountWithData++;
                        Logger.DebugFormat("处理产品: {0} - {1}", productWithStatus.spec, productWithStatus.mfgno);

                        // ===== 防护5: 验证测试数据列表 =====
                        var testDataList = testDataDAL.GetLatestByMfgnoAndSpec(
                            productWithStatus.mfgno,
                            productWithStatus.spec);

                        if (testDataList == null || testDataList.Count == 0)
                        {
                            Logger.WarningFormat("未获取到测试数据 - 制造编号:{0}", productWithStatus.mfgno);
                            productCountNoData++;
                            continue;
                        }

                        Logger.DebugFormat("获取到 {0} 条测试数据", testDataList.Count);

                        // ===== 遍历测试数据 =====
                        foreach (var testData in testDataList)
                        {
                            try
                            {
                                // ===== 防护6: 验证 testData 不为 null =====
                                if (testData == null)
                                {
                                    Logger.Warning("testData对象为null");
                                    continue;
                                }

                                // 跳过已同步的数据
                                if (testData.qms_status == "1")
                                {
                                    skippedCount++;
                                    continue;
                                }

                                // ===== 防护7: 验证 testData 关键字段 =====
                                // 检查型号
                                if (string.IsNullOrWhiteSpace(testData.spec))
                                {
                                    Logger.WarningFormat("testData.spec为空 - ID:{0}", testData.id);
                                    skippedCount++;
                                    invalidDataCount++;
                                    continue;
                                }

                                // 检查单元格名称
                                if (string.IsNullOrWhiteSpace(testData.cell_name))
                                {
                                    Logger.WarningFormat("testData.cell_name为空 - ID:{0}", testData.id);
                                    skippedCount++;
                                    invalidDataCount++;
                                    continue;
                                }

                                // ===== 防护8: 查询 TestModel 配置 =====
                                var testModel = testModelDAL.GetBySpecAndParaname(
                                    testData.spec, testData.cell_name);

                                // ===== 防护9: 验证 testModel 不为 null =====
                                if (testModel == null)
                                {
                                    Logger.WarningFormat(
                                        "未找到TestModel配置 - 型号:{0}, 单元格:{1}",
                                        testData.spec, testData.cell_name);
                                    skippedCount++;
                                    missingConfigCount++;
                                    continue;
                                }

                                // ===== 防护10: 验证 testModel 关键字段 =====
                                // 检查参数名称
                                if (string.IsNullOrWhiteSpace(testModel.paraname))
                                {
                                    Logger.WarningFormat(
                                        "testModel.paraname为空 - 型号:{0}, 单元格:{1}",
                                        testData.spec, testData.cell_name);
                                    skippedCount++;
                                    missingConfigCount++;
                                    continue;
                                }

                                // 检查参数单位
                                if (string.IsNullOrWhiteSpace(testModel.paraunit))
                                {
                                    Logger.WarningFormat(
                                        "testModel.paraunit为空 - 型号:{0}, 单元格:{1}",
                                        testData.spec, testData.cell_name);
                                    skippedCount++;
                                    missingConfigCount++;
                                    continue;
                                }

                                // 检查标准值（上下限）
                                if (string.IsNullOrWhiteSpace(testModel.standmin) ||
                                    string.IsNullOrWhiteSpace(testModel.standmax))
                                {
                                    Logger.WarningFormat(
                                        "testModel标准值不完整 - 型号:{0}, 单元格:{1}",
                                        testData.spec, testData.cell_name);
                                    skippedCount++;
                                    missingConfigCount++;
                                    continue;
                                }

                                // ===== 防护11: 构建请求对象（使用 ?? 防止 null）=====
                                var request = new ProdInfoRequest
                                {
                                    projectNo = productWithStatus.projectno,
                                    productNo = productWithStatus.mfgno,
                                    paraName = testModel.paraname,
                                    paraUnit = testModel.paraunit,
                                    standValue = $"{testModel.standmin}-{testModel.standmax}",
                                    actualValue = testData.cell_value ?? "",  // 防止null
                                    remark = testModel.remark ?? ""           // 防止null
                                };

                                requestList.Add(request);
                                recordIdList.Add(testData.id);
                            }
                            catch (Exception exTestData)
                            {
                                Logger.Error("处理测试数据时发生异常", exTestData);
                                skippedCount++;
                                continue;
                            }
                        }
                    }
                    catch (Exception exRow)
                    {
                        Logger.Error("处理行数据时发生异常", exRow);
                        continue;
                    }
                }

                Logger.InfoFormat(
                    "数据收集完成 - 有效产品:{0}, 无效产品:{1}, 可发送:{2}, 已跳过:{3}, 缺少配置:{4}, 数据无效:{5}",
                    productCountWithData, productCountNoData, requestList.Count,
                    skippedCount, missingConfigCount, invalidDataCount);

                // ===== 情况1: 全部产品都没有测试数据 =====
                if (productCountNoData == selectedRows.Count)
                {
                    Logger.Warning("所有选中的产品都没有测试数据");
                    MessageBox.Show(
                        $"选中的 {selectedRows.Count} 个产品都没有测试数据,无法发送。",
                        "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ===== 情况2: 有测试数据,但全部被跳过 =====
                if (skippedCount > 0 && requestList.Count == 0)
                {
                    Logger.Warning("所有测试数据都被跳过");
                    string message = $"所有测试数据都被跳过,无法发送。\n\n" +
                                     $"统计信息:\n" +
                                     $"  • 有测试数据的产品: {productCountWithData} 个\n" +
                                     $"  • 已同步的测试项: {skippedCount - missingConfigCount - invalidDataCount} 条\n" +
                                     $"  • 缺少配置的测试项: {missingConfigCount} 条\n" +
                                     $"  • 数据无效的测试项: {invalidDataCount} 条";

                    if (productCountNoData > 0)
                    {
                        message += $"\n  • 无测试数据的产品: {productCountNoData} 个";
                    }

                    MessageBox.Show(message, "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ===== 情况3: 其他导致没有可发送数据的情况 =====
                if (requestList.Count == 0)
                {
                    Logger.Warning("没有可发送的数据");
                    MessageBox.Show("没有可发送的数据", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ===== 显示详细的确认信息 =====
                string confirmMessage = $"准备发送:\n" +
                    $"  产品数量: {productCountWithData} 个\n" +
                    $"  测试项数: {requestList.Count} 条\n";

                // 显示跳过信息（排除缺少配置和数据无效的数量）
                if (skippedCount - missingConfigCount - invalidDataCount > 0)
                {
                    confirmMessage += $"  已跳过: {skippedCount - missingConfigCount - invalidDataCount} 条(已同步)\n";
                }

                // 显示缺少配置信息
                if (missingConfigCount > 0)
                {
                    confirmMessage += $"  缺少配置: {missingConfigCount} 条\n";
                }

                // 显示数据无效信息
                if (invalidDataCount > 0)
                {
                    confirmMessage += $"  数据无效: {invalidDataCount} 条\n";
                }

                confirmMessage += $"  预计耗时: 约 {requestList.Count * 0.2:F1} 秒\n\n确定继续?";

                Logger.DebugFormat("显示确认对话框:\n{0}", confirmMessage);

                if (MessageBox.Show(confirmMessage, "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    Logger.Info("用户取消了提交操作");
                    return;
                }

                Logger.Info("用户确认提交，开始准备上传");

                // ===== 禁用控件，防止重复操作 =====
                btnSubmit.Enabled = false;
                btnQuery.Enabled = false;
                panelTop.Enabled = false;
                dgvProducts.Enabled = false;
                DisableCloseButton();  // 禁用窗体关闭按钮
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // 创建进度显示窗口
                    var progressForm = new UploadProgressForm(requestList.Count);
                    progressForm.Owner = this;
                    progressForm.Show();

                    Logger.Info("进度窗口已显示，开始上传");

                    // ===== 防护12: Progress 回调异常处理 =====
                    var progress = new Progress<UploadProgressReport>(report =>
                    {
                        try
                        {
                            // 验证 report 对象
                            if (report == null)
                            {
                                Logger.Warning("Progress回调: report对象为null");
                                return;
                            }

                            // 验证索引范围
                            if (report.Current < 1 || report.Current > recordIdList.Count)
                            {
                                Logger.WarningFormat(
                                    "Progress回调: 索引越界 - Current:{0}, Count:{1}",
                                    report.Current, recordIdList.Count);
                                return;
                            }

                            int testId = recordIdList[report.Current - 1];

                            // 验证 Response 对象
                            if (report.Response == null)
                            {
                                Logger.WarningFormat("Progress回调: Response为null - TestId:{0}", testId);
                                return;
                            }

                            string responseMsg = report.Response.resultMsg ?? "无响应信息";

                            // 更新数据库状态
                            if (report.Response.IsSuccess)
                            {
                                testDataDAL.UpdateQmsStatus(testId, "1",
                                    DateTime.Now, responseMsg);
                                Logger.DebugFormat("测试项 {0} 上传成功", testId);
                            }
                            else
                            {
                                testDataDAL.UpdateQmsStatus(testId, "2",
                                    DateTime.Now, responseMsg);
                                Logger.WarningFormat("测试项 {0} 上传失败: {1}", testId, responseMsg);
                            }

                            // 更新进度窗口
                            string status = report.Response.IsSuccess ?
                                "成功" : string.Format("失败: {0}", responseMsg);
                            progressForm.UpdateProgress(report.Current, report.Total,
                                status, report.Response.IsSuccess);
                        }
                        catch (Exception exProgress)
                        {
                            Logger.Error("Progress回调发生异常", exProgress);
                        }
                    });

                    // 批量发送到QMS
                    var result = await QmsApiClient.SendProdInfoBatchAsync(
                        requestList.ToArray(),
                        delayMilliseconds: 200,
                        progress: progress,
                        cancellationToken: progressForm.CancellationToken
                    );

                    Logger.InfoFormat(
                        "批量上传完成 - 成功:{0}, 失败:{1}, 总数:{2}",
                        result.SuccessCount, result.FailCount, result.TotalCount);

                    // 设置进度窗口完成状态
                    if (progressForm.IsCancelled)
                    {
                        Logger.Info("用户取消了上传操作");
                        progressForm.SetCancelled(result.TotalCount, requestList.Count);
                    }
                    else
                    {
                        progressForm.SetCompleted(result.SuccessCount, result.FailCount);
                    }

                    // ===== 防护13: 窗体状态检查 =====
                    // 如果有数据上传成功或失败，自动刷新查询结果
                    if (result.SuccessCount > 0 || result.FailCount > 0)
                    {
                        Logger.Info("准备自动刷新查询结果");

                        await System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                        {
                            // 检查窗体是否已关闭
                            if (this.IsDisposed || !this.IsHandleCreated)
                            {
                                Logger.Warning("窗体已关闭，跳过自动刷新");
                                return;
                            }

                            try
                            {
                                // 跨线程调用需要使用Invoke
                                if (this.InvokeRequired)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        // 再次检查窗体状态
                                        if (!this.IsDisposed)
                                        {
                                            Logger.Debug("执行自动刷新");
                                            btnQuery_Click(null, null);
                                        }
                                    }));
                                }
                                else
                                {
                                    btnQuery_Click(null, null);
                                }
                            }
                            catch (ObjectDisposedException)
                            {
                                Logger.Warning("自动刷新失败: 窗体已释放");
                            }
                            catch (InvalidOperationException)
                            {
                                Logger.Warning("自动刷新失败: 窗体状态异常");
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
                    EnableCloseButton();  // 恢复窗体关闭按钮
                    this.Cursor = Cursors.Default;

                    Logger.Info("控件状态已恢复");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("批量提交测试数据失败", ex);
                MessageBox.Show($"批量发送失败:{ex.Message}\n\n详细信息:\n{ex.StackTrace}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Logger.Info("=== 批量提交操作结束 ===");
            }
        }

        #endregion

        #region DataGridView 双击事件

        /// <summary>
        /// DataGridView 单元格双击事件
        /// 功能：打开选中产品的测试详情窗口
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数（包含行列索引）</param>
        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 忽略表头双击事件
                if (e.RowIndex < 0)
                {
                    Logger.Debug("双击了表头，忽略");
                    return;
                }

                Logger.DebugFormat("双击了第 {0} 行", e.RowIndex);

                // 获取选中行的数据对象
                if (!(dgvProducts.Rows[e.RowIndex].DataBoundItem is ProductInfoDAL.ProductWithSyncStatus productWithStatus))
                {
                    Logger.Warning("无法获取行数据");
                    return;
                }

                Logger.InfoFormat("打开产品详情 - 型号:{0}, 制造编号:{1}",
                    productWithStatus.spec, productWithStatus.mfgno);

                // 检查是否有测试数据
                if (productWithStatus.total_count == 0)
                {
                    Logger.Debug("产品无测试数据，显示提示信息");
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

                // 打开测试详情窗口
                TestDetailQueryForm detailForm = new TestDetailQueryForm(product);
                detailForm.ShowDialog();

                Logger.Debug("测试详情窗口已关闭");
            }
            catch (Exception ex)
            {
                Logger.Error("打开测试详情失败", ex);
                MessageBox.Show($"打开详情失败:{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}