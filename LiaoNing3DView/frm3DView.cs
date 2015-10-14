using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Diagnostics;
using DevComponents.DotNetBar;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

using GISInfoShow;
using GISFunction;
using CommonBaseTool;
using FlyAccordingPath;

namespace LiaoNing3DView
{
    public partial class frmMain3DView : Form
    {

        #region //变量声明
        public string AppStartPath = @"E:\项目 - 2014 沈阳经济区\data\图集\三维地图\\";    //程序启动路径
        public string MxdGlobeEyePath = "";     //鹰眼地图文件路径
        public string GlobeMapPath = "";        //三维地图路径
        public const string Globe3ddMapName = "沈阳经济区三维展示.3dd";  //三维地图的路径
        public const string MapMxdGlobeEyeName = "GlobeEye.mxd";    //三维鹰眼地图的名称
        public string DataFoldPath = "";        //系统默认数据路径

        //private tspControl ts = new tspControl();//new System.Windows.Forms.Label();
        
        //界面是否显示
        private bool isShowCoordinates = true;          //是否显示位置信息
        
        //飞行功能
        private List<string> FlyPathNameList;
        private string FlyPathFoldPath = "";    //飞行文件的文件夹
        private int btnFlyToSubItemDefaultCount = 0;   //飞行按钮子按钮的个数
        private string FlyClickedPathName = ""; //刚才被点击的子按钮名称
        private string FlyBookmarkName = "";    //飞行与场景切换的场景名称

        private FlyAccordingPath.Fly flyAnimationClass; //声明飞行类
        private System.Threading.Thread threadFlyAnimation = null;  //飞行中使用多线程
        //飞行进度条
        private System.Windows.Forms.Timer TimerProgressBar = null;  //时间控件
        private int ProgressBarValue = 1;   //进度值
        private bool IsFlyByPolyline = true;    //是否是按照Polyline方式飞行
 
        //场景书签功能
        private int btnBookmarkSubItemDefaultCount = 0;//   场景书签按钮默认子按钮的个数
        
        //图层管理功能
        private List<CommonBaseTool.LayerInfo> LayerNameList;   //图层名称列表
        private int btnLayerManagerSubDefaultCount = 0;     //默认子按钮个数
        //各命名空间类的实例化
        ShowInfoOnMap showInfoOnMap = new ShowInfoOnMap();
        GISFunction.SceneBookmark pSceneBookmark = null;  //场景书签
        //GISBrowseTools pGISBrowseTool = new GISBrowseTools();    //所有的Globe浏览工具类

        #endregion

        public frmMain3DView()
        {
            InitializeComponent();
            Initialize();
        }
     
        #region //初始化函数
        private void Initialize()
        {
            try
            {

                //AppStartPath = Application.StartupPath;

                //this.panel1.BackColor = Color.FromArgb(0, Color.Transparent);
                //重绘GlobeControl鼠标中键，实现缩放
                this.MouseWheel += new MouseEventHandler(axGlobeControl1_OnMouseWheel);

                //禁用鹰眼窗口滚轮
                this.axMapControl_GlobeEye.AutoMouseWheel = false;
                this.axGlobeControl1.ShowGlobeTips = esriGlobeTipsType.esriGlobeTipsTypeNone;
            }
            catch { }
        }
        #endregion

        #region //窗体加载事件
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //初始化系统路径
                DataFoldPath = AppStartPath;// +"\\Data\\"; //系统数据文件夹路径
                GlobeMapPath = DataFoldPath + Globe3ddMapName;  //定义三维地图路径
                MxdGlobeEyePath = DataFoldPath + MapMxdGlobeEyeName;    //定义鹰眼地图路径

                //加载地图
                if (System.IO.File.Exists(GlobeMapPath))  //如果该路径存在
                {
                    this.axGlobeControl1.Load3dFile(GlobeMapPath);  //加载三维地图
                }
                if (System.IO.File.Exists(MxdGlobeEyePath))  //如果该路径存在
                {
                    this.axMapControl_GlobeEye.LoadMxFile(MxdGlobeEyePath); //加载鹰眼地图
                }

