using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{`
    internal class CameraManager
    {
        private static readonly Lazy<CameraManager> _instance =
        new Lazy<CameraManager>(() => new CameraManager());

        // 公开访问点
        public static CameraManager Instance => _instance.Value;

        // 私有构造函数（防止外部实例化）
        private CameraManager()
        {
        }

        private SWIGTYPE_p_CAMERA_OBJECT? camera_obj1 = null;
        //图片保存地址
        private const string pictureFile = "picture";
        private readonly string cloudPointSavePath = Utility.GetValidatedSavePath(pictureFile, "CP.ply");
        private readonly string RGBSavePath = Utility.GetValidatedSavePath(pictureFile, "rgb.bmp");
        //连接标识符
        public bool isCameraConnected { get; private set;} = false; 
        //相机参数
        private int camer_num = 0;
        private int camera_ret = -1;

        // 释放相机资源
        private void DiposeCamera()
        {
            if (camera_obj1 != null && isCameraConnected)
            {
                // 停止采集
                DkamSDK_CSharp.AcquisitionStop(camera_obj1);
                // 关闭数据流通道
                DkamSDK_CSharp.StreamOff(camera_obj1, 0);
                DkamSDK_CSharp.StreamOff(camera_obj1, 1);
                DkamSDK_CSharp.StreamOff(camera_obj1, 2);
                // 断开相机连接
                DkamSDK_CSharp.CameraDisconnect(camera_obj1);
                // 销毁相机对象
                DkamSDK_CSharp.DestroyCamera(camera_obj1);
                camera_obj1 = null;
                isCameraConnected = false;
            }
        }

        //保存相机XML配置文件
        public bool SaveCameraXML(string targetPath)
        {
            if(camera_obj1 != null && isCameraConnected)
            {            
                Console.WriteLine(DkamSDK_CSharp.SaveXmlToLocal(camera_obj1, targetPath));
                return true;
            }
            return false;
        }

        public bool InitializeCamera(out string cameraLog)
        {
            cameraLog = "";
            /*****************
            打印相机日志
            SetLogLevel(int error, int debug, int warnning, int info)
            打开1 关闭0
            *****************/
            DkamSDK_CSharp.SetLogLevel(1, 0, 0, 1);
            //*************************************查询相机************************************
            //发现局域网内的相机
            camer_num = DkamSDK_CSharp.DiscoverCamera();
            Console.WriteLine("Camer num is=" + camer_num);
            cameraLog += $"发现相机数量: {camer_num}; ";

            //创建相机
            if (camer_num <= 0)
            {
                Console.WriteLine("No camera");
                cameraLog = "找不到相机";
                return false;
            }

            //对局域网内的相机进行排序0：IP 1:series number	
            int sort = DkamSDK_CSharp.CameraSort(0);
            Console.WriteLine("the camera sort result=" + sort);
            cameraLog += $"相机排序结果: {sort}; ";

            //查找指定IP的相机
            for (int i = 0; i < camer_num; i++)
            {
                //显示局域网内相机IP
                Console.WriteLine("ip is=" + DkamSDK_CSharp.CameraIP(i));
                cameraLog += $"相机IP: {DkamSDK_CSharp.CameraIP(i)}; ";
                if (String.Compare(DkamSDK_CSharp.CameraIP(i), "192.168.58.11") == 0)
                {
                    camera_ret = i;
                }
                else
                {
                    Console.WriteLine("相机IP与设定不符");
                    cameraLog = "相机IP与设定不符";
                    return false;
                }

            }
            //*************************************连接相机************************************
            //连接相机，输入相机的索引号
            camera_obj1 = DkamSDK_CSharp.CreateCamera(camera_ret);
            int connect = DkamSDK_CSharp.CameraConnect(camera_obj1);
            Console.WriteLine("Connect Camera result：" + connect);
            cameraLog += $"连接相机结果: {connect}; ";

            //连接标识符
            isCameraConnected = true;

            //相机和PC机是否在同一个网段内
            Console.WriteLine("WhetherIsSameSegment=" + DkamSDK_CSharp.WhetherIsSameSegment(camera_obj1));
            cameraLog += $"相机与PC是否在同一网段: {DkamSDK_CSharp.WhetherIsSameSegment(camera_obj1)} ;";
            if (connect == 0)
            {
                //*************************************设置相机************************************
                //获取相机CCP状态
                int[] data = new int[1];

                DkamSDK_CSharp.GetCameraCCPStatus(camera_obj1, data);
                int data_ccp = data[0];
                Console.WriteLine("GetCameraCCPStatus=" + data_ccp);
                cameraLog += $"相机CCP状态: {data_ccp}; ";

                //保存XML到本地              
                //Console.WriteLine(DkamSDK_CSharp.SaveXmlToLocal(camera_obj1, "D:\\"));


                //获取连接相机的宽和高(红外)
                SWIGTYPE_p_int width_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_gray, 0);
                int width = DkamSDK_CSharp.intArray_getitem(width_gray, 0);

                SWIGTYPE_p_int height_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_gray, 0);
                int height = DkamSDK_CSharp.intArray_getitem(height_gray, 0);

                Console.WriteLine("gray width={0}  gray height={1}", width, height);
                cameraLog += $"相机h红外分辨率: {width}x{height}; ";

                //获取连接相机的宽和高(RGB)
                SWIGTYPE_p_int width_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_rgb, 1);
                int widthRGB = DkamSDK_CSharp.intArray_getitem(width_rgb, 0);
                SWIGTYPE_p_int height_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_rgb, 1);
                int heightRGB = DkamSDK_CSharp.intArray_getitem(height_rgb, 0);

                Console.WriteLine("rgb width={0}  rgb height={1}", widthRGB, heightRGB);
                cameraLog += $"相机RGB分辨率: {widthRGB}x{heightRGB}; ";

                //设置重发包请求
                DkamSDK_CSharp.SetResendRequest(camera_obj1, 0, 1);
                Console.WriteLine("GetResendRequest=" + DkamSDK_CSharp.GetResendRequest(camera_obj1, 0));
                cameraLog += $"重发包请求设置: {DkamSDK_CSharp.GetResendRequest(camera_obj1, 0)}; ";

                //设置红外触发模式
                int TirggMode = DkamSDK_CSharp.SetTriggerMode(camera_obj1, 1);
                Console.WriteLine("Tirgger Mode=" + TirggMode);
                cameraLog += $"红外触发模式设置: {TirggMode}; ";

                //设置RGB触发模式
                int TirggModeRGB = DkamSDK_CSharp.SetRGBTriggerMode(camera_obj1, 1);
                Console.WriteLine("Tirgger Mode RGB=" + TirggModeRGB);
                cameraLog += $"RGB触发模式设置: {TirggModeRGB}; ";

                //开启数据流通道(0:红外 1:点云 2:RGB)
                int streamgray = DkamSDK_CSharp.StreamOn(camera_obj1, 0);
                Console.WriteLine("Stream On Gray=" + streamgray);
                cameraLog += $"开启灰度通道: {streamgray}; ";

                int streampoint = DkamSDK_CSharp.StreamOn(camera_obj1, 1);
                Console.WriteLine("Stream On PointCloud=" + streampoint);
                cameraLog += $"开启点云通道: {streampoint}; ";

                int streamRGB = DkamSDK_CSharp.StreamOn(camera_obj1, 2);
                Console.WriteLine("Stream On RGB=" + streamRGB);
                cameraLog += $"开启RGB通道: {streamRGB}; ";

                //开始接受数据
                int start = DkamSDK_CSharp.AcquisitionStart(camera_obj1);
                Console.WriteLine("AcquisitionStart=" + start);
                cameraLog += $"开始采集数据: {start}; ";

                //设置曝光模式
                int SetAutoExposureRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 1, 1);
                Console.WriteLine("SetAutoExposureRGB=" + SetAutoExposureRGB);
                int SetAutoExposure = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 1, 0);
                Console.WriteLine("SetAutoExposure=" + SetAutoExposure);

                /*******************
                设置红外曝光时间
                SetExposureTime(int camera_index, int utime, int camera_cnt)
                camera_index:相机下标  utime:曝光时间 camera_cnt:CMOS编号
                ********************/
                int setexposureTime = DkamSDK_CSharp.SetExposureTime(camera_obj1, 30000, 0);
                Console.WriteLine("Set ExposureTime=" + setexposureTime);
                int getexposureTime = DkamSDK_CSharp.GetExposureTime(camera_obj1, 0);
                Console.WriteLine("Get ExposureTime=" + getexposureTime);
                //设置RGB曝光时间
                int setexposureTimeRGB = DkamSDK_CSharp.SetExposureTime(camera_obj1, 30000, 1);
                Console.WriteLine("Set ExposureTime RGB=" + setexposureTimeRGB);
                int getexposureTimeRGB = DkamSDK_CSharp.GetExposureTime(camera_obj1, 1);
                Console.WriteLine("Get ExposureTime RGB=" + getexposureTimeRGB);

                /**********************
                设置曝光等级(当前只对RGB有效)
                SetCamExposureGainLevel(int camera_index, int camera_cnt, int level)
                camera_index:相机下标 camera_cnt:CMOS编号 level：等级>=1
                **********************/
                int SetAutoExpoRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 0, 1);
                int setExposureGainLevel = DkamSDK_CSharp.SetCamExposureGainLevel(camera_obj1, 1, 3);
                Console.WriteLine("Set CamExposureGainLevel=" + setExposureGainLevel);
                int getExposureGainLevel = DkamSDK_CSharp.GetCamExposureGainLevel(camera_obj1, 1);
                Console.WriteLine("Get CamExposureGainLevel=" + getExposureGainLevel);

                /******************
                设置多重曝光
                SetMutipleExposure(int camera_index, int status)
                camera_index:相机下标 status:曝光等级>1
                *********************/
                int setMutipleEx = DkamSDK_CSharp.SetMutipleExposure(camera_obj1, 1);
                Console.WriteLine("Set MutipleExposure=" + setMutipleEx);
                int getMutipleEx = DkamSDK_CSharp.GetMutipleExposure(camera_obj1);
                Console.WriteLine("Get MutipleExposure=" + getMutipleEx);

                /***************
                设置增益
                SetGain(int camera_index, int mode, int value, int camera_cnt)
                camera_index:相机下标 mode:1 模拟增益量 2 数据增益量 value:>1 camera_cnt:CMOS编号
                ***************/
                int setGain = DkamSDK_CSharp.SetGain(camera_obj1, 1, 3, 0);
                Console.WriteLine("SetGain=" + setGain);
                //获取设置的增益值
                int getGain = DkamSDK_CSharp.GetGain(camera_obj1, 1, 0);
                Console.WriteLine("GetGain=" + getGain);
                return true;
            }
            else             {
                return false;
            }
        }

        //保存一张RGB图片
        public bool SaveRGBFile()
        {
            try
            {
                if (DkamSDK_CSharp.CameraConnect(camera_obj1) == 0 && isCameraConnected)
                {
                    //*****************************配置相机******************************************

                    //获取连接相机的宽和高(RGB)
                    SWIGTYPE_p_int width_rgb = DkamSDK_CSharp.new_intArray(0);
                    DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_rgb, 1);
                    int widthRGB = DkamSDK_CSharp.intArray_getitem(width_rgb, 0);
                    SWIGTYPE_p_int height_rgb = DkamSDK_CSharp.new_intArray(0);
                    DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_rgb, 1);
                    int heightRGB = DkamSDK_CSharp.intArray_getitem(height_rgb, 0);

                    Console.WriteLine("rgb width={0}  rgb height={1}", widthRGB, heightRGB);

                    //分配采集的空间大小
                    PhotoInfoCSharp RGB_data = new PhotoInfoCSharp();
                    int RGBsize = widthRGB * heightRGB * 3;
                    byte[] RGB_pixel = new byte[RGBsize];

                    /*****************
                    设置相机RGB触发模式
                    SetRGBTriggerMode(int camera_index, int mode)
                    camera_index:相机索引号 mode:0 连拍模式  1 触发模式
                    *****************/
                    int TirggModeRGB = DkamSDK_CSharp.SetRGBTriggerMode(camera_obj1, 1);
                    Console.WriteLine("Tirgger Mode RGB=" + TirggModeRGB);

                    //开启RGB通道
                    int streamRGB = DkamSDK_CSharp.StreamOn(camera_obj1, 2);
                    Console.WriteLine("Stream On RGB=" + streamRGB);
                    //开始接收数据
                    int start = DkamSDK_CSharp.AcquisitionStart(camera_obj1);
                    Console.WriteLine("AcquisitionStart=" + start);


                    /*****************
                    刷新缓冲区数据
                    FlushBuffer(int camera_index, unsigned short channel_index)
                    camera_index:相机索引号 channel_index：数据流通道
                    *****************/
                    DkamSDK_CSharp.FlushBuffer(camera_obj1, 2);
                    /*****************
                    设置相机RGB触发模式下的触发帧数
                    SetRGBTriggerCount(int camera_index)
                    camera_index:相机索引号 
                    *****************/
                    int rgb_trigger_count = DkamSDK_CSharp.SetRGBTriggerCount(camera_obj1);
                    Console.WriteLine("set RGB trigger count =" + rgb_trigger_count);
                    //*****************************采集数据******************************************
                    //采集RGB数据
                    int capturergb = DkamSDK_CSharp.CaptureCSharp(camera_obj1, 2, RGB_data, RGB_pixel, RGBsize);
                    Console.WriteLine("Capture RGB=" + capturergb);
                    //*****************************保存数据******************************************
                    //保存RGB到指定路径
                    int saveRGB = DkamSDK_CSharp.SaveToBMPCSharp(camera_obj1, RGB_data, RGB_pixel, RGBsize, RGBSavePath);
                    Console.WriteLine("Save RGB=" + saveRGB);
                    //*****************************关闭流通道 断开相机连接******************************************
                    DkamSDK_CSharp.AcquisitionStop(camera_obj1);
                    //关闭RGB数据流通道
                    int streamoffRGB = DkamSDK_CSharp.StreamOff(camera_obj1, 2);
                    Console.WriteLine("StreamOff RGB=" + streamoffRGB);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
            return false;
        }

        //保存一张点云图片
        public bool SaveCloudPointFile()
        {
            if(DkamSDK_CSharp.CameraConnect(camera_obj1) == 0 && isCameraConnected)
            {
                //*************************************设置相机************************************
                //获取连接相机的宽和高
                SWIGTYPE_p_int width_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_gray, 0);
                int width = DkamSDK_CSharp.intArray_getitem(width_gray, 0);

                SWIGTYPE_p_int height_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_gray, 0);
                int height = DkamSDK_CSharp.intArray_getitem(height_gray, 0);

                Console.WriteLine("gray width={0}  gray height={1}", width, height);


                //分配采集的空间大小
                PhotoInfoCSharp PointCloud_data = new PhotoInfoCSharp();
                //PointCloud_data.pixel = (width * height * 6).ToString();
                int pointsize = width * height * 6;
                byte[] point_pixel = new byte[pointsize];
                //设置相机触发模式mode:0 连拍模式  1 触发模式
                int TirggMode = DkamSDK_CSharp.SetTriggerMode(camera_obj1, 1);
                Console.WriteLine("Tirgger Mode=" + TirggMode);

                //开启数据流通道 （0：红外 1：点云 2：RGB）       
                int streampoint = DkamSDK_CSharp.StreamOn(camera_obj1, 1);
                Console.WriteLine("Stream On PointCloud=" + streampoint);
                //开始接受数据
                int start = DkamSDK_CSharp.AcquisitionStart(camera_obj1);
                Console.WriteLine("AcquisitionStart=" + start);

                //刷新缓冲区数据
                DkamSDK_CSharp.FlushBuffer(camera_obj1, 1);
                //设置相机触发模式下的触发帧数
                int trigger_count = DkamSDK_CSharp.SetTriggerCount(camera_obj1);
                Console.WriteLine("set point trigger count =" + trigger_count);
                //*************************************获取数据************************************
                //超时抓取数据
                int capturepoint = DkamSDK_CSharp.TimeoutCaptureCSharp(camera_obj1, 1, PointCloud_data, point_pixel, pointsize, 3000000);
                Console.WriteLine("Capture PointCloud=" + capturepoint);
                //*************************************保存数据************************************
                //保存点云PCD、PLY、TXT三种数据格式
                //int savepoint = DkamSDK_CSharp.SavePointCloudToPcdCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, "2.pcd");
                //Console.WriteLine("Save PointCloud PCD=" + savepoint);

                int savepointply = DkamSDK_CSharp.SavePointCloudToPlyCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, cloudPointSavePath);
                Console.WriteLine("Save PointCloud PLY=" + savepointply);

                //int savepointtxt = DkamSDK_CSharp.SavePointCloudToTxtCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, "2.txt");
                //Console.WriteLine("Save PointCloud TXT=" + savepointtxt);
                //*************************************关闭数据流通道 断开相机************************************
                DkamSDK_CSharp.AcquisitionStop(camera_obj1);
                //关闭点云数据流通道
                int streamoffpoint = DkamSDK_CSharp.StreamOff(camera_obj1, 1);
                Console.WriteLine("StreamOff Point=" + streamoffpoint);

                return true;
            }
            return false;
        }

        public void Camera()
        {
            /*****************
            打印相机日志
            SetLogLevel(int error, int debug, int warnning, int info)
            打开1 关闭0
            *****************/
            DkamSDK_CSharp.SetLogLevel(1, 0, 0, 1);
            //*************************************查询相机************************************
            //发现局域网内的相机
            camer_num = DkamSDK_CSharp.DiscoverCamera();
            Console.WriteLine("Camer num is=" + camer_num);
            //创建相机

            if (camer_num < 0)
            {
                Console.WriteLine("No camera");
                Console.ReadKey();
            }

            //对局域网内的相机进行排序0：IP 1:series number	
            int sort = DkamSDK_CSharp.CameraSort(0);
            Console.WriteLine("the camera sort result=" + sort);

            for (int i = 0; i < camer_num; i++)
            {
                //显示局域网内相机IP
                Console.WriteLine("ip is=" + DkamSDK_CSharp.CameraIP(i));
                if (String.Compare(DkamSDK_CSharp.CameraIP(i), "192.168.58.11") == 0)
                {
                    camera_ret = i;
                }

            }
            //*************************************连接相机************************************
            //连接相机，输入相机的索引号
            SWIGTYPE_p_CAMERA_OBJECT camera_obj1 = DkamSDK_CSharp.CreateCamera(camera_ret);
            int connect = DkamSDK_CSharp.CameraConnect(camera_obj1);
            Console.WriteLine("Connect Camera result：" + connect);
            //相机和PC机是否在同一个网段内
            Console.WriteLine("WhetherIsSameSegment=" + DkamSDK_CSharp.WhetherIsSameSegment(camera_obj1));
            if (connect == 0)
            {
                //*************************************设置相机************************************
                //获取相机CCP状态
                int[] data = new int[1];

                DkamSDK_CSharp.GetCameraCCPStatus(camera_obj1, data);
                int data_ccp = data[0];
                Console.WriteLine("GetCameraCCPStatus=" + data_ccp);

                //保存XML到本地              
                //Console.WriteLine(DkamSDK_CSharp.SaveXmlToLocal(camera_obj1, "D:\\"));


                //获取连接相机的宽和高(红外)
                SWIGTYPE_p_int width_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_gray, 0);
                int width = DkamSDK_CSharp.intArray_getitem(width_gray, 0);

                SWIGTYPE_p_int height_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_gray, 0);
                int height = DkamSDK_CSharp.intArray_getitem(height_gray, 0);

                Console.WriteLine("gray width={0}  gray height={1}", width, height);


                //获取连接相机的宽和高(RGB)
                SWIGTYPE_p_int width_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj1, width_rgb, 1);
                int widthRGB = DkamSDK_CSharp.intArray_getitem(width_rgb, 0);
                SWIGTYPE_p_int height_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj1, height_rgb, 1);
                int heightRGB = DkamSDK_CSharp.intArray_getitem(height_rgb, 0);

                Console.WriteLine("rgb width={0}  rgb height={1}", widthRGB, heightRGB);

                //分配采集点云的大小
                PhotoInfoCSharp PointCloud_data = new PhotoInfoCSharp();
                //PointCloud_data.pixel = (width * height * 6).ToString();
                int pointsize = width * height * 6;
                byte[] point_pixel = new byte[pointsize];
                //分配采集红外的大小
                PhotoInfoCSharp gray_data = new PhotoInfoCSharp();
                int graysize = width * height;
                byte[] gray_pixel = new byte[graysize];
                //分配采集RGB的大小
                PhotoInfoCSharp RGB_data = new PhotoInfoCSharp();
                int RGBsize = widthRGB * heightRGB * 3;
                byte[] RGB_pixel = new byte[RGBsize];
                //分配点云红外融合数据大小
                int gray_cloud_size = width * height * 6;
                float[] gray_cloud = new float[gray_cloud_size];
                // SWIGTYPE_p_float gray_cloud = DkamSDK_CSharp.new_floatArray(width * height * 6);
                //分配点云RGB融合数据大小
                int rgb_cloud_size = width * height * 6;
                float[] rgb_cloud = new float[rgb_cloud_size];
                //SWIGTYPE_p_float rgb_cloud = DkamSDK_CSharp.new_floatArray(width * height * 6);

                //设置重发包请求
                DkamSDK_CSharp.SetResendRequest(camera_obj1, 0, 1);
                Console.WriteLine("GetResendRequest=" + DkamSDK_CSharp.GetResendRequest(camera_obj1, 0));
                //设置红外触发模式
                int TirggMode = DkamSDK_CSharp.SetTriggerMode(camera_obj1, 1);
                Console.WriteLine("Tirgger Mode=" + TirggMode);
                //设置RGB触发模式
                int TirggModeRGB = DkamSDK_CSharp.SetRGBTriggerMode(camera_obj1, 1);
                Console.WriteLine("Tirgger Mode RGB=" + TirggModeRGB);

                //开启数据流通道(0:红外 1:点云 2:RGB)
                int streamgray = DkamSDK_CSharp.StreamOn(camera_obj1, 0);
                Console.WriteLine("Stream On Gray=" + streamgray);

                int streampoint = DkamSDK_CSharp.StreamOn(camera_obj1, 1);
                Console.WriteLine("Stream On PointCloud=" + streampoint);

                int streamRGB = DkamSDK_CSharp.StreamOn(camera_obj1, 2);
                Console.WriteLine("Stream On RGB=" + streamRGB);
                //开始接受数据
                int start = DkamSDK_CSharp.AcquisitionStart(camera_obj1);
                Console.WriteLine("AcquisitionStart=" + start);

                //设置曝光模式
                int SetAutoExposureRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 1, 1);
                Console.WriteLine("SetAutoExposureRGB=" + SetAutoExposureRGB);
                int SetAutoExposure = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 1, 0);
                Console.WriteLine("SetAutoExposure=" + SetAutoExposure);

                /*******************
                设置红外曝光时间
                SetExposureTime(int camera_index, int utime, int camera_cnt)
                camera_index:相机下标  utime:曝光时间 camera_cnt:CMOS编号
                ********************/
                int setexposureTime = DkamSDK_CSharp.SetExposureTime(camera_obj1, 30000, 0);
                Console.WriteLine("Set ExposureTime=" + setexposureTime);
                int getexposureTime = DkamSDK_CSharp.GetExposureTime(camera_obj1, 0);
                Console.WriteLine("Get ExposureTime=" + getexposureTime);
                //设置RGB曝光时间
                int setexposureTimeRGB = DkamSDK_CSharp.SetExposureTime(camera_obj1, 30000, 1);
                Console.WriteLine("Set ExposureTime RGB=" + setexposureTimeRGB);
                int getexposureTimeRGB = DkamSDK_CSharp.GetExposureTime(camera_obj1, 1);
                Console.WriteLine("Get ExposureTime RGB=" + getexposureTimeRGB);

                /**********************
                设置曝光等级(当前只对RGB有效)
                SetCamExposureGainLevel(int camera_index, int camera_cnt, int level)
                camera_index:相机下标 camera_cnt:CMOS编号 level：等级>=1
                **********************/
                int SetAutoExpoRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj1, 0, 1);
                int setExposureGainLevel = DkamSDK_CSharp.SetCamExposureGainLevel(camera_obj1, 1, 3);
                Console.WriteLine("Set CamExposureGainLevel=" + setExposureGainLevel);
                int getExposureGainLevel = DkamSDK_CSharp.GetCamExposureGainLevel(camera_obj1, 1);
                Console.WriteLine("Get CamExposureGainLevel=" + getExposureGainLevel);

                /******************
                设置多重曝光
                SetMutipleExposure(int camera_index, int status)
                camera_index:相机下标 status:曝光等级>1
                *********************/
                int setMutipleEx = DkamSDK_CSharp.SetMutipleExposure(camera_obj1, 1);
                Console.WriteLine("Set MutipleExposure=" + setMutipleEx);
                int getMutipleEx = DkamSDK_CSharp.GetMutipleExposure(camera_obj1);
                Console.WriteLine("Get MutipleExposure=" + getMutipleEx);

                /***************
                设置增益
                SetGain(int camera_index, int mode, int value, int camera_cnt)
                camera_index:相机下标 mode:1 模拟增益量 2 数据增益量 value:>1 camera_cnt:CMOS编号
                ***************/
                int setGain = DkamSDK_CSharp.SetGain(camera_obj1, 1, 3, 0);
                Console.WriteLine("SetGain=" + setGain);
                //获取设置的增益值
                int getGain = DkamSDK_CSharp.GetGain(camera_obj1, 1, 0);
                Console.WriteLine("GetGain=" + getGain);

                int ss = 1;
                while (true)
                {
                    Console.WriteLine("--------capture total :" + ss++ + "-------------");
                    //刷新缓冲区数据
                    DkamSDK_CSharp.FlushBuffer(camera_obj1, 0);
                    DkamSDK_CSharp.FlushBuffer(camera_obj1, 2);
                    DkamSDK_CSharp.FlushBuffer(camera_obj1, 1);
                    //获取相机固件版本号
                    Console.WriteLine("CameraVerions=" + DkamSDK_CSharp.CameraVerions(camera_obj1));
                    //获取SDK版本号
                    Console.WriteLine("SDKVersion=" + DkamSDK_CSharp.SDKVersion(camera_obj1));
                    //设置相机红外触发模式下的触发帧数
                    int trigger_count = DkamSDK_CSharp.SetTriggerCount(camera_obj1);
                    Console.WriteLine("set gray trigger count =" + trigger_count);
                    //设置相机RGB触发模式下的触发帧数
                    int rgb_trigger_count = DkamSDK_CSharp.SetRGBTriggerCount(camera_obj1);
                    Console.WriteLine("set RGB trigger count =" + rgb_trigger_count);
                    //*************************************采集数据  保存数据************************************
                    //采集点云数据
                    int capturepoint = DkamSDK_CSharp.TimeoutCaptureCSharp(camera_obj1, 1, PointCloud_data, point_pixel, pointsize, 3000000);
                    Console.WriteLine("Capture PointCloud=" + capturepoint);

                    //保存点云
                    int savepoint = DkamSDK_CSharp.SavePointCloudToPcdCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, "2.pcd");
                    Console.WriteLine("Save PointCloud PCD=" + savepoint);

                    //采集红外数据
                    int capturegray = DkamSDK_CSharp.CaptureCSharp(camera_obj1, 0, gray_data, gray_pixel, graysize);
                    Console.WriteLine("Capture Gray=" + capturegray);
                    //保存红外
                    int savegray = DkamSDK_CSharp.SaveToBMPCSharp(camera_obj1, gray_data, gray_pixel, graysize, "2.bmp");
                    Console.WriteLine("Save Gray=" + savegray);
                    //采集RGB数据
                    int capturergb = DkamSDK_CSharp.CaptureCSharp(camera_obj1, 2, RGB_data, RGB_pixel, RGBsize);
                    Console.WriteLine("Capture RGB=" + capturergb);

                    //保存RGB
                    int saveRGB = DkamSDK_CSharp.SaveToBMPCSharp(camera_obj1, RGB_data, RGB_pixel, RGBsize, "2_rgb.bmp");
                    Console.WriteLine("Save Gray=" + saveRGB);
                    //保存深度图
                    int savedeptht = DkamSDK_CSharp.SaveDepthToPngCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, "2.png");
                    Console.WriteLine("Save PointCloud depth=" + savedeptht);

                    //保存滤波
                    DkamSDK_CSharp.FilterPointCloudCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, 0.5);
                    int savefilter = DkamSDK_CSharp.SavePointCloudToPcdCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, "2-filter.pcd");
                    Console.WriteLine("Save FilterPointCloud =" + savefilter);
                    //*************************************数据融合************************************
                    //点云红外融合
                    int pointGray = DkamSDK_CSharp.FusionImageTo3DCSharp(camera_obj1, gray_data, gray_pixel, graysize, PointCloud_data, point_pixel, pointsize, gray_cloud, gray_cloud_size);
                    Console.WriteLine("PointWithGray =" + pointGray);
                    int savePointGray = DkamSDK_CSharp.SavePointCloudWithImageToTxtCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, gray_cloud, gray_cloud_size, "2-gray.txt");
                    Console.WriteLine("Save PointWithGray=" + savePointGray);

                    //点云RGB融合
                    int pointRGB = DkamSDK_CSharp.FusionImageTo3DCSharp(camera_obj1, RGB_data, RGB_pixel, RGBsize, PointCloud_data, point_pixel, pointsize, rgb_cloud, rgb_cloud_size);
                    Console.WriteLine("PointWithRGB =" + pointRGB);
                    int savePointRGB = DkamSDK_CSharp.SavePointCloudWithImageToTxtCSharp(camera_obj1, PointCloud_data, point_pixel, pointsize, rgb_cloud, rgb_cloud_size, "2-rgb.txt");
                    Console.WriteLine("Save PointWithRGB=" + savePointRGB);


                    Console.ReadKey();

                }
            }
        }
    }
}
