using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage
{
 public   class SocketReceiveFilterFactory : IReceiveFilterFactory<SocketRequestInfo>
    {
        public IReceiveFilter<SocketRequestInfo> CreateFilter(global::SuperSocket.SocketBase.IAppServer appServer, global::SuperSocket.SocketBase.IAppSession appSession, System.Net.IPEndPoint remoteEndPoint)
        {
            return new SocketReceiveFilter();
        }
    }
}
