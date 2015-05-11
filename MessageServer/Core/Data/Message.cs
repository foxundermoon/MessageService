using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace MessageService.Core.Data
{
    public class Message
    {
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
                        return null;
                    }
                }
                else
                {
                    return null;
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
        public Table DataTable { get; set; }

        public IDictionary<string, string> Propertites { get; set; }

        public Command Command { get; set; }

        public Message()
        {
            Propertites = new Dictionary<string, string>();
        }

        public string GetJsonCommand()
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
            if (DataTable != null && DataTable.Rows != null && DataTable.Rows.Count > 0)
            {
                JObject jtable = GetJsonObjectTable();
                jmessage.Add(DicKeys.dataTable, jtable);
            }
            return jmessage;
        }

        public string GetJsonTable()
        {
            return GetJsonObjectTable().ToString();
        }
        public JObject GetJsonObjectTable()
        {
            JArray columns = new JArray();
            foreach (Column c in DataTable.DataColumns)
            {
                JObject jsonColumn = new JObject();
                jsonColumn.Add(DicKeys.name, c.ColumnName);
                if (!string.IsNullOrWhiteSpace(c.DbType))
                    jsonColumn.Add(DicKeys.dbType, c.DbType);
                columns.Add(jsonColumn);
            }
            JArray rows = new JArray();
            foreach (DataRow r in DataTable.Rows)
            {
                JArray row = new JArray();
                foreach (var item in r.ItemArray)
                {
                    row.Add(item);
                }
                rows.Add(row);
            }
            JObject jtable = new JObject();
            jtable.Add(DicKeys.columns, columns);
            jtable.Add(DicKeys.rows, rows);
            jtable.Add(DicKeys.name, DataTable.TableName);
            jtable.Add(DicKeys.dataBase, DataTable.Database);
            return jtable;
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
        public bool SetJsonCommand(string command)
        {
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(command);
                Command = null;
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
            catch (Exception e)
            {
                return false;
            }


        }
        public bool SetJsonMessage(string message)
        {
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(message);
                if (jo.HasValues)
                {

                    var pairs = jo.GetEnumerator();
                    while (pairs.MoveNext())
                    {
                        if (string.Equals(pairs.Current.Key, DicKeys.dataTable))
                        {
                            setJsonTable((JObject)pairs.Current.Value);
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
        public void setDataTable(DataTable dt)
        {
            DataTable = null;
            DataTable = new Table(dt);
        }
        public void setJsonTable(string jtable)
        {
            JObject tb = JsonConvert.DeserializeObject<JObject>(jtable);
            setJsonTable(tb);
        }

        public void setJsonTable(JObject jtable)
        {
            DataTable = null;
            DataTable = new Table();
            var pairs = jtable.GetEnumerator();
            while (pairs.MoveNext())
            {
                if (string.Equals(pairs.Current.Key, DicKeys.columns))
                {
                    JArray columns = (JArray)pairs.Current.Value;
                    foreach (JObject c in columns)
                    {
                        if (c.HasValues)
                        {
                            var colum = new Column();
                            colum.ColumnName = (string)c.GetValue(DicKeys.name);
                            JToken type; ;
                            var success = c.TryGetValue(DicKeys.dbType, out type);
                            if (success)
                                colum.DbType = (string)type;
                            DataTable.DataColumns.Add(colum);
                        }
                    }
                }
                else if (string.Equals(pairs.Current.Key, DicKeys.rows))
                {
                    JArray rows = (JArray)pairs.Current.Value;
                    if (rows.HasValues)
                    {

                        JArray firstRow = (JArray)rows[0];
                        for (var i = 0; i < firstRow.Count; i++)
                        {
                            if (firstRow[i].Type == JTokenType.String)
                                DataTable.DataColumns[i].DataType = typeof(string);
                            else if (firstRow[i].Type == JTokenType.Integer)
                                DataTable.DataColumns[i].DataType = typeof(int);
                            else if (firstRow[i].Type == JTokenType.Boolean)
                                DataTable.DataColumns[i].DataType = typeof(bool);
                            else if (firstRow[i].Type == JTokenType.Float)
                                DataTable.DataColumns[i].DataType = typeof(float);
                            else if (firstRow[i].Type == JTokenType.Null)
                                DataTable.DataColumns[i].DataType = typeof(DBNull);
                        }
                        foreach (JArray r in rows)
                        {

                            var row = DataTable.NewRow();
                            for (var i = 0; i < r.Count; i++)
                            {
                                //row[i] = r[i];
                                if (r[i].Type == JTokenType.Boolean)
                                {
                                    row[i] = (bool)r[i];

                                }
                                else if (r[i].Type == JTokenType.Integer)
                                {
                                    row[i] = (int)r[i];
                                }
                                else if (r[i].Type == JTokenType.Float)
                                {
                                    row[i] = (double)r[i];
                                }
                                else if (r[i].Type == JTokenType.String)
                                {
                                    row[i] = (string)r[i];
                                }
                                else
                                {
                                    row[i] = (object)r[i];
                                }

                            }
                            DataTable.Rows.Add(row);
                        }
                    }
                }
                else if (string.Equals(pairs.Current.Key, DicKeys.name))
                {
                    DataTable.TableName = (string)pairs.Current.Value;
                }
                else if (string.Equals(pairs.Current.Key, DicKeys.dataBase))
                {
                    DataTable.Database = (string)pairs.Current.Value;
                }

            }

        }
    }
}
