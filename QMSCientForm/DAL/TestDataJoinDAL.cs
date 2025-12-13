using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 测试数据关联查询DAL
    /// </summary>
    public class TestDataJoinDAL : BaseDAL
    {
        #region 四张表联合查询 预留
        /// <summary>
        /// 完整测试信息（包含项目、产品、标准、设备）
        /// </summary>
        public class TestDataFullInfo
        {
            // 测试数据
            public int id { get; set; }
            public string cell_name { get; set; }
            public string cell_value { get; set; }
            public DateTime create_time { get; set; }
            public string tester { get; set; }

            // 项目信息
            public string projectno { get; set; }
            public string projectname { get; set; }

            // 产品信息
            public string mfgno { get; set; }
            public string spec { get; set; }
            public string train { get; set; }

            // 测试标准
            public string paraname { get; set; }
            public string paraunit { get; set; }
            public string standmin { get; set; }
            public string standmax { get; set; }
            public string standard_range { get; set; }

            // 设备信息
            public string deviceno { get; set; }
            public string devicename { get; set; }

            // QMS信息
            public string qms_status { get; set; }
            public DateTime? qms_time { get; set; }

            // 判断结果
            public string test_result { get; set; }
            public bool is_qualified { get; set; }
        }

        /// <summary>
        /// 获取完整的测试信息(包含项目、产品、标准、设备)
        /// </summary>
        public List<TestDataFullInfo> GetFullTestInfo(
            string projectNo = null,
            string mfgNo = null,
            string spec = null,
            string deviceNo = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = freeSql.Select<TestDataModel, ProductInfoModel, ProjectInfoModel,
                TestModelModel, DeviceInfoModel>()
                .LeftJoin((td, prod, proj, tm, dev) => td.mfgno == prod.mfgno)
                .LeftJoin((td, prod, proj, tm, dev) => prod.projectno == proj.projectno)
                .LeftJoin((td, prod, proj, tm, dev) =>
                    td.spec == tm.spec && td.cell_name == tm.cell_name)
                .LeftJoin((td, prod, proj, tm, dev) => td.deviceno == dev.deviceno);

            // 添加查询条件
            if (!string.IsNullOrWhiteSpace(projectNo))
                query = query.Where((td, prod, proj, tm, dev) => proj.projectno == projectNo);

            if (!string.IsNullOrWhiteSpace(mfgNo))
                query = query.Where((td, prod, proj, tm, dev) => prod.mfgno.Contains(mfgNo));

            if (!string.IsNullOrWhiteSpace(spec))
                query = query.Where((td, prod, proj, tm, dev) => td.spec.Contains(spec));

            if (!string.IsNullOrWhiteSpace(deviceNo))
                query = query.Where((td, prod, proj, tm, dev) => td.deviceno == deviceNo);

            if (startDate.HasValue)
                query = query.Where((td, prod, proj, tm, dev) => td.create_time >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where((td, prod, proj, tm, dev) =>
                    td.create_time <= endDate.Value.AddDays(1).AddSeconds(-1));

            var results = query.ToList((td, prod, proj, tm, dev) => new TestDataFullInfo
            {
                // 测试数据
                id = td.id,
                cell_name = td.cell_name,
                cell_value = td.cell_value,
                create_time = td.create_time,
                tester = td.tester,

                // 项目信息
                projectno = proj.projectno,
                projectname = proj.projectname,

                // 产品信息
                mfgno = prod.mfgno,
                spec = prod.spec,
                train = prod.train,

                // 测试标准
                paraname = tm.paraname,
                paraunit = tm.paraunit,
                standmin = tm.standmin,
                standmax = tm.standmax,
                standard_range = tm.standmin != null && tm.standmax != null ?
                    $"{tm.standmin}-{tm.standmax}" : "",

                // 设备信息
                deviceno = dev.deviceno,
                devicename = dev.devicename,

                // QMS信息
                qms_status = td.qms_status,
                qms_time = td.qms_time
            });

            // 计算测试结果
            foreach (var item in results)
            {
                item.test_result = CheckTestResult(item.cell_value, item.standmin, item.standmax);
                item.is_qualified = item.test_result == "合格";
            }

            return results;
        }

        #endregion

        /// <summary>
        /// 判断测试结果
        /// </summary>
        private string CheckTestResult(string value, string min, string max)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                string.IsNullOrWhiteSpace(min) ||
                string.IsNullOrWhiteSpace(max))
            {
                return "未知";
            }

            try
            {
                // 处理特殊值
                if (max == "∞" || max.ToUpper() == "INF")
                {
                    float val = float.Parse(value);
                    float minVal = float.Parse(min);
                    return val >= minVal ? "合格" : "不合格";
                }

                if (min == "-∞" || min.ToUpper() == "-INF")
                {
                    float val = float.Parse(value);
                    float maxVal = float.Parse(max);
                    return val <= maxVal ? "合格" : "不合格";
                }

                // 正常范围判断
                float testVal = float.Parse(value);
                float minValue = float.Parse(min);
                float maxValue = float.Parse(max);

                return (testVal >= minValue && testVal <= maxValue) ? "合格" : "不合格";
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 获取不合格的测试项
        /// </summary>
        public List<TestDataFullInfo> GetUnqualifiedTests(DateTime? startDate = null, DateTime? endDate = null)
        {
            var allTests = GetFullTestInfo(startDate: startDate, endDate: endDate);
            return allTests.Where(t => t.test_result == "不合格").ToList();
        }

        /// <summary>
        /// 获取项目测试统计
        /// </summary>
        public class ProjectTestStatistics
        {
            public string projectno { get; set; }
            public string projectname { get; set; }
            public int product_count { get; set; }
            public int test_count { get; set; }
            public int qualified_count { get; set; }
            public int unqualified_count { get; set; }
            public int qms_success_count { get; set; }
            public int qms_fail_count { get; set; }
            public int qms_pending_count { get; set; }
            public double qualified_rate { get; set; }
        }

        /// <summary>
        /// 获取项目统计信息
        /// </summary>
        /// <returns></returns>
        public List<ProjectTestStatistics> GetProjectStatistics()
        {
            var allTests = GetFullTestInfo();

            return allTests
                .GroupBy(t => new { t.projectno, t.projectname })
                .Select(g => new ProjectTestStatistics
                {
                    projectno = g.Key.projectno,
                    projectname = g.Key.projectname,
                    product_count = g.Select(t => t.mfgno).Distinct().Count(),
                    test_count = g.Count(),
                    qualified_count = g.Count(t => t.is_qualified),
                    unqualified_count = g.Count(t => !t.is_qualified && t.test_result != "未知"),
                    qms_success_count = g.Count(t => t.qms_status == "1"),
                    qms_fail_count = g.Count(t => t.qms_status == "2"),
                    qms_pending_count = g.Count(t => t.qms_status == "0" || string.IsNullOrEmpty(t.qms_status)),
                    qualified_rate = g.Count() > 0 ?
                        Math.Round((double)g.Count(t => t.is_qualified) / g.Count() * 100, 2) : 0
                })
                .ToList();
        }

        // 获取设备统计信息
        public List<DeviceUsageStatistics> GetDeviceStatistics()
        {
            var deviceDAL = new DeviceInfoDAL();
            var recordDAL = new DeviceRecordDAL();

            var devices = deviceDAL.GetAll();
            var allTests = GetFullTestInfo();

            return devices.Select(d => new DeviceUsageStatistics
            {
                deviceno = d.deviceno,
                devicename = d.devicename,
                product_count = allTests.Count(t => t.deviceno == d.deviceno && !string.IsNullOrEmpty(t.mfgno)),
                test_count = allTests.Count(t => t.deviceno == d.deviceno),
                last_use_time = allTests.Where(t => t.deviceno == d.deviceno)
                    .OrderByDescending(t => t.create_time)
                    .FirstOrDefault()?.create_time,
                status_change_count = (int)freeSql.Select<DeviceRecordModel>()
                    .Where(r => r.deviceno == d.deviceno)
                    .Count()
            }).ToList();
        }

        /// <summary>
        /// 获取设备使用统计
        /// </summary>
        public class DeviceUsageStatistics
        {
            public string deviceno { get; set; }
            public string devicename { get; set; }
            public int product_count { get; set; }
            public int test_count { get; set; }
            public DateTime? last_use_time { get; set; }
            public int status_change_count { get; set; }
        }


        #region 联合查询TestData与TestModel表

        public List<ProductTestDetail> GetProductTestDetail(string mfgNo)
        {
            var allData = freeSql.Select<TestDataModel, TestModelModel>()
                .LeftJoin((td, tm) => td.spec == tm.spec && td.cell_name == tm.cell_name)
                .Where((td, tm) => td.mfgno == mfgNo)
                .OrderBy((td, tm) => td.create_time)
                .ToList((td, tm) => new
                {
                    td.cell_name,
                    tm.paraname,
                    td.cell_value,
                    tm.paraunit,
                    tm.standmin,
                    tm.standmax,
                    td.create_time,
                    tm.remark
                });

            // 按 cell_name 分组，每组只取最新的一条
            return allData
                .GroupBy(t => t.cell_name)
                .Select(g => g.OrderByDescending(t => t.create_time).First())
                .Select(t => new ProductTestDetail
                {
                    cell_name = t.cell_name,
                    paraname = t.paraname,
                    cell_value = t.cell_value,
                    paraunit = t.paraunit,
                    standard_range = $"{t.standmin}-{t.standmax}",
                    test_result = CheckTestResult(t.cell_value, t.standmin, t.standmax),
                    create_time = t.create_time,
                    test_remark = t.remark
                })
                .ToList();
        }


        /// <summary>
        /// 获取产品测试详情中间类（用于测试详细信息窗体）
        /// </summary>
        public class ProductTestDetail
        {
            public string cell_name { get; set; }
            public string paraname { get; set; }
            public string cell_value { get; set; }
            public string paraunit { get; set; }
            public string standard_range { get; set; }
            public string test_result { get; set; }
            public string test_remark { get; set; }
            public DateTime create_time { get; set; }
        }
        #endregion
    }
}
