using FR_TCP_Server;
using Newtonsoft.Json;
using Orbbec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini335
{
    internal class Gemini335Camera
    {
        private static readonly Lazy<Gemini335Camera> _instance =
            new Lazy<Gemini335Camera>(() => new Gemini335Camera(640,480,30));
        public static Gemini335Camera Instance => _instance.Value;

        private Pipeline? pipeline = new Pipeline();
        private Config? config = new Config();
        private PointCloudFilter? pointCloud = new PointCloudFilter();
        private StreamProfile? colorProfile;
        private StreamProfile? depthProfile;
        private Device? device;
        private SensorList? sensorList;

        /// <summary>
        /// 创建相机对象
        /// </summary>
        /// <param name="imageWidth">宽度</param>
        /// <param name="imageHeight">高度</param>
        /// <param name="fps">帧率</param>
        public Gemini335Camera (int imageWidth,int imageHeight,int fps) 
        {
            //设置点云格式
            pointCloud.SetCreatePointFormat(Format.OB_FORMAT_POINT);
            // Create configuration files for color and depth streams. 0 is the default configuration.
            this.colorProfile = pipeline.
                GetStreamProfileList(SensorType.OB_SENSOR_COLOR).
                GetVideoStreamProfile(imageWidth, imageHeight, Format.OB_FORMAT_BGR, fps);
            this.depthProfile = pipeline.
                GetStreamProfileList(SensorType.OB_SENSOR_DEPTH).
                GetVideoStreamProfile(imageWidth, imageHeight, Format.OB_FORMAT_Y16, fps);
            TryGetDevice();
        }

        //获取设备信息
        public void TryGetDevice()
        {
            try
            {
                this.device = pipeline.GetDevice();
                if (device == null)
                {
                    Console.WriteLine("未找到奥比中光设备");
                    return;
                }
                
                StringBuilder info = new StringBuilder();
                info.Append($"已找到设备：");
                info.AppendLine($"设备名称：{device.GetDeviceInfo().Name()}");
                info.AppendLine($"设备类型：{device.GetDeviceInfo().DeviceType()}");
                info.AppendLine($"设备UID：{device.GetDeviceInfo().Uid()}");
                info.AppendLine($"软件版本：{device.GetDeviceInfo().FirmwareVersion()}");
                info.AppendLine($"硬件版本：{device.GetDeviceInfo().HardwareVersion()}");
                Console.WriteLine(info);
            }
            catch(Exception ex)
            {
                Console.WriteLine("获取设备失败，异常信息："+ex.Message);
            }
        }

        //开启传感器流
        public void TryEnableAllSensorStream(ref Config config)
        {
            try
            {
                //获取传感器
                this.sensorList = device.GetSensorList();
                for (uint i = 0; i < sensorList.SensorCount(); i++)
                {
                    SensorType sensorType = sensorList.SensorType(i);

                    if (sensorType == SensorType.OB_SENSOR_ACCEL || sensorType == SensorType.OB_SENSOR_GYRO)
                    {
                        continue;
                    }
                    config.EnableStream(sensorType);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("获取设备失败，异常信息："+ex.Message);
            }
        }

        //对齐功能
        public Frame AlignmengtMode()
        {
            //对齐RGB流
            AlignFilter align = new AlignFilter(StreamType.OB_STREAM_COLOR);
            // Capture one frame of data
            var frames = pipeline.WaitForFrames(100);
            // Apply the alignment filter
            Frame alignedFrameset = align.Process(frames);
            return alignedFrameset;
        }

        /// <summary>
        /// 获取RGBDPC图
        /// </summary>
        /// <param name="isOverridePic">是否覆盖同名图像</param>
        /// <param name="fileStartName">文件起始名称</param>
        /// <param name="is3D">同时获取D/PC图</param>
        /// <returns></returns>
        public async Task GetRGBDImgAsync(bool isOverridePic, string fileStartName, bool is3D)
        {
            
            ColorFrame? colorFrame;
            DepthFrame? depthFrame;

            // Enable color and depth streams
            config.EnableStream(colorProfile);
            config.EnableStream(depthProfile);

            // Start the pipeline with config.
            pipeline.Start(config);

            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                await Task.Run(() =>
                {
                    while (!tokenSource.Token.IsCancellationRequested)
                    {
                        // Wait for up to 100ms for a frameset in blocking mode.
                        using (var frames = pipeline.WaitForFrames(100))
                        {
                            // get color and depth frame from frameset.
                            colorFrame = frames?.GetColorFrame();
                            depthFrame = frames?.GetDepthFrame();

                            // 保存 colorFrame.
                            if (colorFrame != null)
                            {
                                //Console.WriteLine("get color frame");
                                SaveFile.SaveColorFrameToPng(colorFrame, $"{fileStartName}", isOverridePic);
                                colorFrame.Dispose();
                            }
                            // 保存 depthFrame.
                            if (depthFrame != null && is3D)
                            {
                                //Console.WriteLine("get depth frame");
                                SaveFile.SaveDeepFrameToTiff(depthFrame, $"{fileStartName}", isOverridePic);

                                // Save point cloud data
                                PointsFrame pointsFrame = pointCloud.Process(depthFrame).As<PointsFrame>();
                                if (pointsFrame.GetFormat() == Format.OB_FORMAT_POINT)
                                {
                                    SaveFile.SavePointsToPly(pointsFrame, "DepthPoints.ply", true);
                                }

                                depthFrame.Dispose();
                                pointsFrame.Dispose();
                            }

                            //取消循环
                            if ((depthFrame != null && colorFrame != null) ||
                                (colorFrame != null && !is3D))
                            {
                                tokenSource.Cancel();
                            }
                        }
                    }
                });
            }

            // Stop the pipeline
            config.DisableAllStream();
            pipeline.Stop();
        }
    }
}
