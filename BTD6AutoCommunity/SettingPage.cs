using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BTD6AutoCommunity.ScriptSettings;
using System.Diagnostics;
using BTD6AutoCommunity;

namespace BTD6AutoCommunity
{
    // 选项设置页面
    public partial class BTD6AutoCommunity
    {
        private ScriptSettings scriptSettings;
        private List<System.Windows.Forms.Button> hotKeysButtonsList;
        private System.Windows.Forms.Button currentButton;
        private bool isSettingHotKey;
        private Type currentType;

        private void InitializeSettingPage()
        {
            scriptSettings = new ScriptSettings();

            currentButton = null;
            isSettingHotKey = false;
            LoadSettings();
        }
        //{ HotkeyAction.DartMonkey, new HotKey(Keys.Q) },
        //{ HotkeyAction.BoomerangMonkey, new HotKey(Keys.W) },
        //{ HotkeyAction.BombShooter, new HotKey(Keys.E) },
        //{ HotkeyAction.TackShooter, new HotKey(Keys.R) },
        //{ HotkeyAction.IceMonkey, new HotKey(Keys.T) },
        //{ HotkeyAction.GlueGunner, new HotKey(Keys.Y) },
        //{ HotkeyAction.SniperMonkey, new HotKey(Keys.Z) },
        //{ HotkeyAction.MonkeySub, new HotKey(Keys.X) },
        //{ HotkeyAction.MonkeyBuccaneer, new HotKey(Keys.C) },
        //{ HotkeyAction.MonkeyAce, new HotKey(Keys.V) },
        //{ HotkeyAction.HeliPilot, new HotKey(Keys.B) },
        //{ HotkeyAction.MortarMonkey, new HotKey(Keys.N) },
        //{ HotkeyAction.DartlingGunner, new HotKey(Keys.M) },
        //{ HotkeyAction.WizardMonkey, new HotKey(Keys.A) },
        //{ HotkeyAction.SuperMonkey, new HotKey(Keys.S) },
        //{ HotkeyAction.NinjaMonkey, new HotKey(Keys.D) },
        //{ HotkeyAction.Alchemist, new HotKey(Keys.F) },
        //{ HotkeyAction.Druid, new HotKey(Keys.G) },
        //{ HotkeyAction.MerMonkey, new HotKey(Keys.O) },
        //{ HotkeyAction.BananaFarm, new HotKey(Keys.H) },
        //{ HotkeyAction.SpikeFactory, new HotKey(Keys.J) },
        //{ HotkeyAction.MonkeyVillage, new HotKey(Keys.K) },
        //{ HotkeyAction.EngineerMonkey, new HotKey(Keys.L) },
        //{ HotkeyAction.BeastHandler, new HotKey(Keys.I) },
        //{ HotkeyAction.Hero, new HotKey(Keys.U) },
        //{ HotkeyAction.UpgradeTopPath, new HotKey(Keys.Oemcomma) },
        //{ HotkeyAction.UpgradeMiddlePath, new HotKey(Keys.OemPeriod) },
        //{ HotkeyAction.UpgradeBottomPath, new HotKey(Keys.OemQuestion) },
        //{ HotkeyAction.SwitchTarget, new HotKey(Keys.Tab) },
        //{ HotkeyAction.ReverseSwitchTarget, new HotKey(Keys.Tab, control: true) },
        //{ HotkeyAction.SetFunction1, new HotKey(Keys.PageDown) },
        //{ HotkeyAction.SetFunction2, new HotKey(Keys.PageUp) },
        //{ HotkeyAction.ChangeSpeed, new HotKey(Keys.Space) },
        //{ HotkeyAction.NextRound, new HotKey(Keys.Space, shift: true) },
        //{ HotkeyAction.Skill1, new HotKey(Keys.D1) },
        //{ HotkeyAction.Skill2, new HotKey(Keys.D2) },
        //{ HotkeyAction.Skill3, new HotKey(Keys.D3) },
        //{ HotkeyAction.Skill4, new HotKey(Keys.D4) },
        //{ HotkeyAction.Skill5, new HotKey(Keys.D5) },
        //{ HotkeyAction.Skill6, new HotKey(Keys.D6) },
        //{ HotkeyAction.Skill7, new HotKey(Keys.D7) },
        //{ HotkeyAction.Skill8, new HotKey(Keys.D8) },
        //{ HotkeyAction.Skill9, new HotKey(Keys.D9) },
        //{ HotkeyAction.Skill10, new HotKey(Keys.D0) },
        //{ HotkeyAction.Skill11, new HotKey(Keys.OemMinus) },
        //{ HotkeyAction.Skill12, new HotKey(Keys.Oemplus) }
        private void InitHotKeyButton()
        {
            DartMonkeyHotkeyBT.Tag = Monkeys.DartMonkey;
            BoomerangMonkeyHotkeyBT.Tag = Monkeys.BoomerangMonkey;
            BombShooterHotkeyBT.Tag = Monkeys.BombShooter;
            TackShooterHotkeyBT.Tag = Monkeys.TackShooter;
            IceMonkeyHotkeyBT.Tag = Monkeys.IceMonkey;
            GlueGunnerHotkeyBT.Tag = Monkeys.GlueGunner;
            SniperMonkeyHotkeyBT.Tag = Monkeys.SniperMonkey;
            MonkeySubHotkeyBT.Tag = Monkeys.MonkeySub;
            MonkeyBuccaneerHotkeyBT.Tag = Monkeys.MonkeyBuccaneer;
            MonkeyAceHotkeyBT.Tag = Monkeys.MonkeyAce;
            HeliPilotHotkeyBT.Tag = Monkeys.HeliPilot;
            MortarMonkeyHotkeyBT.Tag = Monkeys.MortarMonkey;
            DartlingGunnerHotkeyBT.Tag = Monkeys.DartlingGunner;
            WizardMonkeyHotkeyBT.Tag = Monkeys.WizardMonkey;
            SuperMonkeyHotkeyBT.Tag = Monkeys.SuperMonkey;
            NinjaMonkeyHotkeyBT.Tag = Monkeys.NinjaMonkey;
            AlchemistHotkeyBT.Tag = Monkeys.Alchemist;
            DruidHotkeyBT.Tag = Monkeys.Druid;
            MerMonkeyHotkeyBT.Tag = Monkeys.MerMonkey;
            BananaFarmHotkeyBT.Tag = Monkeys.BananaFarm;
            SpikeFactoryHotkeyBT.Tag = Monkeys.SpikeFactory;
            MonkeyVillageHotkeyBT.Tag = Monkeys.MonkeyVillage;
            EngineerMonkeyHotkeyBT.Tag = Monkeys.EngineerMonkey;
            BeastHandlerHotkeyBT.Tag = Monkeys.BeastHandler;

            HeroHotkeyBT.Tag = HotkeyAction.Hero;
            SellHotkeyBT.Tag = HotkeyAction.Sell;
            UpgradeTopPathHotkeyBT.Tag = HotkeyAction.UpgradeTopPath;
            UpgradeMiddlePathHotkeyBT.Tag = HotkeyAction.UpgradeMiddlePath;
            UpgradeBottomPathHotkeyBT.Tag = HotkeyAction.UpgradeBottomPath;
            SwitchTargetHotkeyBT.Tag = HotkeyAction.SwitchTarget;
            ReverseSwitchTargetHotkeyBT.Tag = HotkeyAction.ReverseSwitchTarget;
            SetFunction1HotkeyBT.Tag = HotkeyAction.SetFunction1;
            SetFunction2HotkeyBT.Tag = HotkeyAction.SetFunction2;
            ChangeSpeedHotkeyBT.Tag = HotkeyAction.ChangeSpeed;
            NextRoundHotkeyBT.Tag = HotkeyAction.NextRound;

            Skill1HotkeyBT.Tag = SkillTypes.Skill1;
            Skill2HotkeyBT.Tag = SkillTypes.Skill2;
            Skill3HotkeyBT.Tag = SkillTypes.Skill3;
            Skill4HotkeyBT.Tag = SkillTypes.Skill4;
            Skill5HotkeyBT.Tag = SkillTypes.Skill5;
            Skill6HotkeyBT.Tag = SkillTypes.Skill6;
            Skill7HotkeyBT.Tag = SkillTypes.Skill7;
            Skill8HotkeyBT.Tag = SkillTypes.Skill8;
            Skill9HotkeyBT.Tag = SkillTypes.Skill9;
            Skill10HotkeyBT.Tag = SkillTypes.Skill10;
            Skill11HotkeyBT.Tag = SkillTypes.Skill11;
            Skill12HotkeyBT.Tag = SkillTypes.Skill12;

            HeroObject1HotkeyBT.Tag = HeroObjectTypes.HeroObject1;
            HeroObject2HotkeyBT.Tag = HeroObjectTypes.HeroObject2;
            HeroObject3HotkeyBT.Tag = HeroObjectTypes.HeroObject3;
            HeroObject4HotkeyBT.Tag = HeroObjectTypes.HeroObject4;
            HeroObject5HotkeyBT.Tag = HeroObjectTypes.HeroObject5;
            HeroObject6HotkeyBT.Tag = HeroObjectTypes.HeroObject6;
            HeroObject7HotkeyBT.Tag = HeroObjectTypes.HeroObject7;
            HeroObject8HotkeyBT.Tag = HeroObjectTypes.HeroObject8;
            HeroObject9HotkeyBT.Tag = HeroObjectTypes.HeroObject9;
            HeroObject10HotkeyBT.Tag = HeroObjectTypes.HeroObject10;
            HeroObject11HotkeyBT.Tag = HeroObjectTypes.HeroObject11;
            HeroObject12HotkeyBT.Tag = HeroObjectTypes.HeroObject12;
            HeroObject13HotkeyBT.Tag = HeroObjectTypes.HeroObject13;
            HeroObject14HotkeyBT.Tag = HeroObjectTypes.HeroObject14;
            HeroObject15HotkeyBT.Tag = HeroObjectTypes.HeroObject15;
            HeroObject16HotkeyBT.Tag = HeroObjectTypes.HeroObject16;
        }

