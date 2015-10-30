using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using System.Net.Http.Formatting;
using System.Configuration;
using Microsoft.Owin.Cors;
using MessageService.Core.signalR;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace MessageService
{
    public class HttpApiLauncher
    {
        public static void Launch()
        {

            var port = ConfigurationManager.AppSettings["MessageWebApiPort"].ToString();
            if (string.IsNullOrEmpty(port))
                Program.Exit("MessageWebApiPort没有正确设置，请检查app.config");
            string baseAddress = "http://localhost:"+ port;
            // Start OWIN host 
            Console.WriteLine("Starting webApi service ......");
            WebApp.Start<HttpApiLauncher>(url: baseAddress);
            Console.WriteLine("webApi started ......");

        }


        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "v1",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "by action",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            appBuilder.UseWebApi(config);

            appBuilder.Map("/raw-connection", map =>
            {
                map.UseCors(CorsOptions.AllowAll)
                .RunSignalR<RawConnection>();
            });
            appBuilder.Map("/signalr", map =>
            {
                var hubConfig = new HubConfiguration
                {
                    // versions of IE) require JSONP to work cross domain
                    EnableJSONP = true ,
                  
                };
                map.UseCors(CorsOptions.AllowAll)
                .RunSignalR(hubConfig);
            });
            // Turn tracing on programmatically

            GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;
        }
    }
}
