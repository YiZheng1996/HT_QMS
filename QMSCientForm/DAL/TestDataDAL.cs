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
        /// <summary>
        /// 查询测试数据
        /// </summary>
        public List<TestDataModel> Query(string projectNo, string deviceNo, string spec, 
            string mfgNo, string tester, string qmsStatus, DateTime startDate, DateTime endDate)
        {
            var query = freeSql.Select<TestDataModel>();

            // 时间范围
            query = query.Where(t => t.create_time >= startDate && 
                                    t.create_time <= endDate.AddDays(1).AddSeconds(-1));

            // 按项目筛选（通过产品规格）
            if (!string.IsNullOrWhiteSpace(projectNo))
            {
                var productSpecs = freeSql.Select<ProductInfoModel>()
                    .Where(p => p.projectno == projectNo)
                    .ToList(p => p.spec);
                
                if (productSpecs.Any())
                {
                    query = query.Where(t => productSpecs.Contains(t.spec));
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
        /// 根据制造编号查询测试数据
        /// </summary>
        public List<TestDataModel> GetByMfgno(string mfgno)
        {
            return freeSql.Select<TestDataModel>()
                .Where(t => t.mfgno == mfgno)
                .OrderBy(t => t.create_time)
                .ToList();
        }

        /// <summary>
        /// 根据制造编号获取测试数据
        /// </summary>
        public List<TestDataModel> GetByMfgNo(string mfgNo)
        {
            return freeSql.Select<TestDataModel>()
                .Where(t => t.mfgno == mfgNo)
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
