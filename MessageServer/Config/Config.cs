using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.Config {
    public class ServerConfig {
        public string MongoServer { get; set; }
        public int XmppPort { get; set; }
        public string FileServer { get; set; }
        public int FileServerPort { get; set; }
        public int LogLevel { get; set; }
        public string MongoDatabase { get; set; }
        public string UserCollection { get; set; }
        public string MessageCollection { get; set; }
        public string FileCollection { get; set; }
        public string ServerResource { get; set; }
        public string ServerIp { get; set; }
        public int ServerUid { get; set; }
    }
}
