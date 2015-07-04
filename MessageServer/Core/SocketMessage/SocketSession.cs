using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage
{
  public  class SocketSession : AppSession<SocketSession, SocketRequestInfo>
    {
      public ClientType ClientType { get; set; }
      public string CarID { get; set; }
      public string UserName { get; set; }
      public string Resource { get; set; }
      public bool IsAuthenticated { get; set; }
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
        protected override void HandleUnknownRequest(SocketRequestInfo requestInfo)
        {
            base.HandleUnknownRequest(requestInfo);
            Console.WriteLine("HandleUnknownRequest:" + requestInfo);
        }
    }
  public enum ClientType
  {
      GpsDevice,
      RfidDevice,
      Android,
      PcClenet,
  }
}
