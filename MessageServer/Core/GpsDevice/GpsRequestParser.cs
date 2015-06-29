using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.GpsDevice
{
    class GpsRequestParser : IRequestInfoParser<GpsRequestInfo>
    {
       public GpsRequestInfo ParseRequestInfo(string source)
        {
            var rt = new GpsRequestInfo();
          rt.OriginBody = source;
          return rt;
        }
    }
}