        private void RefreshHotkeyText()
        {
            DartMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.DartMonkey);
            BoomerangMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.BoomerangMonkey);
            BombShooterHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.BombShooter);
            TackShooterHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.TackShooter);
            IceMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.IceMonkey);
            GlueGunnerHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.GlueGunner);
            SniperMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.SniperMonkey);
            MonkeySubHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MonkeySub);
            MonkeyBuccaneerHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MonkeyBuccaneer);
            MonkeyAceHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MonkeyAce);
            HeliPilotHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.HeliPilot);
            MortarMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MortarMonkey);
            DartlingGunnerHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.DartlingGunner);
            WizardMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.WizardMonkey);
            SuperMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.SuperMonkey);
            NinjaMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.NinjaMonkey);
            AlchemistHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.Alchemist);
            DruidHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.Druid);
            MerMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MerMonkey);
            BananaFarmHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.BananaFarm);
            SpikeFactoryHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.SpikeFactory);
            MonkeyVillageHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.MonkeyVillage);
            EngineerMonkeyHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.EngineerMonkey);
            BeastHandlerHotkeyBT.Text = scriptSettings.GetHotKeyString(Monkeys.BeastHandler);

            HeroHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.Hero);
            SellHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.Sell);
            UpgradeTopPathHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.UpgradeTopPath);
            UpgradeMiddlePathHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.UpgradeMiddlePath);
            UpgradeBottomPathHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.UpgradeBottomPath);
            SwitchTargetHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.SwitchTarget);
            ReverseSwitchTargetHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.ReverseSwitchTarget);
            SetFunction1HotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.SetFunction1);
            SetFunction2HotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.SetFunction2);
            ChangeSpeedHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.ChangeSpeed);
            NextRoundHotkeyBT.Text = scriptSettings.GetHotKeyString(HotkeyAction.NextRound);

            Skill1HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill1);
            Skill2HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill2);
            Skill3HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill3);
            Skill4HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill4);
            Skill5HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill5);
            Skill6HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill6);
            Skill7HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill7);
            Skill8HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill8);
            Skill9HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill9);
            Skill10HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill10);
            Skill11HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill11);
            Skill12HotkeyBT.Text = scriptSettings.GetHotKeyString(SkillTypes.Skill12);

            HeroObject1HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject1);
            HeroObject2HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject2);
            HeroObject3HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject3);
            HeroObject4HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject4);
            HeroObject5HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject5);
            HeroObject6HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject6);
            HeroObject7HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject7);
            HeroObject8HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject8);
            HeroObject9HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject9);
            HeroObject10HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject10);
            HeroObject11HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject11);
            HeroObject12HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject12);
            HeroObject13HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject13);
            HeroObject14HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject14);
            HeroObject15HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject15);
            HeroObject16HotkeyBT.Text = scriptSettings.GetHotKeyString(HeroObjectTypes.HeroObject16);
        }

        private void SaveSettingsBT_Click(object sender, EventArgs e)
        {
            scriptSettings.DataReadInterval = (int)GetGameDataUD.Value;
            scriptSettings.OperationInterval = (int)ExecuteUD.Value;
            scriptSettings.GameDpi = GameDpiCB.SelectedIndex;
            scriptSettings.EnableDoubleCoin = DoubleCoinCB.Checked;
            scriptSettings.EnableFastPath = FastPathCB.Checked;
            scriptSettings.EnableLogging = EnableLoggingCB.Checked;
            //foreach (var buttonKeyPairs in hotKeysMap)
            //{
            //    scriptSettings.HotKey[Int32.Parse(buttonKeyPairs.Key.Tag.ToString())] = buttonKeyPairs.Value;
            //}
            scriptSettings.SaveSettings();
        }

        private void LoadSettings()
        {
            scriptSettings = LoadJsonSettings();
            GetGameDataUD.Value = scriptSettings.DataReadInterval;
            ExecuteUD.Value = scriptSettings.OperationInterval;
            GameDpiCB.SelectedIndex = scriptSettings.GameDpi;

            DoubleCoinCB.Checked = scriptSettings.EnableDoubleCoin;
            FastPathCB.Checked = scriptSettings.EnableFastPath;
            EnableLoggingCB.Checked = scriptSettings.EnableLogging;
            InitHotKeyButton();
            RefreshHotkeyText();
        }

        private void HotkeyBT_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            btn.Text = "请按键...";
            isSettingHotKey = true;
            currentButton = btn;
            currentType = btn.Tag.GetType();
        }

        private void StartPrgramTC_KeyDown(object sender, KeyEventArgs e)
        {
            if (StartPrgramTC.SelectedIndex == 2 && isSettingHotKey && currentButton != null)
            {
                var config = new HotKey
                {
                    MainKey = e.KeyCode,
                    Control = e.Control,
                    Shift = e.Shift,
                    Alt = e.Alt
                };

                if (currentType == typeof(Monkeys))
                {
                    scriptSettings.SetHotKey((Monkeys)currentButton.Tag, config);
                }
                else if (currentType == typeof(HotkeyAction))
                {
                    // 升级指令不推荐使用Ctrl+Shift+Alt
                    if ((HotkeyAction)currentButton.Tag == HotkeyAction.UpgradeTopPath || 
                        (HotkeyAction)currentButton.Tag == HotkeyAction.UpgradeMiddlePath || 
                        (HotkeyAction)currentButton.Tag == HotkeyAction.UpgradeBottomPath)
                    {
                        config.Control = false;
                        config.Shift = false;
                        config.Alt = false;
                    }
                    scriptSettings.SetHotKey((HotkeyAction)currentButton.Tag, config);
                }
                else if (currentType == typeof(SkillTypes))
                {
                    scriptSettings.SetHotKey((SkillTypes)currentButton.Tag, config);
                }
                else if (currentType == typeof(HeroObjectTypes))
                {
                    scriptSettings.SetHotKey((HeroObjectTypes)currentButton.Tag, config);
                }
                currentButton.Text = config.ToString();
                if (!(e.Alt || e.Control || e.Shift))
                {
                    currentButton = null;
                    isSettingHotKey = false;
                }
                e.Handled = true;
            }
        }

        private void StartPrgramTC_KeyUp(object sender, KeyEventArgs e)
        {
            if (StartPrgramTC.SelectedIndex == 2 && isSettingHotKey && currentButton != null)
            {
                if (e.Alt || e.Control || e.Shift)
                {
                    currentButton = null;
                    isSettingHotKey = false;
                }
                e.Handled = true;
            }
        }

        private void HotkeyToDefaultBT_Click(object sender, EventArgs e)
        {
            scriptSettings.RestoreDefaultHotKey();
            RefreshHotkeyText();
            currentButton = null;
            isSettingHotKey = false;
        }
    }
}
