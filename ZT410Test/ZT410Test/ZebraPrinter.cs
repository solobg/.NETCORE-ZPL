
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ZT410Test
{
    public class ZebraPrinter
    {

        internal static void PrintLabel(Sample p)
        {
            string code = $@"CT~~CD,~CC^~CT~
^XA
~TA000
~JSN
^LT0
^MNW
^MTT
^PON
^PMN
^LH0,0
^JMA
^PR6,6
~SD15
^JUS
^LRN
^CI27
^PA0,1,1,0
^RS8,,,3
^XZ
^XA
^MMT
^PW827
^LL354
^LS0
^FT558,295^BXN,22,200,0,0,1,_,1
^FH\^FD{p.Id}^FS
^FPH,1^FT616,54^A0N,42,48^FH\^CI28^FD{p.Id}^FS^CI27
^FO527,2^GB0,352,2^FS
^FO5,120^GB522,0,2^FS
^FO1,236^GB526,0,2^FS
{ChangeZhongWen(p.Name)}
^FPH,1^FT76,188^A0N,42,43^FH\^CI28^FD{p.SerialNo}^FS^CI27
^FPH,1^FT76,299^A0N,42,43^FH\^CI28^FD{p.Owner}^FS^CI27
^RFW,H,1,2,1^FD4000^FS
^RFW,A,2,4,1^FD{p.Id}^FS
^PQ1,0,1,Y
^XZ;";
            TCPPrint(code);
        }

        public static void TCPPrint(string cmd)
        {
            TcpClient tcp = new TcpClient();
            try
            {
                tcp.Connect("192.168.1.3", 6101);
                tcp.SendTimeout = 1000;
                tcp.ReceiveTimeout = 1000;
                if (tcp.Connected)
                {
                    tcp.Client.Send(Encoding.Default.GetBytes(cmd));
                }
                else
                {
                    throw new Exception("打印失败，请检查打印机或网络设置。");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (tcp != null)
                {
                    if (tcp.Client != null)
                    {
                        tcp.Client.Close();
                        tcp.Client = null;
                    }
                    tcp.Close();
                }
            }
        }

        private static string ChangeZhongWen(string str)
        {
            var zpl = UnicodeToZPL.UnCompressZPL(str, "name1", new System.DrawingCore.Font("宋体", 40, System.DrawingCore.FontStyle.Bold), 0, 66, 37);
            return zpl;
        }
    }
}
