using FoxundermoonLib.Database.Sql;
using MessageService.Core.SocketMessage;
using MessageService.Core.SocketMessage.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FoxundermoonLib.Database.Sql;
namespace MessageService.Core.Device
{
    public class Manager
    {
        public readonly static Manager Instance = new Manager();

        AspDBManager mdb;
        private Manager()
        {
            gpsCache = new ConcurrentQueue<GpsInfo>();
            gpsTb = new DataTable();
            gpsTb.Columns.AddRange(new DataColumn[]{  
              new DataColumn("车辆GUID",typeof(string)),  
              new DataColumn("经度",typeof(float)),  
              new DataColumn("纬度",typeof(float)),  
              new DataColumn("速度",typeof(float)),  
              new DataColumn("日期",typeof(DateTime)),
              new DataColumn("状态",typeof(string))});

            try
            {
                gpsDbConnectionStr = ConfigurationManager.AppSettings["GpsSqlDbConnectionString"].ToString();
                mdb = new AspDBManager();
                mdb.DbConnectionString = gpsDbConnectionStr;
                mdb.Open();

            }
            catch (Exception e)
            {
                Program.Exit("app config error  @GpsSqlDbConnectionString  :" + e.Message);
            }
            try
            {
                flushSpan = int.Parse(ConfigurationManager.AppSettings["GpsCacheFlushTimeSpan"].ToString());
                if (flushSpan < 1)
                    Program.Exit("app config error  @GpsCacheFlushTimeSpan  must biger than 1  now is " + flushSpan);
                TimeSpan intervalTimeSpan = new TimeSpan(0, 0, flushSpan);
                m_MonitorTimer = new Timer(new TimerCallback(flushCache), new object(), intervalTimeSpan, intervalTimeSpan);
            }
            catch (Exception e)
            {
                Program.Exit("app config error  @GpsCacheFlushTimeSpan  :" + e.Message);
            }

            try
            {
                rfidDbConnectionStr = ConfigurationManager.AppSettings["RfidSqlDbConnectionString"].ToString();
                if (string.IsNullOrEmpty(rfidDbConnectionStr))
                    throw new Exception();
            }
            catch
            {
                Program.Exit("app config error @RfidSqlDbConnectionString");
            }
        }
        ConcurrentQueue<GpsInfo> gpsCache;
        private Timer m_MonitorTimer;
        DataTable gpsTb;
        int flushSpan = 2;
        string gpsDbConnectionStr = null;


        string rfidDbConnectionStr = null;
        public List<String> lstRFIDCodes = new List<string>();

        public List<RfidInfo> lstRFID = new List<RfidInfo>();
        public List<RfidInfo> lstRFIDNull = new List<RfidInfo>();


        public List<GpsInfo> lstRecords = new List<GpsInfo>();
        public List<GpsInfo> lstJustRecords = new List<GpsInfo>();
        public List<GpsInfo> lstJumpRecords = new List<GpsInfo>();

        private void flushCache(object state)
        {
            //flushCache2disk();
            cache2Table();
            TableValuedToDB(gpsTb);
        }



