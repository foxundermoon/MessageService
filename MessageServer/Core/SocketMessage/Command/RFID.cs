using FoxundermoonLib.Database.Sql;
using log4net;
using MessageService.Core.Device;
using MessageService.Core.SocketMessage.Data;
using SuperSocket.SocketBase.Command;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage.Command
{
    public class RFID : CommandBase<SocketSession, SocketRequestInfo>
    {
        Manager manager;
        ILog rfidLogger;
        public RFID()
            : base()
        {
            manager = Manager.Instance;
            rfidLogger = LogManager.GetLogger("RfidReceiver");
        }
        public override void ExecuteCommand(SocketSession session, SocketRequestInfo requestInfo)
        {
            //Console.WriteLine("RFID requestInfo  key:" + requestInfo.Key + "  originBody:" + requestInfo.OriginBody + " targetBody:" + requestInfo.TargetBody);
            var strCarID = session.CarID;
            if (string.IsNullOrEmpty(strCarID))
                session.CarID = "未知";
            rfidLogger.Info("origin body--->" + requestInfo.OriginBody);
            rfidLogger.Info("target body--->" + requestInfo.TargetBody);
            manager.AddRecord(session, requestInfo);
        }

       
    }
}
