using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    /// <summary>
    /// 外接系统获取对应编码⻋辆执⾏任务编码
    /// </summary>
    internal class RequestGetTaskByAgvCodeByRCS:IRCSAPI
    {
        public static string Name => "RequestGetTaskByAgvCodeByRCS";
        public static string APIpath => "Task/GetTaskByAgvCode";
        public static HttpMethod HttpMethod => HttpMethod.Post;
        public static RequestGetTaskByAgvCode CreateRequest(
            string agvCode)
        {
            return new RequestGetTaskByAgvCode
            {
                id = agvCode,
            };
        }
    }
    internal class RequestGetTaskByAgvCode : APIRequest
    {
        /// <summary>
        /// ⻋辆编码
        /// </summary>
        public string id { get; set; }
    }

    internal class ResponseGetTaskByAgvCode : APIReponse
    {
        /// <summary>
        /// RCS⽣成的任务编码
        /// </summary>
    }
}
