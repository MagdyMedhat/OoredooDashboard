using MVCBootstrapDashboard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
using System.Web.Services;

namespace MVCBootstrapDashboard.Analyst
{
    public class AnalystController : ApiController
    {

        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        //connectionString;

        class marker
        {
            public marker(string symbol)
            {
                this.symbol = symbol;
            }
            public string symbol;
        }

        class chartData
        {
            public string color;
            public string name;
            public string type;
            public int yAxis;
            public List<object> data;
        };

        class scatterChartData : chartData
        {
            public marker marker;
        };

        class pieChartData
        {
            public string name;
            public List<object> data;
        };

        [HttpGet]
        public List<string> GetORIGINHOSTNAMEs()
        {
            try
            {
                var ORIGINHOSTNAMEs = new List<string>();
                var queryCommand = @"SELECT DISTINCT USER_ID FROM ooredoo_alarms_organized
where USER_ID IS NOT NULL AND USER_ID <>''
ORDER BY 1";
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();
                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new object[1];
                        reader.GetValues(row);
                        if (!ORIGINHOSTNAMEs.Contains(row[0].ToString()))
                            ORIGINHOSTNAMEs.Add(row[0].ToString());
                    }
                }
                return ORIGINHOSTNAMEs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public List<string> GetAnalysts([FromUri] string dtStartDate, string dtEndDate)
        {
            var analysts = new List<string>();
            var queryCommand = @"select distinct user_id
from ooredoo_alarms_organized 
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) and user_id is not null and user_id<>''
order by 1
";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand command = con.CreateCommand();
                command.CommandText = queryCommand;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new object[1];
                    reader.GetValues(row);
                    analysts.Add(row[0].ToString());
                }
            }
            return analysts;
        }

        [HttpGet]
        public List<string> GetDays([FromUri] string dtStartDate, string dtEndDate)
        {
            var days = new List<string>();
            var queryCommand = @"select distinct cast(modified_date as date) 
from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) and user_id is not null and user_id<>''
order by 1
";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand command = con.CreateCommand();
                command.CommandText = queryCommand;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new object[1];
                    reader.GetValues(row);
                    var temp = row[0].ToString();
                    days.Add(temp.Remove(temp.Length - 12));
                }
            }
            return days;
        }

        [HttpGet]
        public List<string> GetHours([FromUri] string dtStartDate, string dtEndDate)
        {
            var hours = new List<string>();
            var queryCommand = @"select distinct DATEPART(HOUR, modified_date)
from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) and user_id is not null and user_id<>''
order by 1
";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand command = con.CreateCommand();
                command.CommandText = queryCommand;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new object[1];
                    reader.GetValues(row);
                    hours.Add(row[0].ToString());
                }
            }
            return hours;
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_ANALYST([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"With dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized 
where  cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) ),
SRC as (select cast(modified_date as date)  day,count(*) cnt,
(select count(*) cnt from ooredoo_alarms_organized I where cast(I.modified_date as date) = cast(O.modified_date as date)  ) Tot_Cnt
from ooredoo_alarms_organized O
where   user_id = '" + ORIGINHOSTNAMEs + @"'  and cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) 
group by cast(modified_date as date))
select d.day,isnull(SRC.cnt,0)cnt,round(cast(isnull(SRC.cnt,0) as float) /isnull(SRC.Tot_cnt,1) * 100,1)Ratio 
from SRC right join dates d on SRC.day = d.day 
order by 1";

                List<object> filteredRows = new List<object>();
                //HashSet<string> daydates = new HashSet<string>();

                chartData count = new chartData();
                chartData ratio = new chartData();

                count.name = "Count";
                count.type = "column";
                count.yAxis = 0;
                count.data = new List<object>();

                ratio.name = "Ratio";
                ratio.type = "line";
                ratio.yAxis = 1;
                ratio.data = new List<object>();

                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new object[3];
                        reader.GetValues(row);
                      //  var temp = row[0].ToString();
                     //   daydates.Add(temp.Remove(temp.Length - 12));
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        //if (!daydates.Contains(Convert.ToDateTime(row[0].ToString()).ToString("MM/dd/yyyy")))
                        //    daydates.Add(Convert.ToDateTime(row[0].ToString()).ToString("MM/dd/yyyy"));

                        count.data.Add(new object[2]{date, row[1]});
                        ratio.data.Add(new object[2]{date, row[2]});
                    }
                }


                filteredRows.Add(count);
                filteredRows.Add(ratio);

                return new object[] { filteredRows };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_FRD_ANALYST([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"With dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized 
where  cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) ),
SRC as (select cast(modified_date as date)  day,count(*) cnt,
(select count(*) cnt from ooredoo_alarms_organized I where I.status = 2 and cast(I.modified_date as date) = cast(O.modified_date as date)  ) Tot_Cnt
from ooredoo_alarms_organized O
where   status = 2 and user_id = '" + ORIGINHOSTNAMEs + @"'  and cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) 
group by cast(modified_date as date))
select d.day,isnull(SRC.cnt,0)cnt,round(cast(isnull(SRC.cnt,0) as float) /isnull(SRC.Tot_cnt,1) * 100,1)Ratio 
from SRC right join dates d on SRC.day = d.day 
order by 1";

                List<object> filteredRows = new List<object>();
                //HashSet<string> daydates = new HashSet<string>();

                chartData count = new chartData();
                chartData ratio = new chartData();

                count.name = "Count";
                count.type = "column";
                count.yAxis = 0;
                count.data = new List<object>();

                ratio.name = "Ratio";
                ratio.type = "line";
                ratio.yAxis = 1;
                ratio.data = new List<object>();

                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new object[3];
                        reader.GetValues(row);
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;

                        count.data.Add(new object[2]{date, row[1]});
                        ratio.data.Add(new object[2]{date, row[2]});
                    }
                }


                filteredRows.Add(count);
                filteredRows.Add(ratio);

                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_NFR_ANALYST([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"With dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized 
where  cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) ),
SRC as (select cast(modified_date as date)  day,count(*) cnt,
(select count(*) cnt from ooredoo_alarms_organized I where I.status = 4 and cast(I.modified_date as date) = cast(O.modified_date as date)  ) Tot_Cnt
from ooredoo_alarms_organized O
where  status = 4 and  user_id = '" + ORIGINHOSTNAMEs + @"'  and cast(modified_date as date) between cast('" + dtStartDate + @"'as date) and cast('" + dtEndDate + @"' as date) 
group by cast(modified_date as date))
select d.day,isnull(SRC.cnt,0)cnt,round(cast(isnull(SRC.cnt,0) as float) /isnull(SRC.Tot_cnt,1) * 100,1)Ratio 
from SRC right join dates d on SRC.day = d.day 
order by 1";

                List<object> filteredRows = new List<object>();
                
                chartData count = new chartData();
                chartData ratio = new chartData();

                count.name = "Count";
                count.type = "column";
                count.yAxis = 0;
                count.data = new List<object>();

                ratio.name = "Ratio";
                ratio.type = "line";
                ratio.yAxis = 1;
                ratio.data = new List<object>();

                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new object[3];
                        reader.GetValues(row);

                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;

                        count.data.Add(new object[2]{date, row[1]});
                        ratio.data.Add(new object[2]{date, row[2]});
                    }
                }


                filteredRows.Add(count);
                filteredRows.Add(ratio);

                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_PIE([FromUri] string dtStartDate, string dtEndDate, string ORIGINHOSTNAMEs)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date) created_date, cast(modified_date as date) modified_date, USER_ID, Status , GROUPS , count(*) cnt , sum(value) amount  
