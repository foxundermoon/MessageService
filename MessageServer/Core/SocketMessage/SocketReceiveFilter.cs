using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage
{
   public class SocketReceiveFilter : TerminatorReceiveFilter<SocketRequestInfo>
    {
      static readonly byte[] terminator = Encoding.ASCII.GetBytes("\r\n");
      SocketRequestParser gpsParser = new SocketRequestParser();
        public SocketReceiveFilter():base(terminator)
        {
        }
        protected override SocketRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length)
        {
           return  gpsParser.ParseRequestInfo(Encoding.ASCII.GetString(data, offset, length));
        }
    }
}
