using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    internal static class Utility
    {
 
        /// <summary>
        /// 生成文件路径，起始路径为应用程序启动路径，
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        //relativePath为相对路径，fileName为可选的文件名
        public static string GetValidatedSavePath(string relativePath, string fileName = null)
        {
            // 验证输入参数
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("相对路径不能为空");
            }

            // 检查路径中是否包含非法字符
            char[] invalidChars = Path.GetInvalidPathChars();
            if (relativePath.IndexOfAny(invalidChars) >= 0)
            {
                throw new ArgumentException("路径包含非法字符");
            }

            // 防止路径遍历攻击
            if (relativePath.Contains(".."))
            {
                throw new ArgumentException("路径不能包含上级目录引用");
            }

            // 获取启动路径并组合完整路径
            string startupPath = Application.StartupPath;
            string fullPath = Path.Combine(startupPath, relativePath);

            // 确保路径仍在应用程序目录内（安全措施）
            if (!fullPath.StartsWith(startupPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("尝试创建应用程序目录外的路径");
            }

            // 创建/检验目录
            Directory.CreateDirectory(fullPath);

            // 返回路径（可选包含文件名）
            return string.IsNullOrEmpty(fileName) ? fullPath : Path.Combine(fullPath, fileName);
        }
    }
}
