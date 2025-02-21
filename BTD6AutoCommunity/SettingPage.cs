using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity
{
    // 选项设置页面
    public partial class BTD6AutoCommunity
    {
        private Settings mySet;
        private Dictionary<System.Windows.Forms.Button, Keys> hotKeysMap;
        private System.Windows.Forms.Button currentButton;
        private System.Timers.Timer ExecuteTM;
        private bool isSettingHotKey;

        private void InitializeSettingPage()
        {
            mySet = new Settings();
            hotKeysMap = new Dictionary<System.Windows.Forms.Button, Keys>();
            ExecuteTM = new System.Timers.Timer(200);
            ExecuteTM.Elapsed += ExecuteTM_Tick;
            currentButton = null;
            isSettingHotKey = false;
            LoadSettings();
            InitHotKeyMap();
        }

        private void InitHotKeyMap()
        {
            hotKeysMap = new Dictionary<System.Windows.Forms.Button, Keys>
            {
                { hot0, Keys.None },
                { hot1, Keys.None },
                { hot2, Keys.None },
                { hot3, Keys.None },
                { hot4, Keys.None },
                { hot5, Keys.None },
                { hot10, Keys.None },
                { hot11, Keys.None },
                { hot12, Keys.None },
                { hot13, Keys.None },
                { hot14, Keys.None },
                { hot15, Keys.None },
                { hot16, Keys.None },
                { hot20, Keys.None },
                { hot21, Keys.None },
                { hot22, Keys.None },
                { hot23, Keys.None },
                { hot24, Keys.None },
                { hot25, Keys.None },
                { hot30, Keys.None },
                { hot31, Keys.None },
                { hot32, Keys.None },
                { hot33, Keys.None },
                { hot34, Keys.None }
            };
        }

        private void SaveSettingsBT_Click(object sender, EventArgs e)
        {
            mySet.GetGameDataInterval = (int)GetGameDataUD.Value;
            mySet.ExecuteInterval = (int)ExecuteUD.Value;
            mySet.GameDpi = GameDpiCB.SelectedIndex;
            mySet.doubleCoinsEnabled = DoubleCoinCB.Checked;
            mySet.fastPathEnabled = FastPathCB.Checked;
            foreach (var buttonKeyPairs in hotKeysMap)
            {
                mySet.HotKey[Int32.Parse(buttonKeyPairs.Key.Tag.ToString())] = buttonKeyPairs.Value;
            }
            mySet.Save();
        }

        private void LoadSettings()
        {
            string jsonString = File.ReadAllText(@"Settings.Json");
            mySet = JsonConvert.DeserializeObject<Settings>(jsonString);
            GetGameDataUD.Value = mySet.GetGameDataInterval;
            ExecuteUD.Value = mySet.ExecuteInterval;
            GameDpiCB.SelectedIndex = mySet.GameDpi;
            ExecuteTM.Interval = mySet.ExecuteInterval;

            GetGameDataTM.Interval = mySet.GetGameDataInterval;
            DoubleCoinCB.Checked = mySet.doubleCoinsEnabled;
            FastPathCB.Checked = mySet.fastPathEnabled;

            List<System.Windows.Forms.Button> btns = hotKeysMap.Keys.ToList();
            foreach (var buttonKeyPairs in btns)
            {
                Keys currentkey = mySet.HotKey[Int32.Parse(buttonKeyPairs.Tag.ToString())];
                buttonKeyPairs.Text = currentkey.ToString();
                hotKeysMap[buttonKeyPairs] = currentkey;
            }
        }

        private void Hot_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            btn.Text = "请按键...";
            isSettingHotKey = true;
            currentButton = btn;
        }

        private void StartPrgramTC_KeyDown(object sender, KeyEventArgs e)
        {
            if (StartPrgramTC.SelectedIndex == 2 && isSettingHotKey && currentButton != null)
            {
                Keys pressedKey = e.KeyCode;
                hotKeysMap[currentButton] = pressedKey;
                currentButton.Text = pressedKey.ToString();

                isSettingHotKey = false;
                currentButton = null;
            }
        }
    }
}