                this.axMapControl_GlobeEye.ShowScrollbars = false;  //隐藏条。

                this.axGlobeControl1.GlobeDisplay.GestureEnabled = true;           //手势启用？
                this.axGlobeControl1.Navigate = true;       //浏览模式
                GISBrowseTools.Navigate(this.axGlobeControl1);      //地图的默认工具为浏览

                //指北针
                this.axGlobeControl1.GlobeViewer.NorthArrowEnabled = !this.axGlobeControl1.GlobeViewer.NorthArrowEnabled;

                btnFlyToSubItemDefaultCount = btnFlyTo.SubItems.Count; //飞行动画按钮默认字按钮个数
                btnBookmarkSubItemDefaultCount = btnBookmark.SubItems.Count;    //场景书签按钮默认子按钮个数
                btnLayerManagerSubDefaultCount = btnLayerManager.SubItems.Count;    //图层管理的子按钮个数

                #region //场景书签按钮
                if (pSceneBookmark == null)     //如果类为空
                {
                    pSceneBookmark = new SceneBookmark(this.axGlobeControl1, this.btnBookmark, btnBookmark_Click);   //实例化书签类，并传入axGlobeControl
                }
                if (this.btnBookmark.SubItems.Count <= btnBookmarkSubItemDefaultCount)    //如果子按钮少于三个，就说明没有加载场景书签
                {
                    pSceneBookmark.LoadBookmarksAsButton(); //加载场景按钮
                }
                #endregion

                #region //沿路径飞行
                FlyPathFoldPath = AppStartPath + "\\Data\\FlyPath";  //飞行文件的文件夹
                if (!Directory.Exists(FlyPathFoldPath))  //如果路径不存在
                {
                    MessageBox.Show("飞行路径文件丢失，请确保程序的完整性", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    //取得飞行路径list
                    if (FlyPathNameList == null)
                    {
                        FlyPathNameList = FlyAccordingPath.Fly.getFileListFromFoldPath(FlyPathFoldPath);
                    }
                    //添加飞行路径子按钮
                    if (this.btnFlyTo.SubItems.Count == btnFlyToSubItemDefaultCount)      //如果按钮个数为默认个数，则说明没有添加
                    {
                        UIControl.AddFlyPathButton(FlyPathNameList, this.btnFlyTo, btnFlyTo_Click); //以飞行路径名，加载按钮
                    }
                }
                #endregion

                #region //图层管理
                //取得图层list
                if (LayerNameList == null)
                {
                    LayerNameList = CommonBaseTool.BaseGISTools.getLayerNameList(this.axGlobeControl1);
                }
                //添加子按钮
                if (this.btnLayerManager.SubItems.Count == btnLayerManagerSubDefaultCount)
                {
                    UIControl.AddLayerNameButons(LayerNameList, this.btnLayerManager, btnLayerManager_Click);   //添加图层子按钮
                }
                #endregion
            }
            catch
            { }
        }
        #endregion

        #region //所有界面按钮点击事件
        private void AllButton_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonItem btn = (ButtonItem)sender;

