using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Command;
using MessageService.Core.SocketMessage.Command;
using MessageService.Core.SocketMessage.Data;
using System.Collections.Concurrent;
using System.Threading;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using MessageService.Core.Device;
using log4net;

namespace MessageService.Core.SocketMessage
{
    public class GPS : CommandBase<SocketSession, SocketRequestInfo>
    {
        Manager manager;
        ILog gpsLoger;

        public GPS()
            : base()
        {
            manager = Manager.Instance;
            gpsLoger = LogManager.GetLogger("GpsReceiver");
        }
        public override void ExecuteCommand(SocketSession session, SocketRequestInfo requestInfo)
        {
            //Console.WriteLine("receive @ GPS command :  key: " + requestInfo.Key + "  body:" + requestInfo.OriginBody);
            gpsLoger.Info("origin body--->" + requestInfo.OriginBody);
            gpsLoger.Info("target body--->" + requestInfo.TargetBody);
            //throw new NotImplementedException();
            manager.AddRecord(session, requestInfo);
        }


    }
}
