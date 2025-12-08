using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.GameObjects
{
    public class MapInfo
    {
        private Dictionary<Maps, Badges> mapBadges;

        private Dictionary<Maps, bool> checkedMaps;
        public MapInfo()
        {
            checkedMaps = new Dictionary<Maps, bool>();
            mapBadges = new Dictionary<Maps, Badges>();
            foreach (Maps map in Constants.MapsList)
            {
                checkedMaps[map] = false;
                mapBadges[map] = new Badges();
            }
        }

        public bool IsMapChecked(Maps map)
        {
            if (checkedMaps.ContainsKey(map))
            {
                return checkedMaps[map];
            }
            return false;
        }

        public void SetMapChecked(Maps map, bool isChecked)
        {
            if (checkedMaps.ContainsKey(map))
            {
                checkedMaps[map] = isChecked;
            }
        }

        public void SetBadges(Maps map, Badges badges)
        {
            if (mapBadges.ContainsKey(map))
            {
                mapBadges[map] = badges;
            }
        }

        public Badges GetBadges(Maps map)
        {
            if (mapBadges.ContainsKey(map))
            {
                return mapBadges[map];
            }
            return null;
        }

        public bool GetBadgeStatus(Maps map, LevelDifficulties difficulty, LevelModes mode)
        {
            if (!checkedMaps.ContainsKey(map))
            {
                return false;
            }
            if (mapBadges.ContainsKey(map))
            {
                return mapBadges[map].GetBadgeStatus(difficulty, mode);
            }
            return false;
        }
    }

    public class Badges
    {
        private bool EasyStandard { get; set; }
        private bool MediumStandard { get; set; }
        private bool HardStandard { get; set; }
        private bool PrimaryOnly { get; set; }
        private bool MilitaryOnly { get; set; }
        private bool MagicMonkeysOnly { get; set; }
        private bool Deflation { get; set; }
        private bool Apopalypse { get; set; }
        private bool Reverse { get; set; }
        private bool HalfCash { get; set; }
        private bool DoubleHpMoabs { get; set; }
        private bool AlternateBloonsRounds { get; set; }
        private bool Impoppable { get; set; }
        private bool CHIMPS { get; set; }

        // Base Position:
        public readonly Point EasyStandardPos = new Point(406, 366);
        public readonly Point MediumStandardPos = new Point(492, 366);
        public readonly Point HardStandardPos = new Point(580, 366);
        public readonly Point PrimaryOnlyPos = new Point(382, 401);
        public readonly Point MilitaryOnlyPos = new Point(466, 401);
        public readonly Point MagicMonkeysOnlyPos = new Point(543, 377);
        public readonly Point DeflationPos = new Point(436, 401);
        public readonly Point ApopalypsePos = new Point(492, 401);
        public readonly Point ReversePos = new Point(520, 401);
        public readonly Point HalfCashPos = new Point(607, 401);
        public readonly Point DoubleHpMoabsPos = new Point(550, 401);
        public readonly Point AlternateBloonsRoundsPos = new Point(615, 377);
        public readonly Point ImpoppablePos = new Point(666, 366);
        public readonly Point CHIMPSPos = new Point(693, 401);

        public Badges()
        {
            EasyStandard = false;
            MediumStandard = false;
            HardStandard = false;
            PrimaryOnly = false;
            MilitaryOnly = false;
            MagicMonkeysOnly = false;
            Deflation = false;
            Apopalypse = false;
            Reverse = false;
            HalfCash = false;
            DoubleHpMoabs = false;
            AlternateBloonsRounds = false;
            Impoppable = false;
            CHIMPS = false;
        }

        public bool GetBadgeStatus(LevelDifficulties difficulty, LevelModes mode)
        {
            if (difficulty == LevelDifficulties.Easy && mode == LevelModes.Standard) return EasyStandard;
            if (difficulty == LevelDifficulties.Medium && mode == LevelModes.Standard) return MediumStandard;
            if (difficulty == LevelDifficulties.Hard && mode == LevelModes.Standard) return HardStandard;
            if (mode == LevelModes.PrimaryOnly) return PrimaryOnly;
            if (mode == LevelModes.MilitaryOnly) return MilitaryOnly;
            if (mode == LevelModes.MagicMonkeysOnly) return MagicMonkeysOnly;
            if (mode == LevelModes.Deflation) return Deflation;
            if (mode == LevelModes.Apopalypse) return Apopalypse;
            if (mode == LevelModes.Reverse) return Reverse;
            if (mode == LevelModes.HalfCash) return HalfCash;
            if (mode == LevelModes.DoubleHpMoabs) return DoubleHpMoabs;
            if (mode == LevelModes.AlternateBloonsRounds) return AlternateBloonsRounds;
            if (mode == LevelModes.Impoppable) return Impoppable;
            if (mode == LevelModes.CHIMPS) return CHIMPS;
            return false;
        }

        public void SetBadgeStatus(LevelDifficulties difficulty, LevelModes mode, bool status)
        {
            if (difficulty == LevelDifficulties.Easy && mode == LevelModes.Standard) EasyStandard = status;
            else if (difficulty == LevelDifficulties.Medium && mode == LevelModes.Standard) MediumStandard = status;
            else if (difficulty == LevelDifficulties.Hard && mode == LevelModes.Standard) HardStandard = status;
            else if (mode == LevelModes.PrimaryOnly) PrimaryOnly = status;
            else if (mode == LevelModes.MilitaryOnly) MilitaryOnly = status;
            else if (mode == LevelModes.MagicMonkeysOnly) MagicMonkeysOnly = status;
            else if (mode == LevelModes.Deflation) Deflation = status;
            else if (mode == LevelModes.Apopalypse) Apopalypse = status;
            else if (mode == LevelModes.Reverse) Reverse = status;
            else if (mode == LevelModes.HalfCash) HalfCash = status;
            else if (mode == LevelModes.DoubleHpMoabs) DoubleHpMoabs = status;
            else if (mode == LevelModes.AlternateBloonsRounds) AlternateBloonsRounds = status;
            else if (mode == LevelModes.Impoppable) Impoppable = status;
            else if (mode == LevelModes.CHIMPS) CHIMPS = status;
        }

            //{ LevelModes.Standard, "标准" },
            //{ LevelModes.PrimaryOnly, "仅初级" },
            //{ LevelModes.Deflation, "放气" },
            //{ LevelModes.MilitaryOnly, "仅军事" },
            //{ LevelModes.Apopalypse, "天启" },
            //{ LevelModes.Reverse, "相反" },
            //{ LevelModes.MagicMonkeysOnly, "仅魔法" },
            //{ LevelModes.DoubleHpMoabs, "双倍生命" },
            //{ LevelModes.HalfCash, "现金减半" },
            //{ LevelModes.AlternateBloonsRounds, "替代气球" },
            //{ LevelModes.Impoppable, "极难" },
            //{ LevelModes.CHIMPS, "点击" }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (EasyStandard) sb.Append("简单标准 ");
            if (MediumStandard) sb.Append("中级标准 ");
            if (HardStandard) sb.Append("困难标准 ");
            if (PrimaryOnly) sb.Append("仅初级 ");
            if (MilitaryOnly) sb.Append("仅军事 ");
            if (MagicMonkeysOnly) sb.Append("仅魔法 ");
            if (Deflation) sb.Append("放气 ");
            if (Apopalypse) sb.Append("天启 ");
            if (Reverse) sb.Append("相反 ");
            if (HalfCash) sb.Append("现金减半 ");
            if (DoubleHpMoabs) sb.Append("双倍生命 ");
            if (AlternateBloonsRounds) sb.Append("替代气球 ");
            if (Impoppable) sb.Append("极难 ");
            if (CHIMPS) sb.Append("点击 ");
            if (sb.Length == 0) sb.Append("无");
            return sb.ToString().Trim();
        }
    }

}
