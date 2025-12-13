using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 测试数据访问类
    /// </summary>
    public class TestDataDAL : BaseDAL
    {
        public List<TestDataModel> Query(string projectNo, string deviceNo, string spec,
     string mfgNo, string tester, string qmsStatus, DateTime startDate, DateTime endDate)
        {
            var query = freeSql.Select<TestDataModel>();

            // 时间范围
            query = query.Where(t => t.create_time >= startDate &&
                                    t.create_time <= endDate.AddDays(1).AddSeconds(-1));

            // 按项目筛选
            if (!string.IsNullOrWhiteSpace(projectNo))
            {
                // 先获取该项目的所有产品的制造编号
                var productMfgnos = freeSql.Select<ProductInfoModel>()
                    .Where(p => p.projectno == projectNo)
                    .ToList(p => p.mfgno);  // 使用 mfgno

                if (productMfgnos.Any())
                {
                    query = query.Where(t => productMfgnos.Contains(t.mfgno));  // 通过 mfgno 筛选
                }
            }

            // 设备编号
            if (!string.IsNullOrWhiteSpace(deviceNo))
            {
                query = query.Where(t => t.deviceno == deviceNo);
            }

            // 规格
            if (!string.IsNullOrWhiteSpace(spec))
            {
                query = query.Where(t => t.spec.Contains(spec));
            }

            // 制造编号
            if (!string.IsNullOrWhiteSpace(mfgNo))
            {
                query = query.Where(t => t.mfgno.Contains(mfgNo));
            }

            // 测试人员
            if (!string.IsNullOrWhiteSpace(tester))
            {
                query = query.Where(t => t.tester.Contains(tester));
            }

            // QMS状态
            if (!string.IsNullOrWhiteSpace(qmsStatus))
            {
                query = query.Where(t => t.qms_status == qmsStatus);
            }

            return query.OrderByDescending(t => t.create_time).ToList();
        }

        /// <summary>
        /// 根据制造编号和型号获取最新测试数据（每个测试项只取最新的）
        /// </summary>
        public List<TestDataModel> GetLatestByMfgnoAndSpec(string mfgno, string spec)
        {
            return freeSql.Select<TestDataModel>()
               .Where(t => t.mfgno == mfgno && t.spec == spec)
               .OrderBy(t => t.create_time)
               .ToList()
               // 按 cell_name 分组,每组只取最新的
               .GroupBy(t => t.cell_name)
               .Select(g => g.OrderByDescending(t => t.create_time).First())
               .ToList();
        }

        /// <summary>
        /// 根据制造编号获取所有测试数据（可能包含多个型号）
        /// </summary>
        public List<TestDataModel> GetByMfgno(string mfgno)
        {
            return freeSql.Select<TestDataModel>()
               .Where(t => t.mfgno == mfgno)
               .OrderByDescending(t => t.create_time)
               .ToList();
        }

        /// <summary>
        /// 根据ID获取测试数据
        /// </summary>
        public TestDataModel GetById(int id)
        {
            return freeSql.Select<TestDataModel>()
                .Where(t => t.id == id)
                .First();
        }

        /// <summary>
        /// 根据规格和制造编号获取测试数据
        /// </summary>
        public List<TestDataModel> GetBySpecAndMfgNo(string spec, string mfgNo)
        {
            return freeSql.Select<TestDataModel>()
                .Where(t => t.spec == spec && t.mfgno == mfgNo)
                .OrderBy(t => t.cell_name)
                .ToList();
        }

        /// <summary>
        /// 更新QMS调用状态
        /// </summary>
        public bool UpdateQmsStatus(int id, string qmsStatus, DateTime qmsTime, string qmsResponse)
        {
            return freeSql.Update<TestDataModel>()
                .Set(r => r.qms_status, qmsStatus)
                .Set(r => r.qms_time, qmsTime)
                .Set(r => r.qms_response, qmsResponse)
                .Where(r => r.id == id)
                .ExecuteAffrows() > 0;
        }
    }
}