from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select USER_ID ,sum(cnt) FRD
from SRC S
where STATUS = 2
group by USER_ID";
                pieChartData count = new pieChartData();
                count.name = "Count";
                count.data = new List<object>();
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        object[] row = new object[2];
                        reader.GetValues(row);
                        var item = new
                        {
                            name = row[0],
                            y = row[1]
                        };
                        count.data.Add(item);
                    }
                }
                return new object[] { count };
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] AMOUNT_PIE([FromUri] string dtStartDate, string dtEndDate, string ORIGINHOSTNAMEs)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
  select USER_ID ,sum(amount) FRD
  from SRC S
  where STATUS = 2
  group by USER_ID";
                pieChartData count = new pieChartData();
                count.name = "Count";
                count.data = new List<object>();
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        object[] row = new object[2];
                        reader.GetValues(row);
                        var item = new
                        {
                            name = row[0],
                            y = row[1]
                        };
                        count.data.Add(item);
                    }
                }
                return new object[] { count };
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] CHART_14_A([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"With s as (select id status,name,color from status),
dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)),s_dates as(select day,Status,name,color from Dates cross join S),
 SRC as ( select cast(modified_date as date) Day,DATEPART(HOUR, modified_date) Hour,status,count(*) cnt
from  ooredoo_alarms_organized 
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)  and user_id='" + ORIGINHOSTNAMEs + @"'
group by cast(modified_date as date) ,status,DATEPART(HOUR, modified_date))
, SRC2 as(select day,status,max(cnt)Max_Cnt from src group by day,status)
select s.Day,isnull(SRC.Hour,'') Hour,s.name Status,isnull(SRC.cnt,'')cnt,SRC.status,s.color
from  SRC  join SRC2 on  SRC.day=SRC2.day  and SRC.Cnt=SRC2.Max_Cnt and SRC.status=SRC2.status 
right join s_dates s on SRC2.day = s.day and SRC2.status = s.status
order by 1";
                scatterChartData Dup = new scatterChartData();
                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                List<string> Colors = new List<string>();
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();
                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] row = new object[6];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[2].ToString()) && item[2].ToString() != "")
                    {
                        ranges.Add(item[2].ToString());
                        Colors.Add(item[5].ToString());
                    }

                }
                foreach (var range in ranges)
                {
                    scatterChartData d = new scatterChartData();
                    d.name = range;
                    d.type = "scatter";
                    d.yAxis = 0;
                    d.marker = new marker("circle");
                    d.color = Colors[ranges.IndexOf(range)];
                    d.data = new List<object>();
                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[2].ToString() == range.ToString())
                            if (int.Parse(row[3].ToString()) > 0)
                            d.data.Add(new object[2] { date, row[1] });
                            else d.data.Add(new object[2] { date, -1 });
                    }
                    filteredRows.Add(d);
                }
               
                return new object[] { filteredRows };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        [HttpGet]
        public object[] CHART_14_B([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string query = @"select distinct user_id
from ooredoo_alarms_organized 
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) and user_id is not null and user_id<>'' and status = 2
order by 1";
                Dictionary<string, int> analystsIDs = new Dictionary<string, int>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();

                    int analystID = 0;

                    analystsIDs.Add("", analystID++);

                    while (reader.Read())
                    {
                        var row = new object[1];
                        reader.GetValues(row);

                        if (!analystsIDs.ContainsKey(row[0].ToString()))
                            analystsIDs.Add(row[0].ToString(), analystID++);
                    }
                }
                string queryCommand = @"With s as (select id status,name,color from status),
