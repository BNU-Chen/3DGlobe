
using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GlobeCore;

using System.Runtime.InteropServices;

namespace GISFunction
{
    public class ZoomToPointWithAnimation
    {
        #region //点击鹰眼，缩放到相应的位置
        public static void ZoomToPointAnimation(AxGlobeControl axGlobeControl, double MapX, double MapY)
        {
            try
            {
                IPoint point = new PointClass();
                point.PutCoords(MapX, MapY);    //设置点击的点坐标

                //动画缩放到
                IAnimationTrack animaTrack = CreateZoomOverLocationAnimation(axGlobeControl.GlobeDisplay, MapX, MapY, 1000);
                animaTrack.IsEnabled = true;

                IAnimationTracks pTracks = axGlobeControl.Globe as IAnimationTracks;
                //for (int i = 0; i < pTracks.TrackCount; i++)
                //{
                //    IAnimationTrack pTrack = pTracks.Tracks.get_Element(i) as
                //    IAnimationTrack;
                //    pTrack.IsEnabled = true;//设置为true 才可以播放这条轨迹
                //}
                DateTime startTime = DateTime.Now;
                TimeSpan timeSpan;
                double elapsedTime;
                double duration = 7;
                bool play = true;
                do
                {
                    timeSpan = (DateTime.Now).Subtract(startTime);
                    elapsedTime = timeSpan.TotalSeconds;
                    if (elapsedTime > duration)
                    {
                        play = false;
                        elapsedTime = duration;
                    }
                    pTracks.ApplyTracks(axGlobeControl.Globe.GlobeDisplay.ActiveViewer, elapsedTime, duration);
                    axGlobeControl.Globe.GlobeDisplay.RefreshViewers();
                } while (play);


                #region //无动画方式
                //point = CommonBaseTool.BaseGISTools.getGeoPoint(MapX, MapY);        //转换坐标

                //IPoint ptObserver = new PointClass();
                //IPoint ptTarget = new PointClass();

                ////Observer点
                //IClone pClone1 = point as IClone;
                //ptObserver = pClone1.Clone() as IPoint;
                //ptObserver.Y = ptObserver.Y - 0.08;
                //ptObserver.Z = 15.45;
                ////Target点
                //IClone pClone2 = point as IClone;
                //ptTarget = pClone2.Clone() as IPoint;
                //ptTarget.Z = -0.01;

                //axGlobeControl.GlobeCamera.SetObserverLatLonAlt(ptObserver.Y, ptObserver.X, ptObserver.Z);
                //axGlobeControl.GlobeCamera.SetTargetLatLonAlt(ptTarget.Y, ptTarget.X, ptTarget.Z);
                #endregion

                //刷新
                axGlobeControl.GlobeDisplay.RefreshViewers();
            }
            catch { }
        }
        #endregion

        #region Create an ZoomTo ArcGlobe Animation Track
        /// <summary>
        /// Create an ZoomTo ArcGlobe Animation Track.
        /// </summary>
        /// <param name="globe">ESRI IGlobe Interface</param>
        /// <param name="doubleX">Longitude of the location to zoom to.</param>
        /// <param name="doubleY">Latitude of the location to zoom to.</param>
        /// <param name="doubleAltitide">Height of the location to zoom to.</param>
        /// <returns>A new ArcGlobe Animation Track.</returns>
        [ComVisibleAttribute(false)]
        public static IAnimationTrack CreateZoomOverLocationAnimation(IGlobeDisplay globeDisplay, double doubleX, double doubleY, double doubleAltitide)
        {
            // Set Mouse Cursor
            //IMouseCursor mouseCursor = new MouseCursorClass();
            //mouseCursor.SetCursor(2);
            // QI to GlobeDisplayRendering Interface
            IGlobeDisplayRendering globeDisplayRendering = (IGlobeDisplayRendering)
                    globeDisplay;
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
                // Create Last KeyFrame Over Desired Location
                IKeyframe keyframe3 = new GlobeCameraKeyframeClass();
                keyframe3.set_PropertyValueInt(0, 0);
                keyframe3.set_PropertyValueDouble(1, doubleY);
                keyframe3.set_PropertyValueDouble(2, doubleX);
                keyframe3.set_PropertyValueDouble(3, -1d *
                                                   unitConverter.ConvertUnits(globeDisplayRendering.GlobeRadius, esriUnits.esriMeters,
                                                           globe.GlobeUnits)); // (globeDisplayRendering.GlobeRadius / -1000));
                keyframe3.set_PropertyValueDouble(4, doubleY);
                keyframe3.set_PropertyValueDouble(5, doubleX);
                keyframe3.set_PropertyValueDouble(6, doubleAltitide);
                keyframe3.set_PropertyValueDouble(7, 30);
                keyframe3.set_PropertyValueDouble(8, 0);

                #region 获取第一帧和第三帧的参数
                double dX1_Tar, dY1_Tar, dZ1_Tar, dX1_Obs, dY1_Obs, dZ1_Obs;
                GetParametersFromKeyFrame(keyframe1, out dX1_Tar, out dY1_Tar, out dZ1_Tar, out dX1_Obs, out dY1_Obs, out dZ1_Obs);
                double dX3_Tar, dY3_Tar, dZ3_Tar, dX3_Obs, dY3_Obs, dZ3_Obs;
                GetParametersFromKeyFrame(keyframe3, out dX3_Tar, out dY3_Tar, out dZ3_Tar, out dX3_Obs, out dY3_Obs, out dZ3_Obs);
                #endregion

                ////=========================================== 创建中间帧    == == == == == == == == == == == == == == == == == == == == == =
                IKeyframe keyframe2 = CreateMiddleKeyframe(globeDisplay.Globe, dX1_Tar,
                                      dY1_Tar, dZ1_Tar, dX1_Obs, dY1_Obs, dZ1_Obs,
                                      dX3_Tar, dY3_Tar, dZ3_Tar, dX3_Obs, dY3_Obs, dZ3_Obs);  //头晕、高血压、糖尿病患者调用该函数请慎重！
                //=========================================== 创建中间帧    == == == == == == == == == == == == == == == == == == == == == =
                //animationTrack.InsertKeyframe(keyframe2, animationTrack.KeyframeCount);
                animationTrack.InsertKeyframe(keyframe3, animationTrack.KeyframeCount);
                // Set The Animation Track Name
                animationTrack.Name = "Zoom Over Location";
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
        #endregion

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
            catch { }
            return pMidKeyframe;
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
