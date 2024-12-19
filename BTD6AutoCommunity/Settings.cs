using Newtonsoft.Json;
using System.IO;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity
{
    internal class Settings
    {
        public int GetGameDataInterval { get; set; }
        public int ExecuteInterval { get; set; }
        public int GameDpi { get; set; }

        public bool doubleCoinsEnabled { get; set; }

        public bool fastPathEnabled { get; set; }

        public Dictionary<int, Keys> HotKey { get; set; }

        private Dictionary<int, Keys> defaultHotKey;

        private Dictionary<int, string> dpiToDisplay;



        public Settings()
        {
            dpiToDisplay = new Dictionary<int, string>
            {{ 0, "1920x1080" }, { 1, "1280x720" } };
            defaultHotKey = new Dictionary<int, Keys>
            {
                { 0, Keys.Q },
                { 1, Keys.W },
                { 2, Keys.E },
                { 3, Keys.R },
                { 4, Keys.T },
                { 5, Keys.Y },

                { 10, Keys.Z },
                { 11, Keys.X },
                { 12, Keys.C },
                { 13, Keys.V },
                { 14, Keys.B },
                { 15, Keys.N },
                { 16, Keys.M },

                { 20, Keys.A },
                { 21, Keys.S },
                { 22, Keys.D },
                { 23, Keys.F },
                { 24, Keys.G },
                { 25, Keys.O },

                { 30, Keys.H },
                { 31, Keys.J },
                { 32, Keys.K },
                { 33, Keys.L },
                { 34, Keys.I }
            };
        }


        public void Save()
        {
            string filePath = $@"Settings.json";
            //string filePath = "你好.json";
            var partialInfo = new
            {
                GetGameDataInterval,
                ExecuteInterval,
                GameDpi,
                HotKey,
                doubleCoinsEnabled,
                fastPathEnabled
            };
            string jsonString = JsonConvert.SerializeObject(partialInfo, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }
    }
}
