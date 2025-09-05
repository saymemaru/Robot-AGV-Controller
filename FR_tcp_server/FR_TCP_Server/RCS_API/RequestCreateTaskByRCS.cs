using FR_TCP_Server.RCS_API;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    /// <summary>
    /// 外接系统将⼊库任务下发到RCS
    /// </summary>
    internal class RequestCreateTaskByRCS : IRCSAPI
    {
        public static string Name => "RequestCreateTaskByRCS";
        public static string APIpath => "Task/CreateTask";
        public static HttpMethod HttpMethod => HttpMethod.Post;

        public static CreateTaskRequest CreateRequest(
            string receiveTaskID,
            string sysToken,
            string mapCode,
            string taskCode,
            string agvGroupCode,
            string agvCode,
            int priority = 5,
            List<Variables>? variables = null)
        {
            return new CreateTaskRequest
            {
                SysToken = sysToken,
                ReceiveTaskID = receiveTaskID,
                MapCode = mapCode,
                TaskCode = taskCode,
                AgvGroupCode = agvGroupCode,
                AGVCode = agvCode,
                Priority = priority,
                Variables = variables
            };
        }
    }
    internal class CreateTaskRequest : APIRequest
    {
        /// <summary>
        /// 系统token
        /// </summary>
        public string SysToken { get; set; }

        /// <summary>
        /// 单号
        /// </summary>
        public string ReceiveTaskID { get; set; }

        /// <summary>
        /// 地图编码
        /// </summary>
        public string MapCode { get; set; }

        /// <summary>
        /// 任务模板编码
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// 车辆集群分组编码，标识调用指定集群内车辆执行任务，可设置为空
        /// </summary>
        public string AgvGroupCode { get; set; }

        /// <summary>
        /// 车辆编码，标识调用指定车辆执行任务，可设置为空
        /// </summary>
        public string AGVCode { get; set; }

        /// <summary>
        /// 任务优先级 1~10，默认为5
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 任务参数数组，json格式字符串，格式参考示例
        /// </summary>
        [SugarColumn(IsJson = true)]
        public List<Variables>? Variables { get; set; }

    }
    internal class ResponseCreateTask : APIReponse
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

