using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Animation;


using DevComponents.DotNetBar;

namespace GISFunction
{
    public class SceneBookmark
    {

        private AxGlobeControl axGlobeControl = null;
        private IGlobeDisplay pGlobeDisplay = null;
        private IScene pScene = null;
        private ISceneBookmarks pSceneBookmarks = null;
        private int BookmarkCount = 0;
        private List<string> BookmarksNamesList = new List<string>();
        private ButtonItem btnItem = null;
        public string CurrentSceneName = "";
        private EventHandler btnItemClickEvent;
        private string strBtnAhead = "btn_";    //给书签名称自动加上“btn_"
        //private string DatFilePath = "";        //书签保存位置

        public SceneBookmark(AxGlobeControl _axGlobeControl, ButtonItem _btnItem, EventHandler _btnItem_Click)
        {
            try
            {
                this.axGlobeControl = _axGlobeControl;
                this.btnItem = _btnItem;
                this.btnItemClickEvent = _btnItem_Click;

                pGlobeDisplay = this.axGlobeControl.GlobeDisplay;
                pScene = pGlobeDisplay.Scene;
                pSceneBookmarks = pScene as ISceneBookmarks;

                BookmarkCount = pSceneBookmarks.BookmarkCount;      //bookmark的个数

                //DatFilePath = Application.StartupPath + "\\Data\\Bookmark.dat"; //书签的保存位置

                //LoadBookmarkDatFile(DatFilePath, pSceneBookmarks);  //加载场景书签【有错误，停用】
            }
            catch { }
        }

        public void LoadBookmarksAsButton()
        {
            try
            {
                BookmarksNamesList.Clear(); //清空之前保存的
                BookmarksNamesList = getBookmarksNames(pSceneBookmarks);
                if (BookmarksNamesList.Count == 0)
                {
                    return;
                }
                foreach (string name in BookmarksNamesList)
                {

                    if (btnItem.SubItems.Contains(name))    //如果已经包含了这个按钮
                    {
                        continue;   //继续下一次循环
                    }
                    ButtonItem bi = AddSubItemScene(name);
                    btnItem.SubItems.Add(bi);
                }
            }
            catch { }
        }

        //新建一个新的场景
        public void AddBookmarkScene()
        {
            try
            {
                FrmSceneBookmark frmSBM = new FrmSceneBookmark(this);
                frmSBM.ShowDialog();
                if (CurrentSceneName == "" || CurrentSceneName.Length == 0)
                { return; }

                ButtonItem bi = AddSubItemScene(CurrentSceneName);
                AddNewBookmarkScene(CurrentSceneName, pGlobeDisplay, pSceneBookmarks);  //添加场景
                btnItem.SubItems.Add(bi);               //添加按钮

                //保存书签【有错误，停用】
                //SaveBookmarkDatFile(DatFilePath,pSceneBookmarks);   //保存场景书签
            }
            catch { }
        }



