using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QMSCientForm.QMS.Models;

namespace QMSCientForm.QMS
{
    /// <summary>
    /// QMS接口客户端
    /// </summary>
    public class QmsApiClient
    {
        private static readonly HttpClient httpClient;
        private static readonly object lockObj = new object();
        private static int sequenceNumber = 0; // 序列号

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static QmsApiClient()
        {
            // 配置HttpClient
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(QmsConfig.REQUEST_TIMEOUT);

            // 忽略SSL证书验证（仅用于测试环境，生产环境建议配置正确的证书）
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            // 设置安全协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls;
        }

        #region 设备信息接口

        /// <summary>
        /// 发送设备信息
        /// </summary>
        /// <param name="deviceStatus">设备状态（正常开机、正常关机、发生故障、故障恢复）</param>
        /// <param name="deviceStatusCtime">设备状态改变时间</param>
        /// <param name="deviceVld">检定有效期</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        public static QmsResponse SendDeviceInfo(string deviceStatus, DateTime deviceStatusCtime,
            string deviceVld = null, string remark = null)
        {
            // 构建请求对象
            var request = new DeviceInfoRequest
            {
                requestNo = GenerateRequestNo(),
                productCategory = QmsConfig.PRODUCT_CATEGORY,
                suppNo = QmsConfig.SUPP_NO,
                suppSecurity = QmsConfig.SUPP_SECURITY,
                productName = QmsConfig.PRODUCT_NAME,
                processName = QmsConfig.PROCESS_NAME,
                deviceName = QmsConfig.DEVICE_NAME,
                deviceNo = QmsConfig.DEVICE_NO,
                deviceStatus = deviceStatus,
                deviceStatusCtime = deviceStatusCtime.ToString("yyyy-MM-dd HH:mm:ss"),
                deviceVld = deviceVld,
                sendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                remark = remark
            };

            return SendRequest<DeviceInfoRequest>(QmsConfig.DEVICE_INFO_URL, request);
        }

        /// <summary>
        /// 异步发送设备信息
        /// </summary>
        public static async Task<QmsResponse> SendDeviceInfoAsync(string deviceStatus,
            DateTime deviceStatusCtime, string deviceVld = null, string remark = null)
        {
            return await Task.Run(() => SendDeviceInfo(deviceStatus, deviceStatusCtime, deviceVld, remark));
        }

        #endregion

        #region 生产参数接口

