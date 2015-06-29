using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MessageService.Core.GpsDevice;
using System.Configuration;
using MessageService;

namespace MessageService.App_Start
{
    class GpsDeviceServiceLauncher
    {
        public static void Launch()
        {
            Console.WriteLine("starting gps service..");
            new Thread(ThreadBody).Start();
            Console.WriteLine("gps service started");
        }
        public static void ThreadBody()
        {
            int gpsip = 0;
            try
            {
                gpsip = Convert.ToInt32(ConfigurationManager.AppSettings["GpsServicePort"].ToString());
            }
            catch (Exception e)
            {
                Program.Exit("parse config failed @GpsServicePort " + e.Message);
            }
            if (gpsip < 100)
                Program.Exit("GpsServicePort不能＜100");
            if (GpsDeviceServer.StartAt(gpsip))
                Console.WriteLine("gps device service start success");
            else
            {
                Program.Exit("start gps service failed");
            }
        }
    }
}
