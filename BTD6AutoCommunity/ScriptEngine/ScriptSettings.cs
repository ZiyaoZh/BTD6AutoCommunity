using Newtonsoft.Json;
using System.IO;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using BTD6AutoCommunity.Core;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptSettings
    {
        private const string ConfigPath = @"Settings.json";
        public int DataReadInterval { get; set; }

        public int OperationInterval { get; set; }

        public int GameDpi { get; set; }

        public bool EnableDoubleCoin { get; set; }

        public bool EnableFastPath { get; set; }

        public bool EnableLogging { get; set; }

        public bool EnableRecommendInterval { get; set; }

        public Dictionary<Monkeys, HotKey> UserMonkeyHotKey { get; set; }

        public Dictionary<SkillTypes, HotKey> UserSkillHotKey { get; set; }

        public Dictionary<HeroObjectTypes, HotKey> UserHeroObjectHotKey { get; set; }

        public Dictionary<HotkeyAction, HotKey> UserActionHotKey { get; set; }

        private static Dictionary<Monkeys, HotKey> DefaultMonkeyHotKey => new Dictionary<Monkeys, HotKey>
        {
            { Monkeys.DartMonkey, new HotKey(Keys.Q) },
            { Monkeys.BoomerangMonkey, new HotKey(Keys.W) },
            { Monkeys.BombShooter, new HotKey(Keys.E) },
            { Monkeys.TackShooter, new HotKey(Keys.R) },
            { Monkeys.IceMonkey, new HotKey(Keys.T) },
            { Monkeys.GlueGunner, new HotKey(Keys.Y) },
            { Monkeys.Desperado, new HotKey(Keys.None) },
            { Monkeys.SniperMonkey, new HotKey(Keys.Z) },
            { Monkeys.MonkeySub, new HotKey(Keys.X) },
            { Monkeys.MonkeyBuccaneer, new HotKey(Keys.C) },
            { Monkeys.MonkeyAce, new HotKey(Keys.V) },
            { Monkeys.HeliPilot, new HotKey(Keys.B) },
            { Monkeys.MortarMonkey, new HotKey(Keys.N) },
            { Monkeys.DartlingGunner, new HotKey(Keys.M) },
            { Monkeys.WizardMonkey, new HotKey(Keys.A) },
            { Monkeys.SuperMonkey, new HotKey(Keys.S) },
            { Monkeys.NinjaMonkey, new HotKey(Keys.D) },
            { Monkeys.Alchemist, new HotKey(Keys.F) },
            { Monkeys.Druid, new HotKey(Keys.G) },
            { Monkeys.MerMonkey, new HotKey(Keys.O) },
            { Monkeys.BananaFarm, new HotKey(Keys.H) },
            { Monkeys.SpikeFactory, new HotKey(Keys.J) },
            { Monkeys.MonkeyVillage, new HotKey(Keys.K) },
            { Monkeys.EngineerMonkey, new HotKey(Keys.L) },
            { Monkeys.BeastHandler, new HotKey(Keys.I) }
        };

        private static Dictionary<SkillTypes, HotKey> DefaultSkillHotKey => new Dictionary<SkillTypes, HotKey>
        {
            { SkillTypes.Skill1, new HotKey(Keys.D1) },
            { SkillTypes.Skill2, new HotKey(Keys.D2) },
            { SkillTypes.Skill3, new HotKey(Keys.D3) },
            { SkillTypes.Skill4, new HotKey(Keys.D4) },
            { SkillTypes.Skill5, new HotKey(Keys.D5) },
            { SkillTypes.Skill6, new HotKey(Keys.D6) },
            { SkillTypes.Skill7, new HotKey(Keys.D7) },
            { SkillTypes.Skill8, new HotKey(Keys.D8) },
            { SkillTypes.Skill9, new HotKey(Keys.D9) },
            { SkillTypes.Skill10, new HotKey(Keys.D0) },
            { SkillTypes.Skill11, new HotKey(Keys.OemMinus) },
            { SkillTypes.Skill12, new HotKey(Keys.Oemplus) }
        };

        private static Dictionary<HeroObjectTypes, HotKey> DefaultHeroObjectHotKey => new Dictionary<HeroObjectTypes, HotKey>
        {
            { HeroObjectTypes.HeroObject1, new HotKey(Keys.Q, shift: true) },
            { HeroObjectTypes.HeroObject2, new HotKey(Keys.W, shift: true) },
            { HeroObjectTypes.HeroObject3, new HotKey(Keys.E, shift: true) },
            { HeroObjectTypes.HeroObject4, new HotKey(Keys.R, shift: true) },
            { HeroObjectTypes.HeroObject5, new HotKey(Keys.T, shift: true) },
            { HeroObjectTypes.HeroObject6, new HotKey(Keys.A, shift: true) },
            { HeroObjectTypes.HeroObject7, new HotKey(Keys.S, shift: true) },
            { HeroObjectTypes.HeroObject8, new HotKey(Keys.D, shift: true) },
            { HeroObjectTypes.HeroObject9, new HotKey(Keys.F, shift: true) },
            { HeroObjectTypes.HeroObject10, new HotKey(Keys.G, shift: true) },
            { HeroObjectTypes.HeroObject11, new HotKey(Keys.H, shift: true) },
            { HeroObjectTypes.HeroObject12, new HotKey(Keys.Z, shift: true) },
            { HeroObjectTypes.HeroObject13, new HotKey(Keys.X, shift: true) },
            { HeroObjectTypes.HeroObject14, new HotKey(Keys.C, shift: true) },
            { HeroObjectTypes.HeroObject15, new HotKey(Keys.V, shift: true) },
            { HeroObjectTypes.HeroObject16, new HotKey(Keys.B, shift: true) }
        };

        private static Dictionary<HotkeyAction, HotKey> DefaultHotKey => new Dictionary<HotkeyAction, HotKey>
        {
            { HotkeyAction.Hero, new HotKey(Keys.U) },
            { HotkeyAction.Sell, new HotKey(Keys.Back ) },
            { HotkeyAction.UpgradeTopPath, new HotKey(Keys.Oemcomma) },
            { HotkeyAction.UpgradeMiddlePath, new HotKey(Keys.OemPeriod) },
            { HotkeyAction.UpgradeBottomPath, new HotKey(Keys.OemQuestion) },
            { HotkeyAction.SwitchTarget, new HotKey(Keys.Tab) },
            { HotkeyAction.ReverseSwitchTarget, new HotKey(Keys.Tab, control: true) },
            { HotkeyAction.SetFunction1, new HotKey(Keys.PageDown) },
            { HotkeyAction.SetFunction2, new HotKey(Keys.PageUp) },
            { HotkeyAction.ChangeSpeed, new HotKey(Keys.Space) },
            { HotkeyAction.NextRound, new HotKey(Keys.Space, shift: true) }

        };

        public ScriptSettings()
        {
            // 初始化时直接使用静态默认热键
            UserMonkeyHotKey = new Dictionary<Monkeys, HotKey>(DefaultMonkeyHotKey);
            UserSkillHotKey = new Dictionary<SkillTypes, HotKey>(DefaultSkillHotKey);
            UserHeroObjectHotKey = new Dictionary<HeroObjectTypes, HotKey>(DefaultHeroObjectHotKey);
            UserActionHotKey = new Dictionary<HotkeyAction, HotKey>(DefaultHotKey);
            // 核心设置默认值
            DataReadInterval = 1000;  // 默认1秒
            OperationInterval = 200;  // 默认0.5秒
            GameDpi = 1;              // 默认1920x1080
            EnableDoubleCoin = false;
            EnableFastPath = false;
            EnableLogging = true;
            EnableRecommendInterval = true;
        }

        public void RestoreDefaultHotKey()
        {
            //Debug.WriteLine("DartMonkey: " + DefaultHotKey[HotkeyAction.DartMonkey].ToString());
            UserMonkeyHotKey = new Dictionary<Monkeys, HotKey>(DefaultMonkeyHotKey);
            UserSkillHotKey = new Dictionary<SkillTypes, HotKey>(DefaultSkillHotKey);
            UserHeroObjectHotKey = new Dictionary<HeroObjectTypes, HotKey>(DefaultHeroObjectHotKey);
            UserActionHotKey = new Dictionary<HotkeyAction, HotKey>(DefaultHotKey);
        }

        public string GetHotKeyString(HotkeyAction action)
        {
            return UserActionHotKey[action].ToString();
        }

        public string GetHotKeyString(SkillTypes skill)
        {
            return UserSkillHotKey[skill].ToString();
        }

        public string GetHotKeyString(HeroObjectTypes heroObject)
        {
            return UserHeroObjectHotKey[heroObject].ToString();
        }

        public string GetHotKeyString(Monkeys monkey)
        {
            return UserMonkeyHotKey[monkey].ToString();
        }

        public HotKey GetHotKey(HotkeyAction action)
        {
            return UserActionHotKey[action];
        }

        public HotKey GetHotKey(SkillTypes skill)
        {
            return UserSkillHotKey[skill];
        }

        public HotKey GetHotKey(HeroObjectTypes heroObject)
        {
            return UserHeroObjectHotKey[heroObject];
        }

        public HotKey GetHotKey(Monkeys monkey)
        {
            return UserMonkeyHotKey[monkey];
        }

        public void SetHotKey(HotkeyAction action, HotKey hotKey)
        {
            UserActionHotKey[action] = hotKey;
        }

        public void SetHotKey(SkillTypes skill, HotKey hotKey)
        {
            UserSkillHotKey[skill] = hotKey;
        }

        public void SetHotKey(HeroObjectTypes heroObject, HotKey hotKey)
        {
            UserHeroObjectHotKey[heroObject] = hotKey;
        }

        public void SetHotKey(Monkeys monkey, HotKey hotKey)
        {
            UserMonkeyHotKey[monkey] = hotKey;
        }

        public void SaveSettings()
        {
            var settings = new
            {
                DataReadInterval,
                OperationInterval,
                GameDpi,
                UserActionHotKey,
                UserSkillHotKey,
                UserHeroObjectHotKey,
                UserMonkeyHotKey,
                EnableDoubleCoin,
                EnableFastPath,
                EnableLogging,
                EnableRecommendInterval
            };
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static ScriptSettings LoadJsonSettings()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return new ScriptSettings();

                var settings = JsonConvert.DeserializeObject<ScriptSettings>(File.ReadAllText(ConfigPath));

                return settings;
            }
            catch
            {
                // 异常时生成新默认配置（可选：自动修复损坏配置文件）
                var defaultSettings = new ScriptSettings();
                defaultSettings.SaveSettings();
                return defaultSettings;
            }
        }
    }

    [Serializable]
    public struct HotKey
    {
        public Keys MainKey { get; set; }
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        public HotKey(Keys mainKey, bool control = false, bool shift = false, bool alt = false)
        {
            MainKey = mainKey;
            Control = control;
            Shift = shift;
            Alt = alt;
        }

        public override string ToString()
        {
            if (MainKey == Keys.None) return "未设置";
            var sb = new StringBuilder();
            if (Control) sb.Append("Ctrl+");
            if (Shift) sb.Append("Shift+");
            if (Alt) sb.Append("Alt+");
            sb.Append(MainKey);
            return sb.ToString();
        }
    }
}
