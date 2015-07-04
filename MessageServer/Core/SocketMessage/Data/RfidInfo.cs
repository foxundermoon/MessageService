using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage.Data
{
   public class RfidInfo
    {
        public string Code { get; set; }
        public DateTime PutDateTime { get; set; }

        public String DateDesri { get; set; }
        public String Car { get; set; }
    }
}
