using System;

namespace QMSCientForm.QMS
{
    /// <summary>
    /// QMS接口配置
    /// </summary>
    public class QmsConfig
    {
        /// <summary>
        /// 设备信息接口地址
        /// </summary>
        public const string DEVICE_INFO_URL = "https://qms.csrcqsf.com:7102/skd/dev/TskdDeviceInfo/";

        /// <summary>
        /// 生产参数接口地址
        /// </summary>
        public const string PROD_INFO_URL = "https://qms.csrcqsf.com:7102/skd/env/TskdProdInfo/";

        /// <summary>
        /// 供方编号
        /// </summary>
        public const string SUPP_NO = "0000088064";

        /// <summary>
        /// 接口密码
        /// </summary>
        public const string SUPP_SECURITY = "Nhi#968LJo";

        /// <summary>
        /// 产品类别
        /// </summary>
        public const string PRODUCT_CATEGORY = "组装类";

        /// <summary>
        /// 产品名称
        /// </summary>
        public const string PRODUCT_NAME = "制动控制装置";

        /// <summary>
        /// 工序名称
        /// </summary>
        public const string PROCESS_NAME = "例行试验";

        /// <summary>
        /// 设备名称
        /// </summary>
        public const string DEVICE_NAME = "通用制动控制元件试验台-气动类元件试验台";

        /// <summary>
        /// 设备编号
        /// </summary>
        public const string DEVICE_NO = "C739-150";

        /// <summary>
        /// 信息类别
        /// </summary>
        public const string INFO_CATEGORY = "生产参数";

        /// <summary>
        /// 批次号（默认值）
        /// </summary>
        public const string BATCH_NO = "/";

        /// <summary>
        /// 产品部位（默认值）
        /// </summary>
        public const string PRODUCT_POSITION = "/";

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        public const int REQUEST_TIMEOUT = 30;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public const int MAX_RETRY_COUNT = 3;

        /// <summary>
        /// 重试间隔（毫秒）
        /// </summary>
        public const int RETRY_INTERVAL = 1000;
    }
}