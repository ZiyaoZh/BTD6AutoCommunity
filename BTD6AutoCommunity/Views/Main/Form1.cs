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
using BTD6AutoCommunity.Services;

namespace BTD6AutoCommunity.Views.Main
{
    public partial class BTD6AutoUI : Form
    {
        private bool IsStartPageEditButtonClicked = false;
        private MessageBoxService messageBoxService;
        private readonly UpdateService updateService;

        public BTD6AutoUI()
        {
            InitializeComponent();

            messageBoxService = new MessageBoxService();
            updateService = new UpdateService(messageBoxService);

            InitializeStartPage();
            InitializeScriptsEditor();
            InitializeSettingPage();
            InitializeMyScriptsPage();

            BindTabControl();

            //MessageBox.Show("test1");
        }

        private void BindTabControl()
        {
            StartPrgramTC.SelectedIndexChanged += (s, e) =>
            {
                scriptEditorViewModel.SelectedTabIndex = StartPrgramTC.SelectedIndex;
            };
            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedTabIndex")
                {
                    if (!Equals(StartPrgramTC.SelectedIndex, scriptEditorViewModel.SelectedTabIndex))
                    {
                        StartPrgramTC.SelectedIndex = scriptEditorViewModel.SelectedTabIndex;
                        if (StartPrgramTC.SelectedIndex == 0)
                        {
                            startViewModel.UpdateSelectedScript();
                        }
                    }
                }
            };
            //startViewModel.PropertyChanged += (s, e) =>
            //{
            //    if (e.PropertyName == "SelectedTabIndex")
            //    {
            //        if (!Equals(StartPrgramTC.SelectedIndex, startViewModel.SelectedTabIndex))
            //        {
            //            StartPrgramTC.SelectedIndex = startViewModel.SelectedTabIndex;
            //        }
            //    }
            //};
        }

        private void BTD6AutoCommunity_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseAllKeys(); //释放所有热键
            WindowApiWrapper.UnregisterHotKey(Handle, 101); //注册F10热键,根据id值101来判断需要执行哪个函数
            WindowApiWrapper.UnregisterHotKey(Handle, 102);
            WindowApiWrapper.UnregisterHotKey(Handle, 103);
            WindowApiWrapper.UnregisterHotKey(Handle, 104);
            WindowApiWrapper.UnregisterHotKey(Handle, 105);
            WindowApiWrapper.UnregisterHotKey(Handle, 106);
            WindowApiWrapper.UnregisterHotKey(Handle, 107);
            WindowApiWrapper.UnregisterHotKey(Handle, 108);
            WindowApiWrapper.UnregisterHotKey(Handle, 111); //注册F9热键,根据id值111来判断需要执行哪个函数
            WindowApiWrapper.UnregisterHotKey(Handle, 112);
            WindowApiWrapper.UnregisterHotKey(Handle, 113);
            WindowApiWrapper.UnregisterHotKey(Handle, 114);
            WindowApiWrapper.UnregisterHotKey(Handle, 115);
            WindowApiWrapper.UnregisterHotKey(Handle, 116);
            WindowApiWrapper.UnregisterHotKey(Handle, 117);
            WindowApiWrapper.UnregisterHotKey(Handle, 118);
        }

        private void ReleaseAllKeys()
        {
            for (int i = 0x08; i <= 0x87; i++)
            {
                InputSimulator.KeyboardRelease((ushort)i);
            }
        }

        // 将 Load 事件处理器改为异步，以便在 UI 线程安全地 await 更新检查
        private async void BTD6AutoCommunity_Load(object sender, EventArgs e)
        {
            WindowApiWrapper.RegisterHotKey(Handle, 101, 0, Keys.F10); //注册F10热键,根据id值101来判断需要执行哪个函数
            WindowApiWrapper.RegisterHotKey(Handle, 102, 1, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 103, 2, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 104, 3, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 105, 4, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 106, 5, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 107, 6, Keys.F10);
            WindowApiWrapper.RegisterHotKey(Handle, 108, 7, Keys.F10);

            WindowApiWrapper.RegisterHotKey(Handle, 111, 0, Keys.F9); //注册F9热键,根据id值111来判断需要执行哪个函数
            WindowApiWrapper.RegisterHotKey(Handle, 112, 1, Keys.F9);
            WindowApiWrapper.RegisterHotKey(Handle, 113, 2, Keys.F9);
            WindowApiWrapper.RegisterHotKey(Handle, 114, 3, Keys.F9);
            WindowApiWrapper.RegisterHotKey(Handle, 115, 4, Keys.F9);
            WindowApiWrapper.RegisterHotKey(Handle, 116, 5, Keys.F9);   
            WindowApiWrapper.RegisterHotKey(Handle, 117, 6, Keys.F9);
            WindowApiWrapper.RegisterHotKey(Handle, 118, 7, Keys.F9);
            await updateService.CheckUpdateAsync();
        }

        protected override void WndProc(ref Message m) // 重载WndProc函数
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 13:    //Enter 设置
                            mouseCoordinateDisplayService?.HandleHotKeyPressed();
                            break;
                        case 101:
                        case 102:
                        case 103:
                        case 104:
                        case 105:
                        case 106:
                        case 107:
                        case 108:
                            StartProgramBT.PerformClick();
                            break;
                        case 111:
                        case 112:
                        case 113:
                        case 114:
                        case 115:
                        case 116:
                        case 117:
                           PauseBT.PerformClick();
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

    }
}
