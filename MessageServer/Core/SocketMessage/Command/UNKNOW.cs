using SuperSocket.SocketBase.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage.Command
{
    public class UNKNOW : CommandBase<SocketSession, SocketRequestInfo>
    {
        public override void ExecuteCommand(SocketSession session, SocketRequestInfo requestInfo)
        {
            //throw new NotImplementedException();
            Console.WriteLine("unknow cmd  origin：" + requestInfo.OriginBody);
        }
    }
}
