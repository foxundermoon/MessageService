using log4net;
using MessageService.Core.SocketMessage.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageService.Core.SocketMessage.Command;
namespace MessageService.Core.SocketMessage
{
    public class SocketRequestParser : IRequestInfoParser<SocketRequestInfo>
    {
        public SocketRequestInfo ParseRequestInfo(string source)
        {
            ILog log = LogManager.GetLogger("Receiver");
            log.Info(source);
            var rt = new SocketRequestInfo();
            const string GpsSep = "$GPSLOC";
            rt.OriginBody = source;
            //Console.WriteLine("origin----->" + source);
            if (source.Length > 0)  //has content
            {
                if (source.Equals(Command.Inspection.Request))
                {
                    rt.Key = Keys.Inspection;
                    return rt;
                }
                int iPos = source.IndexOf(GpsSep);
                if (iPos < 0) // no   $GPSLOC
                {
                    byte[] bts = Encoding.ASCII.GetBytes(source);
                    String strTarget = ByteToString(bts);
                    rt.Key = Keys.RFID;
                    rt.TargetBody = strTarget;
                    return rt;
                }
                else if (iPos > 10) // rfid info
                {
                    byte[] bt1 = Encoding.ASCII.GetBytes(GpsSep);
                    String label = ByteToString(bt1);
                    byte[] bt2 = Encoding.ASCII.GetBytes(source);
                    String strNew = ByteToString(bt2);
                    String strT = strNew.Substring(0, strNew.IndexOf(label));
                    rt.Key = Keys.RFID;
                    rt.TargetBody = strT;
                    return rt;
                }
                else //$GPSLOC posion  0-10
                {
                    int pos = source.IndexOf(',');
                    if (pos <= 0) //not contains coma  ,
                    {
                        rt.Key = Keys.UNKNOW;
                        return rt;
                    }
                    String strFirst = source.Substring(0, pos);
                    if (strFirst.IndexOf(GpsSep) >= 0)
                    {
                        rt.Key = Keys.GPS;
                    }
                    else
                    {
                        rt.Key = Keys.UNKNOW;
                        return rt;
                    }
                }
            }
            else
            {
                rt.Key = Keys.UNKNOW;
                return rt;
            }
            if (string.IsNullOrEmpty(rt.Key))
                rt.Key = Keys.UNKNOW;
            return rt;
        }

        public string ByteToString(byte[] inBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte inByte in inBytes)
            {
                sb.AppendFormat("{0:X2} ", inByte);
            }
            return sb.ToString();
        }
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public static string ByteToString(byte[] inBytes, int len)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.AppendFormat("{0:X2} ", inBytes[i]);
            }
            return sb.ToString();
        }
    }
}
