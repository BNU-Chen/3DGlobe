using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Analyst3D;

namespace CommonBaseTool
{
    public class BaseGISTools
    {

        public static ESRI.ArcGIS.Display.IColor TransColorToAEColor(Color color)
        {
            IRgbColor rgb = new ESRI.ArcGIS.Display.RgbColorClass();
            rgb.RGB = color.R + color.G * 256 + color.B * 65536;
            return rgb as IColor;
        }

        public static double UnitSacleToMeter(esriUnits unit)
        {
            switch (unit)
            {
                case esriUnits.esriKilometers:
                    return 1000;
                case esriUnits.esriMeters:
                    return 1;
                default:
                    return -1;
            }
        }

        #region     //坐标系转换    getProjectPoint()   getGeoPoint()
        /// <summary>
        /// 坐标系转换-----WGS84转投影坐标系 
        /// </summary>
        /// <param name="point">转换前的IPoint</param>
        /// <returns>转换后的IPoint</returns>
        public static IPoint getProjectPoint(IPoint point)
        {
            try
            {
                ISpatialReferenceFactory pSRF = new SpatialReferenceEnvironmentClass();
                point.SpatialReference = pSRF.CreateGeographicCoordinateSystem((int)(esriSRGeoCSType.esriSRGeoCS_WGS1984));
                point.Project(pSRF.CreateProjectedCoordinateSystem((int)(esriSRProjCSType.esriSRProjCS_WGS1984UTM_31N)));
            }
            catch //(Exception e)
            {
                // MessageBox.Show(e.ToString());
            }
            return point;
        }

        /// <summary>
        /// 坐标系转换-----投影坐标系转WGS84
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns>转换后的IPoint</returns>
        public static IPoint getGeoPoint(double x, double y)
        {
            IPoint pProjPoint = new PointClass();
            pProjPoint.PutCoords(x, y);
            ISpatialReferenceFactory pSRF = new SpatialReferenceEnvironmentClass();
            pProjPoint.SpatialReference = pSRF.CreateProjectedCoordinateSystem((int)(esriSRProjCSType.esriSRProjCS_WGS1984UTM_31N));
            pProjPoint.Project(pSRF.CreateGeographicCoordinateSystem((int)(esriSRGeoCSType.esriSRGeoCS_WGS1984)));
            return pProjPoint;      //此时为经纬度点
        }
        #endregion

        /// <summary>
        /// 取得当前所有图层的list
        /// </summary>
        /// <param name="axGlobeControl">AxGlobeControl</param>
        /// <returns>List<LayerInfo></returns>
        public static List<LayerInfo> getLayerNameList(AxGlobeControl axGlobeControl)
        {
            List<LayerInfo> LayerInfoList = new List<LayerInfo>();
            //判断图层数  
            if (axGlobeControl.GlobeDisplay.Scene.LayerCount == 0)
            {
                return LayerInfoList;
            }
            try
            {

                ILayer layer = null;
                LayerInfo pLayerInfo;
                for (int index = 0; index < axGlobeControl.GlobeDisplay.Scene.LayerCount; index++)
                {
                    layer = axGlobeControl.GlobeDisplay.Scene.get_Layer(index);  //取得图层

                    pLayerInfo = new LayerInfo();   //实例化一个图层信息类
                    pLayerInfo.LayerName = "btn_" + layer.Name;
                    pLayerInfo.LayerIndex = index;
                    pLayerInfo.isVisible = layer.Visible;

                    LayerInfoList.Add(pLayerInfo);  //添加到list                
                }

                return LayerInfoList;
            }
            catch
            {
                return LayerInfoList;
            }
        }


        public static void ShowLayerOrNot(AxGlobeControl axGlobeControl, List<LayerInfo> LayerNameList, string layerName, bool isVisible)
        {
            try
            {
                LayerInfo layerInfo = LayerNameList.Find(delegate(LayerInfo li) { return li.LayerName == layerName; }); //利用委托查找

                ILayer layer = axGlobeControl.GlobeDisplay.Scene.get_Layer(layerInfo.LayerIndex);//getLayerByName(axGlobeControl, layerName);  //有名称取得这个图层
                if (layer == null)  //如果没有找到图层
                {
                    return;
                }
                layer.Visible = isVisible;  //设置图层可见性
                axGlobeControl.GlobeDisplay.RefreshViewers();   //刷新
            }
            catch { }
        }

        //private static bool SelectedLayer(LayerInfo layerInfo)
        //{
        //    //if(layerInfo.LayerName ="
        //}




        private static ILayer getLayerByName(AxGlobeControl axGlobeControl, string LayerName)
        {
            ILayer layer = null;
            for (int index = 0; index < axGlobeControl.GlobeDisplay.Scene.LayerCount; index++)
            {
                layer = axGlobeControl.GlobeDisplay.Scene.get_Layer(index);
                if (layer.Name == LayerName)
                {
                    return layer;
                }
            }

            return layer;
        }

        private static ILayer funReturnLayerByLayerName(IMapControl3 mainMap, string strLayerName)
        {
            ILayer pLayer = null;
            ILayer pL = null;
            for (int i = 0; i < mainMap.LayerCount; i++)
            {
                pL = mainMap.get_Layer(i);
                if (pL is IGroupLayer)
                {
                    ICompositeLayer pGL = pL as ICompositeLayer;
                    for (int j = 0; j < pGL.Count; j++)
                    {
                        if (pGL.get_Layer(j).Name == strLayerName)
                        {
                            pLayer = pGL.get_Layer(j);
                        }
                    }
                }
                if (pL.Name == strLayerName)
                {
                    pLayer = pL;
                }
            }
            return pLayer;
        }


    }
    public struct LayerInfo
    {
        public string LayerName;    //图层名称
        public int LayerIndex;      //图层索引
        public bool isVisible;      //是否显示
    }
}
