using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage
{
  public  class SocketRequestInfo : IRequestInfo
    {
        public string Key{get;set;}
        public string OriginBody { get; set; }
        public string TargetBody { get; set; }
    }
}
