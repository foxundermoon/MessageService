using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.GpsDevice
{
    class GpsRequestInfo : IRequestInfo
    {
        public string Key{get;set;}
        public GPSInfo GpsInfomation { get; set; }
        public string OriginBody { get; set; }

    }
    public class GPSInfo
    {
        public string ID { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Speed { get; set; }
        public double Hight { get; set; }
        public string Date { get; set; }
        public int StarNum { get; set; }
        public int Order { get; set; }

        public double DisMove { get; set; }
        public DateTime StandardTime { get; set; }
        public GPSInfo()
        {
            this.DisMove = -1;
        }
        public GPSInfo(GPSInfo p)
        {
            this.ID = p.ID;
            this.Lat = p.Lat;
            this.Lng = p.Lng;
            this.Speed = p.Speed;
            this.Hight = p.Hight;
            this.Date = p.Date;
            this.StarNum = p.StarNum;
            this.Order = p.Order;
            this.DisMove = p.DisMove;
            this.StandardTime = p.StandardTime;
        }

    }
}
