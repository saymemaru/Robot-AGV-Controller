using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    internal class AGVInfoManager
    {
        private static readonly Lazy<AGVInfoManager> _LazyInstance
       = new Lazy<AGVInfoManager>(() => new AGVInfoManager());

        private AGVInfoManager() { }

        public static AGVInfoManager Instance => _LazyInstance.Value;



        // 存储AGV信息的字典，键为AGVCode，值为AGVInfo对象
        public Dictionary<string, AGVInfo> AGVInfoDic = new Dictionary<string, AGVInfo>();

        // 需要检查的键列表
        //MachineCode AGV编号
        //Name AGV名称
        public readonly static string[] _keysToCheck = new string[] 
        { "MachineCode", "ReceiveTaskID", "MapCode", "TaskCode","AgvGroupCode"};


        /// <summary>
        /// 当在jsonBody中找到"MachineCode"时，注册AGV信息，并将其存储在AGVInfo对象字典中
        /// </summary>
        /// <param name="jsonBody"></param>
        /// <param name="agvInfo"></param>
        /// <returns></returns>
        public bool TryRegisterAGV(string jsonBody, out AGVInfo? agvInfo) 
        {
            try
            {
                // 使用 JsonDocument 解析 JSON 数据
                using JsonDocument doc = JsonDocument.Parse(jsonBody);
                JsonElement root = doc.RootElement;

                //在JSON中找到AGVCode
                if (root.TryGetProperty("MachineCode", out JsonElement AGVCodeElement))
                {
                    string agvCode = AGVCodeElement.GetString() ?? string.Empty;

                    //agvCode 不为空时
                    if (!string.IsNullOrEmpty(agvCode))
                    {
                        // 检查 AGVInfoDic 中是否已经存在该 AGVCode
                        if (AGVInfoDic.TryGetValue(agvCode, out AGVInfo? existingAGVInfo))
                        {
                            // 存在AGVCode，则根据 jsonBody 修改 existingAGVInfo 对象
                            UpdateAGVInfoFromJson(existingAGVInfo, root);
                            agvInfo = existingAGVInfo;
                            return true;
                        }
                        // 不存在AGVCode，则创建新的 AGVInfo 对象
                        else
                        {
                            agvInfo = CreateAGVInfo(root);
                            AGVInfoDic[agvCode] = agvInfo;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                agvInfo = null;
                return false;
            }
            agvInfo = null;
            return  false;
        }

        /// <summary>
        /// 获取AGV信息，并存储在AGVInfo对象字典中
        /// </summary>
        /// <param name="aGVInfo"></param>
        /// <param name="root"></param>
        private static void UpdateAGVInfoFromJson(AGVInfo aGVInfo, JsonElement root)
        {
            foreach (string key in _keysToCheck)
            {
                if (root.TryGetProperty(key, out JsonElement element))
                {
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.String:
                            aGVInfo.AGVInfoDic[key] = element.GetString();
                            break;
                        case JsonValueKind.Number:
                            aGVInfo.AGVInfoDic[key] = element.GetInt32(); // 或 element.GetDouble() 适用于浮点数
                            break;
                        // 可以根据需要添加其他类型的处理
                        default:
                            aGVInfo.AGVInfoDic[key] = element.ToString();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 创建agvinfo对象，获取AGV信息，并存储在AGVInfo对象字典中
        /// </summary>
        /// <param name="jsonBody"></param>
        private static AGVInfo CreateAGVInfo(JsonElement root)
        {
            AGVInfo aGVInfo = new AGVInfo();
            UpdateAGVInfoFromJson(aGVInfo, root);
            return aGVInfo;
        }

    }
}

    internal class AGVInfo
    {
        public Dictionary<string, object> AGVInfoDic = new Dictionary<string, object>
            {
                { "SysToken", null },
                { "ReceiveTaskID", null },
                { "MapCode", null },
                { "TaskCode", null },
                { "AgvGroupCode", null },
                { "MachineCode", null },
                { "name", null }
            };


    }

