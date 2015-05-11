using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XmppClient
{
    public class Ping
    {
        [DllImport("KERNEL32.dll", CharSet = CharSet.Auto, EntryPoint = "GetTickCount")]
        public static extern long GetTickCount();

    }
    struct PingRequest
    {
        public string PingID { get; set; }
        public long StartTime { get; set; }
    }
    struct PingResponse
    {
        public string PingID { get; set; }
        public long ResponseTime { get; set; }
    }
}
