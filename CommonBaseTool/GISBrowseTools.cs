using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;

namespace CommonBaseTool
{

    public class GISBrowseTools
    {
        
        /// <summary>
        /// 添加数据
        /// </summary>

        //添加图层
        public static void AddLayerToGlobe(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsAddDataCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }

        //绕一点旋转查看
        public static void FixedLineOfSight(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeFixedLineOfSightToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //逐渐放大
        public static void FixedZoomIn(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeFixedZoomInCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }

        //逐级缩小
        public static void FixedZoomOut(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeFixedZoomOutCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }


        //飞行工具
        public static void FlyTool(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeFlyToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //全图
        public static void FullExtentGlobe(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeFullExtentCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }

        //信息查询
        public static void Identify(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeIdentifyToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //左右看
        public static void LookAround(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeLookAroundToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //向北看
        public static void LookNorth(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeNorthCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }


        //测距
        public static void MeasureInGlobe(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeMeasureToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //浏览工具
        public static void Navigate(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeNavigateToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //俯视浏览工具
        public static void NavigationMode(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeNavigationModeCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }

        //平移工具
        public static void PanGlobe(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobePanToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }

        //快速缩放
        public static void ZoomInZoomOut(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsGlobeZoomInOutToolClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            _axGlobeControl.CurrentTool = (ITool)pCommand;
        }


        //保存地图
        public static void SaveAsDoc(AxGlobeControl _axGlobeControl)
        {
            ICommand pCommand;
            pCommand = new ControlsSaveAsDocCommandClass();
            pCommand.OnCreate(_axGlobeControl.Object);
            pCommand.OnClick();
        }


    }
}
