using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity
{
    internal class LevelStrategyScript
    {
        public int SelectedMap { get; set; }
        public int SelectedDifficulty { get; set; }
        public int SelectedMode { get; set; }
        public int SelectedHero { get; set; }
        public (int, int) AnchorCoords { get; set; }
        public List<int> ObjectCount { get; set; } // 猴子的数量
        public List<(int, int)> ObjectId { get; set; } // 猴子的ID
        public List<string> Displayinstructions { get; set; }
        public List<string> Digitalinstructions { get; set; }

        public LevelStrategyScript()
        {
            Displayinstructions = new List<string>();
            Digitalinstructions = new List<string>();
            // 初始化计数列表
            ObjectCount = new List<int>();
            ObjectId = new List<(int, int)>();
        }
    }
}
