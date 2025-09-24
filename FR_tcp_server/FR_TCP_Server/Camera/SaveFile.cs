using BitMiracle.LibTiff.Classic;
using FR_TCP_Server;
using Orbbec;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Gemini335
{
    internal class SaveFile
    {
        // 静态文件路径
        public static string dirPath = "Pic";

  
        // 检测静态文件路径
        private static string InitDirPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Path.Combine(AppContext.BaseDirectory, "Pic");
            }
            if (!Directory.Exists(path))
            {
                try 
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("创建文件夹失败，异常信息：" + ex.Message);
                    return Path.Combine(AppContext.BaseDirectory, "Pic");
                }
            }
            else
            {
                return path;
            }
        }


        // 待办
        // 加入utility类
        /// <summary>
        /// 检测静态保存路径，如有则保存在该路径下，否则保存在当前目录
        /// </summary>
        /// <param name="fileName">文件名称（无后缀）</param>
        /// <param name="isFileOverride">是否以相同名称保存/是否覆盖</param>
        /// <returns>保存路径</returns>
        private static string GetSavePath(string fileName, bool isFileOverride)
        {
            //string path = InitDirPath(dirPath);
            string path = Utility.GetValidatedSavePath(dirPath);
            return isFileOverride
                ? Path.Combine(path!, fileName)
                : Path.Combine(path!, $"{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}");
        }

        // 在 SaveColorFrameToPng 方法中添加平台检查，确保仅在 Windows 上调用 Bitmap 相关 API
        public static void SaveColorFrameToPng(ColorFrame colorframe, string fileName, bool isFileOverride)
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("SaveColorFrameToPng 仅支持在 Windows 平台上运行。");

            //保存路径设置
            string savePath = GetSavePath(fileName + "_Color.png", isFileOverride);

            // 获取宽高
            int width = (int)colorframe.GetWidth();
            int height = (int)colorframe.GetHeight();

            // 获取原始RGB数据
            byte[] rgbData = new byte[colorframe.GetDataSize()];
            colorframe.CopyData(ref rgbData);

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData? bmpData = null;
                try
                {
                    bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    Marshal.Copy(rgbData, 0, bmpData.Scan0, rgbData.Length);
                }
                finally
                {
                    if (bmpData != null)
                        bitmap.UnlockBits(bmpData);
                }

                // 保存为png
                bitmap.Save(savePath, ImageFormat.Png);
                Console.WriteLine($"Color frame saved to {savePath}");
            }
        }

        public static void SaveDeepFrameToTiff(DepthFrame depthFrame, string fileName, bool isFileOverride)
        {
            string savePath = GetSavePath(fileName + "_Depth.Tiff", isFileOverride);

            //写入tiff
            using (Tiff tiff = Tiff.Open(savePath, "w"))
            {
                int width = (int)depthFrame.GetWidth();
                int height = (int)depthFrame.GetHeight();
                // 获取深度数据
                byte[] depthData = new byte[depthFrame.GetDataSize()];
                depthFrame.CopyData(ref depthData);

                if (tiff == null)
                {
                    throw new Exception("无法创建TIFF文件");
                }
                tiff.SetField(TiffTag.IMAGEWIDTH, width);
                tiff.SetField(TiffTag.IMAGELENGTH, height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 16);
                tiff.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.UINT);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                tiff.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.ROWSPERSTRIP, height);
                tiff.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                tiff.SetField(TiffTag.IMAGEDESCRIPTION, "Orbbec Gemini 335 Depth Data");

                int bytesPerRow = width * 2; // 每个像素2字节(16位)，所以每行字节数为宽度*2

                //写入图像数据
                for (int row = 0; row < height; row++)
                {
                    int offset = row * bytesPerRow;
                    tiff.WriteScanline(depthData, offset, row, 0);
                }
                Console.WriteLine($"Depth frame saved to {savePath}");


                // 直接保存原始深度数据
                File.WriteAllBytes(GetSavePath(fileName + "_Depth.raw", isFileOverride), depthData);
            }
        }

        public static void SavePointsToPly(PointsFrame frame, string fileName, bool isFileOverride)
        {
            //保存路径
            string pointcloudPath = GetSavePath(fileName + "_DPC.ply", isFileOverride) ;

            byte[] data = new byte[frame.GetDataSize()];
            frame.CopyData(ref data);

            int pointSize = Marshal.SizeOf(typeof(Orbbec.Point));
            int pointsSize = data.Length / Marshal.SizeOf(typeof(Orbbec.Point));

            Orbbec.Point[] points = new Orbbec.Point[pointsSize];

            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);
            for (int i = 0; i < pointsSize; i++)
            {
                IntPtr pointPtr = new IntPtr(dataPtr.ToInt64() + i * pointSize);
                points[i] = Marshal.PtrToStructure<Orbbec.Point>(pointPtr);
            }
            Marshal.FreeHGlobal(dataPtr);

            FileStream fs = new FileStream(pointcloudPath, FileMode.Create);
            var writer = new StreamWriter(fs);
            writer.Write("ply\n");
            writer.Write("format ascii 1.0\n");
            writer.Write("element vertex " + pointsSize + "\n");
            writer.Write("property float x\n");
            writer.Write("property float y\n");
            writer.Write("property float z\n");
            writer.Write("end_header\n");

            for (int i = 0; i < points.Length; i++)
            {
                writer.Write(points[i].x);
                writer.Write(" ");
                writer.Write(points[i].y);
                writer.Write(" ");
                writer.Write(points[i].z);
                writer.Write("\n");
            }

            writer.Close();
            fs.Close();
        }

        public static void SaveRGBPointsToPly(PointsFrame frame, string fileName, bool isFileOverride)
        {
            //保存路径
            string colorPointcloudPath = GetSavePath(fileName + "_RGBDPC.ply", isFileOverride) ;

            byte[] data = new byte[frame.GetDataSize()];
            frame.CopyData(ref data);

            int pointSize = Marshal.SizeOf(typeof(ColorPoint));
            int pointsSize = data.Length / Marshal.SizeOf(typeof(ColorPoint));

            ColorPoint[] points = new ColorPoint[pointsSize];

            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);
            for (int i = 0; i < pointsSize; i++)
            {
                IntPtr pointPtr = new IntPtr(dataPtr.ToInt64() + i * pointSize);
                points[i] = Marshal.PtrToStructure<ColorPoint>(pointPtr);
            }
            Marshal.FreeHGlobal(dataPtr);

            FileStream fs = new FileStream(colorPointcloudPath, FileMode.Create);
            var writer = new StreamWriter(fs);
            writer.Write("ply\n");
            writer.Write("format ascii 1.0\n");
            writer.Write("element vertex " + pointsSize + "\n");
            writer.Write("property float x\n");
            writer.Write("property float y\n");
            writer.Write("property float z\n");
            writer.Write("property uchar red\n");
            writer.Write("property uchar green\n");
            writer.Write("property uchar blue\n");
            writer.Write("end_header\n");

            for (int i = 0; i < points.Length; i++)
            {
                writer.Write(points[i].x);
                writer.Write(" ");
                writer.Write(points[i].y);
                writer.Write(" ");
                writer.Write(points[i].z);
                writer.Write(" ");
                writer.Write(points[i].r);
                writer.Write(" ");
                writer.Write(points[i].g);
                writer.Write(" ");
                writer.Write(points[i].b);
                writer.Write("\n");
            }

            writer.Close();
            fs.Close();
        }
    }
}
