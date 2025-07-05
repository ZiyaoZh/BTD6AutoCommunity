using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.GameObjects
{
    public class MonkeyTowerClass
    {
        public Monkeys Type { get; set; }  // 猴子塔名字
        public (int, int) coordinates { get; set; } = ( 0, 0 );  // 部署坐标 (x, y)
        public List<int> upgradeLevels { get; set; } = new List<int> { 0, 0, 0 }; // 每条升级路线的当前等级
        public List<int> routeLock { get; set; } = new List<int> { 0, 0, 0 }; // 升级路线锁定情况

        public int upgradeCount = 0;

        // 是否存在
        public bool exsitence;

        public bool IsDelete;

        private int monkeyId;

        // 构造函数
        public MonkeyTowerClass(Monkeys name, int id, (int, int) coords) 
        {
            IsDelete = false;
            exsitence = true;
            coordinates = coords;
            monkeyId = id;
        }

        // 获取部署坐标
        public (int X, int Y) GetCoordinates()
        {
            return coordinates;
        }

        // 设置部署坐标
        public void SetCoordinates((int, int) coords)
        {
            coordinates = coords;
        }

        // 获取当前升级等级
        public int GetUpgradeLevel(int route)
        {
            if (route < 0 || route >= upgradeLevels.Count)
            {
                throw new ArgumentOutOfRangeException("Invalid upgrade route");
            }
            return upgradeLevels[route];
        }

        // 获取当前升级
        public string GetAllUpgradeLevel()
        {
            string allLevel = "";
            allLevel += (char)(GetUpgradeLevel(0) + '0');
            allLevel += (char)(GetUpgradeLevel(1) + '0');
            allLevel += (char)(GetUpgradeLevel(2) + '0');
            return allLevel;
        }

        // 获取升级整数表达
        public int GetUpgradeInt()
        {
            int upgradeInt = 0;
            for (int i = 0; i < 3; i++)
            {
                upgradeInt += upgradeLevels[i] * (int)Math.Pow(10, 2 - i);
            }
            return upgradeInt;
        }

        // 路径锁定
        private void LockUpgradeRoute()
        {
            if (upgradeLevels[0] > 0 && upgradeLevels[1] > 0)
            {
                routeLock[2] = 1;
                if (upgradeLevels[0] == 2 && upgradeLevels[1] > 2)
                {
                    routeLock[0] = 1;
                }
                if (upgradeLevels[0] > 2 && upgradeLevels[1] == 2)
                {
                    routeLock[1] = 1;
                }
            }
            if (upgradeLevels[1] > 0 && upgradeLevels[2] > 0)
            {
                routeLock[0] = 1;
                if (upgradeLevels[1] == 2 && upgradeLevels[2] > 2)
                {
                    routeLock[1] = 1;
                }
                if (upgradeLevels[1] > 2 && upgradeLevels[2] == 2)
                {
                    routeLock[2] = 1;
                }
            }
            if (upgradeLevels[0] > 0 && upgradeLevels[2] > 0)
            {
                routeLock[1] = 1;
                if (upgradeLevels[0] == 2 && upgradeLevels[2] > 2)
                {
                    routeLock[0] = 1;
                }
                if (upgradeLevels[0] > 2 && upgradeLevels[2] == 2)
                {
                    routeLock[2] = 1;
                }
            }
            if (upgradeLevels[0] == 5)
            {
                routeLock[0] = 1;
            }
            if (upgradeLevels[1] == 5)
            {
                routeLock[1] = 1;
            }
            if (upgradeLevels[2] == 5)
            {
                routeLock[2] = 1;
            }
        }

        // 升级
        public bool Upgrade(int route)
        {
            if (exsitence == false)
            {
                return false;
            }
            if (routeLock[route] == 0)
            {
                upgradeLevels[route]++;
                upgradeCount++;
                LockUpgradeRoute();
                return true;
            }
            else
            {
                return false;
            }
        }

        // 显示猴子塔的状态
        public void DisplayStatus()
        {
            Console.WriteLine($"Monkey Tower: {Type}");
            Console.WriteLine($"Coordinates: ({coordinates.Item1}, {coordinates.Item2})");
            Console.Write("Upgrade Levels: ");
            foreach (int level in upgradeLevels)
            {
                Console.Write(level + " ");
            }
            Console.WriteLine();
        }

        public bool Sell()
        {
            if (exsitence == false)
            {
                return false;
            }
            exsitence = false;
            return true;
        }

        public void ClearUpgrade()
        {
            upgradeLevels.Clear();
            upgradeLevels = new List<int>{ 0, 0, 0 };
            routeLock.Clear();
            routeLock = new List<int> { 0, 0, 0 };
            return;
        }
    }
}
