using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 设备记录数据访问类
    /// </summary>
    public class DeviceRecordDAL : BaseDAL
    {
        /// <summary>
        /// 查询设备记录
        /// </summary>
        public List<DeviceRecordModel> Query(string deviceNo, string qmsStatus, 
            DateTime startDate, DateTime endDate)
        {
            var query = freeSql.Select<DeviceRecordModel>();

            // 时间范围
            query = query.Where(d => d.create_time >= startDate && 
                                    d.create_time <= endDate.AddDays(1).AddSeconds(-1));

            // 设备编号
            if (!string.IsNullOrWhiteSpace(deviceNo))
            {
                query = query.Where(d => d.deviceno == deviceNo);
            }

            // QMS状态
            if (!string.IsNullOrWhiteSpace(qmsStatus))
            {
                query = query.Where(d => d.qms_status == qmsStatus);
            }

            return query.OrderByDescending(d => d.create_time).ToList();
        }

        /// <summary>
        /// 多条件查询设备记录
        /// </summary>
        public List<DeviceRecordModel> GetByConditions(string deviceno = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = freeSql.Select<DeviceRecordModel>();

            if (!string.IsNullOrWhiteSpace(deviceno))
                query = query.Where(r => r.deviceno == deviceno);

            if (startDate.HasValue)
                query = query.Where(r => r.create_time >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.create_time <= endDate.Value);

            return query.OrderByDescending(r => r.create_time).ToList();
        }

        /// <summary>
        /// 根据ID获取设备记录
        /// </summary>
        public DeviceRecordModel GetById(int id)
        {
            return freeSql.Select<DeviceRecordModel>()
                .Where(d => d.id == id)
                .First();
        }

        /// <summary>
        /// 根据设备编号获取最新记录
        /// </summary>
        public DeviceRecordModel GetLatestByDeviceNo(string deviceNo)
        {
            return freeSql.Select<DeviceRecordModel>()
                .Where(d => d.deviceno == deviceNo)
                .OrderByDescending(d => d.create_time)
                .First();
        }

        /// <summary>
        /// 更新QMS调用状态
        /// </summary>
        public bool UpdateQmsStatus(int id, string qmsStatus, DateTime qmsTime, string qmsResponse)
        {
            return freeSql.Update<DeviceRecordModel>()
                .Set(r => r.qms_status, qmsStatus)
                .Set(r => r.qms_time, qmsTime)
                .Set(r => r.qms_response, qmsResponse)
                .Where(r => r.id == id)
                .ExecuteAffrows() > 0;
        }
    }
}
