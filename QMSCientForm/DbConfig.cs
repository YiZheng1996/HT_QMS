using System.Configuration;

namespace QMSCientForm
{
    /// <summary>
    /// 数据库配置管理类
    /// </summary>
    public static class DbConfig
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                // 优先从配置文件读取
                string connStr = ConfigurationManager.ConnectionStrings["QMSDatabase"]?.ConnectionString;

                if (string.IsNullOrEmpty(connStr))
                {
                    // 如果配置文件中没有，使用默认连接字符串
                    connStr = "Data Source=.;Initial Catalog=QMSDatabase;Integrated Security=True";
                }

                return connStr;
            }
        }

        /// <summary>
        /// 创建FreeSql实例
        /// </summary>
        public static IFreeSql CreateFreeSql()
        {
            return new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.SqlServer, ConnectionString)
                .UseAutoSyncStructure(false) // 不自动同步实体结构到数据库
                .UseMonitorCommand(cmd =>
                {
                    // 可以在这里记录SQL执行日志
                    System.Diagnostics.Debug.WriteLine($"SQL: {cmd.CommandText}");
                })
                .Build();
        }
    }
}