using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace GISFunction
{
    public class SceneBookmarkLightZoom
    {
        //根据书签的名称定位

        public void BookLightZoom(string bookLightName, AxGlobeControl axGlobeControl)
        {
            try
            {
                ESRI.ArcGIS.Analyst3D.ISceneBookmarks sceneBookmarks = (ESRI.ArcGIS.Analyst3D.ISceneBookmarks)axGlobeControl.Globe;
                IBookmark3D pBookmark3D = null;
                sceneBookmarks.FindBookmark(bookLightName, out pBookmark3D);
                if (pBookmark3D == null)
                    return;
                //缩放到书签
                //pBookmark3D.Apply(m_pGlobe.GlobeDisplay.ActiveViewer as ISceneViewer, true, 3.0); //无动态效果
                ZoomToBookmark(axGlobeControl.Globe.GlobeDisplay, pBookmark3D, 3); //有动态效果
            }
            catch { }
        }
        public static void ZoomToBookmark(IGlobeDisplay globeDisplay, IBookmark3D pBookmark3D, double doubleDuration)
        {
            try
            {
                //创建动画轨迹并添加
                IAnimationTrack pAnimationTrack = CreateZoomOverLocationAnimation(globeDisplay, pBookmark3D);

                //播放轨迹
                IGlobe globe = globeDisplay.Globe;
                IAnimationTracks animationTracks = (IAnimationTracks)globe;
                //// Add Track
                //animationTracks.AddTrack(pAnimationTrack);
                // Only enable the track with the parsed name
                for (int i = 0; i <= animationTracks.TrackCount - 1; i++)
                {
                    IAnimationTrack animationTrackTest = (IAnimationTrack)animationTracks.Tracks.get_Element(i);
                    if (animationTrackTest.Name == pAnimationTrack.Name)
                    {
                        animationTrackTest.IsEnabled = true;
                    }
                    else
                    {
                        animationTrackTest.IsEnabled = false;
                    }
                }

                // Play Track
                System.DateTime dateTimeStart = DateTime.Now;
                double doubleElapsed = 0;
                while (doubleElapsed <= doubleDuration)
                {
                    // Animation Viewer
                    animationTracks.ApplyTracks(globe.GlobeDisplay.ActiveViewer, doubleElapsed, doubleDuration);
                    globe.GlobeDisplay.ActiveViewer.Redraw(true);

                    // Get Elapsed Time
                    System.TimeSpan timeSpanElapsed = DateTime.Now.Subtract(dateTimeStart);
                    doubleElapsed = timeSpanElapsed.TotalSeconds;
                }

                // Display Last Frame (if slow viewer)
                animationTracks.ApplyTracks(globe.GlobeDisplay.ActiveViewer, 1d, 1d);

                #region 根据当前摄像机姿态决定应该为Global模式还是Local模式
                AutoSwitchGlobeCameraOrientationMode(globe);
                #endregion

                globe.GlobeDisplay.ActiveViewer.Redraw(true);

                // Remove Track
                animationTracks.RemoveTrack(pAnimationTrack);
            }
            catch { }
        }
        public static void AutoSwitchGlobeCameraOrientationMode(IGlobe pGlobe)
        {
            try
            {
                ESRI.ArcGIS.Analyst3D.ISceneViewer sceneViewer = pGlobe.GlobeDisplay.ActiveViewer;
                ESRI.ArcGIS.Analyst3D.ICamera camera = sceneViewer.Camera;
                ESRI.ArcGIS.GlobeCore.IGlobeCamera globeCamera = (ESRI.ArcGIS.GlobeCore.IGlobeCamera)camera;
                double xTarget;
                double yTarget;
                ESRI.ArcGIS.Geometry.IPoint targetCls = (globeCamera as ICamera).Target;
                targetCls.QueryCoords(out xTarget, out yTarget);
                double zTarget = targetCls.Z;
                // Calculate the current azimuth and inclination of the camera.
                //azimuth = Math.Atan2(xTarget, yTarget) * 180 / Math.PI;
                double inclination = (180 / Math.PI) * (Math.Asin(zTarget / Math.Sqrt(xTarget * xTarget + yTarget * yTarget + zTarget * zTarget))) - 10.0;
                if (inclination > 88 | inclination < -88 | double.IsNaN(inclination))
                {
                    double targetLatitude;
                    double targetLongitude;
                    double targetAltitude;
                    globeCamera.GetTargetLatLonAlt(out targetLatitude, out targetLongitude, out targetAltitude);
                    double observerLatitude;
                    double obsLongitude;
                    double obsAltitude;
                    globeCamera.GetObserverLatLonAlt(out observerLatitude, out obsLongitude, out obsAltitude);
                    //// Set the GlobeCamera to global navigation mode.
                    globeCamera.OrientationMode = ESRI.ArcGIS.GlobeCore.esriGlobeCameraOrientationMode.esriGlobeCameraOrientationGlobal;
                    globeCamera.NavigationType = ESRI.ArcGIS.GlobeCore.esriGlobeNavigationType.esriGlobeNavigationAttached;
                    globeCamera.SetObserverLatLonAlt(targetLatitude, targetLongitude, obsAltitude);
                }
                else
                {
                    globeCamera.OrientationMode = ESRI.ArcGIS.GlobeCore.esriGlobeCameraOrientationMode.esriGlobeCameraOrientationLocal;
                    globeCamera.NavigationType = ESRI.ArcGIS.GlobeCore.esriGlobeNavigationType.esriGlobeNavigationFree;
                }
            }
            catch { }
        }
        /// <summary>
        /// 根据传入的书签创建动画.
        /// </summary>
        /// <param name="globe">ESRI IGlobe Interface</param>
        /// <param name="pBookmark3D">当前书签位置.</param>
        /// <returns>A new ArcGlobe Animation Track.</returns>
        [ComVisibleAttribute(false)]
        public static IAnimationTrack CreateZoomOverLocationAnimation(IGlobeDisplay globeDisplay, IBookmark3D pBookmark3D)
        {
            // Set Mouse Cursor
            //IMouseCursor mouseCursor = new MouseCursorClass();
            //mouseCursor.SetCursor(2);

            // QI to GlobeDisplayRendering Interface
            IGlobeDisplayRendering globeDisplayRendering = (IGlobeDisplayRendering)globeDisplay;

            // Get Elevation Multiplication Factor
            IUnitConverter unitConverter = new UnitConverterClass();

            // Get GlobeDisplay and Camera
            IGlobe globe = globeDisplay.Globe;
            IAnimationTracks animationTracks = (IAnimationTracks)globe;
            IGlobeCamera globeCamera = (IGlobeCamera)globeDisplay.ActiveViewer.Camera;

            // Create New Animation Track
            IAnimationTrack animationTrack = new AnimationTrackClass();
            try
            {
                IAnimationType animationType = new AnimationTypeGlobeCameraClass();
                animationTrack.AnimationType = animationType;

                // Create First KeyFrame At Current Location
                IKeyframe keyframe1 = new GlobeCameraKeyframeClass();
                keyframe1.CaptureProperties((IScene)globe, globeCamera);
                animationTrack.InsertKeyframe(keyframe1, animationTrack.KeyframeCount);


                //// Create Last KeyFrame Over Desired Location
                //pBookmark3D.Apply(globeDisplay.ActiveViewer as ISceneViewer, false, 0);


                //IKeyframe keyframe3 = new GlobeCameraKeyframeClass();
                //keyframe3.CaptureProperties((IScene)globe, globeCamera);

                IKeyframe keyframe3 = CreateKeyframefromBook(globeDisplay.Globe, pBookmark3D) as IKeyframe;

                #region 获取第一帧和第三帧的参数
                double dX1_Tar, dY1_Tar, dZ1_Tar, dX1_Obs, dY1_Obs, dZ1_Obs;
                GetParametersFromKeyFrame(keyframe1, out dX1_Tar, out dY1_Tar, out dZ1_Tar, out dX1_Obs, out dY1_Obs, out dZ1_Obs);
                double dX3_Tar, dY3_Tar, dZ3_Tar, dX3_Obs, dY3_Obs, dZ3_Obs;
                GetParametersFromKeyFrame(keyframe3, out dX3_Tar, out dY3_Tar, out dZ3_Tar, out dX3_Obs, out dY3_Obs, out dZ3_Obs);
                #endregion

                //=========================================== 创建中间帧 ===========================================
                //IKeyframe keyframe2 = CreateMiddleKeyframe(globeDisplay.Globe, dX1_Tar, dY1_Tar, dZ1_Tar, dX1_Obs, dY1_Obs, dZ1_Obs,
                //    dX3_Tar, dY3_Tar, dZ3_Tar, dX3_Obs, dY3_Obs, dZ3_Obs);          //头晕、高血压、糖尿病患者调用该函数请慎重！
                //=========================================== 创建中间帧 ===========================================
                //animationTrack.InsertKeyframe(keyframe2, animationTrack.KeyframeCount);

                animationTrack.InsertKeyframe(keyframe3, animationTrack.KeyframeCount);

                // Set The Animation Track Name
                animationTrack.Name = "Zoom Over Location From Bookmark";

                // Set Track Attachments
                animationTrack.AttachObject(globeCamera);
                animationTrack.ApplyToAllViewers = true;

                // Add The New Track To The Scene
                animationTracks.AddTrack(animationTrack);

                // Return The Newly Create Aninmation Track
            }
            catch { }
            return animationTrack;
        }
        /// <summary>
        /// 从关键桢中获取观察点和目标点参数
        /// </summary>
        /// <param name="pKeyframe">关键桢</param>
        /// <param name="dX_Tar">目标点X</param>
        /// <param name="dY_Tar">目标点Y</param>
        /// <param name="dZ_Tar">目标点Z</param>
        /// <param name="dX_Obs">观察点X</param>
        /// <param name="dY_Obs">观察点Y</param>
        /// <param name="dZ_Obs">观察点Z</param>
        private static void GetParametersFromKeyFrame(IKeyframe pKeyframe, out double dX_Tar, out double dY_Tar, out double dZ_Tar,
                out double dX_Obs, out double dY_Obs, out double dZ_Obs)
        {
            dY_Tar = pKeyframe.get_PropertyValueDouble(1);
            dX_Tar = pKeyframe.get_PropertyValueDouble(2);
            dZ_Tar = pKeyframe.get_PropertyValueDouble(3);
            dY_Obs = pKeyframe.get_PropertyValueDouble(4);
            dX_Obs = pKeyframe.get_PropertyValueDouble(5);
            dZ_Obs = pKeyframe.get_PropertyValueDouble(6);

            return;
        }

        /// <summary>
        /// 内插中间贞函数，只需给出第一帧和最后一帧的状态信息，即可内插出中间帧状态
        /// </summary>
        /// <param name="pGlobe"></param>
        /// <param name="doubleX1_Tar">第一帧目标点X坐标</param>
        /// <param name="doubleY1_Tar">第一帧目标点Y坐标</param>
        /// <param name="doubleZ1_Tar">第一帧目标点Z坐标</param>
        /// <param name="doubleX1_Obs">第一帧观察点X坐标</param>
        /// <param name="doubleY1_Obs">第一帧观察点Y坐标</param>
        /// <param name="doubleZ1_Obs">第一帧观察点Z坐标</param>
        /// <param name="doubleX2_Tar">第二帧目标点X坐标</param>
        /// <param name="doubleY2_Tar">第二帧目标点Y坐标</param>
        /// <param name="doubleZ2_Tar">第二帧目标点Z坐标</param>
        /// <param name="doubleX2_Obs">第二帧观察点X坐标</param>
        /// <param name="doubleY2_Obs">第二帧观察点Y坐标</param>
        /// <param name="doubleZ2_Obs">第二帧观察点Z坐标</param>
        /// <returns></returns>
        private static IKeyframe CreateMiddleKeyframe(IGlobe pGlobe, double doubleX1_Tar, double doubleY1_Tar, double doubleZ1_Tar,
                                                                    double doubleX1_Obs, double doubleY1_Obs, double doubleZ1_Obs,
                                                                    double doubleX2_Tar, double doubleY2_Tar, double doubleZ2_Tar,
                                                                    double doubleX2_Obs, double doubleY2_Obs, double doubleZ2_Obs)
        {
            IUnitConverter unitConverter = new UnitConverterClass();
            IGlobeDisplayRendering globeDisplayRendering = (IGlobeDisplayRendering)pGlobe.GlobeDisplay;
            IKeyframe pMidKeyframe = new GlobeCameraKeyframeClass();
            try
            {
                pMidKeyframe.set_PropertyValueInt(0, 0);
                pMidKeyframe.set_PropertyValueDouble(1, (doubleY1_Tar + doubleY2_Tar) / 2);//doubleY_mid);
                pMidKeyframe.set_PropertyValueDouble(2, (doubleX1_Tar + doubleX2_Tar) / 2);//doubleX_mid);
                pMidKeyframe.set_PropertyValueDouble(3, (doubleZ1_Tar + doubleZ2_Tar) / 2); //doubleZ1_Tar/2);//// (globeDisplayRendering.GlobeRadius / -1000));
                pMidKeyframe.set_PropertyValueDouble(4, (doubleY1_Obs + doubleY2_Obs) / 2);//doubleY_mid);
                pMidKeyframe.set_PropertyValueDouble(5, (doubleX1_Obs + doubleX2_Obs) / 2);//doubleX_mid);
                //改善中间过程用户体验，增加中间点Observer的高度，让高度约等于两点间距离+海拔高度
                double doubleAltitide_mid = ReturnProjectDistance(pGlobe, doubleX1_Obs, doubleY1_Obs, doubleZ1_Obs, doubleX2_Obs, doubleY2_Obs, doubleZ2_Obs) / 1000; //KM
                //加入地表高程
                doubleAltitide_mid += GetGlobeElevation(pGlobe.GlobeDisplay, (doubleX1_Obs + doubleX2_Obs) / 2, (doubleY1_Obs + doubleY2_Obs) / 2, true);

                pMidKeyframe.set_PropertyValueDouble(6, doubleAltitide_mid);
                pMidKeyframe.set_PropertyValueDouble(7, 30);
                pMidKeyframe.set_PropertyValueDouble(8, 0);
            }
            catch
            { }
            return pMidKeyframe;
        }

        /// <summary>
        /// 书签创建关键帧
        /// </summary>
        /// <param name="_pGlobe"></param>
        /// <param name="_pBook3D"></param>
        /// <returns></returns>
        public static IAGKeyframe CreateKeyframefromBook(IGlobe _pGlobe, IBookmark3D _pBook3D)
        {

            IScene _pScene = _pGlobe.GlobeDisplay.Scene;

            IAGAnimationContainer pAGAnimationContainer = _pScene as IAGAnimationContainer;
            IAGAnimationTracks pAGAnimationTracks = _pGlobe as IAGAnimationTracks;
            IAGAnimationUtils pAGAutils = new AGAnimationUtilsClass();
            ESRI.ArcGIS.Animation.IAGAnimationType pAGType = new AnimationTypeGlobeCameraClass();
            IAGKeyframe pGlobeKey = new GlobeCameraKeyframeClass();
            pAGAutils.KeyframeFromBookmark(pAGAnimationContainer, _pBook3D as ISpatialBookmark, out pGlobeKey);

            return pGlobeKey;
        }

        public static double ReturnProjectDistance(IGlobe pGlobe, double dFromX, double dFromY, double dFromZ, double dToX, double dToY, double dToZ)
        {
            IPoint pPnt1 = new Point();
            IPoint pPnt2 = new Point();
            pPnt1.X = dFromX;
            pPnt1.Y = dFromY;
            MakeZAware(pPnt1);
            pPnt1.Z = dFromZ * 1000;
            pPnt1.SpatialReference = pGlobe.GlobeDisplay.Scene.SpatialReference;
            if (pPnt1.SpatialReference is IGeographicCoordinateSystem)
            {
                pPnt1 = ProjectGeometry(pPnt1) as IPoint;
            }

            pPnt2.X = dToX;
            pPnt2.Y = dToY;
            MakeZAware(pPnt2);
            pPnt2.Z = dToZ * 1000;
            pPnt2.SpatialReference = pGlobe.GlobeDisplay.Scene.SpatialReference;
            if (pPnt2.SpatialReference is IGeographicCoordinateSystem)
            {
                pPnt2 = ProjectGeometry(pPnt2) as IPoint;
            }

            IProximityOperator3D pProximityOperator3D = pPnt1 as IProximityOperator3D;
            return pProximityOperator3D.ReturnDistance3D(pPnt2);
        }
        public static double GetGlobeElevation(IGlobeDisplay globeDisplay, double longitude, double latitude, bool maxResolution)
        {
            IUnitConverter unitConverter = new UnitConverterClass();

            double doubleZ = 0;
            globeDisplay.GetSurfaceElevation(longitude, latitude, maxResolution, out doubleZ);
            return unitConverter.ConvertUnits(doubleZ, esriUnits.esriMeters, globeDisplay.Globe.GlobeUnits);
        }

        public static void MakeZAware(IGeometry geometry)
        {
            IZAware zAware = geometry as IZAware;
            zAware.ZAware = true;
        }
        
        public static IGeometry ProjectGeometry(IGeometry pGeo)
        {
            //如果是地理坐标系，则投影到投影坐标系
            if (pGeo.SpatialReference is IGeographicCoordinateSystem)
            {
                ISpatialReferenceFactory srFactory = new SpatialReferenceEnvironment();
                IProjectedCoordinateSystem pcs = srFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_World_Mercator);      //投影到 Mercator 坐标系
                pGeo.Project(pcs);
            }
            return pGeo;
        }


    }
}
