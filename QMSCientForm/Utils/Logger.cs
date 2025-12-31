using System;
using System.IO;
using System.Text;

namespace QMSCientForm.Utils
{
    /// <summary>
    /// 日志工具类 - 提供统一的日志记录功能
    /// 支持多种日志级别、自动文件管理、线程安全
    /// </summary>
    public static class Logger
    {
        #region 私有字段

        /// <summary>
        /// 日志文件锁对象，确保多线程写入安全
        /// </summary>
        private static readonly object logLock = new object();

        /// <summary>
        /// 日志文件存放目录
        /// </summary>
        private static string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        /// 当前日志文件路径
        /// </summary>
        private static string currentLogFile = string.Empty;

        /// <summary>
        /// 是否启用控制台输出（调试模式）
        /// </summary>
        private static bool enableConsole = true;

        /// <summary>
        /// 最低记录级别（低于此级别的日志将被忽略）
        /// </summary>
        private static LogLevel minLevel = LogLevel.Debug;

        #endregion

        #region 日志级别枚举

        /// <summary>
        /// 日志级别
        /// </summary>
        public enum LogLevel
        {
            /// <summary>调试信息 - 用于开发调试</summary>
            Debug = 0,

            /// <summary>普通信息 - 记录程序运行状态</summary>
            Info = 1,

            /// <summary>警告信息 - 不影响运行但需要注意</summary>
            Warning = 2,

            /// <summary>错误信息 - 功能执行失败</summary>
            Error = 3,

            /// <summary>严重错误 - 系统级别错误</summary>
            Fatal = 4
        }

        #endregion

        #region 静态构造函数

        /// <summary>
        /// 静态构造函数 - 初始化日志系统
        /// </summary>
        static Logger()
        {
            try
            {
                // 确保日志目录存在
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 生成当前日志文件名
                UpdateLogFileName();

                // 清理过期日志文件（保留最近30天）
                CleanOldLogs(30);

                // 记录启动信息
                Info("=== 日志系统初始化成功 ===");
                Info($"日志目录: {logDirectory}");
                Info($"最低记录级别: {minLevel}");
            }
            catch (Exception ex)
            {
                // 如果日志系统初始化失败，输出到控制台
                Console.WriteLine($"日志系统初始化失败: {ex.Message}");
            }
        }

        #endregion

        #region 配置方法

        /// <summary>
        /// 设置日志目录
        /// </summary>
        /// <param name="directory">日志目录路径</param>
        public static void SetLogDirectory(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                logDirectory = directory;
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                UpdateLogFileName();
            }
        }

        /// <summary>
        /// 设置最低记录级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public static void SetMinLevel(LogLevel level)
        {
            minLevel = level;
            Info($"日志最低级别已设置为: {level}");
        }

        /// <summary>
        /// 启用或禁用控制台输出
        /// </summary>
        /// <param name="enable">是否启用</param>
        public static void SetConsoleOutput(bool enable)
        {
            enableConsole = enable;
        }

        #endregion

