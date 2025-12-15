using System;
using System.Configuration;

namespace QMSCientForm.QMS
{
    /// <summary>
    /// QMS接口配置(从App.config读取)
    /// </summary>
    public class QmsConfig
    {
        /// <summary>
        /// 设备信息接口地址
        /// </summary>
        public static string DEVICE_INFO_URL
        {
            get
            {
                return ConfigurationManager.AppSettings["DeviceInfoUrl"]
                    ?? "https://qms.csrcqsf.com:7102/skd/dev/TskdDeviceInfo/";
            }
        }

        /// <summary>
        /// 生产参数接口地址
        /// </summary>
        public static string PROD_INFO_URL
        {
            get
            {
                return ConfigurationManager.AppSettings["ProdInfoUrl"]
                    ?? "https://qms.csrcqsf.com:7102/skd/env/TskdProdInfo/";
            }
        }

        /// <summary>
        /// 供方编号
        /// </summary>
        public static string SUPP_NO
        {
            get
            {
                return ConfigurationManager.AppSettings["SuppNo"] ?? "0000088064";
            }
        }

        /// <summary>
        /// 接口密码
        /// </summary>
        public static string SUPP_SECURITY
        {
            get
            {
                return ConfigurationManager.AppSettings["SuppSecurity"] ?? "Nhi#968LJo";
            }
        }

        /// <summary>
        /// 产品类别
        /// </summary>
        public static string PRODUCT_CATEGORY
        {
            get
            {
                return ConfigurationManager.AppSettings["ProductCategory"] ?? "组装类";
            }
        }

        /// <summary>
        /// 产品名称
        /// </summary>
        public static string PRODUCT_NAME
        {
            get
            {
                return ConfigurationManager.AppSettings["ProductName"] ?? "制动控制装置";
            }
        }

        /// <summary>
        /// 工序名称
        /// </summary>
        public static string PROCESS_NAME
        {
            get
            {
                return ConfigurationManager.AppSettings["ProcessName"] ?? "例行试验";
            }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DEVICE_NAME
        {
            get
            {
                return ConfigurationManager.AppSettings["DeviceName"]
                    ?? "通用制动控制元件试验台-气动类元件试验台";
            }
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public static string DEVICE_NO
        {
            get
            {
                return ConfigurationManager.AppSettings["DeviceNo"] ?? "C739-150";
            }
        }

        /// <summary>
        /// 信息类别
        /// </summary>
        public static string INFO_CATEGORY
        {
            get
            {
                return ConfigurationManager.AppSettings["InfoCategory"] ?? "生产参数";
            }
        }

        /// <summary>
        /// 批次号(默认值)
        /// </summary>
        public static string BATCH_NO
        {
            get
            {
                return ConfigurationManager.AppSettings["BatchNo"] ?? "/";
            }
        }

        /// <summary>
        /// 产品部位(默认值)
        /// </summary>
        public static string PRODUCT_POSITION
        {
            get
            {
                return ConfigurationManager.AppSettings["ProductPosition"] ?? "/";
            }
        }

        /// <summary>
        /// 请求超时时间(秒)
        /// </summary>
        public static int REQUEST_TIMEOUT
        {
            get
            {
                string value = ConfigurationManager.AppSettings["RequestTimeout"];
                int timeout;
                if (int.TryParse(value, out timeout) && timeout > 0)
                {
                    return timeout;
                }
                return 30; // 默认30秒
            }
        }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public static int MAX_RETRY_COUNT
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MaxRetryCount"];
                int count;
                if (int.TryParse(value, out count) && count >= 0)
                {
                    return count;
                }
                return 3; // 默认3次
            }
        }

        /// <summary>
        /// 重试间隔(毫秒)
        /// </summary>
        public static int RETRY_INTERVAL
        {
            get
            {
                string value = ConfigurationManager.AppSettings["RetryInterval"];
                int interval;
                if (int.TryParse(value, out interval) && interval > 0)
                {
                    return interval;
                }
                return 1000; // 默认1000毫秒
            }
        }
    }
}