        public void TableValuedToDB(DataTable dt)
        {

            //const string TSqlStatement =
            // "insert into GPSRecord(车号,经度,纬度,卫星,高度,速度,日期,序号)" +
            // " SELECT nc.车号, nc.经度,nc.纬度,nc.卫星,nc.高度,nc.速度,nc.日期,nc.序号" +
            // " FROM @NewBulkTestTvp AS nc";

            const string TSqlStatement =
            "insert into SW_轨迹表(车辆GUID,经度,纬度,速度,时间,状态)" +
            " SELECT nc.车辆GUID, nc.经度,nc.纬度,nc.速度,nc.日期,nc.状态" +
            " FROM @NewBulkTestTvp AS nc";

            SqlCommand cmd = new SqlCommand(TSqlStatement, mdb.cnn);
            SqlParameter catParam = cmd.Parameters.AddWithValue("@NewBulkTestTvp", dt);
            catParam.SqlDbType = SqlDbType.Structured;
            //表值参数的名字叫BulkUdt，在上面的建立测试环境的SQL中有。  
            catParam.TypeName = "dbo.BulkUdt";
            try
            {

                if (dt != null && dt.Rows.Count != 0)
                {
                    cmd.ExecuteNonQuery();
                    gpsTb.Clear();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("插入错误:" + ex.Message);
            }
            finally
            {

            }
        }
        private void cache2Table()
        {
            if (!gpsCache.IsEmpty)
            {
                for (int i = 0; i < gpsCache.Count; i++)
                {
                    GpsInfo item = null;
                    if (gpsCache.TryDequeue(out item))
                    {
                        var r = gpsTb.NewRow();
                        r["车辆GUID"] = item.ID;
                        r["经度"] = item.Lng;
                        r["纬度"] = item.Lat;
                        r["速度"] = item.Speed;
                        r["日期"] = item.Date;
                        if (item.Speed > 0.1)
                            r["状态"] = "正常行驶";
                        else
                            r["状态"] = "停车";
                        gpsTb.Rows.Add(r);
                    }
                }
            }
        }
        private void flushCache2disk()
        {
            Console.WriteLine("flush gps info");

            cache2Table();
            const string tSqlStatement =
            "insert into SW_轨迹表(车辆GUID,经度,纬度,速度,时间,状态)" +
            " SELECT nc.车辆GUID, nc.经度,nc.纬度,nc.速度,nc.日期,nc.状态" +
            " FROM @NewBulkTestTvp AS nc";
            var sqlConn = new SqlConnection(gpsDbConnectionStr);
            try
            {
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Program.Exit("can not open database with :" + sqlConn);
            }
            SqlCommand cmd = new SqlCommand(tSqlStatement, sqlConn);
            var dt = gpsTb;
            SqlParameter catParam = cmd.Parameters.AddWithValue("@NewBulkTestTvp", dt);
            catParam.SqlDbType = SqlDbType.Structured;
            //表值参数的名字叫BulkUdt，在上面的建立测试环境的SQL中有。  
            catParam.TypeName = "dbo.BulkUdt";
            try
            {

                if (dt != null && dt.Rows.Count != 0)
                {
                    cmd.ExecuteNonQuery();
                    gpsTb.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("插入错误:" + ex.Message);
            }
            finally
            {
                sqlConn.Close();
            }

        }



        public void CarLoad(String strCarID)
        {
            if (strCarID != "")
            {
                String strCom = "update  SW_车辆信息表 set 在线状态=0 where 车辆GUID='" + strCarID + "'";
                //mdb.ExecuteSql(strCom);
                strCom = "insert into SW_车辆上下线信息表(车牌号,时间,在线状态) values('" + strCarID + "','" + System.DateTime.Now.ToLocalTime().ToString() + "',0)";
                SqlHelper.ExecteNonQuery(connectionString: rfidDbConnectionStr, cmdType: System.Data.CommandType.Text, cmdText: strCom, commandParameters: null);
                //mdb.ExecuteSql(strCom);
            }

        }

        public bool AddRecord(SocketSession session, GpsInfo record)
        {
            GpsInfo rc = new GpsInfo(record);
            gpsCache.Enqueue(rc);
            return true;
        }



        double dLastLng = -1;
        double dLastLat = -1;


        private object m_FrozedProcessLock = new object();
        public bool AddRFID(SocketSession session, List<RfidInfo> rfids)
        {
            lock (m_FrozedProcessLock)
            {
                for (int i = 0; i < rfids.Count; i++)
                {
                    String strInsertCommand = "insert into SW_垃圾桶收集记录表(卡号,车牌号,收集时间) values('" + rfids[i].Code + "','" + rfids[i].Car + "','" + rfids[i].DateDesri + "')";
                    //mdb.ExecuteSql(strInsertCommand);
                    SqlHelper.ExecteNonQuery(rfidDbConnectionStr, CommandType.Text, strInsertCommand, null);
                }
                return true;
            }
        }


        public void JustFirst(SocketSession session, GpsInfo info)
        {
            if (lstJustRecords.Count < 1)
            {
                QueryHistory(session.CarID);//获得历史的最后一个点的位置
            }
            if (dLastLng > 0 && dLastLat > 0)
            {
                double dis = GetDistance(info.Lat, info.Lng, dLastLat, dLastLng);
                if (dis < 100)
                {
                    lstRecords.Add(info);
                    lstJustRecords.Clear();

                    AddRecord(session, info);
                    Console.WriteLine("*****根据历史添加:");

                    return;
                }
            }
            lstJustRecords.Add(info);

            if (lstJustRecords.Count >= 5)
            {
                //开始判断最后几个点
                //判断第一个
                List<PointDis> lstDis = new List<PointDis>();
                for (int i = lstJustRecords.Count - 1; i >= lstJustRecords.Count - 5; i--)
                {
                    double dis = 0;
                    for (int j = lstJustRecords.Count - 1; j >= lstJustRecords.Count - 5; j--)
                    {
                        if (i == j) continue;
                        dis += GetDistance(lstJustRecords[i].Lat, lstJustRecords[i].Lng, lstJustRecords[j].Lat, lstJustRecords[j].Lng);
                    }
                    PointDis pt = new PointDis();
                    pt.Index = i;
                    pt.Distance = dis;
                    lstDis.Add(pt);
                }
                lstDis.Sort();
                int iFristIndex = lstDis[0].Index;
                for (int i = iFristIndex + 1; i < lstJustRecords.Count - 1; i++)
                {
                    double dis = GetDistance(lstJustRecords[i].Lat, lstJustRecords[i].Lng, lstJustRecords[i - 1].Lat, lstJustRecords[i - 1].Lng);
                    if (dis > 50)
                    {
                        lstJustRecords.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = iFristIndex - 1; i > 0; i--)
                {
                    double dis = GetDistance(lstJustRecords[i].Lat, lstJustRecords[i].Lng, lstJustRecords[i + 1].Lat, lstJustRecords[i + 1].Lng);
                    if (dis > 50)
                    {
                        lstJustRecords.RemoveAt(i);
                    }
                }

                if (lstJustRecords.Count < 1)
                    return;
                //开始写入数据库
                lstRecords.Add(lstJustRecords[lstJustRecords.Count - 1]);
                for (int i = 0; i < lstJustRecords.Count; i++)
                {
                    AddRecord(session, lstJustRecords[i]);
                    Console.WriteLine("*****首次添加:");
                }
                lstJustRecords.Clear();
            }






        }
        public bool QueryHistory(string carId)
        {
            var strCarID = carId;
            String strSQL = "select * from  SW_轨迹表  where 车辆GUID='" + strCarID + "'  and 时间 in (SELECT  max([时间]) as ct FROM SW_轨迹表  where 车辆GUID='" + strCarID + "')";
            System.Data.DataTable table = QueryLastRecord(strSQL);
            if (table != null && table.Rows.Count == 1)
            {
                int iStartIndex = -1;
                dLastLng = Convert.ToDouble(table.Rows[0]["经度"].ToString().Trim());
                dLastLat = Convert.ToDouble(table.Rows[0]["纬度"].ToString().Trim());
                return true;
            }
            return false;
        }
        public System.Data.DataTable QueryLastRecord(String strSql)
        {
            System.Data.DataTable table = SqlHelper.ExecuteDataTable(rfidDbConnectionStr, strSql, null);// mdb.GetDataTable(strSql, "lastRecord");
            return table;
        }
        private const double EARTH_RADIUS = 6378.137;//地球半径
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10;
            return s;
        }
        private double rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        public void AddRecord(SocketSession session, SocketRequestInfo requestInfo)
        {
            String strKey = requestInfo.Key;
            String strVal = requestInfo.OriginBody;
            if (strKey == "" || strKey == string.Empty)
                return;
            bool bAddNewCar = false;
            var strCarID = session.CarID;

            #region RFID
            if (strKey == "RFID")
            {
                strVal = requestInfo.TargetBody;
                if (strCarID == "")
                {
                    strCarID = "未知";
                    CarLoad(strCarID);
                    //((SWGPSServer)(base.AppServer)).CarLoad(this, strCarID);
                }
                //  lock (m_FrozedProcessLock)
                {
                    try
                    {
                        strVal = strVal.Replace(" ", "");
                        string[] strParts = Regex.Split(strVal, "11003F", RegexOptions.IgnoreCase);

                        bool bStart = false;
                        if (strParts.Length >= 1)
                        {

                            if (strVal.Substring(0, 6) == "11003F")
                                bStart = true;
                            else
                                bStart = false;

                            #region 获得此次RFID
                            List<String> lstIDS = new List<string>();
                            for (int i = 0; i < strParts.Length; i++)
                            {
                                if (strParts[i].Length < 26)
                                    continue;
                                if (i == 0)
                                {
                                    if (bStart == false)
                                        continue;
                                }
                                String strRFID = "";
                                strRFID = strParts[i].Substring(0, 26);
                                if (lstIDS.Contains(strRFID))
                                    continue;
                                lstIDS.Add(strRFID);
                                Console.WriteLine("收到-" + strRFID);
                            }
                            #endregion
                            #region 判断是否可入库
                            if (lstIDS.Count > 0)
                            {
                                List<RfidInfo> lstAdd = new List<RfidInfo>();
                                for (int i = 0; i < lstIDS.Count; i++)
                                {
                                    if (lstRFIDCodes.Contains(lstIDS[i]))
                                    {
                                        for (int j = lstRFID.Count - 1; j >= 0; j--)
                                        {
                                            if (lstRFID[j].Code == lstIDS[i])
                                            {
                                                //判断时间间隔
                                                if ((System.DateTime.Now - lstRFID[j].PutDateTime).Minutes < 20)
                                                {
                                                    break;
                                                }
                                                else
                                                {
                                                    lstRFID.RemoveAt(j);
                                                    lstRFIDCodes.RemoveAt(j);
                                                    lstRFIDCodes.Add(lstIDS[i]);
                                                    RfidInfo rf = new RfidInfo();
                                                    rf.Code = lstIDS[i];
                                                    rf.PutDateTime = System.DateTime.Now.ToLocalTime();
                                                    rf.DateDesri = rf.PutDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                                    rf.Car = strCarID;
                                                    lstRFID.Add(rf);
                                                    if (lstRFID.Count > 100)
                                                    {
                                                        lstRFID.RemoveRange(0, lstRFID.Count - 100);
                                                    }
                                                    if (lstRFIDCodes.Count > 100)
                                                        lstRFIDCodes.RemoveRange(0, lstRFIDCodes.Count - 100);
                                                    Console.WriteLine("写入" + rf.Code);
                                                    if (rf.Car == "" || rf.Car == "未知")
                                                    {
                                                        lstRFIDNull.Add(rf);
                                                    }
                                                    else
                                                        lstAdd.Add(rf);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lstRFIDCodes.Add(lstIDS[i]);
                                        if (lstRFIDCodes.Count > 100)
                                            lstRFIDCodes.RemoveRange(0, lstRFIDCodes.Count - 100);
                                        RfidInfo rf = new RfidInfo();
                                        rf.Code = lstIDS[i];
                                        rf.PutDateTime = System.DateTime.Now.ToLocalTime();
                                        rf.DateDesri = rf.PutDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        rf.Car = strCarID;
                                        lstRFID.Add(rf);
                                        if (lstRFID.Count > 100)
                                        {
                                            lstRFID.RemoveRange(0, lstRFID.Count - 100);
                                        }
                                        Console.WriteLine("写入" + rf.Code);
                                        if (rf.Car == "" || rf.Car == "未知")
                                        {
                                            lstRFIDNull.Add(rf);
                                        }
                                        else
                                            lstAdd.Add(rf);
                                    }
                                }
                                if (lstAdd.Count > 0)
                                {

                                    AddRFID(session, lstAdd);

                                    if (strCarID != "" && strCarID != "未知")
                                    {
                                        for (int i = 0; i < lstRFIDNull.Count; i++)
                                        {
                                            lstRFIDNull[i].Car = strCarID;
                                        }
                                        AddRFID(session, lstRFIDNull);
                                        lstRFIDNull.Clear();
                                    }

                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
                return;
            }
            #endregion





            String[] strValues = strVal.Split(',');
            if (strValues == null)
            {
                return;
            }
            if (strValues.Length < 10)
            {
                return;
            }
            String strLog = strValues[0];
            if (strLog.Length > 7)
            {
                String strID = strLog.Replace("$GPSLOC", "");
                strLog = strLog.Substring(strID.Length, strLog.Length - strID.Length);
                strID = "鲁" + strID;
                if (string.IsNullOrEmpty(strCarID) || strCarID == "未知")
                {
                    strCarID = strID;
                    session.CarID = strCarID;
                    CarLoad(strCarID);
                    lstJustRecords.Clear();
                    lstRecords.Clear();
                    Console.WriteLine(strCarID + "上线");

                }
            }
            if (strCarID == "")
            {
                strCarID = "未知";
                CarLoad(strCarID);
                lstJustRecords.Clear();
                lstRecords.Clear();
            }
            String strJD = strValues[2]; //经度
            if (double.Parse(strJD) < 1)
                return;
            {
                if (strJD == "")
                    return;
                if (Convert.ToDouble(strJD) < 0.1)
                    return;
                int iPos = strJD.IndexOf('.');//4
                String str1 = strJD.Substring(0, iPos);
                String strFen = str1.Substring(str1.Length - 2, 2);
                string strDu = str1.Substring(0, str1.Length - strFen.Length);
                string strDot = strJD.Substring(iPos, strJD.Length - str1.Length);
                strJD = (Convert.ToDouble(strDu) + Convert.ToDouble(strFen + strDot) / 60).ToString();
            }
            String strWD = strValues[1]; //纬度
            if (double.Parse(strWD) < 1)
                return;
            {
                if (strWD == "")
                    return;
                if (Convert.ToDouble(strWD) < 0.1)
                    return;
                int iPos = strWD.IndexOf('.');//4
                String str1 = strWD.Substring(0, iPos);
                String strFen = str1.Substring(str1.Length - 2, 2);
                string strDu = str1.Substring(0, str1.Length - strFen.Length);
                string strDot = strWD.Substring(iPos, strWD.Length - str1.Length);
                strWD = (Convert.ToDouble(strDu) + Convert.ToDouble(strFen + strDot) / 60).ToString();
            }
            String strGD = strValues[3]; //高度
            String strTime = strValues[4];//时间
            System.DateTime tm  =default(System.DateTime);
            try
            {
                string strY = strTime.Substring(0, 4);
                string strM = strTime.Substring(4, 2);
                string strD = strTime.Substring(6, 2);
                string strH = strTime.Substring(8, 2);
                // strH = (Convert.ToInt32(strH) + 8).ToString();
                string strMinute = strTime.Substring(10, 2);
                string strsecond = strTime.Substring(12, strTime.Length - 12 - 1);
                strsecond = Convert.ToInt32(Convert.ToDouble(strsecond)).ToString();
                strTime = strY + "-" + strM + "-" + strD + " " + strH + ":" + strM + ":" + strsecond;
                try
                {
                    tm = new DateTime(Convert.ToInt32(strY), Convert.ToInt32(strM), Convert.ToInt32(strD), Convert.ToInt32(strH), Convert.ToInt32(strMinute), Convert.ToInt32(strsecond), DateTimeKind.Utc);
                    System.DateTime tnew = tm.ToLocalTime();
                    strTime = tnew.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch(Exception all)
                {
                    return;
                }

              
            }
            catch (Exception fe)
            {
                session.AppServer.Logger.Error("date format exception :" + fe.Message + fe.StackTrace);
                return;
            }
            string strWXNum = strValues[6];//卫星个数
            string strSpeed = strValues[7];
            double dSpeed = 0;
            if (strSpeed != "")
            {
                try
                {
                    dSpeed = Convert.ToDouble(strSpeed);
                    //dSpeed = dSpeed * 0.5144;//米/秒
                    //dSpeed = dSpeed * 3600 / 1000;
                    dSpeed = dSpeed * 1.85184;
                    strSpeed = dSpeed.ToString("###.##");
                }
                catch (Exception e)
                {

                }
            }
            else
                strSpeed = "0";
            if (strSpeed == "")
                strSpeed = "0";
            string strFix = strValues[5];
            string strHx = strValues[8];
            if (strWXNum == "") strWXNum = "3";
            if (Convert.ToInt32(strWXNum) < 4)
                return;
            if (strGD == "") strGD = "6000";
            if (Convert.ToDouble(strGD) > 3000)
            {
                return;
            }
            if (double.Parse(strSpeed) > 200)
                return;
            String strXH = "0";
            GpsInfo info = new GpsInfo();
            info.ID = strCarID;
            info.Lat = double.Parse(strWD);
            info.Lng = double.Parse(strJD);
            info.Speed = double.Parse(strSpeed);
            info.StarNum = int.Parse(strWXNum);
            info.Hight = double.Parse(strGD);
            info.Order = int.Parse(strXH);
            info.Date = strTime;
            info.StandardTime = tm;


            if (lstRecords.Count < 1)
            {
                JustFirst(session, info);
            }
            else
            {
                //开始判断下一个点是否符合要求
                int iIndex = lstRecords.Count - 1;
                double dDistanceNew = GetDistance(lstRecords[0].Lat, lstRecords[0].Lng, info.Lat, info.Lng);
                if (dDistanceNew < 200)
                {
                    lstRecords.Add(info);
                    lstRecords.RemoveAt(0);
                    if (lstJumpRecords.Count > 0)
                        lstJumpRecords.Clear();
                    AddRecord(session, info);
                    Console.WriteLine("*****添加:" + strVal);
                    return;
                }
                else
                {

                    if (lstJumpRecords.Count < 1)
                    {
                        lstJumpRecords.Add(info);
                        return;
                    }
                    else
                    {
                        double dis = GetDistance(lstJumpRecords[lstJumpRecords.Count - 1].Lat, lstJumpRecords[lstJumpRecords.Count - 1].Lng, info.Lat, info.Lng);
                        if (dis < 200)
                        {
                            lstJumpRecords.Add(info);
                            return;
                        }
                    }
                    if ((info.StandardTime - lstRecords[iIndex].StandardTime).TotalSeconds > 120)
                    {
                        lstRecords.Clear();
                        lstJustRecords.Clear();
                        lstJumpRecords.Clear();
                        dLastLat = -1;
                        dLastLng = -1;
                        Console.WriteLine("&&&&&&&&&&&&&&重新计算:" + strVal);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("异常点舍弃:" + strVal);
                    }
                }

            }


        }
    }
}
