using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 设备信息数据访问类
    /// </summary>
    public class DeviceInfoDAL : BaseDAL
    {
        /// <summary>
        /// 获取所有设备
        /// </summary>
        public List<DeviceInfoModel> GetAll()
        {
            return freeSql.Select<DeviceInfoModel>()
                .OrderBy(d => d.deviceno)
                .ToList();
        }

        /// <summary>
        /// 根据设备编号获取设备
        /// </summary>
        public DeviceInfoModel GetByDeviceNo(string deviceNo)
        {
            return freeSql.Select<DeviceInfoModel>()
                .Where(d => d.deviceno == deviceNo)
                .First();
        }

        /// <summary>
        /// 根据ID获取设备
        /// </summary>
        public DeviceInfoModel GetById(int id)
        {
            return freeSql.Select<DeviceInfoModel>()
                .Where(d => d.id == id)
                .First();
        }
    }
}