                switch (btn.Name)
                {
                    //位置信息
                    case "btnCoordinatesInfo":
                        isShowCoordinates = !isShowCoordinates; //取反
                        break;
                    //鹰眼
                    case "btnGlobeEyeShow":
                        //isShowGlobeEyeInfo = !isShowGlobeEyeInfo;   //bool取反
                        this.axMapControl_GlobeEye.LoadMxFile(MxdGlobeEyePath);
                        this.panelEx_GlobeEye.Visible = !this.panelEx_GlobeEye.Visible;
                        //this.btnGlobeEyeShow.Checked = this.panelEx_GlobeEye.Visible;
                        this.axMapControl_GlobeEye.ActiveView.Refresh();    //刷新一下地图
                        break;
                    //显示指北针
                    case "btnShowNorthArrow":
                        this.axGlobeControl1.GlobeViewer.NorthArrowEnabled = !this.axGlobeControl1.GlobeViewer.NorthArrowEnabled;
                        this.axGlobeControl1.GlobeDisplay.RefreshViewers(); //刷新
                        break;

                    #region //各种浏览工具
                    //浏览工具
                    case "btnToolNavigate":
                        GISBrowseTools.Navigate(this.axGlobeControl1);
                        break;
                    //平移工具
                    case "btnToolPan":
                        GISBrowseTools.PanGlobe(this.axGlobeControl1);
                        break;
                    //飞行工具
                    case "btnToolFlyTo":
                        GISBrowseTools.FlyTool(this.axGlobeControl1);
                        break;
                    //全图
                    case "btnToolFullExtent":
                        GISBrowseTools.FullExtentGlobe(this.axGlobeControl1);
                        break;
                    //俯视
                    case "btnToolNavigationMode":
                        //pGISBrowseTool.NavigationMode(this.axGlobeControl1);
                        pSceneBookmark.ZoomToScene("全球视图");
                        break;
                    //保存
                    case "btnToolSaveGlobe":
                        GISBrowseTools.SaveAsDoc(this.axGlobeControl1);
                        break;
                    //向北看
                    case "btnToolLookNorth":
                        GISBrowseTools.LookNorth(this.axGlobeControl1);
                        break;
                    //绕一点旋转查看
                    case "btnToolFixedLineOfSight":
                        GISBrowseTools.FixedLineOfSight(this.axGlobeControl1);
                        break;
                    //快速缩放
                    case "btnToolZoomInOut":
                        GISBrowseTools.ZoomInZoomOut(this.axGlobeControl1);
                        break;
                    //测距
                    case "btnToolMeasure":
                        GISBrowseTools.MeasureInGlobe(this.axGlobeControl1);
                        break;
                    //信息查询
                    case "btnToolIdentify":
                        GISBrowseTools.Identify(this.axGlobeControl1);
                        break;
                    //逐渐放大
                    case "btnToolFixedZoomIn":
                        GISBrowseTools.FixedZoomIn(this.axGlobeControl1);
                        break;
                    //逐级缩小
                    case "btnToolFixedZoomOut":
                        GISBrowseTools.FixedZoomOut(this.axGlobeControl1);
                        break;
                    #endregion

                    #region //图层管理


                    #endregion
                }
            }
            catch { }
        }
        #endregion

        #region //所有文件菜单按钮事件
        private void FileButton_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonItem btn = (ButtonItem)sender;
                switch (btn.Name)
                {
                    //打开三维地图
                    case "btnOpen3DMap":
                        UIControl.Open3DGlobeMap(this.axGlobeControl1, DataFoldPath);
                        break;
                    //保存三维场景影像
                    case "btnPrintMap":
                        // Windows用户桌面路径
                        string DesktopFoldPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        UIControl.SaveSceneAsImage(this.axGlobeControl1, DesktopFoldPath);
                        break;
                    case "btnHelpDoc":
                        UIControl.OpenHelpDoc(DataFoldPath);
                        break;
                    case "btnAboutUs":
                        UIControl.OpenAboutUsFrm();
                        break;
                    case "btnSetRibbonVisible":
                        this.ribbonControl1.Expanded = !this.ribbonControl1.Expanded;
                        if (this.ribbonControl1.Expanded)
                        {
                            this.btnSetRibbonVisible.Image = this.imageList_RibbonVisible.Images[0];
                        }
                        else
                        {
                            this.btnSetRibbonVisible.Image = this.imageList_RibbonVisible.Images[1];
                        }
                        break;
                }
            }
            catch { }
        }
        #endregion

        #region //GlobeControl事件
        //重载鼠标滚轮 axGlobeControl1_OnMouseWheel()
        private void axGlobeControl1_OnMouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                System.Drawing.Point pSceLoc = axGlobeControl1.PointToScreen(axGlobeControl1.Location);
                System.Drawing.Point Pt = this.PointToScreen(e.Location);
                if (Pt.X < pSceLoc.X || Pt.X > pSceLoc.X + axGlobeControl1.Width || Pt.Y < pSceLoc.Y || Pt.Y > pSceLoc.Y + axGlobeControl1.Height)
                {
                    return;
                }

                double scale = 0.2;
                if (e.Delta < 0) scale = -scale;

                IGlobeCamera pGlobeCamera = axGlobeControl1.GlobeCamera;
                ICamera pCamera = pGlobeCamera as ICamera;
                IGlobeDisplay pGlobeDisplay = axGlobeControl1.GlobeDisplay;
                if (pGlobeCamera.OrientationMode == esriGlobeCameraOrientationMode.esriGlobeCameraOrientationGlobal)
                {
                    double zt, xo, yo, zo;
                    pGlobeCamera.GetObserverLatLonAlt(out xo, out yo, out zo);
                    pGlobeDisplay.GetSurfaceElevation(xo, yo, true, out zt);
                    IPoint pObserver = new PointClass();
                    pObserver.PutCoords(xo, yo);
                    double zt1 = zt * (CommonBaseTool.BaseGISTools.UnitSacleToMeter(axGlobeControl1.Globe.GlobeUnits));
                    zo = (zo - zt1) * (1 + scale);
                    pGlobeCamera.SetObserverLatLonAlt(xo, yo, zo);
                }
                else
                {
                    pCamera.ViewingDistance += pCamera.ViewingDistance * scale;
                }
                axGlobeControl1.GlobeDisplay.RefreshViewers();
            }
            catch
            {
            }
        }

        private void axGlobeControl1_OnMouseUp(object sender, IGlobeControlEvents_OnMouseUpEvent e)
        {
            try
            {
                if (isShowCoordinates)
                {
                    double XX = 0;
                    double YY = 0;
                    double ZZ = 0;
                    showInfoOnMap.getGlobeCoodinates(this.axGlobeControl1, this.axGlobeControl1.Width / 2, this.axGlobeControl1.Height / 2, out XX, out YY, out ZZ);
                    this.lblCoordInfoX.Text = "经度：" + XX.ToString();
                    this.lblCoordInfoY.Text = "纬度：" + YY.ToString();
                    this.lblCoordInfoZ.Text = "高度：" + ZZ.ToString();
                    //刷新显示
                    this.lblCoordInfoX.Refresh();
                    this.lblCoordInfoY.Refresh();
                    this.lblCoordInfoZ.Refresh();
                    this.itemContainer_CoordInfo.Refresh();
                    this.ribbonBar_CoordInfo.Refresh();
                }
                else
                {
                    this.lblCoordInfoX.Text = "经度：" + "0";
                    this.lblCoordInfoY.Text = "纬度：" + "0";
                    this.lblCoordInfoZ.Text = "高度：" + "0";
                    //刷新显示
                    this.lblCoordInfoX.Refresh();
                    this.lblCoordInfoY.Refresh();
                    this.lblCoordInfoZ.Refresh();
                    this.itemContainer_CoordInfo.Refresh();
                    this.ribbonBar_CoordInfo.Refresh();
                }
            }
            catch { }
        }
        #endregion

        #region //鹰眼地图控件事件
        private void axMapControl_GlobeEye_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            try
            {
                showInfoOnMap.ZoomToPoint(this.axGlobeControl1, e.mapX, e.mapY);  //点击鹰眼地图，可以zoom到相应位置
                ////动画飞行
                //GISFunction.ZoomToPointWithAnimation.ZoomToPointAnimation(this.axGlobeControl1, e.mapX, e.mapY);
            }
            catch { }
        }
        #endregion

        #region //关闭或者打开ribbon的界面
        private void switchButton_ONOFFRibbon_ValueChanged(object sender, EventArgs e)
        {
           // this.ribbonControl1.Expanded = this.switchButton_ONOFFRibbon.Value;
        }
        #endregion

        #region //场景书签功能
        private void btnBookmark_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonItem bi = (ButtonItem)sender;
                switch (bi.Name)
                {
                    case "btnBookmark":

                        break;
                    case "btnBookmarkNew":
                        pSceneBookmark.AddBookmarkScene();
                        break;
                    case "btnBookmarkManager":

                        break;
                    default:
                        pSceneBookmark.ZoomToScene(bi.Text);
                        break;
                }
            }
            catch { }
        }
        #endregion

        #region //飞行功能
        //飞行路径 点击事件
        private void btnFlyTo_Click(object sender, EventArgs e)
        {
            try
            {
                //判断是否为按钮
                if (!(sender is ButtonItem))
                {
                    return; //如果不是按钮，则返回
                }
                ButtonItem bi = (ButtonItem)sender;
                switch (bi.Name)
                {
                    case "btnFlyPathManager":
                        //打开相应文件夹
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                        psi.Arguments = "/e," + FlyPathFoldPath;
                        System.Diagnostics.Process.Start(psi);
                        break;
                    case "btnFlyAlong6Cities":
                        FlyAlong6Cities();  //沿海经济带六个城市飞行
                        break;
                    default:
                        if (bi.Name == "btnFlyTo")
                        {
                            return;
                        }
                        //将飞行动画控制面板显示
                        this.ribbonTab_FlyAnimation.Visible = true;
                        this.ribbonControl1.SelectedRibbonTabItem = this.ribbonTab_FlyAnimation;
                        this.ribbonControl1.Refresh();

                        //以bookmark的方式缩放到飞行区域
                        string bookmarkName = UIControl.getBookmarkFromCityName(bi.Text);
                        pSceneBookmark.ZoomToScene(bookmarkName);

                        //System.Threading.Thread.Sleep(3000);    //暂停3秒

                        //如果飞行动画的类为空
                        if (flyAnimationClass == null)
                        {
                            flyAnimationClass = new FlyAccordingPath.Fly();
                        }
                        FlyClickedPathName = bi.Text;   //刚才被点击的按钮名称

                        //寻找此按钮是否有对应的Polyline
                        if (flyAnimationClass.InitAnimationPlay(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath,
                    this.ratingItem_FlySpeed.Rating, this.checkBoxItem_Loop.Checked, this.checkBoxItem_LookDown.Checked))
                        {
                            IsFlyByPolyline = true;     //是按照Polyline的方式飞行                            
                            StartFlyPathThread();// 启动多线程飞行方法

                            //进度条
                            ProgressBarValue = 0;
                            this.progressBarItem_FlyAnimation.Value = 0;
                            this.progressBarItem_FlyAnimation.Text = "";
                            this.progressBarItem_FlyAnimation.Refresh();
                        }
                        else
                        {
                            IsFlyByPolyline = false;     //不是按照Polyline的方式飞行
                            flyAnimationClass.InitAnimationByAgaFile(this.axGlobeControl1, bi.Text, FlyPathFoldPath, false, this.ratingItem_FlySpeed.Rating);
                        }
                        break;
                }
            }
            catch { }
        }

        #region //多线程调用方法
        private void StartFlyPathThread()
        {
            try
            {
                //如果不是第一次运行
                if (threadFlyAnimation != null)
                {
                    threadFlyAnimation.Abort(); //那么就终止多线程
                }
                //再启用新的多线程
                threadFlyAnimation = new Thread(new ThreadStart(ThreadFlyPath));
                threadFlyAnimation.IsBackground = true;
                threadFlyAnimation.Start();
            }
            catch { }
        }
        //专门给多线程调用
        private void ThreadFlyPath()
        {
            //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath,pSceneBookmark);//开始飞行

            flyAnimationClass.InitAnimationPlay(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath,
                this.ratingItem_FlySpeed.Rating,this.checkBoxItem_Loop.Checked,this.checkBoxItem_LookDown.Checked); //飞行
        }
        #endregion

        //场景书签切换
        private void ThreadFlyBookmark()
        {
            try
            {
                pSceneBookmark.ZoomToScene(FlyBookmarkName);
            }
            catch { }
        }

        //沿海六市的飞行
        private void FlyAlong6Cities()
        {
            try
            {
                //System.Threading.Thread thread = null;

                //丹东
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("丹东市影像");   //转到丹东市
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityDandong.Text;
                //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();

                //大连市
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("大连市影像");
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityDalian.Text;
                //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();

                //营口
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("营口市影像");
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityYingkou.Text;
                //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();

                //盘锦市
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("盘锦市影像");
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityPanjin.Text;
                //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();


                //锦州
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("锦州市影像");
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityJinzhou.Text;
                // FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();

                //葫芦岛
                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省
                pSceneBookmark.ZoomToScene("葫芦岛市影像");
                System.Threading.Thread.Sleep(2000);    //暂停两秒钟

                //FlyClickedPathName = this.btnCityHuludao.Text;
                //FlyAccordingPath.Fly.FlyByPath(this.axGlobeControl1, FlyClickedPathName, FlyPathFoldPath);//开始飞行
                //thread = new Thread(new ThreadStart(ThreadFlyPath));
                //thread.IsBackground = true;
                //thread.Start();

                pSceneBookmark.ZoomToScene("全省视图"); //先浏览全省

            }
            catch { }
        }

        private void btnFlyAnimationPlay_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is ButtonItem))
                {
                    return;
                }
                if (flyAnimationClass == null)
                {
                    flyAnimationClass = new FlyAccordingPath.Fly();
                }

                ButtonItem bi = (ButtonItem)sender;
                switch (bi.Name)
                {
                    case "btnItem_AnimationPlay":
                        FlyAnimationPlayStart();
                        break;
                    case "btnItem_AnimationPause":
                        FlyAnimationPlayPause();
                        break;
                    case "btnItem_AnimationStop":
                        FlyAnimationPlayStop();
                        break;
                    case "btnItem_AnimationClose":
                        //切换显示的tab
                        this.ribbonControl1.SelectedRibbonTabItem = this.ribbonTabItem1;
                        this.ribbonTab_FlyAnimation.Visible = false;
                        flyAnimationClass.StopAnimation();      //停止飞行
                        if (threadFlyAnimation != null)
                        {
                            threadFlyAnimation.Abort();         //终止线程
                        }
                        break;
                    default:

                        break;
                }
            }
            catch { }
        }
        
        #region//飞行控制的函数
        //开始飞行
        private void FlyAnimationPlayStart()
        {
            try
            {
                flyAnimationClass.PlayAnimation();
                //进度条
                InitTimerControl(); //初始化timer
                //进度条设置
                this.progressBarItem_FlyAnimation.Maximum = (int)(flyAnimationClass.FlyDurationTime * 10);     //进度条扩大十倍！！！显示精度更加精细
                this.progressBarItem_FlyAnimation.Value = 0;
                if (TimerProgressBar != null)
                {
                    this.TimerProgressBar.Start();
                }
            }
            catch { }
        }
        //暂停飞行
        private void FlyAnimationPlayPause()
        {
            try
            {
                flyAnimationClass.PauseAnimation();
                //进度条
                if (TimerProgressBar != null)
                {
                    this.TimerProgressBar.Stop();
                }
            }
            catch { }
        }
        //停止飞行
        private void FlyAnimationPlayStop()
        {
            try
            {
                flyAnimationClass.StopAnimation();
                if (TimerProgressBar != null)
                {
                    //进度条控制
                    this.TimerProgressBar.Stop();
                    this.progressBarItem_FlyAnimation.Value = this.progressBarItem_FlyAnimation.Maximum;
                    this.progressBarItem_FlyAnimation.Text = "";
                    ProgressBarValue = 0;
                    this.progressBarItem_FlyAnimation.Refresh();
                }
            }
            catch { }
        }
        #endregion

        #region //飞行设置
        //飞行速度设置
        private void ratingItem_FlySpeed_RatingChanged(object sender, EventArgs e)
        {
            try
            {
                flyAnimationClass.FlySpeedRating = this.ratingItem_FlySpeed.Rating;
                FlyAnimationPlayStop();//停止飞行
                if (IsFlyByPolyline)    //如果是按照Polyline的方式飞行
                {
                    flyAnimationClass.CreateAnimationFromPath(flyAnimationClass.globe, flyAnimationClass.FlyPathPolyline,
                        this.ratingItem_FlySpeed.Rating, this.checkBoxItem_Loop.Checked, this.checkBoxItem_LookDown.Checked);
                }
                if (TimerProgressBar != null)
                {
                    //进度条控制
                    this.TimerProgressBar.Stop();
                    this.progressBarItem_FlyAnimation.Value = this.progressBarItem_FlyAnimation.Maximum;
                    this.progressBarItem_FlyAnimation.Text = "";
                    ProgressBarValue = 0;
                    this.progressBarItem_FlyAnimation.Refresh();
                }
                switch (this.ratingItem_FlySpeed.Rating)
                {
                    case 1:
                        this.lblItem_Speed.Text = "很慢";
                        break;
                    case 2:
                        this.lblItem_Speed.Text = "慢速";
                        break;
                    case 3:
                        this.lblItem_Speed.Text = "中速";
                        break;
                    case 4:
                        this.lblItem_Speed.Text = "快速";
                        break;
                    case 5:
                        this.lblItem_Speed.Text = "很快";
                        break;
                    default:
                        this.lblItem_Speed.Text = "中速";
                        break;
                }
            }
            catch { }
        }
        
        //是否循环飞行
        private void checkBoxItem_Loop_CheckedChanged(object sender, CheckBoxChangeEventArgs e)
        {
            try
            {
                flyAnimationClass.IsFlyLoop = this.checkBoxItem_Loop.Checked;
                FlyAnimationPlayStop();//停止飞行
                if (IsFlyByPolyline)
                {
                    flyAnimationClass.CreateAnimationFromPath(flyAnimationClass.globe, flyAnimationClass.FlyPathPolyline,
                        this.ratingItem_FlySpeed.Rating, this.checkBoxItem_Loop.Checked, this.checkBoxItem_LookDown.Checked);
                }
                if (TimerProgressBar != null)
                {
                    //进度条控制
                    this.TimerProgressBar.Stop();
                    this.progressBarItem_FlyAnimation.Value = this.progressBarItem_FlyAnimation.Maximum;
                    this.progressBarItem_FlyAnimation.Text = "";
                    ProgressBarValue = 0;
                    this.progressBarItem_FlyAnimation.Refresh();
                }
            }
            catch { }
        }
        
        //俯视浏览
        private void checkBoxItem_LookDown_CheckedChanged(object sender, CheckBoxChangeEventArgs e)
        {
            try
            {
                flyAnimationClass.IsFlyLookdown = this.checkBoxItem_LookDown.Checked;
                FlyAnimationPlayStop();//停止飞行
                if (IsFlyByPolyline)
                {
                    flyAnimationClass.CreateAnimationFromPath(flyAnimationClass.globe, flyAnimationClass.FlyPathPolyline,
                        this.ratingItem_FlySpeed.Rating, this.checkBoxItem_Loop.Checked, this.checkBoxItem_LookDown.Checked);
                }
                if (TimerProgressBar != null)
                {
                    //进度条控制
                    this.TimerProgressBar.Stop();
                    this.progressBarItem_FlyAnimation.Value = this.progressBarItem_FlyAnimation.Maximum;
                    this.progressBarItem_FlyAnimation.Text = "";
                    ProgressBarValue = 0;
                    this.progressBarItem_FlyAnimation.Refresh();
                }
            }
            catch { }
        }

        #endregion

        #region//进度条控制
        public void InitTimerControl()
        {
            try
            {
                if (TimerProgressBar == null)
                {
                    TimerProgressBar = new System.Windows.Forms.Timer();
                    TimerProgressBar.Interval = 100;
                    TimerProgressBar.Tick += new EventHandler(TimerProgressBar_Tick);
                }
            }
            catch { }
        }
        
        private void TimerProgressBar_Tick(object sender, EventArgs e)
        {
            try
            {
                //进度条
                if (this.progressBarItem_FlyAnimation.Value < this.progressBarItem_FlyAnimation.Maximum)
                {
                    // = this.progressBarItem_FlyAnimation.Value;
                    this.progressBarItem_FlyAnimation.Value = ++ProgressBarValue;
                    this.progressBarItem_FlyAnimation.Text = Convert.ToDouble(ProgressBarValue / 10.0).ToString("F1") + "/" + (this.progressBarItem_FlyAnimation.Maximum / 10.0).ToString("F1") + "s";
                }
                this.progressBarItem_FlyAnimation.Refresh();
            }
            catch { }
        }
        #endregion
        
        #endregion

        #region //图层管理
        private void btnLayerManager_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonItem bi = (ButtonItem)sender;
                LayerInfo layerInfo = LayerNameList.Find(delegate(LayerInfo li) { return li.LayerName == bi.Name; });
                if (layerInfo.LayerName == null || layerInfo.LayerName == "")
                {
                    return;
                }
                CommonBaseTool.BaseGISTools.ShowLayerOrNot(this.axGlobeControl1, LayerNameList, bi.Name, bi.Checked);
            }
            catch { }
        }
        #endregion

        #region //城市浏览按钮
        private void btnZoomToCity_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonItem bi = (ButtonItem)sender;
                pSceneBookmark.ZoomToScene(bi.Text);
                //switch (bi.Name)
                //{
                //    case "btnCityDalian":
                //        pSceneBookmark.ZoomToScene(bi.Text);
                //        break;
                //    case "btnCityDandong":
                //        pSceneBookmark.ZoomToScene("btn_丹东市");
                //        break;
                //    case "btnCityHuludao":
                //        pSceneBookmark.ZoomToScene("btn_葫芦岛市");
                //        break;
                //    case "btnCityJinzhou":
                //        pSceneBookmark.ZoomToScene("btn_锦州市");
                //        break;
                //    case "btnCityPanjin":
                //        pSceneBookmark.ZoomToScene("btn_盘锦市");
                //        break;
                //    case "btnCityYingkou":
                //        pSceneBookmark.ZoomToScene("btn_营口市");
                //        break;
                //    case "btnFullProvince":
                //        pSceneBookmark.ZoomToScene("btn_全省视图");
                //        break;
                //}
            }
            catch { }
        }
        #endregion

        #region //规划成果展示
        private void btnPlanningConclusionShow_Click(object sender, EventArgs e)
        {
            try
            {
                string layerName = "";  //图层名称
                ButtonItem bi = (ButtonItem)sender;
                switch (bi.Name)
                {
                    case "btnAddNewLayer":
                        CommonBaseTool.GISBrowseTools.AddLayerToGlobe(this.axGlobeControl1);
                        break;
                    //辽宁沿海经济带地图
                    case "btnLiaoningBySea":
                        layerName = "btn_" + "市区";
                        break;
                    //海域范围
                    case "btnHaiyufanwei":
                        layerName = "btn_" + "海域范围";
                        break;
                    //生态安全战略
                    case "btnShengtaianquan":
                        layerName = "btn_" + "生态安全战略P";
                        break;
                    //生态综合整治   
                    case "btnShengtaizhengzhi":
                        layerName = "btn_" + "生态综合整治";
                        break;
                    //矿产资源分布
                    case "btnKuangchanziyuan":
                        layerName = "btn_" + "矿产";
                        break;
                    //景点分布
                    case "btnJingdianfenbu":
                        layerName = "btn_" + "景点";
                        break;

                }

                LayerInfo layerInfo = LayerNameList.Find(delegate(LayerInfo li) { return li.LayerName == layerName; });
                if (layerInfo.LayerName == null || layerInfo.LayerName == "")
                {
                    return;
                }
                pSceneBookmark.ZoomToScene("全省视图");

                CommonBaseTool.BaseGISTools.ShowLayerOrNot(this.axGlobeControl1, LayerNameList, layerName, bi.Checked);
            }
            catch { }
        }
        #endregion
    }
}
