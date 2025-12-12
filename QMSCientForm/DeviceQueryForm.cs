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
                cmbDeviceNo.Items.Add("");
                
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
                string deviceNo = cmbDeviceNo.Text.Trim();
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
        /// 提交按钮点击事件 - 发送到QMS
        /// </summary>
        private void btnSubmit_Click(object sender, EventArgs e)
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

                if (MessageBox.Show($"确定要发送选中的 {selectedRows.Count} 条设备记录到QMS吗？", 
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var row in selectedRows)
                {
                    int recordId = Convert.ToInt32(row.Cells["RecordId"].Value);
                    var record = recordDAL.GetById(recordId);
                    if (record == null) continue;

                    // 发送到QMS
                    var response = QmsApiClient.SendDeviceInfo(
                        deviceStatus: record.type,
                        deviceStatusCtime: record.create_time,
                        deviceVld: GetDeviceCheckdate(record.deviceno),
                        remark: ""
                    );

                    if (response.IsSuccess)
                    {
                        recordDAL.UpdateQmsStatus(record.id, "1", 
                            DateTime.Now, response.resultMsg);
                        successCount++;
                    }
                    else
                    {
                        recordDAL.UpdateQmsStatus(record.id, "2", 
                            DateTime.Now, response.resultMsg);
                        failCount++;
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
        /// 获取设备检定日期
        /// </summary>
        private string GetDeviceCheckdate(string deviceNo)
        {
            var device = deviceDAL.GetByDeviceNo(deviceNo);
            return device?.checkdate ?? "";
        }
    }
}
