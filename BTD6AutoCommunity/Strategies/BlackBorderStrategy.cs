using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Strategies
{

    public class BlackBorderStrategyInfo
    {
        // 挑战失败的脚本
        public HashSet<string> FailedScripts {  get; set; } = new HashSet<string>();
        // 已通过的脚本
        public HashSet<string> PassedScripts { get; set; } = new HashSet<string>();
        // 加载失败或缺失的脚本
        public HashSet<string> FailedScriptsToLoad {  get; set; } = new HashSet<string>();
        // 加载成功但未挑战的脚本
        public HashSet<string> UnChallengingScripts {  get; set; } = new HashSet<string>();

        public bool IsValid()
        {
            return FailedScripts != null &&
                   PassedScripts != null &&
                   FailedScriptsToLoad != null &&
                   UnChallengingScripts != null;
        }
    }

    public class BlackBorderStrategy : Base.BaseStrategy
    {
        // 地图选择依赖
        private bool IsMapSelectionComplete = false;
        // 选择地图重试次数
        private int mapReTryCount = 0;
        private bool IsHeroSelectionComplete = false;
        private int heroReTryCount = 0;
        private int modeReTryCount = 0;
        private int levelChallengingCount = 0;
        private int returnableScreenCount = 0;

        private Heroes currentHero = Heroes.Unkown;

        private readonly List<(LevelDifficulties levelDifficulties, LevelModes levelModes)> scriptSequence;
        private int scriptIndex = 0;
        private readonly List<Maps> mapList;
        private int mapIndex = 0;

        private MapInfo mapInfo = new MapInfo();

        private BlackBorderStrategyInfo strategyInfo = new BlackBorderStrategyInfo();


        public BlackBorderStrategy(LogHandler logHandler) : base(logHandler)
        {
            DefaultDataReadInterval = 1000;
            DefaultOperationInterval = 200;
            scriptSequence = new List<(LevelDifficulties, LevelModes)>()
            {
                (LevelDifficulties.Easy, LevelModes.Standard),
                (LevelDifficulties.Easy, LevelModes.PrimaryOnly),
                (LevelDifficulties.Easy, LevelModes.Deflation),
                (LevelDifficulties.Medium, LevelModes.Standard),
                (LevelDifficulties.Medium, LevelModes.MilitaryOnly),
                (LevelDifficulties.Medium, LevelModes.Apopalypse),
                (LevelDifficulties.Medium, LevelModes.Reverse),
                (LevelDifficulties.Hard, LevelModes.Standard),
                (LevelDifficulties.Hard, LevelModes.MagicMonkeysOnly),
                (LevelDifficulties.Hard, LevelModes.DoubleHpMoabs),
                (LevelDifficulties.Hard, LevelModes.HalfCash),
                (LevelDifficulties.Hard, LevelModes.AlternateBloonsRounds),
                (LevelDifficulties.Hard, LevelModes.Impoppable),
                (LevelDifficulties.Hard, LevelModes.CHIMPS)
            };
            mapList = new List<Maps>(Constants.MapsList);

            LoadInfo();
        }

        private void LoadInfo()
        {
            if (!File.Exists(@"config\BlackBorderStrategyInfo.json"))
            {
                SaveInfo();
                return;
            }
            string json = File.ReadAllText(@"config\BlackBorderStrategyInfo.json");
            strategyInfo = JsonConvert.DeserializeObject<BlackBorderStrategyInfo>(json);
            if (strategyInfo == null)
            {
                strategyInfo = new BlackBorderStrategyInfo();
                SaveInfo();
            }
        }

        private void SaveInfo()
        {
            string json = JsonConvert.SerializeObject(strategyInfo, Formatting.Indented);
            File.WriteAllText(@"config\BlackBorderStrategyInfo.json", json);
        }

        protected override void OnPostStop()
        {
            SaveInfo();
            _logs.Log("黑框策略执行终止", LogLevel.Info);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=============================");
            sb.AppendLine("刷黑框策略执行完毕，开始统计结果");
            sb.AppendLine("=============================");
            sb.AppendLine("通过的脚本：");
            foreach (string script in strategyInfo.PassedScripts)
            {
                sb.AppendLine(script);
            }
            sb.AppendLine("=============================");
            sb.AppendLine("未通过的脚本：");
            foreach (string script in strategyInfo.FailedScripts)
            {
                sb.AppendLine(script);
            }
            sb.AppendLine("=============================");
            sb.AppendLine("加载失败的脚本：");
            foreach (string script in strategyInfo.FailedScriptsToLoad)
            {
                sb.AppendLine(script);
            }
            sb.AppendLine("=============================");
            sb.AppendLine("未挑战的脚本：");
            foreach (string script in strategyInfo.UnChallengingScripts)
            {
                sb.AppendLine(script);
            }
            _logs.Log(sb.ToString(), LogLevel.Info);
        }

        private bool NextScript()
        {
            scriptIndex++;
            if (scriptIndex >= scriptSequence.Count)
            {
                mapIndex++;
                scriptIndex = 0;
                if (mapIndex >= mapList.Count)
                {
                    Stop();
                    return false;
                }
            }
            ResetCurrentScript();
            return true;
        }

        private void NextMap()
        {
            mapIndex++;
            scriptIndex = 0;
            if (mapIndex >= mapList.Count)
            {
                Stop();
                return;
            }
            ResetCurrentScript();
        }

        private void ResetCurrentScript()
        {
            scriptMetadata = null;
            executableInstructions?.Clear();
            executableInstructions = null;
        }

        private bool LoadScript()
        {
            string mapName = Constants.GetTypeName(mapList[mapIndex]);
            string difficultyName = Constants.GetTypeName(scriptSequence[scriptIndex].levelDifficulties);
            string modeName = Constants.GetTypeName(scriptSequence[scriptIndex].levelModes);
            string scriptName = modeName + "-黑框";
            string scriptPath = ScriptFileManager.GetScriptFullPath(mapName, difficultyName, scriptName);
            if (mapInfo.GetBadgeStatus(mapList[mapIndex], scriptSequence[scriptIndex].levelDifficulties, scriptSequence[scriptIndex].levelModes))
            {
                _logs.Log($"地图{mapName}已通过{Constants.GetTypeName(scriptSequence[scriptIndex].levelDifficulties)}难度的{Constants.GetTypeName(scriptSequence[scriptIndex].levelModes)}模式，无需重复挑战，开始尝试下一脚本！", LogLevel.Info);
                if (!NextScript()) return true;
                return false;
            }
            if (!GetExecutableInstructions(scriptPath, false))
            {
                _logs.Log($"脚本{scriptPath}加载失败：开始下一脚本加载！", LogLevel.Warning);
                strategyInfo.FailedScriptsToLoad.Add(scriptPath);
                if (!NextScript()) return true;
                return false;
            }
            if (strategyInfo.PassedScripts.Contains(scriptMetadata.ToString()))
            {
                _logs.Log($"脚本{scriptMetadata}已通过，开始下一脚本！", LogLevel.Info);
                if (!NextScript()) return true;
                return false;
            }
            _logs.Log($"脚本{scriptMetadata}加载成功！", LogLevel.Info);
            return true;
        }

        private void InitializeRelys()
        {
            mapReTryCount = 0;
            heroReTryCount = 0;
            modeReTryCount = 0;
            levelChallengingCount = 0;
            returnableScreenCount = 0;
            IsMapSelectionComplete = false;
            IsHeroSelectionComplete = false;
            IsStrategyExecutionCompleted = false;
        }

        protected override void InitializeStateHandlers()
        {
            stateHandlers = new Dictionary<GameState, Action>
            {
                { GameState.GameMainScreen, HandleMainScreen },
                { GameState.RaceResultsScreen, HandleRaceResultsScreen },
                { GameState.BossResultsScreen, HandleBossResultsScreen },
                { GameState.LevelSelectionScreen, HandleLevelSelection },
                { GameState.LevelSearchScreen, HandleReturnableScreen },
                { GameState.LevelSearchedScreen, HandleReturnableScreen },
                { GameState.LevelDifficultySelectionScreen, HandleLevelDifficultySelection },
                { GameState.LevelEasyModeSelectionScreen, HandleLevelEasyModeSelection },
                { GameState.LevelMediumModeSelectionScreen, HandleLevelMediumModeSelection },
                { GameState.LevelHardModeSelectionScreen, HandleLevelHardModeSelection },
                { GameState.HeroSelectionScreen, HandleHeroSelection },
                { GameState.LevelTipScreen, HandleLevelTipScreen },
                { GameState.LevelChallengingScreen, HandleLevelChallengingScreen },
                { GameState.LevelChallengingWithTipScreen, HandleLevelChallengingWithTipScreen },
                { GameState.LevelSettingScreen, HandleLevelSettingScreen },
                { GameState.LevelPassedScreen, HandleLevelPassScreen },
                { GameState.LevelSettlementScreen, HandleLevelSettlementScreen },
                { GameState.LevelFailedScreen,  HandleLevelFailedScreen },
                { GameState.LevelUpgradingScreen, HandleLevelUpgradingScreen },
                { GameState.ReturnableScreen, HandleReturnableScreen },
                { GameState.CollectionActivitiesAvailableScreen, HandleChestCollection },
                { GameState.CollectionActivitiesScreen, HandleReturnableScreen },
                { GameState.ThreeChestsScreen, HandleThreeChestsScreen },
                { GameState.TwoChestsScreen, HandleTwoChestsScreen },
                { GameState.InstaScreen, HandleInstaScreen },
                { GameState.ChestsOpenedScreen, HandleChestsOpenedScreen }
                //{ GameState.UnKnown, HandleReturnableScreen }

                // 添加更多状态处理...
            };
        }

        private void HandleMainScreen()
        {
            InitializeRelys();
            while (!LoadScript()) { }
            EchoScript();
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 940);
        }

        private void HandleRaceResultsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 800);
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
        }

        private void HandleBossResultsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 880);
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
            Thread.Sleep(500);
            Return();
        }

        private void HandleLevelSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，无法选择难度，返回", LogLevel.Warning);
                return;
            }
            MapTypes mapType = Constants.GetMapType(scriptMetadata.SelectedMap);
            Point mapTypePos = Constants.GetMapTypePos(mapType);
            Point mapPos = GameVisionRecognizer.GetMapPos(_context, (int)scriptMetadata.SelectedMap);
            if (mapPos.X == -1)
            {
                if (mapReTryCount > 8)
                {
                    mapReTryCount = 0;
                    NextMap();
                    strategyInfo.UnChallengingScripts.Add(scriptMetadata.ToString());
                    _logs.Log($"未找到地图位置, 开始尝试下一张地图：{Constants.GetTypeName(mapList[mapIndex])}！", LogLevel.Warning);
                    return;
                }
                mapReTryCount++;
                InputSimulator.MouseMoveAndLeftClick(_context, mapTypePos.X, mapTypePos.Y);
                return;
            }
            if (!mapInfo.IsMapChecked(scriptMetadata.SelectedMap))
            {
                Badges badges = GameVisionRecognizer.GetMapBadges(_context, GameVisionRecognizer.GetMapEreaIndex(mapPos), screenshotCapturer.CurrentScreenshot);
                _logs.Log($"地图徽章信息：{badges}", LogLevel.Info);
                mapInfo.SetBadges(scriptMetadata.SelectedMap, badges);
                mapInfo.SetMapChecked(scriptMetadata.SelectedMap, true);
            }
            else             
            {
                Badges badges = mapInfo.GetBadges(scriptMetadata.SelectedMap);
                if (badges.GetBadgeStatus(scriptMetadata.SelectedDifficulty, scriptMetadata.SelectedMode))
                {
                    _logs.Log($"地图已通过{Constants.GetTypeName(scriptMetadata.SelectedDifficulty)}难度的{Constants.GetTypeName(scriptMetadata.SelectedMode)}模式，无需重复挑战，开始尝试下一脚本！", LogLevel.Info);
                    NextScript();
                    return;
                }
                InputSimulator.MouseMoveAndLeftClick(_context, mapPos.X, mapPos.Y);
                IsMapSelectionComplete = true;
                _logs.Log($"已选择地图：{Constants.GetTypeName(scriptMetadata.SelectedMap)}", LogLevel.Info);
            }
        }

        private void HandleLevelDifficultySelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，返回", LogLevel.Warning);
                return;
            }
            if (!IsMapSelectionComplete)
            {
                Return();
                _logs.Log("地图选择未完成，无法选择难度，返回", LogLevel.Warning);
                return;
            }
            switch (scriptMetadata.SelectedDifficulty)
            {
                case LevelDifficulties.Easy:
                    InputSimulator.MouseMoveAndLeftClick(_context, 630, 400);
                    break;
                case LevelDifficulties.Medium:
                    InputSimulator.MouseMoveAndLeftClick(_context, 970, 400);
                    break;
                case LevelDifficulties.Hard:
                    InputSimulator.MouseMoveAndLeftClick(_context, 1300, 400);
                    break;
            }
        }

        private void HandleLevelEasyModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，无法进入简单模式，返回", LogLevel.Warning);
                return;
            }
            if (!IsMapSelectionComplete)
            {
                Return();
                _logs.Log("地图选择未完成，无法进入简单模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelModes.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Easy)
            {
                Return();
                _logs.Log("当前模式不是简单模式，无法进入简单模式，返回", LogLevel.Warning);
                return;
            }
            // 若英雄已经选好了，选择挑战关卡
            if (!IsHeroSelectionComplete && scriptMetadata.SelectedHero != currentHero)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
                return;
            }
            ChooseMode(LevelDifficulties.Easy);
        }

        private void HandleLevelMediumModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，无法进入中级模式，返回", LogLevel.Warning);
                return;
            }
            if (!IsMapSelectionComplete)
            {
                Return();
                _logs.Log("地图选择未完成，无法进入中级模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelModes.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Medium)
            {
                Return();
                _logs.Log("当前模式不是中级模式，无法进入中级模式，返回", LogLevel.Warning);
                return;
            }
            if (!IsHeroSelectionComplete && scriptMetadata.SelectedHero != currentHero)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
                return;
            }
            ChooseMode(LevelDifficulties.Medium);
        }

        private void HandleLevelHardModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，无法进入困难模式，返回", LogLevel.Warning);
                return;
            }
            if (!IsMapSelectionComplete)
            {
                Return();
                _logs.Log("地图选择未完成，无法进入困难模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelModes.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Hard)
            {
                Return();
                _logs.Log("当前模式不是困难模式，无法进入困难模式，返回", LogLevel.Warning);
                return;
            }
            if (!IsHeroSelectionComplete && scriptMetadata.SelectedHero != currentHero)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
                return;
            }
            ChooseMode(LevelDifficulties.Hard);
        }

        private void ChooseMode(LevelDifficulties difficulties)
        {
            IsHeroSelectionComplete = false;
            Point point = Constants.GetLevelModePos(scriptMetadata.SelectedMode);
            InputSimulator.MouseMoveAndLeftClick(_context, point.X, point.Y);

            modeReTryCount++;
            if (modeReTryCount > 2)
            {
                modeReTryCount = 0;
                strategyInfo.UnChallengingScripts.Add(scriptMetadata.ToString());
                _logs.Log($"未找到{Constants.GetTypeName(difficulties)}模式：{Constants.GetTypeName(scriptMetadata.SelectedMode)}位置，请确认是否已解锁，开始尝试下一脚本！", LogLevel.Warning);
                NextScript();
                return;
            }
        }

        private void HandleHeroSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                Return();
                _logs.Log("脚本未加载，无法选择英雄，返回", LogLevel.Warning);
                return;
            }
            if (IsHeroSelectionComplete || scriptMetadata == null)
            {
                Return();
                _logs.Log("英雄选择已完成，返回", LogLevel.Warning);
                return;
            }
            Point heroPosition = GameVisionRecognizer.GetHeroPosition(_context, scriptMetadata.SelectedHero);
            if (heroPosition.X == -1)
            {
                
                if (heroReTryCount > 7)
                {
                    heroReTryCount = 0;
                    NextScript();
                    strategyInfo.UnChallengingScripts.Add(scriptMetadata.ToString());
                    _logs.Log("未找到英雄位置，请确认英雄是否已解锁，开始尝试下一脚本！", LogLevel.Warning);
                    return;
                }
                heroReTryCount++;
                InputSimulator.MouseWheel(-10);
                return;
            }
            InputSimulator.MouseMoveAndLeftClick(_context, heroPosition.X, heroPosition.Y);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 1120, 620);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
            IsHeroSelectionComplete = true;
            currentHero = scriptMetadata.SelectedHero;
            _logs.Log($"已选择英雄：{Constants.GetTypeName(scriptMetadata.SelectedHero)}", LogLevel.Info);
        }

        private void HandleLevelTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 1140, 730);
        }

        private void HandleLevelChallengingScreen()
        {
            levelChallengingCount++;
            if (scriptMetadata == null)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("脚本未加载，无法进入战斗，返回", LogLevel.Warning);
                return;
            }
            if (!IsMapSelectionComplete)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("地图选择未完成，无法进入战斗，返回", LogLevel.Warning);
                return;
            }
            if (levelChallengingCount < 3)
            {
                return;
            }
            if (IsStrategyExecutionCompleted)
            {
                StopLevelTimer();
                return;
            }
            StartLevelTimer(0, _settings.EnableRecommendInterval);
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelSettingScreen()
        {
        }

        private void HandleLevelPassScreen()
        {
            Point returnPos = GameVisionRecognizer.GetSettlementScreenReturnPosition(_context);
            InputSimulator.MouseMoveAndLeftClick(_context, returnPos.X, returnPos.Y);
        }

        private void HandleLevelSettlementScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 910);
            StopLevelTimer();
            if (IsStrategyExecutionCompleted)
            {
                IsStrategyExecutionCompleted = false;
            }
            strategyInfo.PassedScripts.Add(scriptMetadata.ToString());
            NextScript();
            _logs.Log($"进入关卡结算界面，挑战成功，停止策略执行, 本关用时：{(int)(levelChallengingCount * 1.5 / 60)}分{(int)(levelChallengingCount * 1.5 % 60)}秒", LogLevel.Info);
        }

        private void HandleLevelFailedScreen()
        {
            StopLevelTimer();
            Point returnPos = GameVisionRecognizer.GetFailedScreenReturnPosition(_context);
            InputSimulator.MouseMoveAndLeftClick(_context, returnPos.X, returnPos.Y);
            strategyInfo.FailedScripts.Add(scriptMetadata.ToString());
            _logs.Log($"检测到关卡失败界面，失败脚本：{scriptMetadata.ScriptName}。回到主页，开始下一脚本！", LogLevel.Warning);
            NextScript();
        }

        private void HandleLevelUpgradingScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 980);
            _logs.Log("检测到升级界面，点击确认", LogLevel.Info);
        }

        private void HandleReturnableScreen()
        {
            returnableScreenCount++;
            if (returnableScreenCount < 2) return;
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
            returnableScreenCount = 0;
        }

        private void Return()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
        }

        private void HandleChestCollection()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 680);
            _logs.Log("收集宝箱可打开，开始开箱", LogLevel.Info);
        }

        private void HandleThreeChestsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            _logs.Log("获得3个insta", LogLevel.Info);
        }

        private void HandleTwoChestsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1110, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1110, 540);
            Thread.Sleep(1000);
            _logs.Log("获得2个insta", LogLevel.Info);
        }

        private void HandleInstaScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
        }

        private void HandleChestsOpenedScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 1000);
        }

    }
}
