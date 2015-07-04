using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MessageService.Core.SocketMessage;
using System.Configuration;
using MessageService;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace MessageService.App_Start
{
    class SocketServiceLauncher
    {
        public static void Launch()
        {
            Console.WriteLine("starting gps service..");
            new Thread(ThreadBody).Start();
            Console.WriteLine("gps service started");
        }
        public static void Bootstrap()
        {
            Console.WriteLine("starting socket message service by bootstrap!");
            new Thread(bootstrap).Start();
            Console.WriteLine("start success!");

        }

        private static void bootstrap()
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            if (!bootstrap.Initialize())
                Program.Exit("Supersocket  initlialize failed!");
            var result = bootstrap.Start();
            Console.WriteLine("Start result: {0}!", result);
            if (result == StartResult.Failed)
                Program.Exit("Start Socket message service failed!");

        }
        public static void ThreadBody()
        {
            int gpsport = 0;
            try
            {
                gpsport = Convert.ToInt32(ConfigurationManager.AppSettings["GpsServicePort"].ToString());
            }
            catch (Exception e)
            {
                Program.Exit("parse config failed @GpsServicePort " + e.Message);
            }
            if (gpsport < 100)
                Program.Exit("GpsServicePort不能＜100");
            var config = new ServerConfig()
            {
                MaxConnectionNumber=1000,
                MaxRequestLength=50000,
                Port=gpsport,
                KeepAliveTime=90000,
                
            };
            if (SocketServer.StartAt(config))
                Console.WriteLine("gps device service start success @"+gpsport);
            else
            {
                Program.Exit("start gps service failed");
            }
        }
    }
}