dates as (select distinct DATEPART(HOUR, modified_date) Hour from ooredoo_alarms_organized) ,s_dates as(select Hour,Status,name,color from Dates cross join S),
 SRC as ( select DATEPART(HOUR, modified_date)  Hour,user_id Analyst,status,count(*) cnt
from  ooredoo_alarms_organized 
 where cast(modified_date as date) = cast('" + ORIGINHOSTNAMEs + @"' as date) 
group by DATEPART(HOUR, modified_date) ,status,user_id)
, SRC2 as(select Hour,status,max(cnt)Max_Cnt from src group by Hour,status)
select s.Hour,isnull(SRC.Analyst,'') Analyst,s.name Status,isnull(SRC.cnt,'')cnt,SRC.status,s.color
from  SRC  join SRC2 on  SRC.Hour=SRC2.Hour and SRC.Cnt=SRC2.Max_Cnt and SRC.status=SRC2.status 
right join s_dates s on SRC2.Hour = s.Hour and SRC2.status = s.status
order by 1";
                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                List<string> Days = new List<string>();
                List<string> Colors = new List<string>();
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();
                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] row = new object[6];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                foreach (var item in Rows)
                {
                    if (!Days.Contains(item[0].ToString()))
                    {
                        Days.Add(item[0].ToString());
                    }
                    if (!ranges.Contains(item[2].ToString()) && item[2].ToString() != "")
                    {
                        ranges.Add(item[2].ToString());
                        Colors.Add(item[5].ToString());
                    }

                }
                foreach (var range in ranges)
                {
                    scatterChartData d = new scatterChartData();
                    d.name = range;
                    d.type = "scatter";
                    d.yAxis = 0;
                    d.marker = new marker("circle");
                    d.color = Colors[ranges.IndexOf(range)];
                    d.data = new List<object>();
                    foreach (var row in Rows)
                    {
                        if (row[2].ToString() == range.ToString())
                            if (int.Parse(row[3].ToString()) > 0)
                                d.data.Add(new object[2] { row[0].ToString(), analystsIDs[row[1].ToString()] });
                            else d.data.Add(new object[2] { row[0].ToString(), analystsIDs[""] });
                    }
                    filteredRows.Add(d);
                }
               
               
                return new object[] { filteredRows,analystsIDs.Keys };


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] CHART_14_C([FromUri]string ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                //enumerate analysts
                string query = @"select distinct user_id
from ooredoo_alarms_organized 
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) and user_id is not null and user_id<>''
order by 1";
                Dictionary<string, int> analystsIDs = new Dictionary<string, int>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();

                    int analystID = 0;
                    analystsIDs.Add("", analystID++);
                    while (reader.Read())
                    {
                        var row = new object[1];
                        reader.GetValues(row);

                        if (!analystsIDs.ContainsKey(row[0].ToString()))
                            analystsIDs.Add(row[0].ToString(), analystID++);
                    }

                    
                }

              
          string queryCommand = @"With s as (select id status,name,color from status),
dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where  cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),s_dates as(select day,Status,name,color from Dates cross join S),
 SRC as ( select cast(modified_date as date) Day,user_id Analyst,status,count(*) cnt
from  ooredoo_alarms_organized 
where   cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)  and DATEPART(HOUR, modified_date) = " + ORIGINHOSTNAMEs + @" 
group by cast(modified_date as date) ,status,user_id)
, SRC2 as(select day,status,max(cnt)Max_Cnt from src group by day,status)
select s.day,isnull(SRC.Analyst,'0') Analyst,s.name Status,isnull(SRC.cnt,'')cnt,SRC.status,s.color
from  SRC  join SRC2 on  SRC.day=SRC2.day  and SRC.Cnt=SRC2.Max_Cnt and SRC.status=SRC2.status 
right join s_dates s on SRC2.day = s.day and SRC2.status = s.status
order by 1";
                scatterChartData Dup = new scatterChartData();
               List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                List<string> Colors = new List<string>();
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();
                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] row = new object[6];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[2].ToString()) && item[2].ToString() != "")
                    {
                        ranges.Add(item[2].ToString());
                        Colors.Add(item[5].ToString());
                    }

                }
                foreach (var range in ranges)
                {
                    scatterChartData d = new scatterChartData();
                    d.name = range;
                    d.type = "scatter";
                    d.yAxis = 0;
                    d.marker = new marker("circle");
                    d.color = Colors[ranges.IndexOf(range)];
                    d.data = new List<object>();
                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[2].ToString() == range.ToString())
                            if (int.Parse(row[3].ToString()) > 0)
                                d.data.Add(new object[2] { date,analystsIDs[row[1].ToString()] });
                            else d.data.Add(new object[2] { date,analystsIDs[""] });
                    }
                    filteredRows.Add(d);
                }
               
                return new object[] { filteredRows,analystsIDs.Keys };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
    
}
