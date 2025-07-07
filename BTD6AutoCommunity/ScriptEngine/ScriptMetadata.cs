using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptMetadata
    {
        public string Version { get; set; } = "1.0";
        public string ScriptName { get; set; }
        public Maps SelectedMap { get; set; }
        public LevelDifficulties SelectedDifficulty { get; set; }
        public LevelMode SelectedMode { get; set; }
        public Heroes SelectedHero { get; set; }
        public (int X, int Y) AnchorCoords { get; set; }

        public ScriptMetadata() { }

        public ScriptMetadata(
            string scriptName,
            Maps selectedMap,
            LevelDifficulties selectedDifficulty,
            LevelMode selectedMode,
            Heroes selectedHero,
            (int x, int y) anchorCoords)
        {
            ScriptName = scriptName;
            SelectedMap = selectedMap;
            SelectedDifficulty = selectedDifficulty;
            SelectedMode = selectedMode;
            SelectedHero = selectedHero;
            AnchorCoords = anchorCoords;
        }

        public override string ToString()
        {
            return $"[{SelectedMap}-{SelectedDifficulty}-{SelectedMode}-{SelectedHero}] Anchor: ({AnchorCoords.X},{AnchorCoords.Y})";
        }
    }
}

