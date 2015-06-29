using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.GpsDevice
{
    class GpsReceiveFilter : TerminatorReceiveFilter<GpsRequestInfo>
    {
      static readonly byte[] terminator = Encoding.ASCII.GetBytes("\n\r");
      GpsRequestParser gpsParser = new GpsRequestParser();
        public GpsReceiveFilter():base(terminator)
        {
        }
        protected override GpsRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length)
        {
           return  gpsParser.ParseRequestInfo(Encoding.ASCII.GetString(data, offset, length));
        }
    }
}
