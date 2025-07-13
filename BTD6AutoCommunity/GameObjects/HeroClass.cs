using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.GameObjects
{
    public class HeroClass
    {
        public string name;  // 猴子塔的名字
        public (int, int) coordinates  = (0, 0);  // 部署坐标 (x, y)

        public bool exsitence;
        public bool storeOpen;
        public HeroClass()
        {
            exsitence = false;
            storeOpen = false;
            coordinates = (0, 0);
        }

        public HeroClass Clone()
        {
            HeroClass hero = new HeroClass
            {
                name = name,
                coordinates = coordinates,
                exsitence = exsitence,
                storeOpen = storeOpen
            };
            return hero;
        }

        public bool PlaceHero((int, int) pos)
        {
            if (exsitence) return false;
            coordinates = pos;
            exsitence = true;
            return true;
        }

        public bool Sell()
        {
            if (!exsitence) return false;
            exsitence = false;
            return true;
        }

        public (int X, int Y) GetCoordinates()
        {
            return coordinates;
        }


    }
}
