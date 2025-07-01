using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BTD6AutoCommunity.Core.GameVisionRecognizer;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace BTD6AutoCommunity.Core
{
    public enum NumberType
    {
        Round,
        Cash,
        Life    
    }
    public class LevelDataMonitor
    {
        // 基于1920x1080的Area
        private readonly Rectangle baseRoundNumberArea = new Rectangle(1370, 25, 195, 70);
        private readonly Rectangle baseRightUpGradeRoundNumberArea = new Rectangle(970, 25, 195, 70);
        private readonly Rectangle baseCashNumberArea = new Rectangle(360, 20, 180, 50);
        private readonly Rectangle baseLeftUpGradeCashNumberArea = new Rectangle(750, 20, 180, 50);
        private readonly Rectangle baseLifeNumberArea = new Rectangle(140, 20, 130, 50);
        private readonly Rectangle baseLeftUpGradeLifeNumberArea = new Rectangle(530, 20, 130, 50);

        private Rectangle roundNumberArea;
        private Rectangle rightUpGradeRoundNumberArea;
        private Rectangle cashNumberArea;
        private Rectangle leftUpGradeCashNumberArea;
        private Rectangle lifeNumberArea;
        private Rectangle leftUpGradelifeNumberArea;

        private readonly GameContext _context;
        private const string templatePath = @"data\templates\digits\"; // 模板图片路径
        private readonly Dictionary<int, Mat> lifeAndCashNumberTemplates;
        private readonly Dictionary<int, Mat> roundNumberTemplates;
        private readonly Mat slashTemplate;

        public LevelDataMonitor(GameContext context)
        {
            string lifeAndCashNumberTemplatePath;
            string roundNumberTemplatePath;
            string slashTemplatePath;

            _context = context;
            roundNumberArea = ConvertToScreenRectangle(baseRoundNumberArea, _context);
            rightUpGradeRoundNumberArea = ConvertToScreenRectangle(baseRightUpGradeRoundNumberArea, _context);
            cashNumberArea = ConvertToScreenRectangle(baseCashNumberArea, _context);
            leftUpGradeCashNumberArea = ConvertToScreenRectangle(baseLeftUpGradeCashNumberArea, _context);
            lifeNumberArea = ConvertToScreenRectangle(baseLifeNumberArea, _context);
            leftUpGradelifeNumberArea = ConvertToScreenRectangle(baseLeftUpGradeLifeNumberArea, _context);

            lifeAndCashNumberTemplates = new Dictionary<int, Mat>();
            roundNumberTemplates = new Dictionary<int, Mat>();
            for (int i = 0; i <= 9; i++)
            {
                if (_context.ResolutionScale == 1) // 1920x1080
                {
                    lifeAndCashNumberTemplatePath = templatePath + $@"digit_{i}.png";
                    roundNumberTemplatePath = templatePath + $@"digit_r{i}0.png";
                    slashTemplatePath = templatePath + @"slash0.png";
                }
                else // 1280x720
                {
                    lifeAndCashNumberTemplatePath = templatePath + $@"digit_1{i}.png";
                    roundNumberTemplatePath = templatePath + $@"digit_r{i}1.png";
                    slashTemplatePath = templatePath + @"slash1.png";
                }
                lifeAndCashNumberTemplates.Add(i, Cv2.ImRead(lifeAndCashNumberTemplatePath, ImreadModes.Unchanged));
                roundNumberTemplates.Add(i, Cv2.ImRead(roundNumberTemplatePath, ImreadModes.Unchanged));
                slashTemplate = Cv2.ImRead(slashTemplatePath, ImreadModes.Unchanged);
            }
        }
        public List<string> GetCurrentGameData() 
        {
            List<string> gameData = new List<string>();

            Task<string>[] tasks = new Task<string>[3];
            if (IsRightUpgrading(_context)) // 右侧升级
            {
                tasks[0] = Task.Run(() => GetNumberFromScreenArea(rightUpGradeRoundNumberArea, NumberType.Round));
            }
            else
            {
                tasks[0] = Task.Run(() => GetNumberFromScreenArea(roundNumberArea, NumberType.Round));
            }
            if (IsLeftUpgrading(_context)) // 左侧升级
            {
                tasks[1] = Task.Run(() => GetNumberFromScreenArea(leftUpGradeCashNumberArea, NumberType.Cash));
                tasks[2] = Task.Run(() => GetNumberFromScreenArea(leftUpGradelifeNumberArea, NumberType.Life));
            }
            else
            {
                tasks[1] = Task.Run(() => GetNumberFromScreenArea(cashNumberArea, NumberType.Cash));
                tasks[2] = Task.Run(() => GetNumberFromScreenArea(lifeNumberArea, NumberType.Life));
            }

            // 等待所有任务完成
            Task.WaitAll(tasks);

            // 将识别结果添加到列表
            gameData.Add(tasks[0].Result);
            gameData.Add(tasks[1].Result);
            gameData.Add(tasks[2].Result);
            //Debug.WriteLine(tasks[0].Result + " " + tasks[1].Result + " " + tasks[2].Result);
            // 将识别结果写入文件
            //string status = $"Round: {roundNumber}, cash: {cashNumber}, Life: {lifeNumber}";
            //File.WriteAllText(outputFilePath, status);
            return gameData;
        }

        private string GetNumberFromScreenArea(Rectangle area, NumberType type)
        {
            using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }

                // 转换为OpenCV Mat
                Mat image = BitmapConverter.ToMat(bitmap);
                if (type == NumberType.Round)
                {
                    return RecognizeRoundNumber(image);
                }
                else if (type == NumberType.Cash || type == NumberType.Life)
                {
                    return RecognizeLifeAndCashNumber(image);
                }
                else
                {
                    return "";
                }
            }
        }

        private string RecognizeRoundNumber(Mat image)
        {
            List<(int, int)> digits = new List<(int, int)>();

            // 识别分隔符
            Mat slashResult = new Mat();
            Mat slashMask = CreateMask(slashTemplate);

            Cv2.MatchTemplate(image, slashTemplate, slashResult, TemplateMatchModes.SqDiffNormed, slashMask);
            Cv2.MinMaxLoc(slashResult, out double slashMinVal, out double slashMaxVal, out OpenCvSharp.Point slashMinLoc, out OpenCvSharp.Point slashMaxLoc);

            if (slashMinVal <= 0.2 && !double.IsInfinity(slashMinVal) && !double.IsNaN(slashMinVal))
            {
                Rect rect = new Rect(slashMinLoc.X + slashTemplate.Width / 2, slashMinLoc.Y, image.Width - slashMinLoc.X, slashTemplate.Height);
                Cv2.Rectangle(image, rect, Scalar.Green, -1);
            }
            // 识别回合数字
            foreach (var template in roundNumberTemplates)
            {
                Mat result = new Mat();
                Mat mask = CreateMask(template.Value);

                int tryGet = 0;
                while (tryGet < 9)
                {
                    tryGet++;

                    Cv2.MatchTemplate(image, template.Value, result, TemplateMatchModes.SqDiffNormed, mask);
                    Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);
                    if (minVal <= 0.1)
                    {
                        digits.Add((minLoc.X, template.Key));
                        //maxVals.Add(maxVal);
                        Rect rect = new Rect(minLoc.X, minLoc.Y, (int)(template.Value.Width * 0.9), (int)(template.Value.Height * 0.9));
                        Cv2.Rectangle(image, rect, Scalar.Green, -1);
                    }
                    else
                    {
                        break;
                        //if (double.IsInfinity(minVal) || double.IsNaN(minVal))
                        //{
                        //    //Rect rect = new Rect(maxLoc.X, maxLoc.Y, (int)(template.Value.Width * 0.9), (int)(template.Value.Height * 0.9));
                        //    //Cv2.Rectangle(image, rect, Scalar.Green, -1);
                        //    continue;
                        //}
                        //else
                        //{
                        //    break;
                        //}
                    }
                }
            }
            if (digits.Count == 0)
            {
                return "";
            }

            var sortedList = digits.OrderBy(t => t.Item1);
            List<int> posresult = sortedList.Select(t => t.Item1).ToList();
            List<int> digitresult = sortedList.Select(t => t.Item2).ToList();

            return string.Concat(digitresult);
        }

        private string RecognizeLifeAndCashNumber(Mat image)
        {
            List<(int, int)> digits = new List<(int, int)>();
            foreach (var template in lifeAndCashNumberTemplates)
            {
                Mat result = new Mat();
                Mat mask = CreateMask(template.Value);
                int tryGet = 0;
                while (tryGet < 9)
                {
                    tryGet++;
                    Cv2.MatchTemplate(image, template.Value, result, TemplateMatchModes.SqDiffNormed, mask);
                    Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);
                    Debug.WriteLine($"num: {template.Key} minVal: {minVal}");
                    if (minVal <= 0.075)
                    {
                        digits.Add((minLoc.X, template.Key));
                        Rect rect = new Rect(minLoc.X, minLoc.Y, (int)(template.Value.Width * 0.9), (int)(template.Value.Height * 0.9));
                        Cv2.Rectangle(image, rect, Scalar.Green, -1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (digits.Count == 0)
            {
                return "";
            }

            var sortedList = digits.OrderBy(t => t.Item1);
            List<int> posresult = sortedList.Select(t => t.Item1).ToList();
            List<int> digitresult = sortedList.Select(t => t.Item2).ToList();
            Debug.WriteLine(string.Join(",", digitresult));
            return string.Concat(digitresult);
        }

        private Mat CreateMask(Mat templateImage)
        {
            Mat mask = new Mat();
            Mat[] channels = templateImage.Split();
            //Debug.WriteLine($"Channel: {channels.Length}");
            if (channels.Length == 4)
            {
                Cv2.Threshold(channels[3], mask, 1, 255, ThresholdTypes.Binary);
            }
            else
            {
                mask = Mat.Ones(templateImage.Size(), MatType.CV_8U);
            }
            return mask;
        }

        private Rectangle ConvertToScreenRectangle(Rectangle rectangle, GameContext context)
        {
            System.Drawing.Point topLeft = new System.Drawing.Point(rectangle.Left, rectangle.Top);
            System.Drawing.Point bottomRight = new System.Drawing.Point(rectangle.Right, rectangle.Bottom);

            System.Drawing.Point scaledTopLeft = context.ConvertGamePosition(topLeft);
            System.Drawing.Point scaledBottomRight = context.ConvertGamePosition(bottomRight);

            return new Rectangle(scaledTopLeft, new System.Drawing.Size(scaledBottomRight.X - scaledTopLeft.X, scaledBottomRight.Y - scaledTopLeft.Y));
        }
    }
}
