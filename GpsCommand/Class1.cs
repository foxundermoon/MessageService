using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Command;

namespace GpsCommand
{
    public class GpsCommsnd:StringCommandBase
    {
        public override void ExecuteCommand(SuperSocket.SocketBase.AppSession session, SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            Console.WriteLine("cmd-------------"+requestInfo.Body);
        }
    }
}
