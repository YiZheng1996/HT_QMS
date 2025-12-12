using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 项目信息数据访问类
    /// </summary>
    public class ProjectInfoDAL : BaseDAL
    {
        /// <summary>
        /// 获取所有项目
        /// </summary>
        public List<ProjectInfoModel> GetAll()
        {
            return freeSql.Select<ProjectInfoModel>()
                .OrderByDescending(p => p.create_time)
                .ToList();
        }

        /// <summary>
        /// 根据项目编号获取项目
        /// </summary>
        public ProjectInfoModel GetByProjectNo(string projectNo)
        {
            return freeSql.Select<ProjectInfoModel>()
                .Where(p => p.projectno == projectNo)
                .First();
        }

        /// <summary>
        /// 根据ID获取项目
        /// </summary>
        public ProjectInfoModel GetById(int id)
        {
            return freeSql.Select<ProjectInfoModel>()
                .Where(p => p.id == id)
                .First();
        }
    }
}
