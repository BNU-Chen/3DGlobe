using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;

using DevComponents.DotNetBar;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using CommonBaseTool;

namespace LiaoNing3DView
{
    public class UIControl
    {
        /// <summary>
        /// 添加飞行子按钮
        /// </summary>
        /// <param name="FlyPathList">飞行路径list</param>
        /// <param name="FlyButton">主按钮</param>
        /// <param name="btnItemClickEvent">按钮事件</param>
        public static void AddFlyPathButton(List<string> FlyPathList, DevComponents.DotNetBar.ButtonItem FlyButton, System.EventHandler btnItemClickEvent)
        {
            try
            {
                //如果路径为空
                if (FlyPathList.Count == 0)
                {
                    return;
                }

                foreach (string pathName in FlyPathList)
                {
                    ButtonItem bi = AddSubItem(pathName, btnItemClickEvent);    //新建按钮
                    FlyButton.SubItems.Add(bi); //添加为按钮
                }
            }
            catch { }
        }

        private static ButtonItem AddSubItem(string PathName, System.EventHandler btnItemClickEvent)
        {
            DevComponents.DotNetBar.ButtonItem bi = new DevComponents.DotNetBar.ButtonItem();
            try
            {
                bi.Name = PathName;         //包含字符串头的名称，防止用“非字母，下划线，”开头。
                bi.Text = PathName.Substring(4, PathName.Length - 4);       //去除"btn_"字符串头
                bi.Click += new EventHandler(btnItemClickEvent);        //添加事件
            }
            catch { }
            return bi;
        }


        /// <summary>
        /// 添加图层管理子按钮
        /// </summary>
        /// <param name="LayerInfoList">图层信息list</param>
        /// <param name="layerBtn">图层管理主按钮</param>
        /// <param name="btnItemClickedEvent">按钮事件</param>
        public static void AddLayerNameButons(List<LayerInfo> LayerInfoList, DevComponents.DotNetBar.ButtonItem layerBtn, System.EventHandler btnItemClickedEvent)
        {
            try
            {
                //如果图层为空
                if (LayerInfoList.Count == 0)
                {
                    return;
                }
                foreach (LayerInfo layerInfo in LayerInfoList)
                {
                    ButtonItem bi = AddLayerSubItem(layerInfo, btnItemClickedEvent);    //取得按钮
                    layerBtn.SubItems.Add(bi);  //添加按钮
                }
            }
            catch { }
        }

        private static ButtonItem AddLayerSubItem(LayerInfo layerInfo, System.EventHandler btnItemClickEvent)
        {
            DevComponents.DotNetBar.ButtonItem bi = new DevComponents.DotNetBar.ButtonItem();
            try
            {
                bi.Name = layerInfo.LayerName;         //包含字符串头的名称，防止用“非字母，下划线，”开头。
                bi.Text = layerInfo.LayerName.Substring(4, layerInfo.LayerName.Length - 4);       //去除"btn_"字符串头
                bi.AutoCheckOnClick = true; //点击后自动选中或不选中
                bi.Checked = layerInfo.isVisible;   //是否被选中代表着图层是否显示
                bi.Click += new EventHandler(btnItemClickEvent);        //添加事件
            }
            catch { }
            return bi;
        }

        /// <summary>
        /// 打开地图文件
        /// </summary>
        /// <param name="axGlobeControl"></param>
        /// <param name="InitDirectory"></param>
        public static void Open3DGlobeMap(AxGlobeControl axGlobeControl, string InitDirectory)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = InitDirectory;
                ofd.Multiselect = false;
                ofd.RestoreDirectory = true;
                ofd.Title = "打开三维地图文档";
                ofd.Filter = "三维地图(*.3dd)|*.3dd|所有文件(*.*)|*.*";
                ofd.CheckFileExists = true; //检查文件是否存在

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(ofd.FileName))      //判断文件是否存在
                    {
                        axGlobeControl.Load3dFile(ofd.FileName);    //加载地图
                    }
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 保存影像
        /// </summary>
        /// <param name="axGlobeControl">AxGlobeControl</param>
        public static void SaveSceneAsImage(AxGlobeControl axGlobeControl, string initFoldPath)
        {
            try
            {
                SaveFileDialog sfdImage = new SaveFileDialog();
                sfdImage.Title = "导出三维场景图像";
                sfdImage.Filter = "所有文件(*.*)|*.*|Jpeg Files(*.jpg,*.jpeg)|*.jpg,*.jpeg";
                sfdImage.RestoreDirectory = true;
                sfdImage.ValidateNames = true;
                sfdImage.OverwritePrompt = true;
                sfdImage.DefaultExt = "jpg";
                sfdImage.InitialDirectory = initFoldPath;   //初试路径

                if (sfdImage.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                axGlobeControl.GlobeDisplay.ActiveViewer.GetScreenShot(esri3DOutputImageType.JPEG, sfdImage.FileName);
                //保存成功提示
                MessageBox.Show("导出jpg图像成功，图像保存在：" + sfdImage.FileName + "。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                //MessageBox.Show(ex.Message); 
                return;
            }
        }

        /// <summary>
        /// 打开帮助文档
        /// </summary>
        /// <param name="appStartPath">系统数据路径</param>
        public static void OpenHelpDoc(string dataFoldPath)
        {
            try
            {
                string helpDocPath = dataFoldPath + "\\Help.pdf";
                if (File.Exists(helpDocPath))
                {
                    System.Diagnostics.Process.Start(helpDocPath);
                }
                else
                {
                    MessageBox.Show("帮助文档可能丢失，请确保程序完整性。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }

        /// <summary>
        /// 打开“关于我们”窗体
        /// </summary>
        public static void OpenAboutUsFrm()
        {
            frmAboutUs frmAU = new frmAboutUs();
            frmAU.ShowDialog();
        }

        public static string getBookmarkFromCityName(string _CityNameStr)
        {
            string bookmarkName = "";
            try
            {
                int cityIndex = _CityNameStr.IndexOf("市");
                if (cityIndex < 0)
                {
                    return bookmarkName;
                }
                bookmarkName = _CityNameStr.Substring(0, cityIndex + 1);
                bookmarkName += "中心";
            }
            catch
            { }
            return bookmarkName;
        }
    }
}
