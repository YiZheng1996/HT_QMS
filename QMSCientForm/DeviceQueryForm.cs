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
    /// 设备信息查询发送
    /// </summary>
    public partial class DeviceQueryForm : Form
    {
        private DeviceRecordDAL recordDAL = new DeviceRecordDAL();
        private DeviceInfoDAL deviceDAL = new DeviceInfoDAL();

        public DeviceQueryForm()
        {
            InitializeComponent();
            LoadDevices();
            dtpStartDate.Value = dtpStartDate.Value.Date.AddDays(-30); // 默认查询一个月的数据
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
                if (dgvRecords.DataSource == null) return;

                foreach (DataGridViewRow row in dgvRecords.Rows)
                {
                    row.Cells["colSelect"].Value = headerCheckBox.Checked;
                }
                dgvRecords.Refresh();
            };

            // 将 checkbox 添加到表头
            dgvRecords.Controls.Add(headerCheckBox);

            // 自定义绘制表头
            dgvRecords.Paint += (sender, e) =>
            {
                // 获取第一列（checkbox列）的表头位置
                Rectangle rect = dgvRecords.GetCellDisplayRectangle(0, -1, true);

                // 计算checkbox位置（靠左）
                Point checkBoxLocation = new Point(
                    rect.Location.X + 45,  // 左边距5像素
                    rect.Location.Y + (rect.Height - headerCheckBox.Height) / 2 + 1
                );

                headerCheckBox.Location = checkBoxLocation;
                headerCheckBox.BringToFront(); // 确保checkbox在最前面
            };

            // 自定义绘制表头内容（绘制"全选"文字）
            dgvRecords.CellPainting += (sender, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex == 0)
                {
                    e.PaintBackground(e.CellBounds, true);

                    // 在checkbox右侧绘制"全选"文字
                    TextRenderer.DrawText(
                        e.Graphics,
                        "全选",
                        dgvRecords.Font,
                        new Point(e.CellBounds.X + 15, e.CellBounds.Y + 5),  // checkbox宽度约20，所以从25开始
                        dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor
                    );

                    e.Handled = true;
                }
            };

            // 初始化checkbox位置
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvRecords.ColumnHeadersHeight = 23; // 设置表头高度
        }


        /// <summary>
        /// 加载设备列表
        /// </summary>
        private void LoadDevices()
        {
            try
            {
                var devices = deviceDAL.GetAll();

                cmbDeviceNo.Items.Clear();
                cmbDeviceNo.Items.Add("全选");

                foreach (var device in devices)
                {
                    cmbDeviceNo.Items.Add(device.deviceno);
                }

                if (cmbDeviceNo.Items.Count > 0)
                    cmbDeviceNo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载设备失败：{ex.Message}", "错误",
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
                string deviceNo = cmbDeviceNo.SelectedItem?.ToString();
                if (deviceNo == "全选") deviceNo = null;
                
                // 获取查询条件
                string deviceName = txtDeviceName.Text.Trim();
                DateTime? startDate = dtpStartDate.Checked ? dtpStartDate.Value.Date : (DateTime?)null;
                DateTime? endDate = dtpEndDate.Checked ? dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;

                // 查询设备记录
                var records = recordDAL.GetByConditions(
                    deviceno: deviceNo,
                    startDate: startDate,
                    endDate: endDate
                );

                // 如果有设备名称条件，需要关联查询
                if (!string.IsNullOrEmpty(deviceName))
                {
                    var devices = deviceDAL.GetAll().Where(d => d.devicename.Contains(deviceName)).ToList();
                    var deviceNos = devices.Select(d => d.deviceno).ToList();
                    records = records.Where(r => deviceNos.Contains(r.deviceno)).ToList();
                }

                // 绑定到DataGridView
                BindRecords(records);

                // 数据绑定后,设置列的只读属性
                foreach (DataGridViewColumn column in dgvRecords.Columns)
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

                lblCount.Text = $"共 {records.Count} 条记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 绑定记录到DataGridView
        /// </summary>
        private void BindRecords(List<DeviceRecordModel> records)
        {
            // 获取所有设备信息用于显示设备名称
            var devices = deviceDAL.GetAll();
            var deviceDict = devices.ToDictionary(d => d.deviceno, d => d);

            // 创建显示数据
            var displayList = records.Select((r, index) => new
            {
                序号 = index + 1,
                设备编号 = r.deviceno,
                设备名称 = deviceDict.ContainsKey(r.deviceno) ? deviceDict[r.deviceno].devicename : "",
                状态类型 = r.type,
                时间 = r.create_time,
                检定日期 = deviceDict.ContainsKey(r.deviceno) ? deviceDict[r.deviceno].checkdate : "",
                判定 = GetJudgment(r.deviceno, deviceDict),
                同步状态 = GetQmsStatusText(r.qms_status),
                RecordId = r.id
            }).ToList();

            dgvRecords.DataSource = null;
            dgvRecords.DataSource = displayList;

            // 隐藏RecordId列
            if (dgvRecords.Columns["RecordId"] != null)
                dgvRecords.Columns["RecordId"].Visible = false;

            // 设置列宽
            if (dgvRecords.Columns["序号"] != null)
                dgvRecords.Columns["序号"].Width = 60;

            if (dgvRecords.Columns["设备编号"] != null)
                dgvRecords.Columns["设备编号"].Width = 100;

            if (dgvRecords.Columns["设备名称"] != null)
                dgvRecords.Columns["设备名称"].Width = 250;

            if (dgvRecords.Columns["状态类型"] != null)
                dgvRecords.Columns["状态类型"].Width = 100;

            if (dgvRecords.Columns["时间"] != null)
                dgvRecords.Columns["时间"].Width = 150;

            if (dgvRecords.Columns["检定日期"] != null)
                dgvRecords.Columns["检定日期"].Width = 100;

            if (dgvRecords.Columns["判定"] != null)
                dgvRecords.Columns["判定"].Width = 80;

            if (dgvRecords.Columns["同步状态"] != null)
                dgvRecords.Columns["同步状态"].Width = 80;
        }

        /// <summary>
        /// 获取判定结果
        /// </summary>
        private string GetJudgment(string deviceNo, Dictionary<string, DeviceInfoModel> deviceDict)
        {
            if (!deviceDict.ContainsKey(deviceNo))
                return "未知";

            var device = deviceDict[deviceNo];
            if (string.IsNullOrEmpty(device.checkdate))
                return "未检定";

            try
            {
                DateTime checkDate = DateTime.Parse(device.checkdate);
                return checkDate > DateTime.Now ? "有效" : "过期";
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 获取QMS状态文本
        /// </summary>
        private string GetQmsStatusText(string status)
        {
            switch (status)
            {
                case "0": return "未同步";
                case "1": return "已同步";
                case "2": return "失败";
                default: return "未同步";
            }
        }

        /// <summary>
        /// 提交按钮点击事件 - 发送到QMS（带进度和取消）
        /// </summary>
        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中的行
                var selectedRows = dgvRecords.Rows.Cast<DataGridViewRow>()
                    .Where(row => row.Cells[0].Value != null && (bool)row.Cells[0].Value)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("请至少选择一条记录", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    string.Format("确定要发送选中的 {0} 条设备记录到QMS吗？", selectedRows.Count),
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 准备请求列表和记录ID列表
                var requestList = new List<DeviceInfoRequest>();

                // DeviceRecord表记录Id列表，修改接口调用信息
                var recordIdList = new List<int>();

                // 跳过已经同步的数量
                int skippedCount = 0; 

                foreach (var row in selectedRows)
                {
                    int recordId = Convert.ToInt32(row.Cells["RecordId"].Value);
                    var record = recordDAL.GetById(recordId);
                    if (record == null) continue;

                    // 跳过已同步的数据
                    if (record.qms_status == "1")
                    {
                        skippedCount++;
                        continue;
                    }

                    // 添加请求对象
                    var request = new DeviceInfoRequest
                    {
                        deviceNo = record.deviceno,
                        deviceStatus = record.type,
                        deviceStatusCtime = record.create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                        deviceVld = GetDeviceCheckdate(record.deviceno),
                        remark = "" // 数据表内无备注字段
                    };

                    requestList.Add(request);
                    recordIdList.Add(recordId); // 添加记录ID到列表
                }

                // 情况1：全部记录都已同步
                if (skippedCount > 0 && requestList.Count == 0)
                {
                    MessageBox.Show(
                        $"选中的 {skippedCount} 条记录都已同步完成，无需重复上传。",
                        "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 情况2：其他原因导致没有可发送数据
                if (requestList.Count == 0)
                {
                    MessageBox.Show("选中的记录无效，无法发送。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                string confirmMessage = $"确定要发送选中的 {requestList.Count} 条设备记录到QMS吗？";
                if (skippedCount > 0)
                {
                    confirmMessage += $"\n\n（已跳过 {skippedCount} 条已同步的数据）";
                }

                if (MessageBox.Show(confirmMessage, "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 禁用按钮
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
                        // 更新数据库状态
                        int recordId = recordIdList[report.Current - 1];
                        if (report.Response.IsSuccess)
                        {
                            recordDAL.UpdateQmsStatus(recordId, "1",
                                DateTime.Now, report.Response.resultMsg);
                        }
                        else
                        {
                            recordDAL.UpdateQmsStatus(recordId, "2",
                                DateTime.Now, report.Response.resultMsg);
                        }

                        // 更新进度窗口
                        string status = report.Response.IsSuccess ?
                            "成功" : string.Format("失败: {0}", report.Response.resultMsg);
                        progressForm.UpdateProgress(report.Current, report.Total,
                            status, report.Response.IsSuccess);
                    });

                    // 批量发送
                    var result = await QmsApiClient.SendDeviceInfoBatchAsync(
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

                    // 刷新数据
                    btnQuery_Click(null, null);
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
                MessageBox.Show(string.Format("发送失败：{0}", ex.Message), "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取设备检定日期（确保格式为 yyyy-MM-dd HH:mm:ss）
        /// </summary>
        private string GetDeviceCheckdate(string deviceNo)
        {
            var device = deviceDAL.GetByDeviceNo(deviceNo);

            if (device == null || string.IsNullOrWhiteSpace(device.checkdate))
            {
                System.Diagnostics.Debug.WriteLine($"设备 {deviceNo} 无检定日期");
                return "";
            }

            try
            {
                // 尝试解析日期字符串
                DateTime checkDate = DateTime.Parse(device.checkdate);

                // 统一格式化为 yyyy-MM-dd HH:mm:ss
                string formattedDate = checkDate.ToString("yyyy-MM-dd HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"设备 {deviceNo} 检定日期: {device.checkdate} -> {formattedDate}");

                return formattedDate;
            }
            catch (Exception ex)
            {
                // 记录错误但不影响主流程
                System.Diagnostics.Debug.WriteLine($"设备 {deviceNo} 日期格式错误: {device.checkdate}, 错误: {ex.Message}");
                return "";
            }
        }
    }
}
