using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.Models;
using BTD6AutoCommunity.Models.Instruction;
using BTD6AutoCommunity.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine.InstructionSystem
{
    public class InstructionArgumentDefinition
    {
        public string Name { get; set; }
        public bool IsPassive { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool IsSelectable { get; set; } = false;
        public string Placeholder { get; set; } = "";

        public List<IDisplayItem> Options { get; set; }
        public IDisplayItem SelectedItem { get; set; }
        public string InputText { get; set; } = ""; // 用户手动输入的文本

        public object GetValue()
        {
            try
            {
                if (IsSelectable)
                {
                    return SelectedItem.Value;
                }
                else
                {
                    if (Int32.TryParse(InputText, out int result))
                        return result;
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    public class InstructionTriggerDefinition
    {
        public int RoundTrigger { get; set; }
        public int CoinTrigger { get; set; }

        public string RoundTriggerInputText { get; set; } = "";
        public string CoinTriggerInputText { get; set; } = "";

        public string RoundTriggerText => RoundTrigger.ToString();
        public string CoinTriggerText => CoinTrigger.ToString();

        public int GetRoundTrigger()
        {
            if (Int32.TryParse(RoundTriggerInputText, out int result))
            {
                return result;
            }
            return 0;
        }

        public int GetCoinTrigger()
        {
            if (Int32.TryParse(CoinTriggerInputText, out int result))
            {
                return result;
            }
            return 0;
        }
    }

    public class InstructionCoordinateDefinition
    {
        public bool IsVisible { get; set; } = false;
        public int X { get; set; }
        public int Y { get; set; }
        public string XInputText { get; set; } = "";
        public string YInputText { get; set; } = "";
        public string XText => X.ToString();
        public string YText => Y.ToString();

        public (int X, int Y) GetCoordinate()
        {
            if (Int32.TryParse(XInputText, out int x) && Int32.TryParse(YInputText, out int y))
            {
                return (x, y);
            }
            return (-1, -1);
        }
    }

    // 每个指令可能有多个参数
    public class InstructionDefinition
    {
        public ActionTypes Action { get; set; }                       // 指令名称

        public List<InstructionArgumentDefinition> Arguments { get; set; }
        public InstructionTriggerDefinition Trigger { get; set; }
        public InstructionCoordinateDefinition Coordinate { get; set; }
    }

    public class InstructionArgumentHandler
    {
        private readonly IScriptService scriptService;
        private Dictionary<ActionTypes, Func<Instruction, InstructionDefinition>> instructionMap;

        public InstructionArgumentHandler(IScriptService scriptService)
        {
            this.scriptService = scriptService;
            RegisterDefaultInstructions();
        }

        private void RegisterDefaultInstructions()
        {
            instructionMap = new Dictionary<ActionTypes, Func<Instruction, InstructionDefinition>>
            {
                { ActionTypes.PlaceMonkey, HandlePlaceMonkeyControls },
                { ActionTypes.UpgradeMonkey, HandleUpgradeMonkeyControls },
                { ActionTypes.SwitchMonkeyTarget, HandleSwitchMonkeyTargetControls },
                { ActionTypes.SetMonkeyFunction, HandleSetMonkeyFunctionControls },
                { ActionTypes.AdjustMonkeyCoordinates, HandleAdjustMonkeyCoordinatesControls },
                { ActionTypes.SellMonkey, HandleSellMonkeyControls },
                { ActionTypes.PlaceHero, HandlePlaceHeroControls },
                { ActionTypes.UpgradeHero, HandleUpgradeHeroControls },
                { ActionTypes.PlaceHeroItem, HandlePlaceHeroObjectControls },
                { ActionTypes.SwitchHeroTarget, HandleSwitchHeroTargetControls },
                { ActionTypes.SetHeroFunction, HandleSetHeroFunctionControls },
                { ActionTypes.SellHero, HandleSellHeroControls },
                { ActionTypes.UseAbility, HandleUseAbilityControls },
                { ActionTypes.SwitchSpeed, HandleSwitchSpeedControls },
                { ActionTypes.MouseClick, HandleMouseClickControls },
                { ActionTypes.WaitMilliseconds, HandleWaitMillisecondsControls },
                { ActionTypes.StartFreeplay, HandleStartFreeplayControls },
                { ActionTypes.EndFreeplay, HandleEndFreeplayControls },
                { ActionTypes.Jump, HandleJumpControls },
                { ActionTypes.InstructionsBundle, HandleInstructionsBundleControls }
            };
        }

        private InstructionDefinition HandlePlaceMonkeyControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.PlaceMonkey,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.MonkeysList.Select(m => new DisplayItem<Monkeys>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<Monkeys>((Monkeys)inst[1]) : new DisplayItem<Monkeys>(Constants.MonkeysList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "放置检测",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.PlaceChecksList.Select(m => new DisplayItem<PlaceCheckTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<PlaceCheckTypes>((PlaceCheckTypes)(inst[2] != -1 ? inst[2] : 0)) : new DisplayItem<PlaceCheckTypes>(Constants.PlaceChecksList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = true,
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleUpgradeMonkeyControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.UpgradeMonkey,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = scriptService.GetInstructionsMonkeyIds().Select(m => new DisplayItem<MonkeyId>(new MonkeyId { Id = m })).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyId>(new MonkeyId { Id = inst[1] }) : new DisplayItem<MonkeyId>(new MonkeyId { Id = scriptService.GetInstructionsMonkeyIds().FirstOrDefault() }),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "升级类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.UpgradePathsList.Select(m => new DisplayItem<UpgradeTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<UpgradeTypes>((UpgradeTypes)inst[2]) : new DisplayItem<UpgradeTypes>(Constants.UpgradePathsList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false,
                }
            };
            return result;
        }

        private InstructionDefinition HandleSwitchMonkeyTargetControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SwitchMonkeyTarget,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = scriptService.GetInstructionsMonkeyIds().Select(m => new DisplayItem<MonkeyId>(new MonkeyId { Id = m })).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyId>(new MonkeyId { Id = inst[1] }) : new DisplayItem<MonkeyId>(new MonkeyId { Id = scriptService.GetInstructionsMonkeyIds().FirstOrDefault() }),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "目标类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.TargetsList.Select(m => new DisplayItem<TargetTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<TargetTypes>((TargetTypes)inst[2]) : new DisplayItem<TargetTypes>(Constants.TargetsList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSetMonkeyFunctionControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SetMonkeyFunction,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = scriptService.GetInstructionsMonkeyIds().Select(m => new DisplayItem<MonkeyId>(new MonkeyId { Id = m })).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyId>(new MonkeyId { Id = inst[1] }) : new DisplayItem<MonkeyId>(new MonkeyId { Id = scriptService.GetInstructionsMonkeyIds().FirstOrDefault() }),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "功能类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.MonkeyFunctionsList.Select(m => new DisplayItem<MonkeyFunctionTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyFunctionTypes>((MonkeyFunctionTypes)inst[2]) : new DisplayItem<MonkeyFunctionTypes>(Constants.MonkeyFunctionsList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleAdjustMonkeyCoordinatesControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.AdjustMonkeyCoordinates,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = scriptService.GetInstructionsMonkeyIds().Select(m => new DisplayItem<MonkeyId>(new MonkeyId { Id = m })).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyId>(new MonkeyId { Id = inst[1] }) : new DisplayItem<MonkeyId>(new MonkeyId { Id = scriptService.GetInstructionsMonkeyIds().FirstOrDefault() }),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = true,
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSellMonkeyControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SellMonkey,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "猴子名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = scriptService.GetInstructionsMonkeyIds().Select(m => new DisplayItem<MonkeyId>(new MonkeyId { Id = m })).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyId>(new MonkeyId { Id = inst[1] }) : new DisplayItem<MonkeyId>(new MonkeyId { Id = scriptService.GetInstructionsMonkeyIds().FirstOrDefault() }),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false,
                }
            };
            return result;
        }

        private InstructionDefinition HandlePlaceHeroControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.PlaceHero,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = true,
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleUpgradeHeroControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.UpgradeHero,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false,
                }
            };
            return result;
        }

        private InstructionDefinition HandlePlaceHeroObjectControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.PlaceHeroItem,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "物品类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.HeroObjectsList.Select(m => new DisplayItem<HeroObjectTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<HeroObjectTypes>((HeroObjectTypes)inst[1]) : new DisplayItem<HeroObjectTypes>(Constants.HeroObjectsList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "坐标类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.CoordinatesList.Select(m => new DisplayItem<CoordinateTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<CoordinateTypes>((CoordinateTypes)inst[2]) : new DisplayItem<CoordinateTypes>(Constants.CoordinatesList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSwitchHeroTargetControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SwitchHeroTarget,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "目标类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.TargetsList.Select(m => new DisplayItem<TargetTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<TargetTypes>((TargetTypes)inst[1]) : new DisplayItem<TargetTypes>(Constants.TargetsList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSetHeroFunctionControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SetHeroFunction,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "功能类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.MonkeyFunctionsList.Select(m => new DisplayItem<MonkeyFunctionTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<MonkeyFunctionTypes>((MonkeyFunctionTypes)inst[1]) : new DisplayItem<MonkeyFunctionTypes>(Constants.MonkeyFunctionsList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSellHeroControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SellHero,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false,
                }
            };
            return result;
        }

        private InstructionDefinition HandleUseAbilityControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.UseAbility,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "技能类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.SkillsList.Select(m => new DisplayItem<SkillTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<SkillTypes>((SkillTypes)inst[1]) : new DisplayItem<SkillTypes>(Constants.SkillsList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "坐标类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.CoordinatesList.Select(m => new DisplayItem<CoordinateTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<CoordinateTypes>((CoordinateTypes)inst[2]) : new DisplayItem<CoordinateTypes>(Constants.CoordinatesList.FirstOrDefault()),
                        Placeholder = ""
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = inst != null && inst.Coordinates != (-1, -1),
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleSwitchSpeedControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.SwitchSpeed,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "速度类型",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = Constants.SpeedsList.Select(m => new DisplayItem<SpeedTypes>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = inst != null ? new DisplayItem<SpeedTypes>((SpeedTypes)inst[1]) : new DisplayItem<SpeedTypes>(Constants.SpeedsList.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        private InstructionDefinition HandleMouseClickControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.MouseClick,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "点击次数",
                        IsPassive = inst != null,
                        IsVisible = true,
                        IsSelectable = false,
                        InputText = inst != null ? inst[1].ToString() : "输入点击次数",
                        Placeholder = "输入点击次数"
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = true,
                    X = inst == null ? -1 : inst.Coordinates.X,
                    Y = inst == null ? -1 : inst.Coordinates.Y
                }
            };
            return result;
        }

        private InstructionDefinition HandleWaitMillisecondsControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.WaitMilliseconds,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "等待时间(ms)",
                        IsPassive = inst != null,
                        IsVisible = true,
                        IsSelectable = false,
                        InputText = inst != null ? inst[1].ToString() : "输入等待时间(ms)",
                        Placeholder = "输入等待时间(ms)"
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        private InstructionDefinition HandleStartFreeplayControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.StartFreeplay,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        private InstructionDefinition HandleEndFreeplayControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.EndFreeplay,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        private InstructionDefinition HandleJumpControls(Instruction inst = null)
        {
            var result = new InstructionDefinition
            {
                Action = ActionTypes.Jump,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "跳转指令行号",
                        IsPassive = inst != null,
                        IsVisible = true,
                        IsSelectable = false,
                        InputText = inst != null ? inst[1].ToString() : "输入跳转指令行号",
                        Placeholder = "输入跳转指令行号"
                    },
                    new InstructionArgumentDefinition
                    {
                        IsVisible = false
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        private InstructionDefinition HandleInstructionsBundleControls(Instruction inst = null)
        {
            InstructionsBundle instructionsBundle = new InstructionsBundle();
            var result = new InstructionDefinition
            {
                Action = ActionTypes.InstructionsBundle,
                Arguments = new List<InstructionArgumentDefinition>
                {
                    new InstructionArgumentDefinition
                    {
                        Name = "指令集名称",
                        IsVisible = true,
                        IsSelectable = true,
                        Options = instructionsBundle.BundleNames.Select(m => new DisplayItem<string>(m)).Cast<IDisplayItem>().ToList(),
                        SelectedItem = new DisplayItem<string>(instructionsBundle.BundleNames.FirstOrDefault()),
                        Placeholder = ""
                    },
                    new InstructionArgumentDefinition
                    {
                        Name = "添加数量",
                        IsPassive = false,
                        IsVisible = true,
                        IsSelectable = false,
                        InputText = inst != null ? inst[2].ToString() : "输入添加数量",
                        Placeholder = "输入添加数量"
                    }
                },
                Trigger = new InstructionTriggerDefinition
                {
                    RoundTrigger = inst == null ? 0 : inst.RoundTrigger,
                    CoinTrigger = inst == null ? 0 : inst.CoinTrigger
                },
                Coordinate = new InstructionCoordinateDefinition
                {
                    IsVisible = false
                }
            };
            return result;
        }

        public InstructionDefinition GetDefinition(ActionTypes action, Instruction inst = null)
        {
            if (instructionMap.TryGetValue(action, out var handler))
            {
                return handler.Invoke(inst);
            }
            return null;
        }
    }
}
