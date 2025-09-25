using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace FR_TCP_Server.Vision
{
    internal class DLSegmentModel
    {
        private HDevelopExport export = new();

        private HObject ho_ImageBatch = new(), ho_DepthImage = new();

        // Local control variables 
        private HTuple hv_ImageDir = new HTuple(), hv_ExampleDataDir = new HTuple();
        private HTuple hv_UsePretrainedModel = new HTuple(), hv_PreprocessParamFileName = new HTuple();
        private HTuple hv_RetrainedModelFileName = new HTuple(), hv_DataDirectory = new HTuple();
        private HTuple hv_DLDeviceHandles = new HTuple(), hv_DLDevice = new HTuple();
        private HTuple hv_BatchSizeInference = new HTuple(), hv_MinConfidence = new HTuple();
        private HTuple hv_MaxOverlap = new HTuple(), hv_MaxOverlapClassAgnostic = new HTuple();
        private HTuple hv_DLModelHandle = new HTuple(), hv_DLPreprocessParam = new HTuple();
        private HTuple hv_DLDataInfo = new HTuple(), hv_ClassNames = new HTuple();
        private HTuple hv_ClassIDs = new HTuple(), hv_DLSampleBatch = new HTuple();
        private HTuple hv_DLResultBatch = new HTuple(), hv_SampleIndex = new HTuple();
        private HTuple hv_DLSample = new HTuple(), hv_DLResult = new HTuple();
        private HTuple hv_DetectedClassIDs = new HTuple(), hv_NumberDetectionsPerClass = new HTuple();
        private HTuple hv_Index = new HTuple(), hv_bbox_row = new HTuple();
        private HTuple hv_bbox_col = new HTuple(), hv_cx = new HTuple();
        private HTuple hv_cy = new HTuple(), hv_Pose_CamToTool = new HTuple();
        private HTuple hv_u = new HTuple(), hv_v = new HTuple(), hv_depth = new HTuple();
        private HTuple hv_fx = new HTuple(), hv_fy = new HTuple(), hv_X_camera = new HTuple();
        private HTuple hv_Y_camera = new HTuple(), hv_Z_camera = new HTuple();
        private HTuple hv_X = new HTuple(), hv_Y = new HTuple(), hv_Z = new HTuple();
        private HTuple hv_Rx = new HTuple(), hv_Ry = new HTuple(), hv_Rz = new HTuple();
        private HTuple hv_Pose_BaseToTool = new HTuple(), hv_Pose_ToolToCam = new HTuple();
        private HTuple hv_Pose_BaseToCam = new HTuple(), hv_HomMat3D_BaseToCam = new HTuple();
        private HTuple hv_Xb = new HTuple(), hv_Yb = new HTuple(), hv_Zb = new HTuple();

        private readonly string ImageDir = Utility.GetValidatedSavePath("Pic");
        private readonly string ExampleDataDir = Utility.GetValidatedSavePath("Vision");
        private readonly string PreprocessParamFileName = "model_test1.hdict";
        private readonly string RetrainedModelFileName = "model_test1.hdl";
        private readonly string ImageBatch = Utility.GetValidatedSavePath("Pic", "1_Color.png");
        private readonly string PoseCamToToolDat = Utility.GetValidatedSavePath("Vision", "movingcam_final_pose_cam_tool.dat");
        private readonly string DepthImage = Utility.GetValidatedSavePath("Pic", "1_Depth.Tiff");

        public void Initialize()
        {
            try
            {
                //初始设置
                //（path）输入图片路径
                hv_ImageDir.Dispose();
                hv_ImageDir = ImageDir;
                //(path)模型数据路径
                hv_ExampleDataDir.Dispose();
                hv_ExampleDataDir = ExampleDataDir;
                //
                //(bool)是否使用预训练模型
                hv_UsePretrainedModel.Dispose();
                hv_UsePretrainedModel = 1;
                //(path)模型数据名称
                if ((int)(hv_UsePretrainedModel) != 0)
                {
                    //Use the pretrained model and preprocessing parameters shipping with HALCON.
                    //预训练参数路径hdict
                    hv_PreprocessParamFileName.Dispose();
                    hv_PreprocessParamFileName = ExampleDataDir + "/" + PreprocessParamFileName;
                    //预训练模型路径hdl
                    hv_RetrainedModelFileName.Dispose();
                    hv_RetrainedModelFileName = ExampleDataDir + "/" + RetrainedModelFileName;
                }
                else
                {
                    //File name of the dict containing parameters used for preprocessing.
                    //Note: Adapt DataDirectory after preprocessing with another image size.
                    hv_DataDirectory.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DataDirectory = hv_ExampleDataDir + "/dldataset_pill_bag_512x320";
                    }
                    hv_PreprocessParamFileName.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PreprocessParamFileName = hv_DataDirectory + "/dl_preprocess_param.hdict";
                    }
                    //File name of the finetuned object detection model.
                    hv_RetrainedModelFileName.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_RetrainedModelFileName = hv_ExampleDataDir + "/best_dl_model_detection.hdl";
                    }
                }

                //选择运行设备（当前设置优先选择 GPU ）
                hv_DLDeviceHandles.Dispose();
                HOperatorSet.QueryAvailableDlDevices((new HTuple("runtime")).TupleConcat("runtime"),
                    (new HTuple("gpu")).TupleConcat("cpu"), out hv_DLDeviceHandles);
                if ((int)(new HTuple((new HTuple(hv_DLDeviceHandles.TupleLength())).TupleEqual(
                    0))) != 0)
                {
                    throw new HalconException("没有找到支持该模型的设备");
                }
                
                hv_DLDevice.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DLDevice = hv_DLDeviceHandles.TupleSelect(
                        0);
                }
                //
                //推理批次数量.
                hv_BatchSizeInference.Dispose();
                hv_BatchSizeInference = 1;
                //最小至信度
                hv_MinConfidence.Dispose();
                hv_MinConfidence = 0.4;
                //最大重叠度
                hv_MaxOverlap.Dispose();
                hv_MaxOverlap = 0.2;
                //类别不可知最大重叠度
                hv_MaxOverlapClassAgnostic.Dispose();
                hv_MaxOverlapClassAgnostic = 0.7;

                //检查文件是否齐全
                export.check_data_availability(hv_ExampleDataDir, hv_PreprocessParamFileName, hv_RetrainedModelFileName,hv_UsePretrainedModel);

                //读取重训练模型
                hv_DLModelHandle.Dispose();
                HOperatorSet.ReadDlModel(hv_RetrainedModelFileName, out hv_DLModelHandle);
                //
                //设置批次数量.
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "batch_size", hv_BatchSizeInference);
                //
                //初始化模型推断.
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "device", hv_DLDevice);
                //
                //设置模型后处理参数.
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "min_confidence", hv_MinConfidence);
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "max_overlap", hv_MaxOverlap);
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "max_overlap_class_agnostic",
                    hv_MaxOverlapClassAgnostic);
                //
                //获取预处理参数.
                hv_DLPreprocessParam.Dispose();
                HOperatorSet.ReadDict(hv_PreprocessParamFileName, new HTuple(), new HTuple(),
                    out hv_DLPreprocessParam);
                //
                hv_DLDataInfo.Dispose();
                HOperatorSet.CreateDict(out hv_DLDataInfo);
                hv_ClassNames.Dispose();
                HOperatorSet.GetDlModelParam(hv_DLModelHandle, "class_names", out hv_ClassNames);
                HOperatorSet.SetDictTuple(hv_DLDataInfo, "class_names", hv_ClassNames);
                hv_ClassIDs.Dispose();
                HOperatorSet.GetDlModelParam(hv_DLModelHandle, "class_ids", out hv_ClassIDs);
                HOperatorSet.SetDictTuple(hv_DLDataInfo, "class_ids", hv_ClassIDs);
            }
            catch (HalconException HDevExpDefaultException)
            {
                Console.WriteLine(HDevExpDefaultException.Message);
                throw;
            }
        }

        /// <summary>
        /// 推理，返回 DLResult 中的目标数量
        /// </summary>
        /// <returns>0 表示没有检测到目标</returns>
        private int Inference()
        {
            //推理区
            ho_ImageBatch.Dispose();
            HOperatorSet.ReadImage(out ho_ImageBatch, ImageBatch);
            //
            //Generate the DLSampleBatch.
            hv_DLSampleBatch.Dispose();
            export.gen_dl_samples_from_images(ho_ImageBatch, out hv_DLSampleBatch);
            //
            //Preprocess the DLSampleBatch.
            export.preprocess_dl_samples(hv_DLSampleBatch, hv_DLPreprocessParam);
            //
            //Apply the DL model on the DLSampleBatch.
            hv_DLResultBatch.Dispose();
            HOperatorSet.ApplyDlModel(hv_DLModelHandle, hv_DLSampleBatch, new HTuple(),
                out hv_DLResultBatch);
            //

            HTuple end_val83 = hv_BatchSizeInference - 1;
            HTuple step_val83 = 1;
            for (hv_SampleIndex = 0; hv_SampleIndex.Continue(end_val83, step_val83); hv_SampleIndex = hv_SampleIndex.TupleAdd(step_val83))
            {
                //
                //Get sample and according results.
                hv_DLSample.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DLSample = hv_DLSampleBatch.TupleSelect(
                        hv_SampleIndex);
                }
                hv_DLResult.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DLResult = hv_DLResultBatch.TupleSelect(
                        hv_SampleIndex);
                }
                //
                //Count detected pills for each class.
                hv_DetectedClassIDs.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DetectedClassIDs = hv_DLResult.TupleGetDictTuple(
                        "bbox_class_id");
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumberDetectionsPerClass.Dispose();
                    HOperatorSet.TupleGenConst(new HTuple(hv_ClassIDs.TupleLength()), 0, out hv_NumberDetectionsPerClass);
                }
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_ClassIDs.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if (hv_NumberDetectionsPerClass == null)
                        hv_NumberDetectionsPerClass = new HTuple();
                    hv_NumberDetectionsPerClass[hv_Index] = ((hv_DetectedClassIDs.TupleEqualElem(
                        hv_ClassIDs.TupleSelect(hv_Index)))).TupleSum();
                }
            }

            return hv_DLResult.Length;
        }

        /// <summary>
        /// 只获取一个目标，获取物体在机械臂基坐标系下的坐标
        /// </summary>
        /// <param name="toolToBasePoseArray">工具到基座位姿（单位：mm）</param>
        /// <returns>计算标定后的物体上方工具位姿（单位：mm）</returns>
        public float[] CalculateItemToBasePose(float[] toolToBasePoseArray)
        {
            if (Inference() == 0)
            {
                return Array.Empty<float>();
            }

            //计算区
            //获取所有物体 中心点（像素坐标）到数组hv_bbox_row，hv_bbox_col
            hv_bbox_row.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_bbox_row = (((((hv_DLResult.TupleGetDictTuple(
                    "bbox_row2"))).TupleSelect(0)) - (((hv_DLResult.TupleGetDictTuple("bbox_row1"))).TupleSelect(
                    0))) / 2) + (((hv_DLResult.TupleGetDictTuple("bbox_row1"))).TupleSelect(0));
            }
            hv_bbox_col.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_bbox_col = (((((hv_DLResult.TupleGetDictTuple(
                    "bbox_col2"))).TupleSelect(0)) - (((hv_DLResult.TupleGetDictTuple("bbox_col1"))).TupleSelect(
                    0))) / 2) + (((hv_DLResult.TupleGetDictTuple("bbox_col1"))).TupleSelect(0));
            }

            //获取距离图片中心距离最近的目标坐标（DLResult.bbox_row，DLResult.bbox_col）
            //area_center (ImageBatch, Area, image_Row_Center, image_Col_Center)
            //计算距离元组
            //distances := sqrt(exp2(DLResult.bbox_row - cx) + exp2(DLResult.bbox_col - cy))
            //找到最小距离
            //tuple_min (distances, min_distance)
            //找到最小距离index
            //tuple_find_first (distances, min_distance, targetIndex)
            //目标坐标
            //target_Row := DLResult.bbox_row[targetIndex]
            //target_Col := DLResult.bbox_col[targetIndex]


            //1. 读取标定数据
            hv_Pose_CamToTool.Dispose();
            HOperatorSet.ReadPose(PoseCamToToolDat, out hv_Pose_CamToTool);

            //读取数组序号为0的物体坐标，单位像素
            hv_u.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_u = hv_bbox_row.TupleSelect( 0);
            }

            hv_v.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_v = hv_bbox_col.TupleSelect( 0);
            }

            //读取tiff文件获取深度，单位mm
            ho_DepthImage.Dispose();
            HOperatorSet.ReadImage(out ho_DepthImage, DepthImage);
            hv_depth.Dispose();
            HOperatorSet.GetGrayval(ho_DepthImage, hv_v, hv_u, out hv_depth);

            //相机内参单位（像素）
            hv_fx.Dispose();
            hv_fx = 460.082031;
            hv_fy.Dispose();
            hv_fy = 460.323242;
            hv_cx.Dispose();
            hv_cx = 325.222778;
            hv_cy.Dispose();
            hv_cy = 241.309494;

            //物体在相机坐标系下的坐标
            hv_X_camera.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_X_camera = (((hv_u - hv_cx) * hv_depth) / hv_fx) / 1000;
            }
            hv_Y_camera.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Y_camera = (((hv_v - hv_cy) * hv_depth) / hv_fy) / 1000;
            }
            hv_Z_camera.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Z_camera = hv_depth / 1000;
            }


            //2. 获取当前工具位姿
            hv_X.Dispose();
            hv_X = toolToBasePoseArray[0];
            hv_Y.Dispose();
            hv_Y = -toolToBasePoseArray[1];
            hv_Z.Dispose();
            hv_Z = toolToBasePoseArray[2];
            hv_Rx.Dispose();
            hv_Rx = toolToBasePoseArray[3];
            hv_Ry.Dispose();
            hv_Ry = toolToBasePoseArray[4];
            hv_Rz.Dispose();
            hv_Rz = toolToBasePoseArray[5];
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Pose_BaseToTool.Dispose();
                HOperatorSet.CreatePose(hv_X / 1000, hv_Y / 1000, hv_Z / 1000, hv_Rx, hv_Ry, hv_Rz,
                    "Rp+T", "gba", "point", out hv_Pose_BaseToTool);
            }

            //物体到基座矩阵HomMat3D_BaseToCam
            hv_Pose_ToolToCam.Dispose();
            HOperatorSet.PoseInvert(hv_Pose_CamToTool, out hv_Pose_ToolToCam);
            hv_Pose_BaseToCam.Dispose();
            HOperatorSet.PoseCompose(hv_Pose_BaseToTool, hv_Pose_ToolToCam, out hv_Pose_BaseToCam);
            hv_HomMat3D_BaseToCam.Dispose();
            HOperatorSet.PoseToHomMat3d(hv_Pose_BaseToCam, out hv_HomMat3D_BaseToCam);

            //物体到基坐标系（Xb, Yb, Zb）
            hv_Xb.Dispose(); hv_Yb.Dispose(); hv_Zb.Dispose();
            HOperatorSet.AffineTransPoint3d(hv_HomMat3D_BaseToCam, hv_X_camera, hv_Y_camera,
                hv_Z_camera, out hv_Xb, out hv_Yb, out hv_Zb);

            //返回mm
            return [(float)(hv_Xb * 1000).D, (float)(hv_Yb * 1000).D, (float)(hv_Zb * 1000).D];
        }

        private void DisposeAll()
        {
            ho_ImageBatch.Dispose();
            ho_DepthImage.Dispose();

            hv_ImageDir.Dispose();
            hv_ExampleDataDir.Dispose();
            hv_UsePretrainedModel.Dispose();
            hv_PreprocessParamFileName.Dispose();
            hv_RetrainedModelFileName.Dispose();
            hv_DataDirectory.Dispose();
            hv_DLDeviceHandles.Dispose();
            hv_DLDevice.Dispose();
            hv_BatchSizeInference.Dispose();
            hv_MinConfidence.Dispose();
            hv_MaxOverlap.Dispose();
            hv_MaxOverlapClassAgnostic.Dispose();
            hv_DLModelHandle.Dispose();
            hv_DLPreprocessParam.Dispose();
            hv_DLDataInfo.Dispose();
            hv_ClassNames.Dispose();
            hv_ClassIDs.Dispose();
            hv_DLSampleBatch.Dispose();
            hv_DLResultBatch.Dispose();
            hv_SampleIndex.Dispose();
            hv_DLSample.Dispose();
            hv_DLResult.Dispose();
            hv_DetectedClassIDs.Dispose();
            hv_NumberDetectionsPerClass.Dispose();
            hv_Index.Dispose();
            hv_bbox_row.Dispose();
            hv_bbox_col.Dispose();
            hv_cx.Dispose();
            hv_cy.Dispose();
            hv_Pose_CamToTool.Dispose();
            hv_u.Dispose();
            hv_v.Dispose();
            hv_depth.Dispose();
            hv_fx.Dispose();
            hv_fy.Dispose();
            hv_X_camera.Dispose();
            hv_Y_camera.Dispose();
            hv_Z_camera.Dispose();
            hv_X.Dispose();
            hv_Y.Dispose();
            hv_Z.Dispose();
            hv_Rx.Dispose();
            hv_Ry.Dispose();
            hv_Rz.Dispose();
            hv_Pose_BaseToTool.Dispose();
            hv_Pose_ToolToCam.Dispose();
            hv_Pose_BaseToCam.Dispose();
            hv_HomMat3D_BaseToCam.Dispose();
            hv_Xb.Dispose();
            hv_Yb.Dispose();
            hv_Zb.Dispose();
        }
    
    }
}
