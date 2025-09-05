using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    /// <summary>
    /// 外接系统通过任务编码恢复AGV当前执⾏任务
    /// </summary>
    internal class RequestRecoverAgvTaskByTaskByRCS : IRCSAPI
    {
        public static string Name => "RecoverAgvTaskByTaskByRCS";
        public static string APIpath => "Task/RecoverAgvTaskByTask";
        public static HttpMethod HttpMethod => HttpMethod.Post;
        public static RequestRecoverAgvTaskByTask CreateRequest(
            string mapCode,
            string taskCode)
        {
            return new RequestRecoverAgvTaskByTask
            {
                TaskCode = taskCode,
                MapCode = mapCode,
            };
        }
    }
    
    internal class RequestRecoverAgvTaskByTask : APIRequest
    {
        /// <summary>
        /// 地图编码
        /// </summary>
        public string MapCode { get; set; }
        /// <summary>
        /// 任务模板编码
        /// </summary>
        public string TaskCode { get; set; }
    }

    internal class ResponseRecoverAgvTaskByTask : APIReponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 【Success】为false时【Content】为失败原因。
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 错误代码，"0"为成功
        /// </summary>
        public string Code { get; set; }
    }
}
