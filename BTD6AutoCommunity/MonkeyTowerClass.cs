using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity
{
    public class MonkeyTowerClass
    {
        public string name { get; set; }  // 猴子塔的名字
        public int deployCost { get; set; }    // 部署价格
        public List<int> upgradeCosts { get; set; }  // 每条升级路线的价格
        public (int, int) coordinates { get; set; } = ( 0, 0 );  // 部署坐标 (x, y)
        public List<int> upgradeLevels { get; set; } = new List<int> { 0, 0, 0 }; // 每条升级路线的当前等级
        public List<int> routeLock { get; set; } = new List<int> { 0, 0, 0 }; // 升级路线锁定情况

        public int upgradeCount = 0;

        public bool exsitence = true;

        public bool IsDelete = true;

        public List<double> difficultyCost = new List<double>{ 0.85, 1.0, 1.08, 1.2 };

        public List<double> onSale = new List<double> { 1.0, 0.90, 0.85, 0.80, 0.75 };
        // 构造函数
        public MonkeyTowerClass() { }


        // 获取猴子塔名字
        public string GetName()
        {
            return name;
        }

        // 获取部署价格
        public int GetDeployCost()
        {
            return deployCost;
        }

        // 获取部署坐标
        public (int, int) GetCoordinates()
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

        public int GetCurrentUpgradeCost(int route, int difficulty, int onsale)
        {
            int cost;
            if (routeLock[route] != 1)
            {
                cost = upgradeCosts[route * 5 + upgradeLevels[route]];
            }
            else
            {
                cost = -1;
            }
            if (cost != -1)
            {
                cost = ((int)(cost * difficultyCost[difficulty] / 5 + 0.5)) * 5;
            }
            if (onsale != 0)
            {
                cost = ((int)(cost * onSale[onsale] / 5 + 0.5)) * 5;
            }
            return cost;
        }

        public int GetCurrentDeployCost(int difficulty)
        {
            int cost = ((int)(deployCost * difficultyCost[difficulty] / 5 + 0.5)) * 5;
            return cost;
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
            //if (exsitence == false)
            //{
            //    return false;
            //}
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
            Console.WriteLine($"Monkey Tower: {name}");
            Console.WriteLine($"Deploy Cost: {deployCost}");
            Console.WriteLine($"Coordinates: ({coordinates.Item1}, {coordinates.Item2})");
            Console.Write("Upgrade Levels: ");
            foreach (int level in upgradeLevels)
            {
                Console.Write(level + " ");
            }
            Console.WriteLine();
        }

        public bool Sale()
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

        public override string ToString()
        {
            return $"Name: {name}, Deploy Cost: {deployCost}, Upgrade Costs: {string.Join(", ", upgradeCosts)}";
        }
    }
}