        public void RemoveSubItemScene(ButtonItem btnItem, string SceneName)
        {
            try
            {
                if (btnItem.SubItems.Contains(SceneName))
                {
                    return;
                }
                btnItem.SubItems.Remove(SceneName);     //删除按钮

                DeleteBookmarkScene(SceneName); //删除场景                
            }
            catch
            {
                MessageBox.Show("删除失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);//删除失败
            }
        }

        /// <summary>
        /// 用名称，转到相应场景
        /// </summary>
        /// <param name="SceneName">场景名称</param>
        public void ZoomToScene(string SceneName)
        {
            try
            {
                IBookmark3D pBookmark;
                pSceneBookmarks.FindBookmark(SceneName, out pBookmark); //取出bookmark
                if (pBookmark == null)
                {
                    return;
                }
                //pBookmark.Apply(pGlobeDisplay.ActiveViewer, true, 2000);      //没有动态效果
                SceneBookmarkLightZoom.ZoomToBookmark(pGlobeDisplay, pBookmark, 3);    //这个动态效果很好， 是3秒，不是毫秒
                axGlobeControl.GlobeDisplay.RefreshViewers();
                //pScene.SceneGraph.RefreshViewers(); 
            }
            catch { }
        }

        /// <summary>
        /// 添加一个按钮
        /// </summary>
        /// <param name="SceneName">场景</param>
        /// <returns></returns>
        private ButtonItem AddSubItemScene(string SceneName)
        {
            DevComponents.DotNetBar.ButtonItem bi = new DevComponents.DotNetBar.ButtonItem();
            try
            {
                bi.Name = SceneName; //给按钮命名，需要加上btn，防止用“非字母，下划线，”开头。
                bi.Text = SceneName.Substring(4, SceneName.Length - 4);
                bi.Click += new EventHandler(btnItemClickEvent);
            }
            catch { }
            return bi;
        }

        /// <summary>
        /// 添加新的场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        private void AddNewBookmarkScene(string sceneName, IGlobeDisplay _GlobeDisplay, ISceneBookmarks _SceneBookmarks)
        {
            try
            {
                //sBookName = sBookName + "|创建日期：" + DateTime.Today.ToShortDateString() + "，时间：" + DateTime.Now.ToLongTimeString();

                ICamera camera = _GlobeDisplay.ActiveViewer.Camera;

                IBookmark3D p3DBookmark = new ESRI.ArcGIS.Analyst3D.Bookmark3D();
                p3DBookmark.Name = sceneName;
                p3DBookmark.Capture(camera);
                _SceneBookmarks.AddBookmark(p3DBookmark);
            }
            catch { }
        }

        /// <summary>
        /// 删除某一个场景
        /// </summary>
        /// <param name="SceneName">场景名称</param>
        private void DeleteBookmarkScene(string SceneName)
        {
            try
            {
                if (BookmarksNamesList.Count == 0)
                {
                    return;
                }

                IBookmark3D p3DBookmark = null;
                pSceneBookmarks.FindBookmark(SceneName, out p3DBookmark);   //查询场景

                if (p3DBookmark == null)    //如果场景不存在
                {
                    return;
                }

                DialogResult dr = MessageBox.Show("是否删除书签", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult.No == dr)
                {
                    return;
                }

                pSceneBookmarks.RemoveBookmark(p3DBookmark);    //删除场景
                pGlobeDisplay.RefreshViewers();
                //pScene.SceneGraph.RefreshViewers();
            }
            catch { }
        }

        private void LoadBookmarkDatFile(string filePath, ISceneBookmarks _SceneBookmarks)
        {
            try
            {
                IMemoryBlobStream pMemoryBlobStream = new MemoryBlobStreamClass();
                pMemoryBlobStream.LoadFromFile(filePath);
                IObjectStream pObjectStream = new ObjectStreamClass();
                pObjectStream.Stream = pMemoryBlobStream as IStream;
                _SceneBookmarks.LoadBookmarks(pObjectStream.Stream);
            }
            catch { }
        }

        private void SaveBookmarkDatFile(string filePath, ISceneBookmarks _SceneBookmarks)
        {
            try
            {
                IObjectStream pObjStream = new ObjectStreamClass();
                _SceneBookmarks.SaveBookmarks(pObjStream.Stream);
                IMemoryBlobStream pMemBlobStream = pObjStream.Stream as IMemoryBlobStream;
                pMemBlobStream.SaveToFile(filePath);
            }
            catch { }
        }


        private List<string> getBookmarksNames(ISceneBookmarks _SceneBookmarks)
        {
            List<string> bookmarksNames = new List<string>();
            try
            {
                if (_SceneBookmarks.BookmarkCount == 0)
                {
                    return bookmarksNames;
                }

                for (int index = 0; index < BookmarkCount; index++)
                {
                    IBookmark3D pBookmark3D = _SceneBookmarks.Bookmarks.get_Element(index) as IBookmark3D;
                    bookmarksNames.Add(strBtnAhead + pBookmark3D.Name);
                }

                return bookmarksNames;
            }
            catch
            {
                return bookmarksNames;
            }
        }
    }
}
