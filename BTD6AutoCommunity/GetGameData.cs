using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Forms;
using System.Threading;
using static BTD6AutoCommunity.DisPlayMouseCoordinates;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using static System.Net.Mime.MediaTypeNames;
using OpenCvSharp.Flann;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace BTD6AutoCommunity
{
    internal class GetGameData
    {
        //private static string outputFilePath = "game_status.txt";
        private Dictionary<int, Mat> digitTemplates;
        private Mat yellowBlockTemplates;
        private IntPtr hWnd;
        private RECT clientRect;
        private DisPlayMouseCoordinates.POINT clientTopLeft;
        private int gameDpi;
        private double windowdpi;
        public GetGameData(double mydpi, int mygameDpi, IntPtr myhWnd)
        {
            windowdpi = mydpi;
            gameDpi = mygameDpi;
            hWnd = myhWnd;
            digitTemplates = new Dictionary<int, Mat>();
            if (GetClientRect(hWnd, out clientRect))
            {
                clientTopLeft.X = clientRect.Left;
                clientTopLeft.Y = clientRect.Top;
                DisPlayMouseCoordinates.ClientToScreen(hWnd, ref clientTopLeft);
            }
            for (int i = 0; i <= 9; i++)
            {
                if (gameDpi == 0) // 1920x1080
                {
                    string templatePath = $@"data\templates\digits\digit_{i}.png"; // 模板图片路径
                    digitTemplates.Add(i, Cv2.ImRead(templatePath, ImreadModes.Unchanged));
                }
                else // 1280x720
                {
                    string templatePath = $@"data\templates\digits\digit_1{i}.png"; // 模板图片路径
                    digitTemplates.Add(i, Cv2.ImRead(templatePath, ImreadModes.Unchanged));
                }
            }
        }

        private static (float, float, float) RGBToHSV(Color color)
        {
            float r = color.R / 255.0f;
            float g = color.G / 255.0f;
            float b = color.B / 255.0f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float h, s, v = max; // v is the value (brightness)

            float d = max - min;
            s = max == 0 ? 0 : d / max; // s is the saturation

            //计算色相
            if (max == min)
            {
                h = 0; // achromatic
            }
            else
            {
                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else // max == b
                {
                    h = (r - g) / d + 4;
                }
                h /= 6;
            }

            // 将色相转化为度数
            h *= 360;
            return (h, s, v);
        }

        public List<string> GetCurrentGameData(bool ifContinue) // 自由游戏
        {

            // 截取屏幕指定区域
            List<string> gameData = new List<string>();

            if (GetClientRect(hWnd, out clientRect))
            {
                clientTopLeft.X = clientRect.Left;
                clientTopLeft.Y = clientRect.Top;
                if (!DisPlayMouseCoordinates.ClientToScreen(hWnd, ref clientTopLeft))
                {
                    gameData.Add("未获取游戏窗口");
                    gameData.Add("未获取游戏窗口");
                    gameData.Add("未获取游戏窗口");
                    return gameData;
                }
            }
            else
            {
                gameData.Add("未获取游戏窗口");
                gameData.Add("未获取游戏窗口");
                gameData.Add("未获取游戏窗口");
                return gameData;
            }
            //MessageBox.Show(dpi.ToString());
            Rectangle roundNumberArea;
            Rectangle goldNumberArea;
            Rectangle lifeNumberArea;

            if (gameDpi == 0)
            {
                if (!ifContinue)
                {
                    roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 1370, (int)(clientTopLeft.Y * windowdpi) + 30, 125, 70); // 替换为回合数区域的实际坐标和尺寸
                }
                else
                {
                    roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 1475, (int)(clientTopLeft.Y * windowdpi) + 30, 125, 70); // 替换为回合数区域的实际坐标和尺寸
                }
                //roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 1370, (int)(clientTopLeft.Y * windowdpi) + 30, 125, 70); // 替换为回合数区域的实际坐标和尺寸
                goldNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 360, (int)(clientTopLeft.Y * windowdpi) + 20, 180, 50); // 替换为金币数区域的实际坐标和尺寸
                lifeNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 140, (int)(clientTopLeft.Y * windowdpi) + 20, 130, 50); // 替换为生命值区域的实际坐标和尺寸
            }
            else
            {
                if (!ifContinue)
                {
                    roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 913, (int)(clientTopLeft.Y * windowdpi) + 20, 83, 47); // 替换为回合数区域的实际坐标和尺寸
                }
                else
                {
                    roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 985, (int)(clientTopLeft.Y * windowdpi) + 20, 83, 47); // 替换为回合数区域的实际坐标和尺寸

                }
                //roundNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 913, (int)(clientTopLeft.Y * windowdpi) + 20, 83, 47); // 替换为回合数区域的实际坐标和尺寸
                goldNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 240, (int)(clientTopLeft.Y * windowdpi) + 13, 120, 33); // 替换为金币数区域的实际坐标和尺寸
                lifeNumberArea = new Rectangle((int)(clientTopLeft.X * windowdpi) + 93, (int)(clientTopLeft.Y * windowdpi) + 13, 87, 33); // 替换为生命值区域的实际坐标和尺寸

            }

            if (IfRightUpgrading())
            {
                roundNumberArea.X -= ((gameDpi == 0) ? 400 : 260);
            }
            if (IfLeftUpgrading())
            {
                goldNumberArea.X += ((gameDpi == 0) ? 390 : 260);
                lifeNumberArea.X += ((gameDpi == 0) ? 390 : 260);
            }
            string roundNumber = GetNumberFromScreenArea(roundNumberArea);
            string goldNumber = GetNumberFromScreenArea(goldNumberArea);
            string lifeNumber = GetNumberFromScreenArea(lifeNumberArea);
            gameData.Add(roundNumber);
            gameData.Add(goldNumber);
            gameData.Add(lifeNumber);
            // 将识别结果写入文件
            //string status = $"Round: {roundNumber}, Gold: {goldNumber}, Life: {lifeNumber}";
            //File.WriteAllText(outputFilePath, status);
            return gameData;
        }

        private string GetNumberFromScreenArea(Rectangle area)
        {
            using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }

                // 转换为OpenCV Mat
                Mat image = BitmapConverter.ToMat(bitmap);
                if (area.Height == 70 || area.Height == 47)
                {
                    return RecognizeDigits(image, 2); // round
                }
                else
                {
                    return RecognizeDigits(image, 1); // cash or life
                }
            }
        }

        private Color GetGameColor(int x, int y)
        {
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    if (gameDpi == 0)
                    {
                        g.CopyFromScreen(x + (int)(clientTopLeft.X * windowdpi), y + (int)(clientTopLeft.Y * windowdpi), 0, 0, new System.Drawing.Size(1, 1));
                    }
                    else
                    {
                        g.CopyFromScreen((int)(x * 1.0 / 1.5) + (int)(clientTopLeft.X * windowdpi), (int)(y * 1.0 / 1.5) + (int)(clientTopLeft.Y * windowdpi), 0, 0, new System.Drawing.Size(1, 1));
                    }
                }
                return bitmap.GetPixel(0, 0);
            }
        }

        public bool IfLeftUpgrading()
        {
            Color c1 = GetGameColor(415, 200);
            Color c2 = GetGameColor(415, 720);
            if (Math.Abs(c1.R - 0xb6) < 16 && Math.Abs(c1.G - 0x84) < 16 && Math.Abs(c1.B - 0x4c) < 16
                && Math.Abs(c2.R - 0xb6) < 16 && Math.Abs(c2.G - 0x84) < 16 && Math.Abs(c2.B - 0x4c) < 16)
            {
                return true;
            }
            return false;
        }

        public bool IfRightUpgrading()
        {
            Color c1 = GetGameColor(1260, 200);
            Color c2 = GetGameColor(1260, 720);
            if (Math.Abs(c1.R - 0xb6) < 16 && Math.Abs(c1.G - 0x84) < 16 && Math.Abs(c1.B - 0x4c) < 16
                && Math.Abs(c2.R - 0xb6) < 16 && Math.Abs(c2.G - 0x84) < 16 && Math.Abs(c2.B - 0x4c) < 16)
            {
                return true;
            }
            return false;
        }

        public bool GetYellowBlockCount(int index, int p)
        {
            List<(int, int)> pos = new List<(int, int)>
            { 
                (56, 440),
                (56, 465),
                (56, 490),
                (56, 515),
                (56, 540),
                (56, 590),
                (56, 615),
                (56, 640),
                (56, 665),
                (56, 690),
                (56, 740),
                (56, 765),
                (56, 790),
                (56, 815),
                (56, 840),
                (1278, 440),
                (1278, 465),
                (1278, 490),
                (1278, 515),
                (1278, 540),
                (1278, 590),
                (1278, 615),
                (1278, 640),
                (1278, 665),
                (1278, 690),
                (1278, 740),
                (1278, 765),
                (1278, 790),
                (1278, 815),
                (1278, 840)
            };
            int x = pos[index * 5 + 5 - p].Item1;
            int y = pos[index * 5 + 5 - p].Item2;
            Color c1 = GetGameColor(x, y);
            //Color c2 = GetGameColor(x, y + 9);
            //MessageBox.Show(posColor.R.ToString() + " " +  posColor.G.ToString() + " " + posColor.B.ToString() + " ");
            //File.AppendAllText(@"color.txt", posColor.R.ToString() + " " + posColor.G.ToString() + " " + posColor.B.ToString() + " ");
            if (c1.B < 5 && c1.R > 40 && c1.G > 220)
            {
                return true;
            }
            //else if (!(Math.Abs(posColor.R - 0x80) < 30 && Math.Abs(posColor.G - 0x4a) < 30 && Math.Abs(posColor.B - 0x24) < 30
            //    && !(Math.Abs(posColor.R - 0x97) < 30 && Math.Abs(posColor.G - 0x6e) < 30 && Math.Abs(posColor.B - 0x3b) < 30)))
            //{
            //    count = -1;
            //    break;
            //}
            //File.AppendAllText(@"color.txt", "\n");
            //MessageBox.Show(count.ToString());
            return false;

        }

        public bool Complete()
        {
            int x1 = 557, y1 = 189, x2 = 1370, y2 = 320;
            Color c1 = GetGameColor(x1, y1), c2 = GetGameColor(x2, y2);
            if (c1.R > 250 && c1.G > 250 && c1.B > 250 && c2.R > 250 && c2.G > 250 && c2.B > 250 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public bool IfFasterComplete()
        //{
        //    int x1 = 565, y1 = 200, x2 = 1365, y2 = 325, x3 = 942, y3 = 347;
        //    Color c1 = GetGameColor(x1, y1), c2 = GetGameColor(x2, y2), c3 = GetGameColor(x3, y3);
        //    if (c1.R > 250 && c1.G > 250 && c1.B > 250 && c2.R > 250 && c2.G > 250 && c2.B > 250  && c3.R < 5 && c3.G < 5 && c3.B < 5)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public bool InGame()
        {
            Color c1 = GetGameColor(1656, 16);
            if (c1.R > 0xbe && c1.R < 0xc5 && c1.G > 0x95 && c1.G < 0xa4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IfDeploy()
        {
            Color c1 = GetGameColor(1600, 120);
            Color c2 = GetGameColor(1600, 98);
            if (c1.R > 250 && c1.G > 250 && c1.B > 250
                && Math.Abs(c2.R - 0xff) < 20 && Math.Abs(c2.G - 0x79) < 20 && Math.Abs(c2.B - 0x00) < 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int AbilityReady(int index)
        {
            Color c1 = GetGameColor(200 + index * 100, 1035);
            return c1.R + c1.G + c1.B;
        }

        public bool IfHeroDeploy()
        {
            Color c1 = GetGameColor(1757, 272);
            Color c2 = GetGameColor(1670, 274);
            if ((c1.R > 245 && c1.B < 10) || (c2.R > 245 && c2.B < 10))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ifFail()
        {
            Color c1 = GetGameColor(948, 237);
            Color c2 = GetGameColor(837, 239);
            if (c2.R > 250 && c2.G > 250 && c2.B > 25 &&
                Math.Abs(c1.R - 0xdd) < 30 && Math.Abs(c1.G - 0x92) < 30 && Math.Abs(c1.B - 0x34) < 30)
            {
                return true;
            }
            return false;
        }

        public int GetRestartX()
        {
            Color c1 = GetGameColor(850, 765);
            Color c2 = GetGameColor(960, 765);
            Color c3 = GetGameColor(1330, 800);
            if (Math.Abs(c1.R - 0x71) < 30 && Math.Abs(c1.G - 0xe8) < 30 && Math.Abs(c1.B - 0x00) < 30)
            {
                return 850;
            }
            if (Math.Abs(c1.R - 0xff) < 30 && Math.Abs(c1.G - 0xdd) < 30 && Math.Abs(c1.B - 0x00) < 30)
            {
                return 850;
            }
            if (Math.Abs(c2.R - 0x71) < 30 && Math.Abs(c2.G - 0xe8) < 30 && Math.Abs(c2.B - 0x00) < 30)
            {
                return 960;
            }
            if (Math.Abs(c3.R - 0x71) < 30 && Math.Abs(c3.G - 0xe8) < 30 && Math.Abs(c3.B - 0x00) < 30)
            {
                return 1330;
            }
            return -1;
        }

        private Mat CreateMask(Mat templateImage)
        {
            Mat mask = new Mat();
            Mat[] channels = templateImage.Split();
            //MessageBox.Show(channels.Length.ToString());
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

        private string RecognizeDigits(Mat image, int mode)
        {
            //Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
            //Cv2.Threshold(image, image, 128, 255, ThresholdTypes.Binary);

            List<(int, int)> digits = new List<(int, int)>();
            if (mode != 1) // round
            {
                if (gameDpi == 0)
                {
                    OpenCvSharp.Size size = new OpenCvSharp.Size((int)(125 * 0.662), (int)(70 * 0.662));
                    Cv2.Resize(image, image, size);
                }
                else
                {
                    OpenCvSharp.Size size = new OpenCvSharp.Size((int)(83 * 0.662), (int)(47 * 0.662));
                    Cv2.Resize(image, image, size);
                }
                //Cv2.ImShow("123", image);
                //Thread.Sleep(1000);
                //MessageBox.Show(image.Width.ToString() + ", " + image.Height.ToString());
            }
            //List<double> maxVals = new List<double>();
            foreach (var template in digitTemplates)
            {
                Mat result = new Mat();
                Mat mask = CreateMask(template.Value);
                //Cv2.Threshold(result, result, 0.8, 1.0, ThresholdTypes.Tozero);
                int tryGet = 0;
                while (tryGet < 9)
                {
                    tryGet++;
                    Cv2.MatchTemplate(image, template.Value, result, TemplateMatchModes.CCoeffNormed, mask);
                    OpenCvSharp.Point maxLoc;
                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out maxLoc);
                    
                    if (maxVal >= 0.80 && !double.IsInfinity(maxVal) && !double.IsNaN(maxVal))
                    {
                        digits.Add((maxLoc.X, template.Key));
                        //maxVals.Add(maxVal);
                        Rect rect = new Rect(maxLoc.X, maxLoc.Y, (int)(template.Value.Width * 0.9), (int)(template.Value.Height * 0.9));
                        Cv2.Rectangle(image, rect, Scalar.Green, -1);
                        //OpenCvSharp.Point Center;
                        //Center.X = maxLoc.X + template.Value.Width / 2;
                        //Center.Y = maxLoc.Y + template.Value.Height / 2;
                        //Cv2.FloodFill(result, maxLoc, new Scalar(0));
                    }
                    else
                    {
                        if (double.IsInfinity(maxVal) || double.IsNaN(maxVal))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            if (digits.Count == 0)
            {
                return "";
            }
            //string log = "";

            var sortedList = digits.OrderBy(t => t.Item1);
            List<int> posresult = sortedList.Select(t => t.Item1).ToList();
            List<int> digitresult = sortedList.Select(t => t.Item2).ToList();
            
            //for (int i = 0; i < digits.Count; i++)
            //{
            //    log += digitresult[i].ToString() + " " + posresult[i].ToString() + "   ";
            //}
            //File.AppendAllText(@"D:\numtest\num.txt", log + "\n");
            //List<int> roundNum = new List<int>();
            //if (mode == 2) // round
            //{
            //    roundNum.Add(digitresult[0]);
            //    if (digits.Count > 1)
            //    {
            //        for (int i = 1; i < digits.Count; i++)
            //        {
            //            if (posresult[i] - posresult[i - 1] > (gameDpi == 1 ? 16 : 25))
            //            {
            //                break;
            //            }
            //            roundNum.Add(digitresult[i]);
            //        }
            //    }
            //    return string.Concat(roundNum);
            //}
            //image.ImWrite($@"D:\numtest\{numString}.png");
            return string.Concat(digitresult); ;
        }

        public (int, int) GetMapPos(int mapName)
        {
            if (GetClientRect(hWnd, out clientRect))
            {
                clientTopLeft.X = clientRect.Left;
                clientTopLeft.Y = clientRect.Top;
                DisPlayMouseCoordinates.ClientToScreen(hWnd, ref clientTopLeft);
            }
            //MessageBox.Show(dpi.ToString());
            Rectangle area = new Rectangle((int)(clientTopLeft.X * windowdpi), (int)(clientTopLeft.Y * windowdpi), 1920, 1080); // 替换为回合数区域的实际坐标和尺寸
            Mat image = new Mat();
            using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }

                // 转换为OpenCV Mat
                image = BitmapConverter.ToMat(bitmap);
            }
            string templatePath = $@"data\templates\maps\{mapName}.png"; // 模板图片路径
            Mat TemplateMap = Cv2.ImRead(templatePath, ImreadModes.Unchanged);
            if (gameDpi == 1)
            {
                OpenCvSharp.Size size = new OpenCvSharp.Size((int)(TemplateMap.Width / 1.5), (int)(TemplateMap.Height / 1.5));
                Cv2.Resize(TemplateMap, TemplateMap, size);
            }
            Mat result = new Mat();
            Cv2.MatchTemplate(image, TemplateMap, result, TemplateMatchModes.CCoeffNormed);
            OpenCvSharp.Point maxLoc;
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out maxLoc);
            if (maxVal >= 0.90)
            {                
                OpenCvSharp.Point Center;
                Center.X = maxLoc.X + TemplateMap.Width / 2;
                Center.Y = maxLoc.Y + TemplateMap.Height / 2;
                return (Center.X, Center.Y);
            }
            else
            {
                return (-1, -1);
            }
        }
        public int GetMapId()
        {
            if (GetClientRect(hWnd, out clientRect))
            {
                clientTopLeft.X = clientRect.Left;
                clientTopLeft.Y = clientRect.Top;
                DisPlayMouseCoordinates.ClientToScreen(hWnd, ref clientTopLeft);
            }
            //MessageBox.Show(dpi.ToString());
            Rectangle area = new Rectangle((int)(clientTopLeft.X * windowdpi), (int)(clientTopLeft.Y * windowdpi), 1920, 1080); // 替换为回合数区域的实际坐标和尺寸
            Mat image = new Mat();
            using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }

                // 转换为OpenCV Mat
                image = BitmapConverter.ToMat(bitmap);
            }
            int maxMapId = 90;
            double maxMapVal = 0;
            for (int i = 90; i < 102; i++)
            {
                string templatePath = $@"data\templates\maps\{i}.png"; // 模板图片路径
                Mat TemplateMap = Cv2.ImRead(templatePath, ImreadModes.Unchanged);
                if (gameDpi == 1)
                {
                    OpenCvSharp.Size size = new OpenCvSharp.Size((int)(TemplateMap.Width / 1.5), (int)(TemplateMap.Height / 1.5));
                    Cv2.Resize(TemplateMap, TemplateMap, size);
                }
                Mat result = new Mat();
                Cv2.MatchTemplate(image, TemplateMap, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
                if (maxMapVal < maxVal)
                {
                    maxMapVal = maxVal;
                    maxMapId = i;
                }
            }
            return maxMapId;
        }
        public (int, int) GetHeroPos(int heroName)
        {
            if (GetClientRect(hWnd, out clientRect))
            {
                clientTopLeft.X = clientRect.Left;
                clientTopLeft.Y = clientRect.Top;
                DisPlayMouseCoordinates.ClientToScreen(hWnd, ref clientTopLeft);
            }
            //MessageBox.Show(dpi.ToString());
            Rectangle area = new Rectangle((int)(clientTopLeft.X * windowdpi), (int)(clientTopLeft.Y * windowdpi), 1920, 1080); // 替换为回合数区域的实际坐标和尺寸
            Mat image = new Mat();
            using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                }

                // 转换为OpenCV Mat
                image = BitmapConverter.ToMat(bitmap);
            }
            string templatePath = $@"data\templates\heros\{heroName}.png"; // 模板图片路径
            Mat TemplateMap = Cv2.ImRead(templatePath, ImreadModes.Unchanged);
            if (gameDpi == 1)
            {
                OpenCvSharp.Size size = new OpenCvSharp.Size((int)(TemplateMap.Width / 1.5), (int)(TemplateMap.Height / 1.5));
                Cv2.Resize(TemplateMap, TemplateMap, size);
            }
            Mat result = new Mat();
            Cv2.MatchTemplate(image, TemplateMap, result, TemplateMatchModes.CCoeffNormed);
            OpenCvSharp.Point maxLoc;
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out maxLoc);
            if (maxVal >= 0.80)
            {
                OpenCvSharp.Point Center;
                Center.X = maxLoc.X + TemplateMap.Width / 2;
                Center.Y = maxLoc.Y + TemplateMap.Height / 2;
                return (Center.X, Center.Y);
            }
            else
            {
                return (-1, -1);
            }
        }
    }




    internal struct NewStruct
    {
        public int X;
        public int Y;

        public NewStruct(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is NewStruct other &&
                   X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static implicit operator (int X, int Y)(NewStruct value)
        {
            return (value.X, value.Y);
        }

        public static implicit operator NewStruct((int X, int Y) value)
        {
            return new NewStruct(value.X, value.Y);
        }
    }
}
