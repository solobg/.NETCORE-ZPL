using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.DrawingCore.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// need unsafe
/// </summary>
namespace ZT410Test
{
    /// <summary>
    /// Unicode字符转化成对应的ZPL指令
    /// </summary>
    public class UnicodeToZPL
    {
        /// <summary>
        /// 生成图片回调显示
        /// </summary>
        public static event Action<Image> OnViewImageCallBack
        {
            add
            {
                Action<Image> action = UnicodeToZPL.ImageCallBack;
                Action<Image> action2;
                do
                {
                    action2 = action;
                    Action<Image> value2 = (Action<Image>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<Image>>(ref UnicodeToZPL.ImageCallBack, value2, action2);
                }
                while (action != action2);
            }
            remove
            {
                Action<Image> action = UnicodeToZPL.ImageCallBack;
                Action<Image> action2;
                do
                {
                    action2 = action;
                    Action<Image> value2 = (Action<Image>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<Image>>(ref UnicodeToZPL.ImageCallBack, value2, action2);
                }
                while (action != action2);
            }
        }

        /// <summary>
        /// 未压缩算法处理
        /// Unicode字符转化成对应的ZPL指令
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="name">生成的图片名称，名称需要唯一</param>
        /// <param name="font">字体</param>
        /// <param name="textDirection">文字方向（0,45,90，180，270）</param>
        /// <returns>
        /// 返回转换完未压缩的ZPL指令
        /// </returns>
        public static string UnCompressZPL(string content, string name, Font font, int textDirection, int startX, int startY)
        {
            Bitmap bitmap;
            string text;
            System.DrawingCore.Size size;

            bitmap = UnicodeToZPL.CreateImage(content, font);
            if (textDirection != (int)TextDirection.Zero)
            {
                bitmap = UnicodeToZPL.SetTextDirection(bitmap, textDirection);
            }
            if (UnicodeToZPL.ImageCallBack != null)
            {
                UnicodeToZPL.ImageCallBack(bitmap);
            }
            text = UnicodeToZPL.ToZPL(bitmap);
            size = bitmap.Size;

            string h = ((size.Width / 8 + ((bitmap.Size.Width % 8 == 0) ? 0 : 1)) * bitmap.Size.Height).ToString();
            string w = (bitmap.Size.Width / 8 + ((bitmap.Size.Width % 8 == 0) ? 0 : 1)).ToString();
            string zpl = string.Format($"~DG{name}.GRF,{h},{w},{text}^FO{startX},{startY}^XG{name}^FS");
            return zpl;
        }

        /// <summary>
        /// 压缩算法处理
        /// Unicode字符转化成对应的ZPL指令
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="name">生成的图片名称，名称需要唯一</param>
        /// <param name="font">字体</param>
        /// <param name="textDirection">文字方向（0,45,90，180，270）</param>
        /// <returns>
        /// 返回转换完压缩的ZPL指令
        /// </returns>
        public static string CompressZPL(string content, string name, Font font, int textDirection, int startX, int startY)
        {
            Bitmap bitmap;
            string text;

            bitmap = UnicodeToZPL.CreateImage(content, font);
            if (textDirection != (int)TextDirection.Zero)
            {
                bitmap = UnicodeToZPL.SetTextDirection(bitmap, textDirection);
            }
            if (UnicodeToZPL.ImageCallBack != null)
            {
                UnicodeToZPL.ImageCallBack(bitmap);
            }
            text = UnicodeToZPL.ToZPL(bitmap);
            text = CompressCharacter.Compress(text);

            string h = ((bitmap.Size.Width / 8 + ((bitmap.Size.Width % 8 == 0) ? 0 : 1)) * bitmap.Size.Height).ToString();
            string w = (bitmap.Size.Width / 8 + ((bitmap.Size.Width % 8 == 0) ? 0 : 1)).ToString();
            string zpl = string.Format($"~DG{name}.GRF,{h},{w},{text}^FO{startX},{startY}^XG{name}^FS");
            return zpl;

        }

        /// <summary>
        /// 生成文字图片
        /// </summary>
        /// <param name="content"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        private static Bitmap CreateImage(string content, Font font)
        {
            StringFormat stringFormat;
            Bitmap bitmap;
            Graphics graphics;
            int width;
            int height;
            Rectangle rectangle;

            stringFormat = new StringFormat(StringFormatFlags.NoClip);
            bitmap = new Bitmap(1, 1);
            graphics = Graphics.FromImage(bitmap);
            SizeF sizeF = graphics.MeasureString(content, font, PointF.Empty, stringFormat);
            width = (int)(sizeF.Width + 1f);
            height = (int)(sizeF.Height + 1f);
            rectangle = new Rectangle(0, 0, width, height);

            bitmap.Dispose();
            bitmap = new Bitmap(width, height);
            graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.FillRectangle(new SolidBrush(Color.White), rectangle);
            graphics.DrawString(content, font, Brushes.Black, rectangle, stringFormat);
            return bitmap;
        }

        /// <summary>
        /// 设置图片方向
        /// </summary>
        /// <param name="bmp">图片</param>
        /// <param name="direction">方向（0,45,90，180，270）</param>
        /// <returns></returns>
        private static Bitmap SetTextDirection(Bitmap bmp, int direction)
        {
            int width;
            int height;
            int num4;
            int num5;
            Bitmap bitmap;
            Graphics graphics;

            direction %= 360;
            double num = (double)direction * 3.1415926535897931 / 180.0;
            double num2 = Math.Cos(num);
            double num3 = Math.Sin(num);
            width = bmp.Width;
            height = bmp.Height;
            num4 = (int)Math.Max(Math.Abs((double)width * num2 - (double)height * num3), Math.Abs((double)width * num2 + (double)height * num3));
            num5 = (int)Math.Max(Math.Abs((double)width * num3 - (double)height * num2), Math.Abs((double)width * num3 + (double)height * num2));
            bitmap = new Bitmap(num4, num5);
            graphics = Graphics.FromImage(bitmap);
            graphics.InterpolationMode = InterpolationMode.Bilinear;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            Point point = new Point((num4 - width) / 2, (num5 - height) / 2);
            Rectangle rect = new Rectangle(point.X, point.Y, width, height);
            Point point2 = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            graphics.TranslateTransform((float)point2.X, (float)point2.Y);
            graphics.RotateTransform((float)(360 - direction));
            graphics.TranslateTransform((float)(-(float)point2.X), (float)(-(float)point2.Y));
            graphics.DrawImage(bmp, rect);
            graphics.ResetTransform();
            graphics.Save();
            graphics.Dispose();
            return bitmap;
        }

        /// <summary>
        /// 将图片转换成ZPL编码
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private unsafe static string ToZPL(Bitmap bitmap)
        {
            StringBuilder stringBuilder;
            BitmapData bitmapData;
            byte* ptr;
            int num;

            stringBuilder = new StringBuilder();
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            ptr = (byte*)bitmapData.Scan0.ToPointer();
            num = 0;
            goto IL_166;
        IL_F3:
            int num2;
            string value = num2.ToString("X").PadLeft(2, '0');
            stringBuilder.Append(value);
            num2 = 0;
            int num3 = 0;
        IL_11D:
            if (num3 >= 8)
            {
                string value2 = num2.ToString("X").PadLeft(2, '0');
                stringBuilder.Append(value2);
                num2 = 0;
                num3 = 0;
            }
            int num4;
            num4++;
        IL_155:
            if (num4 >= bitmapData.Width)
            {
                num++;
            }
            else
            {
                float num5 = 0.11f * (float)ptr[num * bitmapData.Stride + num4 * 3] + 0.59f * (float)ptr[num * bitmapData.Stride + num4 * 3 + 1] + 0.3f * (float)ptr[num * bitmapData.Stride + num4 * 3 + 2];
                num2 *= 2;
                if (num5 < 187f)
                {
                    num2++;
                }
                num3++;
                if (num4 == bitmapData.Width - 1 && num3 < 8)
                {
                    num2 *= (2 ^ 8 - num3);
                    goto IL_F3;
                }
                goto IL_11D;
            }
        IL_166:
            if (num >= bitmapData.Height)
            {
                bitmap.UnlockBits(bitmapData);
                return stringBuilder.ToString();
            }
            num3 = 0;
            num2 = 0;
            num4 = 0;
            goto IL_155;
        }

        private static Action<Image> ImageCallBack;
    }

    /// <summary>
    /// 文字方向
    /// </summary>
    public enum TextDirection
    {
        /// <summary>
        /// 零度
        /// </summary>
        Zero,
        /// <summary>
        /// 四十五度
        /// </summary>
        FortyFive = 45,
        /// <summary>
        /// 九十度
        /// </summary>
        Ninety = 90,
        /// <summary>
        /// 一百八十度
        /// </summary>
        OneHundredEighty = 180,
        /// <summary>
        /// 二百七十度
        /// </summary>
        TwoHundredAndSeventy = 270
    }

    internal class CompressCharacter
    {
        /// <summary>
        /// 压缩zpl字符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Compress(string text)
        {
            List<CompressCodeInfo> list;
            StringBuilder stringBuilder;
            int num2;

            list = new List<CompressCodeInfo>();
            char c = '.';
            int num = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c2 = text[i];
                if (c2 != c)
                {
                    if (num > 0)
                    {
                        list.Add(new CompressCodeInfo(c.ToString(), num));
                    }
                    num = 0;
                    c = c2;
                }
                num++;
                if (i == text.Length - 1 && num > 0)
                {
                    list.Add(new CompressCodeInfo(c.ToString(), num));
                }
            }
            stringBuilder = new StringBuilder();
            num2 = 0;
            goto IL_71E;

        IL_11F:
            CompressCodeInfo compressCodeInfo;
            int num3;
            while (compressCodeInfo.Count - num3 > 0)
            {
                if (compressCodeInfo.Count - num3 < 20)
                {
                    stringBuilder.AppendFormat("{0}{1}", ((CompressCode)(compressCodeInfo.Count - num3)).ToString(), compressCodeInfo.Code);
                    break;
                }
                if (compressCodeInfo.Count - num3 >= 400)
                {
                    num3 += 400;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.z.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 380)
                {
                    num3 += 380;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.y.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 360)
                {
                    num3 += 360;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.x.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 340)
                {
                    num3 += 340;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.w.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 320)
                {
                    num3 += 320;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.v.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 300)
                {
                    num3 += 300;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.u.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 280)
                {
                    num3 += 280;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.t.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 260)
                {
                    num3 += 260;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.s.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 240)
                {
                    num3 += 240;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.r.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 220)
                {
                    num3 += 220;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.q.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 200)
                {
                    num3 += 200;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.p.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 180)
                {
                    num3 += 180;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.o.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 160)
                {
                    num3 += 160;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.n.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 140)
                {
                    num3 += 140;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.m.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 120)
                {
                    num3 += 120;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.l.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 100)
                {
                    num3 += 100;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.k.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 80)
                {
                    num3 += 80;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.j.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 60)
                {
                    num3 += 60;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.i.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 40)
                {
                    num3 += 40;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.h.ToString(), compressCodeInfo.Code);
                }
                else if (compressCodeInfo.Count - num3 >= 20)
                {
                    num3 += 20;
                    stringBuilder.AppendFormat("{0}{1}", CompressCode.g.ToString(), compressCodeInfo.Code);
                }
            }
            goto IL_718;
            goto IL_11F;
        IL_718:
            num2++;
        IL_71E:
            if (num2 >= list.Count)
            {
                return stringBuilder.ToString();
            }
            compressCodeInfo = list[num2];
            if (compressCodeInfo.Count == 0)
            {
                goto IL_718;
            }
            if (compressCodeInfo.Count == 1)
            {
                stringBuilder.Append(compressCodeInfo.Code);
                goto IL_718;
            }
            if (compressCodeInfo.Count <= 20)
            {
                stringBuilder.AppendFormat("{0}{1}", ((CompressCode)compressCodeInfo.Count).ToString(), compressCodeInfo.Code);
                goto IL_718;
            }
            num3 = 0;
            goto IL_11F;
        }

    }

    public enum CompressCode
    {
        G = 1,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        g,
        h = 40,
        i = 60,
        j = 80,
        k = 100,
        l = 120,
        m = 140,
        n = 160,
        o = 180,
        p = 200,
        q = 220,
        r = 240,
        s = 260,
        t = 280,
        u = 300,
        v = 320,
        w = 340,
        x = 360,
        y = 380,
        z = 400
    }

    public class CompressCodeInfo
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="code">压缩字符</param>
        /// <param name="count">数量</param>
        public CompressCodeInfo(string code, int count)
        {
            this.code = code;
            this.count = count;
        }

        /// <summary>
        /// 压缩的字符
        /// </summary>
        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
            }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        private string code;

        private int count;
    }
}
