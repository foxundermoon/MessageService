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
    public class User
    {
        public string Name { get; set; }
        public string Resource { get; set; }
        public User(string name, string resource)
        {
            Name = name;
            Resource = resource;
        }
    }
}