        /// <summary>
        /// 发送生产参数
        /// </summary>
        /// <param name="projectNo">项目编号</param>
        /// <param name="productNo">生产编号</param>
        /// <param name="paraName">参数名称</param>
        /// <param name="paraUnit">参数单位</param>
        /// <param name="standValue">标准值（如"28-38"）</param>
        /// <param name="actualValue">实测值</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        public static QmsResponse SendProdInfo(string projectNo, string productNo,
            string paraName, string paraUnit, string standValue, string actualValue,
            string remark = null)
        {
            // 构建请求对象
            var request = new ProdInfoRequest
            {
                requestNo = GenerateRequestNo(),
                productCategory = QmsConfig.PRODUCT_CATEGORY,
                suppNo = QmsConfig.SUPP_NO,
                suppSecurity = QmsConfig.SUPP_SECURITY,
                productName = QmsConfig.PRODUCT_NAME,
                infoCategory = QmsConfig.INFO_CATEGORY,
                projectNo = projectNo,
                productNo = productNo,
                batchNo = QmsConfig.BATCH_NO,
                prodName = QmsConfig.PROCESS_NAME,
                productPosition = QmsConfig.PRODUCT_POSITION,
                paraName = paraName,
                paraUnit = paraUnit,
                standValue = standValue,
                actualValue = actualValue,
                sendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                remark = remark
            };

            return SendRequest<ProdInfoRequest>(QmsConfig.PROD_INFO_URL, request);
        }

        /// <summary>
        /// 异步发送生产参数
        /// </summary>
        public static async Task<QmsResponse> SendProdInfoAsync(string projectNo, string productNo,
            string paraName, string paraUnit, string standValue, string actualValue,
            string remark = null)
        {
            return await Task.Run(() => SendProdInfo(projectNo, productNo, paraName,
                paraUnit, standValue, actualValue, remark));
        }

        /// <summary>
        /// 批量发送生产参数（带延迟和进度）
        /// </summary>
        /// <param name="requests">生产参数列表</param>
        /// <param name="delayMilliseconds">每次请求之间的延迟（毫秒），默认200ms</param>
        /// <param name="progressCallback">进度回调 (当前索引, 总数, 当前结果)</param>
        /// <returns>批量发送结果</returns>
        public static BatchSendResult SendProdInfoBatch(
            ProdInfoRequest[] requests,
            int delayMilliseconds = 200,
            Action<int, int, QmsResponse> progressCallback = null)
        {
            var result = new BatchSendResult();

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];

                // 补充固定字段
                request.requestNo = GenerateRequestNo();
                request.productCategory = QmsConfig.PRODUCT_CATEGORY;
                request.suppNo = QmsConfig.SUPP_NO;
                request.suppSecurity = QmsConfig.SUPP_SECURITY;
                request.productName = QmsConfig.PRODUCT_NAME;
                request.infoCategory = QmsConfig.INFO_CATEGORY;
                request.batchNo = QmsConfig.BATCH_NO;
                request.prodName = QmsConfig.PROCESS_NAME;
                request.productPosition = QmsConfig.PRODUCT_POSITION;
                request.sendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 发送请求
                var response = SendRequest<ProdInfoRequest>(QmsConfig.PROD_INFO_URL, request);

                // 统计结果
                if (response.IsSuccess)
                    result.SuccessCount++;
                else
                    result.FailCount++;

                // 回调进度
                if (progressCallback != null)
                    progressCallback(i + 1, requests.Length, response);

                // 延迟（最后一个请求不需要延迟）
                if (i < requests.Length - 1 && delayMilliseconds > 0)
                {
                    System.Threading.Thread.Sleep(delayMilliseconds);
                }
            }

            return result;
        }

        /// <summary>
        /// 批量发送生产参数（异步）
        /// </summary>
        public static async Task<BatchSendResult> SendProdInfoBatchAsync(ProdInfoRequest[] requests)
        {
            return await Task.Run(() => SendProdInfoBatch(requests));
        }

        #endregion

        #region 通用发送方法

        /// <summary>
        /// 发送请求（带重试机制）
        /// </summary>
        private static QmsResponse SendRequest<T>(string url, T request)
        {
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount <= QmsConfig.MAX_RETRY_COUNT)
            {
                try
                {
                    // 序列化请求对象
                    string jsonContent = JsonConvert.SerializeObject(request);

                    // 记录日志
                    LogRequest(url, jsonContent);

                    // 发送HTTP POST请求
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var httpResponse = httpClient.PostAsync(url, content).Result;

                    // 读取响应
                    string responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                    // 记录日志
                    LogResponse(url, responseContent);

                    // 反序列化响应
                    var response = JsonConvert.DeserializeObject<QmsResponse>(responseContent);

                    if (response == null)
                    {
                        response = new QmsResponse
                        {
                            resultCode = "500",
                            resultMsg = "响应解析失败"
                        };
                    }

                    // 如果成功，直接返回
                    if (response.IsSuccess)
                    {
                        return response;
                    }

                    // 如果失败且还有重试次数，继续重试
                    if (retryCount < QmsConfig.MAX_RETRY_COUNT)
                    {
                        System.Threading.Thread.Sleep(QmsConfig.RETRY_INTERVAL);
                        retryCount++;
                        continue;
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    // 记录错误日志
                    LogError(url, ex);

                    // 如果还有重试次数，继续重试
                    if (retryCount < QmsConfig.MAX_RETRY_COUNT)
                    {
                        System.Threading.Thread.Sleep(QmsConfig.RETRY_INTERVAL);
                        retryCount++;
                        continue;
                    }

                    // 重试次数用完，返回失败响应
                    return new QmsResponse
                    {
                        resultCode = "500",
                        resultMsg = $"请求失败：{ex.Message}"
                    };
                }
            }

            // 理论上不会到这里，但为了安全还是返回失败
            return new QmsResponse
            {
                resultCode = "500",
                resultMsg = lastException?.Message ?? "未知错误"
            };
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 生成请求流水号（供应商编码+时间戳）
        /// </summary>
        private static string GenerateRequestNo()
        {
            lock (lockObj)
            {
                // 格式：供应商编码+年月日时分秒+3位随机数
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                // 序号自增，超过99999后重置
                sequenceNumber++;
                if (sequenceNumber > 99999)
                    sequenceNumber = 1;

                string sequence = sequenceNumber.ToString().PadLeft(5, '0');

                return $"{QmsConfig.SUPP_NO}{timestamp}{sequence}";
            }
        }

        /// <summary>
        /// 记录请求日志
        /// </summary>
        private static void LogRequest(string url, string content)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] QMS请求 URL={url}\r\n{content}\r\n";
                System.Diagnostics.Debug.WriteLine(logMessage);

                // 可选：写入日志文件
                // System.IO.File.AppendAllText("QmsApi.log", logMessage);
            }
            catch { }
        }

        /// <summary>
        /// 记录响应日志
        /// </summary>
        private static void LogResponse(string url, string content)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] QMS响应 URL={url}\r\n{content}\r\n";
                System.Diagnostics.Debug.WriteLine(logMessage);

                // 写入日志文件
                System.IO.File.AppendAllText("QmsApi.log", logMessage);
            }
            catch { }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private static void LogError(string url, Exception ex)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] QMS错误 URL={url}\r\n{ex.ToString()}\r\n";
                System.Diagnostics.Debug.WriteLine(logMessage);

                // 可选：写入日志文件
                // System.IO.File.AppendAllText("QmsApi.log", logMessage);
            }
            catch { }
        }

        #endregion
    }
}