        #region 核心日志方法

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Info(string message)
        {
            WriteLog(LogLevel.Info, message, null);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Warning(string message)
        {
            WriteLog(LogLevel.Warning, message, null);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Error(string message)
        {
            WriteLog(LogLevel.Error, message, null);
        }

        /// <summary>
        /// 记录错误日志（带异常信息）
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象</param>
        public static void Error(string message, Exception ex)
        {
            WriteLog(LogLevel.Error, message, ex);
        }

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象</param>
        public static void Fatal(string message, Exception ex = null)
        {
            WriteLog(LogLevel.Fatal, message, ex);
        }

        #endregion

        #region 格式化日志方法

        /// <summary>
        /// 记录调试日志（格式化）
        /// </summary>
        public static void DebugFormat(string format, params object[] args)
        {
            WriteLog(LogLevel.Debug, string.Format(format, args), null);
        }

        /// <summary>
        /// 记录信息日志（格式化）
        /// </summary>
        public static void InfoFormat(string format, params object[] args)
        {
            WriteLog(LogLevel.Info, string.Format(format, args), null);
        }

        /// <summary>
        /// 记录警告日志（格式化）
        /// </summary>
        public static void WarningFormat(string format, params object[] args)
        {
            WriteLog(LogLevel.Warning, string.Format(format, args), null);
        }

        /// <summary>
        /// 记录错误日志（格式化）
        /// </summary>
        public static void ErrorFormat(string format, params object[] args)
        {
            WriteLog(LogLevel.Error, string.Format(format, args), null);
        }

        #endregion

        #region 私有实现方法

        /// <summary>
        /// 写入日志的核心方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象（可选）</param>
        private static void WriteLog(LogLevel level, string message, Exception ex)
        {
            // 检查日志级别
            if (level < minLevel)
                return;

            try
            {
                // 构建日志内容
                StringBuilder logBuilder = new StringBuilder();

                // 时间戳
                logBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                logBuilder.Append(" ");

                // 日志级别
                logBuilder.Append($"[{GetLevelString(level)}]");
                logBuilder.Append(" ");

                // 线程ID（方便追踪多线程问题）
                logBuilder.Append($"[线程{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
                logBuilder.Append(" ");

                // 日志消息
                logBuilder.Append(message);

                // 异常信息
                if (ex != null)
                {
                    logBuilder.AppendLine();
                    logBuilder.AppendLine($"异常类型: {ex.GetType().Name}");
                    logBuilder.AppendLine($"异常消息: {ex.Message}");
                    logBuilder.AppendLine($"堆栈跟踪: {ex.StackTrace}");

                    // 内部异常
                    if (ex.InnerException != null)
                    {
                        logBuilder.AppendLine($"内部异常: {ex.InnerException.Message}");
                    }
                }

                string logContent = logBuilder.ToString();

                // 输出到控制台
                if (enableConsole)
                {
                    ConsoleColor originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetConsoleColor(level);
                    Console.WriteLine(logContent);
                    Console.ForegroundColor = originalColor;
                }

                // 写入文件（线程安全）
                lock (logLock)
                {
                    // 检查是否需要创建新的日志文件（跨天）
                    if (NeedNewLogFile())
                    {
                        UpdateLogFileName();
                    }

                    // 追加写入日志文件
                    File.AppendAllText(currentLogFile, logContent + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception logEx)
            {
                // 日志系统本身出错，输出到控制台
                Console.WriteLine($"日志写入失败: {logEx.Message}");
            }
        }

        /// <summary>
        /// 更新日志文件名（按日期）
        /// </summary>
        private static void UpdateLogFileName()
        {
            string fileName = $"QMS_{DateTime.Now:yyyyMMdd}.log";
            currentLogFile = Path.Combine(logDirectory, fileName);
        }

        /// <summary>
        /// 检查是否需要创建新的日志文件
        /// </summary>
        /// <returns>是否需要新文件</returns>
        private static bool NeedNewLogFile()
        {
            if (string.IsNullOrEmpty(currentLogFile))
                return true;

            // 如果文件不存在，需要创建
            if (!File.Exists(currentLogFile))
                return true;

            // 检查文件日期是否为今天
            string expectedFileName = $"QMS_{DateTime.Now:yyyyMMdd}.log";
            string currentFileName = Path.GetFileName(currentLogFile);

            return currentFileName != expectedFileName;
        }

        /// <summary>
        /// 获取日志级别字符串
        /// </summary>
        private static string GetLevelString(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return "DEBUG";
                case LogLevel.Info: return "INFO ";
                case LogLevel.Warning: return "WARN ";
                case LogLevel.Error: return "ERROR";
                case LogLevel.Fatal: return "FATAL";
                default: return "UNKNOWN";
            }
        }

        /// <summary>
        /// 获取控制台颜色
        /// </summary>
        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return ConsoleColor.Gray;
                case LogLevel.Info: return ConsoleColor.White;
                case LogLevel.Warning: return ConsoleColor.Yellow;
                case LogLevel.Error: return ConsoleColor.Red;
                case LogLevel.Fatal: return ConsoleColor.DarkRed;
                default: return ConsoleColor.White;
            }
        }

        /// <summary>
        /// 清理过期日志文件
        /// </summary>
        /// <param name="keepDays">保留天数</param>
        private static void CleanOldLogs(int keepDays)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                    return;

                DateTime cutoffDate = DateTime.Now.AddDays(-keepDays);
                var files = Directory.GetFiles(logDirectory, "QMS_*.log");

                int deletedCount = 0;
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                }

                if (deletedCount > 0)
                {
                    Info($"已清理 {deletedCount} 个过期日志文件");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理过期日志失败: {ex.Message}");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        /// <returns>日志文件完整路径</returns>
        public static string GetCurrentLogFilePath()
        {
            return currentLogFile;
        }

        /// <summary>
        /// 打开日志目录
        /// </summary>
        public static void OpenLogDirectory()
        {
            try
            {
                if (Directory.Exists(logDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", logDirectory);
                }
            }
            catch (Exception ex)
            {
                Error("打开日志目录失败", ex);
            }
        }

        #endregion
    }
}