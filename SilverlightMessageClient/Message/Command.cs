using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightMessageClient.MessageManager
{
    public class Command
    {
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool NeedBroadcast { get; set; }
        public bool NeedResponse { get; set; }
        public string Condition { set; get; }
        public string Sql { set; get; }

    }
}
