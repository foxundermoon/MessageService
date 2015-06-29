using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MessageService.Core.GpsDevice
{
    class GpsDeviceServer : AppServer<GpsSession,GpsRequestInfo>
    {
        public static readonly GpsDeviceServer Instance = new GpsDeviceServer();
        public GpsDeviceServer() : base(new GpsReceiveFilterFactory()) { }
        public static bool StartAt(int port)
        {
            registerHandler();
            if (Instance.Setup(port))
                return Instance.Start();
            return false;
        }

        private static void registerHandler()
        {
            Instance.NewRequestReceived += Instance_NewRequestReceived;
           
        }

        static void Instance_NewRequestReceived(GpsSession session, GpsRequestInfo requestInfo)
        {
            Console.WriteLine(requestInfo.OriginBody);
        }
        
    }
}
