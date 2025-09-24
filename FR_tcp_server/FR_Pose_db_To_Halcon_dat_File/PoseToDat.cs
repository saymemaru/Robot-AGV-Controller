using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FR_Pose_db_To_Halcon_dat_File
{
    internal class PoseToDat
    {
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
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
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

        /// <summary>
        /// 将FR_Pose点位数据库中的位姿数据导出为Halcon .dat文件
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        /// <param name="datDir">输出文件夹</param>
        public static void ExportDbToDat(string dbPath, string datDir)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                //从 points 表中读取数据
                using (var cmd = new SQLiteCommand("SELECT Name, x, y, z, rx, ry, rz FROM points", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["Name"]?.ToString() ?? "unknown";
                        string fileName = $"movingcam_robot_pose_{name}.dat";
                        string filePath = Path.Combine(datDir, fileName);

                        Console.WriteLine($"Exporting pose for {name} to {filePath}");

                        using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            writer.WriteLine("#");
                            writer.WriteLine("# 3D POSE PARAMETERS: rotation and translation");
                            writer.WriteLine("#");
                            writer.WriteLine();
                            writer.WriteLine("# Used representation type:");
                            writer.WriteLine("f 2");
                            writer.WriteLine();
                            writer.WriteLine("# Rotation angles [deg] or Rodriguez-vector:");
                            writer.WriteLine($"r {reader["rx"]} {reader["ry"]} {reader["rz"]}");
                            writer.WriteLine();
                            writer.WriteLine("# Translational vector (x y z [m]):");
                            double x = Convert.ToDouble(reader["x"]) / 1000.0;
                            double y = Convert.ToDouble(reader["y"]) / 1000.0;
                            double z = Convert.ToDouble(reader["z"]) / 1000.0;
                            writer.WriteLine($"t {x} {y} {z}");
                            writer.WriteLine();
                            writer.WriteLine("#");
                            writer.WriteLine("# HALCON version 6.1 --   Wed Nov 20 11:42:00 2002");
                            writer.WriteLine();
                            writer.WriteLine("#");
                        }
                    }
                }
            }
        }
        
    }
}

