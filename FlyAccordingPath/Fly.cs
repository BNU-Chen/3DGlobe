using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Windows.Forms;
using System.Threading;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Geodatabase;

using System.Runtime.InteropServices;
using DevComponents.DotNetBar;

namespace FlyAccordingPath
{
    public class Fly
    {
        #region//变量定义
        //private IAGAnimationPlayer pAGAplayer = null;   //播放控制
        private IScene scene = null;
        public IGlobe globe = null;
        private IAGAnimationUtils pAgAnimationUtils;//
        private IAnimationExtension pAnimationExtension = null;
        private IAGAnimationPlayer animPlayer = null;
        private string ANIMATIONPATH_LayerName = "飞行路线";//图层名称 AnimationPath";
        private string ANIMATIONPATH_FieldName = "PathName";    //存储飞行的字段名称
        public IPolyline FlyPathPolyline = null;
        public double FlyDurationTime = 0;  //飞行持续时间
        private ESRI.ArcGIS.Animation.IAGImportPathOptions pAGImportPathOptions = null;

        //飞行设置
        public int FlySpeedRating = 3; //默认中速为3
        public bool IsFlyLoop = false; //是否循环飞行
        public bool IsFlyLookdown = false; //是否俯视飞行
        //进度条
        //public DevComponents.DotNetBar.ProgressBarItem ProgressBarFlyAnimation = null; //飞行进度条

        private bool IsFlyByPolyline = true;

        #endregion

        #region //原有按录制的路径飞行方式
        #region //按已有的aga路径飞行
        public static void FlyByPath(AxGlobeControl _axGlobeControl, string _FlyPathName, string FlyFoldPath,GISFunction.SceneBookmark pSceneBookmark)
        {
            try
            {
                string FlyFilePath = FlyFoldPath + "\\" + _FlyPathName + ".aga";
                if (!File.Exists(FlyFilePath))    //如果此文件不存在
                {
                    return;
                }
                IGlobe globe = _axGlobeControl.Globe;
                IBasicScene2 basicScene = (IBasicScene2)globe;
                basicScene.LoadAnimation(FlyFilePath);

                double duration = 10;
                int numCycles = 1;  //循环次数

                PlayAnimationTrack(duration, numCycles, FlyFilePath, globe, pSceneBookmark);
            }
            catch
            { }
        }
        #endregion

        #region //由路径，取得文件夹内的所有文件（不查询子目录）
        public static List<string> getFileListFromFoldPath(string FoldPath)
        {
            List<string> fileList = new List<string>();

            try
            {
                DirectoryInfo theFolder = new DirectoryInfo(FoldPath);
                FileInfo[] fileInfo = theFolder.GetFiles();
                //遍历文件夹
                string fileName = "";
                foreach (FileInfo file in fileInfo)
                {
                    fileName = file.Name;
                    //为了防止以数字开头的路径名称导致按钮无法运行，所以这里增加了字符串头，而且用substring去掉了后缀
                    fileList.Add("btn_" + fileName.Substring(0, fileName.Length - 4));
                }

                return fileList;
            }
            catch
            {
                return fileList;
            }
        }
        #endregion

