using System;
using System.Collections.Generic;
using System.Linq;
using QMSCientForm.Model;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 测试型号配置数据访问类
    /// </summary>
    public class TestModelDAL : BaseDAL
    {
        /// <summary>
        /// 根据规格获取测试型号配置
        /// </summary>
        public List<TestModelModel> GetBySpec(string spec)
        {
            return freeSql.Select<TestModelModel>()
                .Where(t => t.spec == spec)
                .ToList();
        }

        /// <summary>
        /// 根据规格和参数名称获取配置
        /// </summary>
        public TestModelModel GetBySpecAndParaname(string spec, string paraname)
        {
            return freeSql.Select<TestModelModel>()
                .Where(t => t.spec == spec && t.paraname == paraname)
                .First();
        }

        /// <summary>
        /// 获取所有规格列表
        /// </summary>
        public List<string> GetAllSpecs()
        {
            return freeSql.Select<TestModelModel>()
                .GroupBy(t => t.spec)
                .ToList(g => g.Key);
        }

        /// <summary>
        /// 根据ID获取测试型号配置
        /// </summary>
        public TestModelModel GetById(int id)
        {
            return freeSql.Select<TestModelModel>()
                .Where(t => t.id == id)
                .First();
        }
    }
}
