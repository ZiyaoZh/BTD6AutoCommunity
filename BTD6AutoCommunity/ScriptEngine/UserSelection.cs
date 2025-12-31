using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.Views.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class UserSelection
    {
        public FunctionTypes selectedFunction;
        public Maps selectedMap;
        public LevelDifficulties selectedDifficulty;
        public string selectedScript;
        public int selectedIndex;
    }

    public enum FunctionTypes
    {
        Custom = 0,
        Collection = 1,
        Circulation = 2,
        Race = 3,
        BlackBorder = 4,
    }
}
