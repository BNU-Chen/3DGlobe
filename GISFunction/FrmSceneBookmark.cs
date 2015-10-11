using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GISFunction
{
    public partial class FrmSceneBookmark : Form
    {

        private SceneBookmark sbm = null;
        private bool isClickOK = false;

        public FrmSceneBookmark(SceneBookmark _sbm)
        {
            InitializeComponent();
            sbm = _sbm;
        }

        private void btnX_Ok_Click(object sender, EventArgs e)
        {
            try
            {
                isClickOK = true;   //已经点击过ok按钮

                string sceneName = this.txtBox_SceneName.Text.Trim();
                if (sceneName == "" || sceneName.Length == 0)
                {
                    return;
                }
                sbm.CurrentSceneName = "btn_" + sceneName;

                this.Close();
                this.Dispose();
            }
            catch { }
        }

        private void FrmSceneBookmark_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                string sceneName = this.txtBox_SceneName.Text.Trim();
                if (sceneName == "" || sceneName.Length == 0 || !isClickOK)   //如果场景名为空，或者长度为0，或者没有点击确定按钮
                {
                    sbm.CurrentSceneName = "";
                }
            }
            catch { }
        }

    }
}
