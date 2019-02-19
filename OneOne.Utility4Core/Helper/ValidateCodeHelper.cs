using System;
using System.Collections.Generic;
using System.Drawing;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace OneOne.Utility4Core.Helper
{
    public class ValidateCodeHelper
    {
        public ValidateCodeHelper()
        {
        }

        /// <summary>
        /// 验证码的最大长度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }

        /// <summary>
        /// 验证码的最小长度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns></returns>
        public static string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int) DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int) seekRand.Next(0, Int32.MaxValue - length*10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1*(int) Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="validateNum">验证码</param>
        public static byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling((double)GetImageWidth(validateCode.Length)), (int)GetImageHeight());
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(System.DrawingCore.Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(System.DrawingCore.Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", FontSize(), (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new System.DrawingCore.Rectangle(0, 0, image.Width, image.Height),
                 System.DrawingCore.Color.Blue, System.DrawingCore.Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 4, 4);
                //画图片的前景干扰点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, System.DrawingCore.Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(System.DrawingCore.Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 得到验证码图片的长度
        /// </summary>
        /// <param name="validateNumLength">验证码的长度</param>
        /// <returns></returns>
        public static int GetImageWidth(int validateNumLength)
        {
            return (int)(FontSize() * validateNumLength);
        }

        public static int FontSize()
        {
            return 14;
        }

        /// <summary>
        /// 得到验证码的高度
        /// </summary>
        /// <returns></returns>
        public static double GetImageHeight()
        {
            return 30;
        }
    }
}
