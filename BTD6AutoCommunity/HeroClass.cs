﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity
{
    public class HeroClass
    {
        public string name;  // 猴子塔的名字
        public int deployCost;    // 部署价格
        public List<int> upgradeCosts;  // 每条升级路线的价格
        public (int, int) coordinates  = (0, 0);  // 部署坐标 (x, y)

        public bool exsitence;
        public bool storeOpen;
        public HeroClass()
        {
            exsitence = false;
            storeOpen = false;
            coordinates = (0, 0);
        }
    }
}
