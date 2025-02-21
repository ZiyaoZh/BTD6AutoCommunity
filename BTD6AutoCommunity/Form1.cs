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

namespace BTD6AutoCommunity
{
    public partial class BTD6AutoCommunity : Form
    {
        public BTD6AutoCommunity()
        {
            InitializeComponent();
            InitializeScriptsEditor();
            InitializeMyScriptsPage();
            InitializeSettingPage();
            InitializeStartPage();
        }

        private void BTD6AutoCommunity_Activated(object sender, EventArgs e)
        {
            KeyEvents.RegisterHotKey(Handle, 101, 0, Keys.F1); //注册F1热键,根据id值101来判断需要执行哪个函数
        }

        private void BTD6AutoCommunity_Leave(object sender, EventArgs e)
        {
            KeyEvents.UnregisterHotKey(Handle, 101); //注册F1热键,根据id值101来判断需要执行哪个函数
        }

    }
}
