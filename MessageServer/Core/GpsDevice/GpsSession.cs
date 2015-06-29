using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.GpsDevice
{
    class GpsSession : AppSession<GpsSession, GpsRequestInfo>
    {
        protected override void OnSessionClosed(CloseReason reason)
        {
            Console.WriteLine("session closed!");
            base.OnSessionClosed(reason);
        }
        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();
            Console.WriteLine("Contencted!");
        }
        protected override void HandleException(Exception e)
        {
            base.HandleException(e);
            Console.WriteLine("HandleException:" + e.Message);
        }
        protected override void HandleUnknownRequest(GpsRequestInfo requestInfo)
        {
            base.HandleUnknownRequest(requestInfo);
            Console.WriteLine("HandleUnknownRequest:" + requestInfo);
        }
    }
}
