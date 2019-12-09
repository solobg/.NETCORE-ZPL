using System;

namespace ZT410Test
{
    class Program
    {
        static void Main(string[] args)
        {

            //http://labelary.com/viewer.html
            //打印结果见demo.png 支持中文
            ZebraPrinter.PrintLabel(new Sample
            {
                Id = 1,
                Name = "中文名",
                SerialNo = "No.321",
                Owner = "wnz"
            });
        }
    }
}
