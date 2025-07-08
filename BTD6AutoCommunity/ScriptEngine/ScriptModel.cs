using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptModel
    {
        public ScriptMetadata Metadata { get; set; }

        public List<List<int>> InstructionsList { get; set; }

        public List<int> MonkeyCounts { get; set; }

        public List<int> MonkeyIds { get; set; }

        public ScriptModel() { }

        public ScriptModel(ScriptMetadata metadata, List<List<int>> sequence, List<int> monkeyCounts, List<int> monkeyIds)
        {
            Metadata = metadata;
            InstructionsList = sequence;
            MonkeyCounts = monkeyCounts;
            MonkeyIds = monkeyIds;
        }

        public static ScriptModel Convert(ScriptModelOld oldModel, string scriptName)
        {
            var monkeyCountsOld = oldModel.ObjectCount;
            var monkeyIdsOld = oldModel.ObjectId;

            var metadata = new ScriptMetadata(scriptName, oldModel.SelectedMap, oldModel.SelectedDifficulty, oldModel.SelectedMode, oldModel.SelectedHero, oldModel.AnchorCoords);
            var instructionsList = new List<List<int>>();
            var monkeyCounts = new List<int>();
            var monkeyIds = new List<int>();
            foreach (var instruction in oldModel.Digitalinstructions)
            {
                var arguments = new List<int>();
                for (int i = 0; i < 12; i++) arguments.Add(-1);
                List<int> argumentsOld = instruction.Split(' ').Select(int.Parse).ToList();
                switch ((ActionTypes)argumentsOld[0])
                { 
                    case ActionTypes.PlaceMonkey:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[7] = monkeyIdsOld[argumentsOld[2]].Item1 + monkeyIdsOld[argumentsOld[2]].Item2 * 100;
                        monkeyIds.Add(arguments[7]);
                        arguments[8] = argumentsOld[3];
                        arguments[9] = argumentsOld[4];
                        arguments[10] = argumentsOld[5];
                        arguments[11] = argumentsOld[6];
                        break;
                    case ActionTypes.UpgradeMonkey:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = monkeyIdsOld[argumentsOld[1]].Item1 + monkeyIdsOld[argumentsOld[1]].Item2 * 100;
                        arguments[2] = argumentsOld[2];
                        if (argumentsOld.Count == 6)
                        {
                            arguments[10] = argumentsOld[4];
                            arguments[11] = argumentsOld[5];
                        }
                        else
                        {
                            arguments[10] = argumentsOld[3];
                            arguments[11] = argumentsOld[4];
                        }
                        break;
                    case ActionTypes.SwitchMonkeyTarget:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = monkeyIdsOld[argumentsOld[1]].Item1 + monkeyIdsOld[argumentsOld[1]].Item2 * 100;
                        arguments[2] = argumentsOld[2];
                        arguments[10] = argumentsOld[3];
                        arguments[11] = argumentsOld[4];
                        break;
                    case ActionTypes.UseAbility:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        if (argumentsOld[2] == 1)
                        {
                            arguments[8] = argumentsOld[2];
                            arguments[9] = argumentsOld[3];
                            arguments[10] = argumentsOld[4];
                            arguments[11] = argumentsOld[5];
                        }
                        else
                        {
                            arguments[10] = argumentsOld[2];
                            arguments[11] = argumentsOld[3];
                        }
                        break;
                    case ActionTypes.SwitchSpeed:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[10] = argumentsOld[2];
                        arguments[11] = argumentsOld[3];
                        break;
                    case ActionTypes.SellMonkey:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = monkeyIdsOld[argumentsOld[1]].Item1 + monkeyIdsOld[argumentsOld[1]].Item2 * 100;
                        arguments[10] = argumentsOld[2];
                        arguments[11] = argumentsOld[3];
                        break;
                    case ActionTypes.SetMonkeyFunction:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = monkeyIdsOld[argumentsOld[1]].Item1 + monkeyIdsOld[argumentsOld[1]].Item2 * 100;
                        arguments[2] = argumentsOld[2];
                        if (argumentsOld[2] == 1)
                        {
                            arguments[8] = argumentsOld[3];
                            arguments[9] = argumentsOld[4];
                            arguments[10] = argumentsOld[5];
                            arguments[11] = argumentsOld[6];
                        }
                        else
                        {
                            arguments[10] = argumentsOld[3];
                            arguments[11] = argumentsOld[4];
                        }
                        break;
                    case ActionTypes.PlaceHero:
                        arguments[0] = argumentsOld[0];
                        arguments[8] = argumentsOld[1];
                        arguments[9] = argumentsOld[2];
                        arguments[10] = argumentsOld[3];
                        arguments[11] = argumentsOld[4];
                        break;
                    case ActionTypes.UpgradeHero:
                        arguments[0] = argumentsOld[0];
                        arguments[10] = argumentsOld[1];
                        arguments[11] = argumentsOld[2];
                        break;
                    case ActionTypes.PlaceHeroItem:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        if (argumentsOld[2] == 1)
                        {
                            arguments[8] = argumentsOld[3];
                            arguments[9] = argumentsOld[4];
                            arguments[10] = argumentsOld[5];
                            arguments[11] = argumentsOld[6];
                        }
                        else
                        {
                            arguments[10] = argumentsOld[3];
                            arguments[11] = argumentsOld[4];
                        }
                        break;
                    case ActionTypes.SwitchHeroTarget:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[10] = argumentsOld[2];
                        arguments[11] = argumentsOld[3];
                        break;
                    case ActionTypes.SetHeroFunction:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        if (argumentsOld[2] == 1)
                        {
                            arguments[8] = argumentsOld[3];
                            arguments[9] = argumentsOld[4];
                            arguments[10] = argumentsOld[5];
                            arguments[11] = argumentsOld[6];
                        }
                        else
                        {
                            arguments[10] = argumentsOld[3];
                            arguments[11] = argumentsOld[4];
                        }
                        break;
                    case ActionTypes.SellHero:
                        arguments[0] = argumentsOld[0];
                        arguments[10] = argumentsOld[1];
                        arguments[11] = argumentsOld[2];
                        break;
                    case ActionTypes.MouseClick:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[8] = argumentsOld[2];
                        arguments[9] = argumentsOld[3];
                        arguments[10] = argumentsOld[4];
                        arguments[11] = argumentsOld[5];
                        break;
                    case ActionTypes.AdjustMonkeyCoordinates:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = monkeyIdsOld[argumentsOld[1]].Item1 + monkeyIdsOld[argumentsOld[1]].Item2 * 100;
                        arguments[8] = argumentsOld[2];
                        arguments[9] = argumentsOld[3];
                        arguments[10] = argumentsOld[4];
                        arguments[11] = argumentsOld[5];
                        break;
                    case ActionTypes.WaitMilliseconds:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[10] = argumentsOld[2];
                        arguments[11] = argumentsOld[3];
                        break;
                    case ActionTypes.StartFreeplay:
                    case ActionTypes.EndFreeplay:
                        arguments[0] = argumentsOld[0];
                        arguments[10] = argumentsOld[1];
                        arguments[11] = argumentsOld[2];
                        break;
                    case ActionTypes.Jump:
                        arguments[0] = argumentsOld[0];
                        arguments[1] = argumentsOld[1];
                        arguments[10] = argumentsOld[2];
                        arguments[11] = argumentsOld[3];
                        break;
                    default:
                        break;
                }
                instructionsList.Add(arguments);
            }
            for (int i = 0; i < monkeyCountsOld.Count; i++)
            {
                monkeyCounts.Add(monkeyCountsOld[i]);
            }
            return new ScriptModel(metadata, instructionsList, monkeyCounts, monkeyIds);
        }
    }

    // 旧版ScriptModel
    public class ScriptModelOld
    {
        public Maps SelectedMap { get; set; }
        public LevelDifficulties SelectedDifficulty { get; set; }
        public LevelMode SelectedMode { get; set; }
        public Heroes SelectedHero { get; set; }
        public (int, int) AnchorCoords { get; set; }
        public List<int> ObjectCount { get; set; } // 猴子的数量
        public List<(int, int)> ObjectId { get; set; } // 猴子的ID
        public List<string> Digitalinstructions { get; set; }
    }
}