        #region //PlayAnimation
        private static void PlayAnimationTrack(double duration, int numCycles, string FlyFilePath, IGlobe globe, GISFunction.SceneBookmark pSceneBookmark)
        {
            try
            {
                IAnimationTracks tracks = (IAnimationTracks)globe;
                IViewers3D viewers3D = globe.GlobeDisplay;

                //exit if document doesn't contain animation..
                string sError;
                if (tracks.TrackCount == 0)
                {
                    sError = FlyFilePath;
                    if (sError == "")
                    {
                        sError = "飞行路径文件可能丢失，请确保程序的完整性";
                        System.Windows.Forms.MessageBox.Show(sError, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("加载飞行文件失败，请尝试重新安装程序。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                DateTime startTime;
                TimeSpan timeSpan;
                int j;
                double elapsedTime;

                for (int i = 1; i <= numCycles; i++)
                {
                    startTime = DateTime.Now;
                    j = 0;
                    do
                    {
                        timeSpan = (DateTime.Now).Subtract(startTime);
                        elapsedTime = timeSpan.TotalSeconds;
                        if (elapsedTime > duration) elapsedTime = duration;
                        tracks.ApplyTracks(null, elapsedTime, duration);
                        viewers3D.RefreshViewers();
                        j = j + 1;
                    }
                    while (elapsedTime < duration);
                }
                //if(FlyFilePath.Length>4)
                //{

                //    int index = FlyFilePath.IndexOf("飞行");
                //    int indexFlyPath = FlyFilePath.IndexOf("FlyPath");
                //    if (index > 0&&indexFlyPath>0)
                //    {
                //        string cityName = FlyFilePath.Substring(indexFlyPath + 8, index - indexFlyPath-8);
                //        pSceneBookmark.ZoomToScene(cityName);
                //    }
                //}
            }
            catch
            { }
        }
        #endregion

        #region //PlayAnimationFast
        private void PlayAnimationFast(int cycles, int iteration, string FlyFilePath, AxGlobeControl axGlobeControl)
        {
            try
            {
                IGlobe globe = axGlobeControl.Globe;
                IGlobeDisplay globeDisplay = globe.GlobeDisplay;
                Scene scene = (Scene)globeDisplay.Scene;
                IAnimationTracks sceneTracks = (IAnimationTracks)scene;

                IArray trackCamArray = new ArrayClass();
                IArray trackGlbArray = new ArrayClass();
                IArray trackLyrArray = new ArrayClass();

                string sError;
                if (sceneTracks.TrackCount == 0)
                {
                    sError = FlyFilePath;
                    if (sError == "")
                    {
                        sError = "To get a Sample animation file, Developer Kit Samples need to be installed!";
                        System.Windows.Forms.MessageBox.Show("The current document doesn't contain animation file." + 0x000A + sError);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("The current document doesn't contain animation file." + 0x000A + "Load " + FlyFilePath + @"\AnimationSample.aga for sample.");
                    }
                    return;
                }

                IAnimationTrack track;
                IAnimationTrack trackLayer;
                IAnimationTrack trackGlobe = null;
                IAnimationType animType;
                IAnimationType animLayer;
                IAnimationType animGlobeCam = null;
                IKeyframe kFGlbCam;
                IKeyframe kFGlbLayer;
                int k;
                int[] count = new int[1000];

                //get each track from the scene and store tracks of the same kind in an Array
                for (int i = 0; i <= sceneTracks.TrackCount - 1; i++)
                {
                    track = (IAnimationTrack)sceneTracks.Tracks.get_Element(i);
                    k = i;
                    animType = track.AnimationType;

                    if (animType.CLSID.Value.ToString() == "{7CCBA704-3933-4D7A-8E89-4DFEE88AA937}")
                    {
                        //GlobeLayer
                        trackLayer = new AnimationTrackClass();
                        trackLayer = track;
                        trackLayer.AnimationType = animType;
                        kFGlbLayer = new GlobeLayerKeyframeClass();
                        animLayer = animType;
                        //Store the keyframe count of each track in an array
                        count[i] = trackLayer.KeyframeCount;
                        trackLyrArray.Add(trackLayer);
                    }
                    else if (animType.CLSID.Value.ToString() == "{D4565495-E2F9-4D89-A8A7-D0B69FD7A424}")
                    {
                        //Globe Camera type
                        trackGlobe = new AnimationTrackClass();
                        trackGlobe = track;
                        trackGlobe.AnimationType = animType;
                        kFGlbCam = new GlobeCameraKeyframeClass();
                        animGlobeCam = animType;
                        //Store the keyframe count of each track in an array
                        count[i] = trackGlobe.KeyframeCount;
                        trackGlbArray.Add(trackGlobe);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Animation Type " + animType.Name + " Not Supported. Check if the animation File is Valid!");
                        return;
                    }
                }

                int larger = Greatest(ref count);
                //if nothing gets passed by the argument it takes the max no of keyframes
                if (iteration == 0) iteration = larger;

                IAnimationTrack trackCamera;
                IAnimationType animCam = null;
                IKeyframe kFBkmark;
                double time = 0;
                int keyFrameLayerCount; int keyFrameCameraCount; int keyFrameCount;

                for (int i = 1; i <= cycles; i++) //no of cycles...
                {
                    for (int start = 0; start <= iteration; start++) //no of iterations...
                    {
                        for (int j = 0; j <= trackCamArray.Count - 1; j++)
                        {
                            trackCamera = (IAnimationTrack)trackCamArray.get_Element(j);
                            if (trackCamera != null)
                            {
                                if (time >= trackCamera.BeginTime)
                                {
                                    keyFrameCameraCount = trackGlobe.KeyframeCount;
                                    kFBkmark = trackCamera.get_Keyframe(keyFrameCameraCount - keyFrameCameraCount);
                                    //reset object
                                    animCam.ResetObject(scene, kFBkmark);
                                    //interpolate by using track
                                    trackCamera.InterpolateObjectProperties(scene, time);
                                    keyFrameCameraCount = keyFrameCameraCount - 1;
                                }
                            }
                        }

                        for (k = 0; k <= trackGlbArray.Count - 1; k++)
                        {
                            trackGlobe = (IAnimationTrack)trackGlbArray.get_Element(k);
                            if (trackGlobe != null)
                            {
                                if (time >= trackGlobe.BeginTime)
                                {
                                    keyFrameCount = trackGlobe.KeyframeCount;
                                    kFGlbCam = trackGlobe.get_Keyframe(trackGlobe.KeyframeCount - keyFrameCount);
                                    //reset object
                                    animGlobeCam.ResetObject(scene, kFGlbCam);
                                    //interpolate by using track
                                    trackGlobe.InterpolateObjectProperties(scene, time);
                                    keyFrameCount = keyFrameCount - 1;
                                }
                            }
                        }

                        for (int t = 0; t <= trackLyrArray.Count - 1; t++)
                        {
                            trackLayer = (IAnimationTrack)trackLyrArray.get_Element(t);
                            if (trackLayer != null)
                            {
                                if (time >= trackLayer.BeginTime)
                                {
                                    keyFrameLayerCount = trackLayer.KeyframeCount;
                                    kFGlbLayer = trackLayer.get_Keyframe(trackLayer.KeyframeCount - keyFrameLayerCount);
                                    //interpolate by using track
                                    trackLayer.InterpolateObjectProperties(scene, time);
                                    keyFrameLayerCount = keyFrameLayerCount - 1;
                                }
                            }
                        }

                        //reset interpolation Point
                        time = start / iteration;
                        //refresh the globeviewer(s)
                        globeDisplay.RefreshViewers();
                    }
                }
            }
            catch
            { }
        }
        #endregion
        
        #region //取得最大的帧数
        private int Greatest(ref int[] array)
        {

            //Function to find the largest count of keyframes (in any one of the Animation tracks)
            int max = 0;
            try
            {
                int length = array.Length;
                for (int i = 0; i < length; i++)
                {
                    if (max == 0)
                    {
                        max = array[i];
                    }
                    else if (array[i] > max)
                    {
                        max = array[i];
                    }
                }
            }
            catch { }
            return max;
        }
        #endregion
        #endregion

        #region//按照aga录制文件的方式飞行
        public void InitAnimationByAgaFile(AxGlobeControl _axGlobeControl, string _FlyPathName, string FlyFoldPath, bool _NotFlyByPolyline, int _FlySpeedRating)
        {
            try
            {
                IsFlyByPolyline = _NotFlyByPolyline;
                string FlyFilePath = FlyFoldPath + "\\" + _FlyPathName + ".aga";
                if (!File.Exists(FlyFilePath))    //如果此文件不存在
                {
                    return;
                }
                pAgAnimationUtils = new AGAnimationUtilsClass();
                IBasicScene2 basicScene2 = (IBasicScene2)scene; // Explicit Cast
                pAnimationExtension = basicScene2.AnimationExtension;
                //飞行时长设置
                switch (_FlySpeedRating)
                {
                    case 1:
                        FlyDurationTime = 40;
                        break;
                    case 2:
                        FlyDurationTime = 30;
                        break;
                    case 3:
                        FlyDurationTime = 20;
                        break;
                    case 4:
                        FlyDurationTime = 15;
                        break;
                    case 5:
                        FlyDurationTime = 10;
                        break;
                    default:
                        FlyDurationTime = 20;
                        break;
                }
                pAnimationExtension.AnimationEnvironment.AnimationDuration = FlyDurationTime;

                IAGAnimationContainer pContainer = pAnimationExtension.AnimationTracks.AnimationObjectContainer;
                pAgAnimationUtils.LoadAnimationFile(pContainer, FlyFilePath);//值不在预期的范围内；
                animPlayer = (IAGAnimationPlayer)pAgAnimationUtils;
            }
            catch { }
        }
        #endregion
        
        #region //Play的方式动画飞行
        //按预订的路径飞行
        public bool InitAnimationPlay(AxGlobeControl _axGlobeControl, string _FlyPathName, string FlyFoldPath,
            int _FlySpeedRating,bool _IsFlyLoop,bool _IsFlyLookdown)
        {
            bool IsFoundPolyline = true;    //是否能找到对应的Polyline
            try
            {
                scene = _axGlobeControl.GlobeDisplay.Scene;
                globe = _axGlobeControl.Globe;
                IBasicScene2 basicScene = (IBasicScene2)globe;
                pAgAnimationUtils = new AGAnimationUtilsClass();
                //按路径飞行
                ILayer layer = getLayerByName(scene, ANIMATIONPATH_LayerName);
                if (layer == null)
                {
                    IsFoundPolyline = false;
                    return IsFoundPolyline;
                }
                FlyPathPolyline = getPolylineFromLayer(layer, _FlyPathName);
                if (FlyPathPolyline == null)
                {
                    IsFoundPolyline = false;
                    return IsFoundPolyline;
                }
                //启用三维线
                IZAware flyPathZAware = (IZAware)FlyPathPolyline;
                flyPathZAware.ZAware = true;

                pAnimationExtension = basicScene.AnimationExtension;
                CreateAnimationFromPath(globe, FlyPathPolyline, FlySpeedRating, IsFlyLoop, IsFlyLookdown);

                //配置飞行
                //InitAnimationParameters();

                //IAGAnimationContainer pContainer = pAnimationExtension.AnimationTracks.AnimationObjectContainer;
                //pAgAnimationUtils.LoadAnimationFile(pContainer, FlyFilePath);//值不在预期的范围内；
                animPlayer = (IAGAnimationPlayer)pAgAnimationUtils;                                
            }
            catch
            { }
            return IsFoundPolyline;

        }
        
        //飞行设置
        //速度等级，是否循环，是否俯视
        private void InitAnimationParameters(int _FlySpeedRating, bool _IsFlyLoop,bool _IsLookDown)
        {
            
        }

        //开始动画
        public void PlayAnimation()
        {
            try
            {
                if (pAgAnimationUtils == null)
                {
                    //InitAnimationParameters();
                    if (FlyPathPolyline == null)
                    {
                        MessageBox.Show("请先选择飞行路线按钮再点击播放。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (IsFlyByPolyline)
                    {
                        CreateAnimationFromPath(globe, FlyPathPolyline, FlySpeedRating, IsFlyLoop, IsFlyLookdown);
                    }
                }
                if (animPlayer != null)
                {
                    //if (pAnimationExtension.AnimationEnvironment.PlayTime == 0)
                    //{
                    //    pAnimationExtension.AnimationEnvironment.PlayTime = pAnimationExtension.AnimationEnvironment.AnimationDuration;
                    //}
                    animPlayer.PlayAnimation(pAnimationExtension.AnimationTracks, pAnimationExtension.AnimationEnvironment, null);
                }
            }
            catch { }
        }

        //暂停动画
        public void PauseAnimation()
        {
            try
            {
                if (animPlayer != null)
                {
                    animPlayer.PauseAnimation();
                }
            }
            catch { }
        }

        //停止动画
        public void StopAnimation()
        {
            try
            {
                if (animPlayer != null)
                {
                    animPlayer.StopAnimation();
                }
            }
            catch { }
        }

        #endregion
                
        #region //Polyline飞行并设置飞行环境
        /// <summary>
        ///第三个参数是移动的方式，其中1表示移动观察者，2表示移动目标，其他表示两个都移动
        /// </summary>
        /// <param name="_pScene">_pGlobe</param>
        /// <param name="_pPolyline">_pPolyline</param>
        /// <param name="_pType">_pType</param>
        public void CreateAnimationFromPath(IGlobe _pGlobe, IPolyline _pPolyline, int _FlySpeedRating, bool _IsFlyLoop, bool _IsFlyLookdown)//int _pType,, double _pDuration)
        {
            try
            {
                #region //esri写的
                scene = _pGlobe.GlobeDisplay.Scene;
                // 获取动画扩展对象
                //ESRI.ArcGIS.Analyst3D.IBasicScene2 pBasicScene2 = (ESRI.ArcGIS.Analyst3D.IBasicScene2)_pScene; // Explicit Cast
                //ESRI.ArcGIS.Animation.IAnimationExtension pAnimationExtension = pBasicScene2.AnimationExtension;
                //创建两个对象，一个用于导入路径，一个用于播放
                //ESRI.ArcGIS.Animation.IAGAnimationUtils pAGAnimationUtils = new ESRI.ArcGIS.Animation.AGAnimationUtilsClass();
                pAGImportPathOptions = new ESRI.ArcGIS.Animation.AGImportPathOptionsClass();
                // 设置参数
                //参数设置不正确会出错，尤其是类型，对象等信息！
                pAGImportPathOptions.BasicMap = (ESRI.ArcGIS.Carto.IBasicMap)scene;
                pAGImportPathOptions.AnimationTracks = (ESRI.ArcGIS.Animation.IAGAnimationTracks)scene;
                // pAGImportPathOptions.AnimationType = new ESRI.ArcGIS.GlobeCore.AnimationTypeCameraClass();    //在Globe中不能用这个
                pAGImportPathOptions.AnimationType = new AnimationTypeGlobeCameraClass();
                pAGImportPathOptions.LookaheadFactor = 1;
                pAGImportPathOptions.PutAngleCalculationMethods(esriPathAngleCalculation.esriAngleAddRelative,
                        esriPathAngleCalculation.esriAngleAddRelative,
                        esriPathAngleCalculation.esriAngleAddRelative);
                //pAGImportPathOptions.AnimatedObject = _pScene.SceneGraph.ActiveViewer.Camera; //在Globe中不能用这个
                pAGImportPathOptions.AnimatedObject = _pGlobe.GlobeDisplay.ActiveViewer.Camera;
                pAGImportPathOptions.PathGeometry = _pPolyline;
                //俯视飞行
                if (_IsFlyLoop)
                {
                    pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathObserver;//观察者移动
                }
                else   //正常飞行
                {
                    pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathObsAndTarget;//都移动
                }
                //else
                //{
                //    pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathTarget;
                //}
                pAGImportPathOptions.LookaheadFactor = 1;
                pAGImportPathOptions.RollFactor = 0;
                pAGImportPathOptions.AnimationEnvironment = pAnimationExtension.AnimationEnvironment;

                //
                //InitAnimationParameters();

                //持续时间
                double FlyPathLength = FlyPathPolyline.Length;
                double flyDuration = FlyPathLength *500 /_FlySpeedRating; //飞行路线长度*300*飞行速度等级/中速等级
                FlyDurationTime = flyDuration;  //记录飞行时间
                pAnimationExtension.AnimationEnvironment.AnimationDuration = flyDuration;
                pAnimationExtension.AnimationEnvironment.IsIntervalPlay = false;
                pAnimationExtension.AnimationEnvironment.PlayMode = esriAnimationPlayMode.esriAnimationPlayOnceForward;
                pAnimationExtension.AnimationEnvironment.PlayTime = pAnimationExtension.AnimationEnvironment.AnimationDuration;

                //IAGAnimationEnvironment pAGAeviroment = new AGAnimationEnvironmentClass();
                ESRI.ArcGIS.Animation.IAGAnimationContainer AGAnimationContainer = pAnimationExtension.AnimationTracks.AnimationObjectContainer;

                // 创建飞行路线类似ArcGlobe中的Import，通过ArcGlobe学习ArcGlobe开发！
                pAgAnimationUtils.CreateFlybyFromPath(AGAnimationContainer, pAGImportPathOptions);

                //该接口相当于播放的界面，可以自己做一个界面
                animPlayer = pAgAnimationUtils as IAGAnimationPlayer;
                //animPlayer.PlayAnimation(_pScene as IAGAnimationTracks, pAGAeviroment, null); //开始飞行                
                #endregion
            }
            catch
            { }
            #region //用改变观察者和观察点的方式
            //if (this.m_pScene.AreaOfInterest == null)
            //    return;
            //// Explicit Cast
            //IBasicScene2 basicScene2 = (IBasicScene2)_pGlobe;
            //m_pAnimationExtension = basicScene2.AnimationExtension;

            //m_pAgAnimationUtils = new AGAnimationUtilsClass();
            //IAGImportPathOptions agImportPathOptions = new AGImportPathOptionsClass();

            //agImportPathOptions.AnimationEnvironment = m_pAnimationExtension.AnimationEnvironment;
            //IAGAnimationContainer AGAnimationContainer = m_pAnimationExtension.AnimationTracks.AnimationObjectContainer;

            //if (AGAnimationContainer != null)
            //{
            //    m_pAgAnimationUtils.CreateFlybyFromPath(AGAnimationContainer, agImportPathOptions);
            //}
            #endregion
        }
        
        #endregion

        #region //取得polyline
        private ILayer getLayerByName(IScene _Scene,string layerName)
        {
            ILayer layer = null;
            try
            {
                int layerCount = _Scene.LayerCount;
                for (int c = 0; c < layerCount; c++)
                {
                    ILayer pLayer = _Scene.get_Layer(c);
                    if (pLayer is IFeatureLayer)
                    {
                        if (pLayer.Name == layerName)
                        {
                            layer = pLayer;
                            break;
                        }
                    }
                }
            }
            catch { }
            return layer;
        }

        private IPolyline getPolylineFromLayer(ILayer _Layer, string _PathName)
        {
            IPolyline polyline = null;
            try
            {
                if (_Layer == null)
                {
                    return null;
                }
                IFeatureLayer featureLayer = (IFeatureLayer)_Layer;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                IFeatureCursor featureCursor = featureClass.Search(null, false);

                IFeature feature = featureCursor.NextFeature();
                int FieldIndex = feature.Fields.FindField(ANIMATIONPATH_FieldName);
                string FieldValue = "";
                if (FieldIndex < 0)
                {
                    return null;
                }
                while (feature != null)
                {
                    FieldValue = (string)feature.get_Value(FieldIndex);
                    if (FieldValue == _PathName)
                    {
                        polyline = feature.ShapeCopy as IPolyline;
                        break;
                    }
                    feature = featureCursor.NextFeature();
                }
            }
            catch { }
            return polyline;
        }
        #endregion

    }
}
