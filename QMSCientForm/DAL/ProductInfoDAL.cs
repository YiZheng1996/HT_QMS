using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 产品信息数据访问类
    /// </summary>
    public class ProductInfoDAL : BaseDAL
    {
        /// <summary>
        /// 查询产品信息
        /// </summary>
        public List<ProductInfoModel> Query(string projectNo, string spec, string mfgNo,
            string train, string qmsStatus, DateTime startDate, DateTime endDate)
        {
            var query = freeSql.Select<ProductInfoModel>();

            // 时间范围
            query = query.Where(p => p.create_time >= startDate &&
                                    p.create_time <= endDate.AddDays(1).AddSeconds(-1));

            // 项目编号
            if (!string.IsNullOrWhiteSpace(projectNo))
            {
                query = query.Where(p => p.projectno == projectNo);
            }

            // 规格
            if (!string.IsNullOrWhiteSpace(spec))
            {
                query = query.Where(p => p.spec.Contains(spec));
            }

            // 制造编号
            if (!string.IsNullOrWhiteSpace(mfgNo))
            {
                query = query.Where(p => p.mfgno.Contains(mfgNo));
            }

            // 车列号
            if (!string.IsNullOrWhiteSpace(train))
            {
                query = query.Where(p => p.train.Contains(train));
            }

            // QMS状态
            if (!string.IsNullOrWhiteSpace(qmsStatus))
            {
                query = query.Where(p => p.qms_status == qmsStatus);
            }

            return query.OrderByDescending(p => p.create_time).ToList();
        }

        /// <summary>
        /// 多条件查询产品信息
        /// </summary>
        public List<ProductInfoModel> GetByConditions(string projectno = null, string train = null, string spec = null, string mfgno = null)
        {
            var query = freeSql.Select<ProductInfoModel>();

            if (!string.IsNullOrWhiteSpace(projectno))
                query = query.Where(p => p.projectno == projectno);

            if (!string.IsNullOrWhiteSpace(train))
                query = query.Where(p => p.train.Contains(train));

            if (!string.IsNullOrWhiteSpace(spec))
                query = query.Where(p => p.spec.Contains(spec));

            if (!string.IsNullOrWhiteSpace(mfgno))
                query = query.Where(p => p.mfgno.Contains(mfgno));

            return query.OrderByDescending(p => p.create_time).ToList();
        }

        /// <summary>
        /// 根据ID获取产品信息
        /// </summary>
        public ProductInfoModel GetById(int id)
        {
            return freeSql.Select<ProductInfoModel>()
                .Where(p => p.id == id)
                .First();
        }

        /// <summary>
        /// 根据制造编号和型号获取产品信息（唯一标识）
        /// </summary>
        public ProductInfoModel GetByMfgNoAndSpec(string mfgNo, string spec)
        {
            return freeSql.Select<ProductInfoModel>()
                .Where(p => p.mfgno == mfgNo && p.spec == spec)
                .First();
        }

        /// <summary>
        /// 根据制造编号获取产品列表（可能有多个不同型号）
        /// </summary>
        public List<ProductInfoModel> GetListByMfgNo(string mfgNo)
        {
            return freeSql.Select<ProductInfoModel>()
                .Where(p => p.mfgno == mfgNo)
                .OrderByDescending(p => p.create_time)
                .ToList();
        }


        #region 带同步状态的查询

        /// <summary>
        /// 产品信息(带同步状态统计)
        /// </summary>
        public class ProductWithSyncStatus
        {
            public int id { get; set; }
            public string projectno { get; set; }
            public string projectname { get; set; }
            public string train { get; set; }
            public string spec { get; set; }
            public string mfgno { get; set; }
            public DateTime create_time { get; set; }

            // 同步状态统计
            public int total_count { get; set; }      // 总测试项数
            public int synced_count { get; set; }     // 已同步数
            public int failed_count { get; set; }     // 失败数
            public string sync_status { get; set; }   // 同步状态
        }

        /// <summary>
        /// 查询产品信息(带同步状态统计
        /// </summary>
        public List<ProductWithSyncStatus> GetProductsWithSyncStatus(
            string projectno = null,
            string train = null,
            string spec = null,
            string mfgno = null)
        {
            // 第一步: 查询产品列表
            var productQuery = freeSql.Select<ProductInfoModel>();

            if (!string.IsNullOrWhiteSpace(projectno))
                productQuery = productQuery.Where(p => p.projectno == projectno);
            if (!string.IsNullOrWhiteSpace(train))
                productQuery = productQuery.Where(p => p.train.Contains(train));
            if (!string.IsNullOrWhiteSpace(spec))
                productQuery = productQuery.Where(p => p.spec.Contains(spec));
            if (!string.IsNullOrWhiteSpace(mfgno))
                productQuery = productQuery.Where(p => p.mfgno.Contains(mfgno));

            var products = productQuery.OrderByDescending(p => p.create_time).ToList();

            if (products.Count == 0)
                return new List<ProductWithSyncStatus>();

            // 第二步: 批量查询测试数据（一次性查询所有相关产品的测试数据）
            var mfgnoList = products.Select(p => p.mfgno).Distinct().ToList();
            var specList = products.Select(p => p.spec).Distinct().ToList();

            // 查询所有相关的测试数据
            var allTestData = freeSql.Select<TestDataModel>()
                .Where(t => mfgnoList.Contains(t.mfgno) && specList.Contains(t.spec))
                .ToList();

            // 第三步: 在内存中分组统计（性能影响很小，因为数据已在内存）
            var testStats = allTestData
                .GroupBy(t => new { t.mfgno, t.spec })
                .Select(g => new
                {
                    mfgno = g.Key.mfgno,
                    spec = g.Key.spec,
                    total = g.Count(),
                    synced = g.Count(t => t.qms_status == "1"),
                    failed = g.Count(t => t.qms_status == "2")
                })
                .ToDictionary(x => $"{x.mfgno}_{x.spec}");

            // 第四步: 组装结果
            var results = products.Select(p =>
            {
                string key = $"{p.mfgno}_{p.spec}";
                var stat = testStats.ContainsKey(key) ? testStats[key] : null;

                var result = new ProductWithSyncStatus
                {
                    id = p.id,
                    projectno = p.projectno,
                    projectname = p.projectname,
                    train = p.train,
                    spec = p.spec,
                    mfgno = p.mfgno,
                    create_time = p.create_time,
                    total_count = stat?.total ?? 0,
                    synced_count = stat?.synced ?? 0,
                    failed_count = stat?.failed ?? 0
                };

                // 计算同步状态
                if (result.total_count == 0)
                    result.sync_status = "无测试数据";
                else if (result.synced_count == result.total_count)
                    result.sync_status = "全部同步";
                else if (result.synced_count == 0 && result.failed_count == 0)
                    result.sync_status = "未同步";
                else if (result.failed_count > 0)
                    result.sync_status = $"部分同步({result.failed_count}失败)";
                else
                    result.sync_status = "部分同步";

                return result;
            }).ToList();

            return results;
        }

        #endregion
    }
}