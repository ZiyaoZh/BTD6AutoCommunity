using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using static BTD6AutoCommunity.Core.Constants;

namespace BTD6AutoCommunity.Core
{
    public class GameVisionRecognizer
    {
        private const double MatchThreshold = 0.90;


        public static int RecognizeMapId(GameContext context)
        {
            // 获取截图区域（基于GameContext计算）
            Rectangle captureArea = CalculateCaptureArea(context);
            //Debug.WriteLine($"captureAreaX: {captureArea.X}, captureAreaY: {captureArea.Y}, captureAreaWidth: {captureArea.Width}, captureAreaHeight: {captureArea.Height}");
            // 截取屏幕图像
            using (var screenImage = CaptureScreenRegion(captureArea))
            {
                // 执行模板匹配
                return MatchTemplateWithDpi(context, screenImage, startId: 90, endId: 101);
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

        public static int GetMapEreaIndex(GameContext context, System.Drawing.Point mapPos)
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

        public static System.Drawing.Point GetHeroPosition(GameContext context, Heroes heroName)
        {
            try
            {
                var captureArea = CalculateCaptureArea(context);
                using (var screenImage = CaptureScreenRegion(captureArea))
                {
                    using (var template = LoadHeroTemplate(context, heroName))
                    {
                        using (var result = new Mat())
                        {
                            Cv2.MatchTemplate(screenImage, template, result, TemplateMatchModes.CCoeffNormed);

                            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

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

        private static Mat LoadHeroTemplate(GameContext context, Heroes heroName)
        {
            var templatePath = Path.Combine("data", "templates", "heros", $"{(int)heroName}.png");
            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Template not found: {templatePath}");

            var template = Cv2.ImRead(templatePath, ImreadModes.Unchanged);

            // 封装DPI缩放逻辑
            if (context.ResolutionScale != 1)
            {
                Cv2.Resize(template, template,
                new OpenCvSharp.Size(template.Width * context.ResolutionScale, template.Height * context.ResolutionScale));
            }
            return template;
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

        private static Color GetGameColor(GameContext context, System.Drawing.Point basePoint)
        {
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    var point = context.ConvertGamePosition(basePoint);
                    g.CopyFromScreen(point.X, point.Y, 0, 0, new System.Drawing.Size(1, 1));
                }
                return bitmap.GetPixel(0, 0);
            }
        }

        public static bool CheckColor(GameContext context, int x, int y, int expectedColor)
        {
            Color color = GetGameColor(context, new System.Drawing.Point(x, y));
            Color expected = Color.FromArgb(expectedColor >> 16, (expectedColor >> 8) & 0xFF, expectedColor & 0xFF);

            int diff = Math.Abs(color.R - expected.R)
                     + Math.Abs(color.G - expected.G)
                     + Math.Abs(color.B - expected.B);

            return diff < 50;
        }

        public static bool GetYellowBlockCount(GameContext context, int index, int p)
        {
            System.Drawing.Point point = UpgradeYellowPosition[index * 5 + 5 - p];
            Debug.WriteLine($"point: {point}");
            Color c1 = GetGameColor(context, point);
            if (c1.B < 5 && c1.R > 40 && c1.G > 220)
            {
                return true;
            }
            return false;
        }

        public static bool IsInGame(GameContext context)
        {
            if (CheckColor(context, 1910, 40, 0xB1814A) && CheckColor(context, 13, 40, 0xB1814A))
            {
                return true;
            }
            return false;
        }

        public static bool IsMonkeyDeploy(GameContext context)
        {
            Color c1 = GetGameColor(context, new System.Drawing.Point(1600, 120));
            Color c2 = GetGameColor(context, new System.Drawing.Point(1600, 98));
            if (c1.R > 250 && c1.G > 250 && c1.B > 250
                && Math.Abs(c2.R - 0xff) < 20 && Math.Abs(c2.G - 0x79) < 20 && Math.Abs(c2.B - 0x00) < 20)
            {
                return true;
            }
            return false;
        }

        public static bool IsHeroDeploy(GameContext context)
        {
            Color c1 = GetGameColor(context, new System.Drawing.Point(1757, 272));
            Color c2 = GetGameColor(context, new System.Drawing.Point(1670, 274));
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
            return CheckColor(context, 415, 120, 0xbe925a) && CheckColor(context, 415, 870, 0xb48149);
        }

        public static bool IsRightUpgrading(GameContext context)
        {
            return CheckColor(context, 1260, 200, 0xbe925a) && CheckColor(context, 1260, 870, 0xb48149);
        }

        public static int AbilityRgbSum(GameContext context, int index)
        {
            Color c1 = GetGameColor(context, new System.Drawing.Point(200 + index * 100, 1035));
            return c1.R + c1.G + c1.B;
        }

        public static System.Drawing.Point GetFailedScreenReturnPosition(GameContext context)
        {
            if (CheckColor(context, 630, 800, 0xFFFFFF)) return new System.Drawing.Point(630, 810);
            return new System.Drawing.Point(740, 810);

        }

        public static System.Drawing.Point GetRestartPos(GameContext context)
        {
            Color c1 = GetGameColor(context, new System.Drawing.Point(850, 765));
            Color c2 = GetGameColor(context, new System.Drawing.Point(960, 765));
            Color c3 = GetGameColor(context, new System.Drawing.Point(1330, 800));
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
            Color c1 = GetGameColor(context, new System.Drawing.Point(200 + index * 100, 1035));
            return c1.R + c1.G + c1.B;
        }
    }
}
