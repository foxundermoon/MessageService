using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    public class Message
    {
        public User ToUser { get; set; }
        public User FromUser { get; set; }

        public bool HasError
        {
            get
            {
                return Propertites.Keys.Contains("error");
            }
        }
        public string ErrorType
        {
            get
            {
                return GetProperty("error", "");
            }
        }
        public string ErrorMessage
        {
            get
            {
                return GetProperty("errorMessage", "");
            }
        }
        public string Id
        {
            get
            {
                if (Propertites != null && Propertites.ContainsKey(DicKeys.id))
                {
                    var id = "";
                    var success = Propertites.TryGetValue(DicKeys.id, out id);
                    if (success)
                    {
                        return id;
                    }
                    else
                    {
                        id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
                        Id = id;
                        return id;
                    }
                }
                else
                {
                    Id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
                    return Id;
                }
            }
            set
            {
                if (Propertites == null)
                    Propertites = new Dictionary<string, string>();
                if (Propertites.ContainsKey(DicKeys.id))
                    Propertites.Remove(DicKeys.id);
                Propertites.Add(DicKeys.id, value);
            }
        }


        public IDictionary<string, string> Propertites { get; set; }
        private Command command;

        public Command Command
        {
            get
            {
                if (command == null)
                {
                    command = new Command();
                }
                return command;
            }
            set
            {
                command = value;
            }
        }

        public Message()
        {
            Propertites = new Dictionary<string, string>();
        }

        public string GetJsonCommand()
        {
            if (Command != null)
            {

                JObject jo = new JObject();
                if (!string.IsNullOrWhiteSpace(Command.Name))
                    jo.Add(DicKeys.name, Command.Name);
                if (!string.IsNullOrWhiteSpace(Command.Condition))
                    jo.Add(DicKeys.condition, Command.Condition);
                if (!string.IsNullOrWhiteSpace(Command.Operation))
                    jo.Add(DicKeys.operation, Command.Operation);
                if (Command.NeedBroadcast)
                    jo.Add(DicKeys.needBroadcast, true);
                if (Command.NeedResponse)
                    jo.Add(DicKeys.needResponse, true);
                return jo.ToString();
            }
            return "";

        }
        //public string ToJson()
        //{
        //    return ToJsonObject().ToString();
        //}
        public override string ToString()
        {
            return ToJson();
        }
        public JObject ToJsonObject()
        {
            JObject jmessage = new JObject();
            //if (!string.IsNullOrWhiteSpace(Id))
            //    jmessage.Add(DicKeys.id, Id);
            if (Propertites.Count > 0)
            {

                foreach (var item in Propertites)
                {
                    jmessage.Add(item.Key, item.Value);
                }
            }
            //if (DataTable != null && DataTable.Rows != null && DataTable.Rows.Count > 0)
            //{
            //    JObject jtable = GetJsonObjectTable();
            //    jmessage.Add(DicKeys.dataTable, jtable);
            //}
            return jmessage;
        }

        public string ToJson()
        {
            return ToJsonObject().ToString();
        }
        public string ToJson(bool prety)
        {
            if (prety)
            {
                return ToJsonObject().ToString(Formatting.Indented);
            }
            return ToJson();
        }

        public void AddProperty(string key, string value)
        {
            if (Propertites.ContainsKey(key))
            {
                Propertites.Remove(key);
            }
            Propertites.Add(key, value);
        }
        public string GetProperty(string key, string defaultValue)
        {
            var result = "";
            if (Propertites.ContainsKey(key))
                if (Propertites.TryGetValue(key, out result))
                    return result;
            return defaultValue;


        }
        public string GetProperty(string key)
        {
            return GetProperty(key, "");
        }
        public bool SetJsonCommand(string command)
        {
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(command);
                Command = null;
                if (jo != null && jo.HasValues)
                {
                    Command = new Command();
                    var numberable = jo.GetEnumerator();
                    while (jo.HasValues && numberable.MoveNext())
                    {
                        if (string.Equals(numberable.Current.Key, DicKeys.name))
                        {
                            Command.Name = (string)numberable.Current.Value;
                        }
                        else if (string.Equals(numberable.Current.Key, DicKeys.needResponse))
                        {
                            Command.NeedResponse = (bool)numberable.Current.Value;

                        }
                        else if (string.Equals(numberable.Current.Key, DicKeys.operation))
                        {
                            Command.Operation = (string)numberable.Current.Value;

                        }
                        else if (string.Equals(numberable.Current.Key, DicKeys.condition))
                        {
                            Command.Condition = (string)numberable.Current.Value;

                        }
                        else if (string.Equals(numberable.Current.Key, DicKeys.needBroadcast))
                        {
                            Command.NeedBroadcast = (bool)numberable.Current.Value;

                        }
                        else if (string.Equals(numberable.Current.Key, DicKeys.sql))
                        {
                            Command.Sql = (string)numberable.Current.Value;

                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }




        }
        public void SwitchDirection()
        {
            var to = ToUser;
            var from = FromUser;
            ToUser = from;
            FromUser = to;
        }
        public bool SetJsonMessage(string message)
        {
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(message);
                if (jo != null && jo.HasValues)
                {

                    var pairs = jo.GetEnumerator();
                    while (pairs.MoveNext())
                    {
                        if (string.Equals(pairs.Current.Key, DicKeys.dataTable))
                        {
                            //setJsonTable((JObject)pairs.Current.Value);
                        }
                        else //if (pairs.Current.Value != null)
                        {
                            Propertites.Add(pairs.Current.Key, (string)pairs.Current.Value);
                        }
                    }
                    return true;
                }
                else { return false; }
            }
            catch (Exception e)
            {
                return false;
            }

        }
}
}
