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
        /// 根据制造编号获取产品信息
        /// </summary>
        public ProductInfoModel GetByMfgNo(string mfgNo)
        {
            return freeSql.Select<ProductInfoModel>()
                .Where(p => p.mfgno == mfgNo)
                .First();
        }
    }
}
