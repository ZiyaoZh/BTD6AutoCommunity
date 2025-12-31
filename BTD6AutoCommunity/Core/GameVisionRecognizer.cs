using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.Services;
using BTD6AutoCommunity.Views;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BTD6AutoCommunity.Core
{
    public class GameVisionRecognizer
    {
        private const double MatchThreshold = 0.90;

        public static int RecognizeMapId(GameContext context, int startId, int endId)
        {
            // 获取截图区域（基于GameContext计算）
            Rectangle captureArea = CalculateCaptureArea(context);
            //Debug.WriteLine($"captureAreaX: {captureArea.X}, captureAreaY: {captureArea.Y}, captureAreaWidth: {captureArea.Width}, captureAreaHeight: {captureArea.Height}");
            // 截取屏幕图像
            using (var screenImage = CaptureScreenRegion(captureArea))
            {
                // 执行模板匹配
                return MatchTemplateWithDpi(context, screenImage, startId, endId);
            }
        }

        public static System.Drawing.Point GetMapPos(GameContext context, int mapId)
        {
            try
            {
                var captureArea = CalculateCaptureArea(context);
                using (var screenImage = CaptureScreenRegion(captureArea))
                {
                    using (var template = LoadMapTemplate(context, mapId))
                    {
                        using (var result = new Mat())
                        {
                            Cv2.MatchTemplate(screenImage, template, result, TemplateMatchModes.CCoeffNormed);

                            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
                            //Debug.WriteLine($"mapId: {mapId}" + maxVal.ToString());
                            return maxVal >= MatchThreshold
                                ? CalculateCenter(context, maxLoc, template.Size())
                                : new System.Drawing.Point(-1, -1);
                        }
                    }
                }
            }
            catch
            {
                return new System.Drawing.Point(-1, -1);
            }
        }

        public static int GetMapEreaIndex(System.Drawing.Point mapPos)
        {
            if (mapPos.Y < 430 && mapPos.Y > 120)
            {
                if (mapPos.X < 740) return 0;
                if (mapPos.X < 1170) return 1;
                if (mapPos.X < 1600) return 2;
            }
            if (mapPos.Y > 430 && mapPos.Y < 750)
            {
                if (mapPos.X < 740) return 3;
                if (mapPos.X < 1170) return 4;
                if (mapPos.X < 1600) return 5;
            }
            return -1;
        }

        public static Badges GetMapBadges(GameContext context, int mapEreaId, Bitmap bitmap)
        {
            if (mapEreaId == -1) return null;
            Badges badges = new Badges();
            int deltaX = (mapEreaId % 3) * 423;
            int deltaY = (mapEreaId / 3) * 313;
            badges.SetBadgeStatus(LevelDifficulties.Easy, LevelModes.Standard, !CheckColorFromBitmap(context, bitmap, badges.EasyStandardPos.X + deltaX, badges.EasyStandardPos.Y + deltaY, new List<int> { 0xA88859, 0xA9B9C4, 0xBE7813}));
            badges.SetBadgeStatus(LevelDifficulties.Medium, LevelModes.Standard, !CheckColorFromBitmap(context, bitmap, badges.MediumStandardPos.X + deltaX, badges.MediumStandardPos.Y + deltaY, new List<int> { 0xA88859, 0xA9B9C4, 0xBE7813 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.Standard, !CheckColorFromBitmap(context, bitmap, badges.HardStandardPos.X + deltaX, badges.HardStandardPos.Y + deltaY, new List<int> { 0xA88859, 0xA9B9C4, 0xBE7813 }));
            badges.SetBadgeStatus(LevelDifficulties.Easy, LevelModes.PrimaryOnly, !CheckColorFromBitmap(context, bitmap, badges.PrimaryOnlyPos.X + deltaX, badges.PrimaryOnlyPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Medium, LevelModes.MilitaryOnly, !CheckColorFromBitmap(context, bitmap, badges.MilitaryOnlyPos.X + deltaX, badges.MilitaryOnlyPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.MagicMonkeysOnly, !CheckColorFromBitmap(context, bitmap, badges.MagicMonkeysOnlyPos.X + deltaX, badges.MagicMonkeysOnlyPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Easy, LevelModes.Deflation, !CheckColorFromBitmap(context, bitmap, badges.DeflationPos.X + deltaX, badges.DeflationPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Medium, LevelModes.Apopalypse, !CheckColorFromBitmap(context, bitmap, badges.ApopalypsePos.X + deltaX, badges.ApopalypsePos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Medium, LevelModes.Reverse, !CheckColorFromBitmap(context, bitmap, badges.ReversePos.X + deltaX, badges.ReversePos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.HalfCash, !CheckColorFromBitmap(context, bitmap, badges.HalfCashPos.X + deltaX, badges.HalfCashPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.DoubleHpMoabs, !CheckColorFromBitmap(context, bitmap, badges.DoubleHpMoabsPos.X + deltaX, badges.DoubleHpMoabsPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.AlternateBloonsRounds, !CheckColorFromBitmap(context, bitmap, badges.AlternateBloonsRoundsPos.X + deltaX, badges.AlternateBloonsRoundsPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.Impoppable, !CheckColorFromBitmap(context, bitmap, badges.ImpoppablePos.X + deltaX, badges.ImpoppablePos.Y + deltaY, new List<int> { 0xA88859, 0xA9B9C4, 0xBE7813 }));
            badges.SetBadgeStatus(LevelDifficulties.Hard, LevelModes.CHIMPS, !CheckColorFromBitmap(context, bitmap, badges.CHIMPSPos.X + deltaX, badges.CHIMPSPos.Y + deltaY, new List<int> { 0xB08959, 0xA9BCCA, 0xC37503 }));

            return badges;

        }

        public static System.Drawing.Point GetHeroPosition(GameContext context, Heroes heroName)
        {
            try
            {
                var captureArea = CalculateCaptureArea(context);
                using (var screenImage = CaptureScreenRegion(captureArea))
                {
                    List<Mat> templateList = LoadHeroTemplate(context, heroName);
                    using (var result = new Mat())
                    {
                        foreach (var template in templateList)
                        {
                            Cv2.MatchTemplate(screenImage, template, result, TemplateMatchModes.CCoeffNormed);

                            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                            if (maxVal >= MatchThreshold)
                            {
                                return CalculateCenter(context, maxLoc, template.Size());
                            }
                        }
                    }
                    templateList.Clear();
                    return new System.Drawing.Point(-1, -1);
                }
            }
            catch
            {
                return new System.Drawing.Point(-1, -1);
            }
        }

        private static Rectangle CalculateCaptureArea(GameContext context)
        {
            return new Rectangle(
                context.ClientTopLeft.X,
                context.ClientTopLeft.Y,
                context.CurrentResolution.Width,
                context.CurrentResolution.Height
            );
        }

        private static Mat CaptureScreenRegion(Rectangle area)
        {
            using (var bitmap = new Bitmap(area.Width, area.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }
                return BitmapConverter.ToMat(bitmap);
            }
        }

        private static int MatchTemplateWithDpi(GameContext context, Mat screenImage, int startId, int endId)
        {
            int maxMapId = startId;
            double maxConfidence = 0;

            for (int templateId = startId; templateId <= endId; templateId++)
            {
                using (var template = LoadMapTemplate(context, templateId))
                {
                    var confidence = GetMatchConfidence(screenImage, template);
                    //Debug.WriteLine($"templateId: {templateId}, confidence: {confidence}");
                    if (confidence > maxConfidence)
                    {
                        maxConfidence = confidence;
                        maxMapId = templateId;
                    }
                }
            }

            return maxConfidence > MatchThreshold ? maxMapId : -1; // 添加置信度阈值
        }

        private static Mat LoadMapTemplate(GameContext context, int templateId)
        {
            var path = Path.Combine("data", "templates", "maps", $"{templateId}.png");
            var template = Cv2.ImRead(path, ImreadModes.Unchanged);

            // DPI缩放处理

            if (context.ResolutionScale != 1)
            {
                Cv2.Resize(template, template,
                    new OpenCvSharp.Size(template.Width * context.ResolutionScale, template.Height * context.ResolutionScale));
            }

            return template;
        }

        private static List<Mat> LoadHeroTemplate(GameContext context, Heroes heroName)
        {
            List<string> templatePath = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                string pngPath = Path.Combine("data", "templates", "heros", $"{(int)heroName}-{i}.png");
                if (File.Exists(pngPath))
                {
                    templatePath.Add(pngPath);
                }
            }
            List<Mat> templateList = new List<Mat>();
            foreach (string path in templatePath)
            {
                var template = Cv2.ImRead(path, ImreadModes.Unchanged);

                // 封装DPI缩放逻辑
                if (context.ResolutionScale != 1)
                {
                    Cv2.Resize(template, template,
                    new OpenCvSharp.Size(template.Width * context.ResolutionScale, template.Height * context.ResolutionScale));
                }
                templateList.Add(template);
            }

            return templateList;
        }

        private static double GetMatchConfidence(Mat screenImage, Mat template)
        {
            using (var result = new Mat())
            {
                Cv2.MatchTemplate(screenImage, template, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
                return maxVal;
            }
        }

        private static System.Drawing.Point CalculateCenter(GameContext context, OpenCvSharp.Point location, OpenCvSharp.Size templateSize)
        {
            return (
                new System.Drawing.Point
                (
                    (int)((location.X + templateSize.Width / 2) / context.ResolutionScale),
                    (int)((location.Y + templateSize.Height / 2) / context.ResolutionScale)
                )
            );
        }

        private static Color GetGameColorFromBitmap(GameContext context, Bitmap bmp, System.Drawing.Point basePoint)
        {
            MaskWindow.Instance.ShowCrosshair(basePoint, context);

            return bmp.GetPixel(
                (int)(basePoint.X * context.ResolutionScale),
                (int)(basePoint.Y * context.ResolutionScale)
            );
        }

        private static Color GetGameColorFromScreen(GameContext context, System.Drawing.Point basePoint)
        {
            MaskWindow.Instance.ShowCrosshair(basePoint, context);
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    var point = context.ConvertGamePosition(basePoint);
                    g.CopyFromScreen(point.X, point.Y, 0, 0, new System.Drawing.Size(1, 1));
                }
                var color = bitmap.GetPixel(0, 0);
                return color;
            }
        }

        public static bool CheckColorFromBitmap(GameContext context, Bitmap bmp, int x, int y, int expectedColor, int tolerance = 50)
        {
            Color color = GetGameColorFromBitmap(context, bmp, new System.Drawing.Point(x, y));
            Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);

            int diff = Math.Abs(color.R - expected.R)
                     + Math.Abs(color.G - expected.G)
                     + Math.Abs(color.B - expected.B);

            return diff < tolerance;
        }

        public static bool CheckColorFromBitmap(GameContext context, Bitmap bmp, System.Drawing.Point point, int expectedColor, int tolerance = 50)
        {
            Color color = GetGameColorFromBitmap(context, bmp, point);
            Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);
            int diff = Math.Abs(color.R - expected.R)
                     + Math.Abs(color.G - expected.G)
                     + Math.Abs(color.B - expected.B);
            return diff < tolerance;
        }

        public static bool CheckColorFromBitmap(GameContext context, Bitmap bmp, int x, int y, List<int> expectedColors)
        {
            Color color = GetGameColorFromBitmap(context, bmp, new System.Drawing.Point(x, y));
            foreach (var expectedColor in expectedColors)
            {
                Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);
                int diff = Math.Abs(color.R - expected.R)
                         + Math.Abs(color.G - expected.G)
                         + Math.Abs(color.B - expected.B);
                if (diff < 50)
                {
                    Debug.WriteLine($"Found matching color at ({x}, {y}): R={color.R}, G={color.G}, B={color.B}");
                    return true;
                }
                Debug.WriteLine($"No match for color at ({x}, {y}): R={color.R}, G={color.G}, B={color.B} vs Expected R={expected.R}, G={expected.G}, B={expected.B} with diff {diff}");
            }
            return false;
        }

        public static bool CheckColorFromBitmap(GameContext context, Bitmap bmp, System.Drawing.Point point, List<int> expectedColors)
        {
            Color color = GetGameColorFromBitmap(context, bmp, point);
            foreach (var expectedColor in expectedColors)
            {
                Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);
                int diff = Math.Abs(color.R - expected.R)
                         + Math.Abs(color.G - expected.G)
                         + Math.Abs(color.B - expected.B);
                if (diff < 50)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckColorFromScreen(GameContext context, int x, int y, int expectedColor)
        {
            Color color = GetGameColorFromScreen(context, new System.Drawing.Point(x, y));
            Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);

            Debug.WriteLine($"Checking color at ({x}, {y}): R={color.R}, G={color.G}, B={color.B} vs Expected R={expected.R}, G={expected.G}, B={expected.B}");

            int diff = Math.Abs(color.R - expected.R)
                     + Math.Abs(color.G - expected.G)
                     + Math.Abs(color.B - expected.B);

            return diff < 50;
        }

        public static bool GetYellowBlockCount(GameContext context, int index, int p)
        {
            if (index == -1) return false;
            System.Drawing.Point point = Constants.UpgradeYellowPosition[index * 5 + 5 - p];
            //Debug.WriteLine($"point: {point}");
            Color c1 = GetGameColorFromScreen(context, point);
            if (c1.B < 5 && c1.R > 40 && c1.G > 220)
            {
                return true;
            }
            return false;
        }

        public static bool IsInGame(GameContext context)
        {
            if (CheckColorFromScreen(context, 1910, 40, 0xB1814A) && CheckColorFromScreen(context, 13, 40, 0xB1814A))
            {
                return true;
            }
            return false;
        }

        public static bool IsMonkeyDeploy(GameContext context)
        {
            Color c1 = GetGameColorFromScreen(context, new System.Drawing.Point(1600, 120));
            Color c2 = GetGameColorFromScreen(context, new System.Drawing.Point(1600, 98));
            if (c1.R > 250 && c1.G > 250 && c1.B > 250
                && Math.Abs(c2.R - 0xff) < 20 && Math.Abs(c2.G - 0x79) < 20 && Math.Abs(c2.B - 0x00) < 20)
            {
                return true;
            }
            return false;
        }

        public static bool IsHeroDeploy(GameContext context)
        {
            Color c1 = GetGameColorFromScreen(context, new System.Drawing.Point(1757, 272));
            Color c2 = GetGameColorFromScreen(context, new System.Drawing.Point(1670, 274));
            if ((c1.R > 245 && c1.B < 10) || (c2.R > 245 && c2.B < 10))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsLeftUpgrading(GameContext context)
        {
            return CheckColorFromScreen(context, 415, 120, 0xbe925a) && CheckColorFromScreen(context, 415, 870, 0xb48149) && CheckColorFromScreen(context, 400, 82, 0x623811);
        }

        public static bool IsRightUpgrading(GameContext context)
        {
            return CheckColorFromScreen(context, 1260, 200, 0xbe925a) && CheckColorFromScreen(context, 1260, 870, 0xb48149) && CheckColorFromScreen(context, 1620, 82, 0x623811);
        }

        public static int AbilityRgbSum(GameContext context, int index)
        {
            Color c1 = GetGameColorFromScreen(context, new System.Drawing.Point(200 + index * 100, 1035));
            return c1.R + c1.G + c1.B;
        }

        public static System.Drawing.Point GetFailedScreenReturnPosition(GameContext context)
        {
            if (CheckColorFromScreen(context, 630, 800, 0xFFFFFF)) return new System.Drawing.Point(630, 810);
            return new System.Drawing.Point(740, 810);
        }

        public static System.Drawing.Point GetSettlementScreenReturnPosition(GameContext context)
        {
            if (CheckColorFromScreen(context, 840, 840, 0xFFFFFF)) return new System.Drawing.Point(840, 850);
            return new System.Drawing.Point(720, 850);
        }

        public static System.Drawing.Point GetRestartPos(GameContext context)
        {
            Color c1 = GetGameColorFromScreen(context, new System.Drawing.Point(850, 765));
            Color c2 = GetGameColorFromScreen(context, new System.Drawing.Point(960, 765));
            Color c3 = GetGameColorFromScreen(context, new System.Drawing.Point(1330, 800));
            if (Math.Abs(c1.R - 0x71) < 30 && Math.Abs(c1.G - 0xe8) < 30 && Math.Abs(c1.B - 0x00) < 30)
            {
                return new System.Drawing.Point(850, 815);
            }
            if (Math.Abs(c1.R - 0xff) < 30 && Math.Abs(c1.G - 0xdd) < 30 && Math.Abs(c1.B - 0x00) < 30)
            {
                return new System.Drawing.Point(850, 815);
            }
            if (Math.Abs(c2.R - 0x71) < 30 && Math.Abs(c2.G - 0xe8) < 30 && Math.Abs(c2.B - 0x00) < 30)
            {
                return new System.Drawing.Point(960, 815);
            }
            if (Math.Abs(c3.R - 0x71) < 30 && Math.Abs(c3.G - 0xe8) < 30 && Math.Abs(c3.B - 0x00) < 30)
            {
                return new System.Drawing.Point(1330, 815);
            }
            return new System.Drawing.Point(0, 0);
        }

        public static int AbilityReady(GameContext context, int index)
        {
            Color c1 = GetGameColorFromScreen(context, new System.Drawing.Point(200 + index * 100, 1035));
            return c1.R + c1.G + c1.B;
        }
    }
}
