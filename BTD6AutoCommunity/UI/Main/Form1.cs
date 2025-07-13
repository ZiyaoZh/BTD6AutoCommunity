using Newtonsoft.Json;
using OpenCvSharp.Dnn;
using OpenCvSharp.Internal.Vectors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;
using static OpenCvSharp.Stitcher;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using BTD6AutoCommunity.Core;

namespace BTD6AutoCommunity.UI.Main
{
    public partial class BTD6AutoUI : Form
    {
        private bool IsStartPageEditButtonClicked = false;
        public BTD6AutoUI()
        {
            InitializeComponent();
            InitializeStartPage();
            InitializeScriptsEditor();
            InitializeSettingPage();
            InitializeMyScriptsPage();
        }

        private void BTD6AutoCommunity_Activated(object sender, EventArgs e)
        {
        }

        private void BTD6AutoCommunity_Leave(object sender, EventArgs e)
        {
        }

        private void BTD6AutoCommunity_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseAllKeys(); //释放所有热键
            WindowApiWrapper.UnregisterHotKey(Handle, 101); //注册F1热键,根据id值101来判断需要执行哪个函数
            WindowApiWrapper.UnregisterHotKey(Handle, 102);
            WindowApiWrapper.UnregisterHotKey(Handle, 103);
            WindowApiWrapper.UnregisterHotKey(Handle, 104);
            WindowApiWrapper.UnregisterHotKey(Handle, 105);
            WindowApiWrapper.UnregisterHotKey(Handle, 106);
            WindowApiWrapper.UnregisterHotKey(Handle, 107);
            WindowApiWrapper.UnregisterHotKey(Handle, 108);

        }


        private void ReleaseAllKeys()
        {
            for (int i = 0x08; i <= 0x87; i++)
            {
                InputSimulator.KeyboardRelease((ushort)i);
            }
        }

        private void BTD6AutoCommunity_Load(object sender, EventArgs e)
        {
            WindowApiWrapper.RegisterHotKey(Handle, 101, 0, Keys.F1); //注册F1热键,根据id值101来判断需要执行哪个函数
            WindowApiWrapper.RegisterHotKey(Handle, 102, 1, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 103, 2, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 104, 3, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 105, 4, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 106, 5, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 107, 6, Keys.F1);
            WindowApiWrapper.RegisterHotKey(Handle, 108, 7, Keys.F1);
        }

        private void StartPrgramTC_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (StartPrgramTC.SelectedIndex == 0) InitializeStartPage();
            //else if (StartPrgramTC.SelectedIndex == 1) InitializeScriptsEditor();
            //else if (StartPrgramTC.SelectedIndex == 2) InitializeSettingPage();
            //else if (StartPrgramTC.SelectedIndex == 3) InitializeMyScriptsPage();
        }
    }
}
