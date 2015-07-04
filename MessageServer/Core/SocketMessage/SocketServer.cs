using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MessageService.Core.SocketMessage
{
 public class  SocketServer : AppServer<SocketSession,SocketRequestInfo>
    {
     public static readonly SocketServer Instance = new SocketServer();
       public SocketServer() : base(new SocketReceiveFilterFactory()) { }
     
       public static bool StartAt(IServerConfig config)
       {
           if (Instance.Setup(config))
               if (Instance.Start())
               {
                   registerHandler();
                   return true;
               }
           return false;
       }

       private static void registerHandler()
       {
           Instance.NewRequestReceived += Instance_NewRequestReceived;
           Instance.NewSessionConnected += Instance_NewSessionConnected;

       }

        static void Instance_NewSessionConnected(SocketSession session)
        {
            Console.WriteLine("new session connected:"+session);
        }

        static void Instance_NewRequestReceived(SocketSession session, SocketRequestInfo requestInfo)
        {
            Console.WriteLine(requestInfo.OriginBody);
        }
        
    }
}
