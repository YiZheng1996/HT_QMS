using System;

namespace QMSCientForm.DAL
{
    /// <summary>
    /// 数据访问层基类
    /// </summary>
    public class BaseDAL
    {
        protected static IFreeSql freeSql;

        /// <summary>
        /// 静态构造函数，初始化FreeSql
        /// </summary>
        static BaseDAL()
        {
            try
            {
                freeSql = DbConfig.CreateFreeSql();
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化数据库连接失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取FreeSql实例
        /// </summary>
        protected IFreeSql GetFreeSql()
        {
            return freeSql;
        }
    }
}
