using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

using DevComponents.DotNetBar;
using CommonBaseTool;

namespace GISInfoShow
{
    public class ShowInfoOnMap
    {

        //创建一个文本元素
        private ITextElement myTextElement = null;
        private IGraphicsLayer graphicsLayer = null;
        private IGraphicsContainer3D graphicsContain3D = null;

        public void ShowCoordinatesOnMap(AxGlobeControl axGlobeControl)
        {
            try
            {
                ITextSymbol pTextSymbol = new TextSymbolClass();
                //pTextSymbol.Font = new Font("Consolas", 10, FontStyle.Regular);    //设置字体
                pTextSymbol.Size = 12;          //字体大小
                pTextSymbol.Color = BaseGISTools.TransColorToAEColor(Color.White);    //字体颜色


                myTextElement = new TextElementClass(); ;
                myTextElement.Symbol = pTextSymbol; //设置样式
                myTextElement.Text = "这是现实的信息 \n 这是第二行 \n 这是第三行";


                graphicsLayer = axGlobeControl.GlobeDisplay.Scene.BasicGraphicsLayer;
                graphicsContain3D = (IGraphicsContainer3D)graphicsLayer;
                graphicsContain3D.AddElement(myTextElement as IElement);
                axGlobeControl.GlobeDisplay.RefreshViewers();
            }
            catch { }
        }


        public void getGlobeCoodinates(AxGlobeControl axGlobeControl, int x, int y, out double X, out double Y, out double Z)
        {
            //string coordInfo = "坐标信息";
            try
            {
                //获取点击坐标的X、Y
                IPoint globePoint = new PointClass();
                IGlobeDisplay globeDisplay = axGlobeControl.GlobeDisplay;
                ISceneViewer sceneViewer = globeDisplay.ActiveViewer;
                System.Object owner = System.Type.Missing;
                System.Object object1 = System.Type.Missing;
                globeDisplay.Locate(sceneViewer, x, y, false, false, out globePoint, out owner, out object1);

                //coordInfo = globePoint.X.ToString("F8") + "," + globePoint.Y.ToString("F8") + "," + globePoint.Z.ToString("F4");
                X = globePoint.X;
                Y = globePoint.Y;
                Z = globePoint.Z;
                //return coordInfo;
            }
            catch
            {
                X = 0;
                Y = 0;
                Z = 0;
                //return coordInfo;
            }
        }

        #region //点击鹰眼，缩放到相应的位置
        public void ZoomToPoint(AxGlobeControl axGlobeControl, double MapX, double MapY)
        {
            try
            {
                IPoint point = new PointClass();
                point.PutCoords(MapX, MapY);    //设置点击的点坐标
                //point = CommonBaseTool.BaseGISTools.getGeoPoint(MapX, MapY);        //转换坐标

                IPoint ptObserver = new PointClass();
                IPoint ptTarget = new PointClass();

                //Observer点
                IClone pClone1 = point as IClone;
                ptObserver = pClone1.Clone() as IPoint;
                ptObserver.Y = ptObserver.Y - 0.08;
                ptObserver.Z = 15.45;
                //Target点
                IClone pClone2 = point as IClone;
                ptTarget = pClone2.Clone() as IPoint;
                ptTarget.Z = -0.01;

                axGlobeControl.GlobeCamera.SetObserverLatLonAlt(ptObserver.Y, ptObserver.X, ptObserver.Z);
                axGlobeControl.GlobeCamera.SetTargetLatLonAlt(ptTarget.Y, ptTarget.X, ptTarget.Z);
                //刷新
                axGlobeControl.GlobeDisplay.RefreshViewers();
            }
            catch { }
        }
        #endregion

    }
}
