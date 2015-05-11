using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessageService.Core.Utils
{
    public class TimeHelper
    {
        public static byte[] GetByteTimeStamp()
        {
            return BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
        }
        public static DateTime ConvertDateTime(byte[]  bts)
        {
            return DateTime.FromBinary(BitConverter.ToInt64(bts, 0));
        }
    }
}