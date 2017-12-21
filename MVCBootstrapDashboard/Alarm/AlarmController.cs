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

namespace MVCBootstrapDashboard.Alarm
{
    public class AlarmController : ApiController
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;


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
        public object[] MORE_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"SELECT * FROM (SELECT CONVERT(VARCHAR(24),CREATED_DATE,120) CREATED_DATE, CONVERT(VARCHAR(24),modified_date,120) MODIFIED_DATE,ALERT_COUNT,STATUS,USER_ID SCORE
 ,VALUE CDR_COUNT,REFERENCE_VALUE,ACCOUNT_NAME,SUBSCRIBER_NAME, GROUPS DISPLAY_VALUE, RULE_NAME,
ALERT_CDR_COUNT ,ALERT_REPEAT_COUNT, ROW_NUMBER() OVER(ORDER BY MODIFIED_DATE) RowNumber
 FROM OOREDOO_ALARMS_ORGANIZED
 where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
@") as tempTable WHERE  RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"order by 1";

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
                        object[] row = new object[13];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = @"SELECT COUNT(*) FROM (SELECT CREATED_DATE,MODIFIED_DATE,ALERT_COUNT,STATUS,USER_ID SCORE
 ,VALUE CDR_COUNT,REFERENCE_VALUE,ACCOUNT_NAME,SUBSCRIBER_NAME, GROUPS DISPLAY_VALUE, RULE_NAME,
ALERT_CDR_COUNT ,ALERT_REPEAT_COUNT, ROW_NUMBER() OVER(ORDER BY MODIFIED_DATE) RowNumber
 FROM OOREDOO_ALARMS_ORGANIZED
 where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
@") as tempTable";

                using (SqlConnection con = new SqlConnection())
                {

                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                        reader.GetValues(count);
                }

                string[] tableHeaders = { "Created Date", "Modified Date", "Alert Count", "Status", "Score",
                                            "CDR Count", "Reference Value", "Account Name", "Subscriber Name",
                                            "Display Value", "Rule Name", "Alert CDR Count", "Alert Repeat Count" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_FRD_NFR_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"with SRC as(
select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount
  from ooredoo_alarms_organized
where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select * from(
  select modified_date, ROW_NUMBER() OVER(ORDER BY modified_date) RowNumber ,(select round(isnull(sum(cnt),0),0)count from src where status =2  and modified_date = S.modified_date) FRD,
  (select round(isnull(sum(cnt),0),0)count from src where status =4 and modified_date = S.modified_date) NFR
  from SRC S
group by modified_date ) as tempTable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
  order by 1";

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
                        object[] row = new object[4];
                        reader.GetValues(row);
                        var row1 = new object[3];
                        row1[0] = row[0].ToString().Split(' ')[0];//.Substring(0, 10);
                        row1[1] = row[2];
                        row1[2] = row[3];
                        Rows.Add(row1);
                    }
                }

                object[] count = new object[1];
                queryCommand = @"with SRC as(
select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount
  from ooredoo_alarms_organized
where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select COUNT(*) from(
  select modified_date, ROW_NUMBER() OVER(ORDER BY modified_date) RowNumber ,(select round(isnull(sum(cnt),0),0)count from src where status =2  and modified_date = S.modified_date) FRD,
  (select round(isnull(sum(cnt),0),0)count from src where status =4 and modified_date = S.modified_date) NFR
  from SRC S
group by modified_date ) as tempTable
  order by 1";

                using (SqlConnection con = new SqlConnection())
                {

                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                        reader.GetValues(count);
                }

                string[] tableHeaders = { "Date Time", "FRD", "NFR" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_FRD_NFR([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                var Rows = new List<Object[]>();
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
 between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" + @" ),RSLT as (select cast(modified_date as date)day ,(select count(*) from ooredoo_alarms_organized where status =2  and cast(modified_date as date) = cast(s.modified_date as date)) FRD,
  (select count(*) from ooredoo_alarms_organized where status =4 and cast(modified_date as date) = cast(s.modified_date as date)) NFR
  from ooredoo_alarms_organized s
  where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" + @" 
  group by cast(modified_date as date)
  )
  select D.Day,R.FRD,R.NFR from RSLT R right join Dates d on d.day=R.Day
  order by 1";

                List<object> filteredRows = new List<object>();


                chartData FRD = new chartData();
                FRD.name = "FRD";
                FRD.type = "column";
                FRD.yAxis = 0;
                FRD.data = new List<object>();

                chartData NFR = new chartData();
                NFR.name = "NFR";
                NFR.type = "column";
                NFR.yAxis = 0;
                NFR.data = new List<object>();

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
                        object[] row = new object[3];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                foreach (var row in Rows)
                {
                    var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                    var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    var date = (temp - temp1).TotalMilliseconds;
                    FRD.data.Add(new object[2] { date, row[1] });
                    NFR.data.Add(new object[2] { date, row[2] });
                }
                //scaleSeries(FRD.data, NFR.data);
                filteredRows.Add(FRD);
                filteredRows.Add(NFR);
                return new Object[] { filteredRows };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] AMOUNT_CLOSED_ALARMS_FRD_NFR([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                var Rows = new List<Object[]>();
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
 between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" + @" ),RSLT as (select cast(modified_date as date)day ,(select sum(isnull(value,0)) 
 from ooredoo_alarms_organized where status =2  and cast(modified_date as date) = cast(s.modified_date as date)) FRD,
  (select sum(isnull(value,0)) from ooredoo_alarms_organized where status =4 and cast(modified_date as date) = cast(s.modified_date as date)) NFR
  from ooredoo_alarms_organized s
  where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" + @" 
  group by cast(modified_date as date)
  )
  select D.Day,R.FRD,R.NFR from RSLT R right join Dates d on d.day=R.Day
  order by 1";


                List<object> filteredRows = new List<object>();

                chartData FRD = new chartData();
                FRD.name = "FRD";
                FRD.type = "column";
                FRD.yAxis = 0;
                FRD.data = new List<object>();

                chartData NFR = new chartData();
                NFR.name = "NFR";
                NFR.type = "column";
                NFR.yAxis = 0;
                NFR.data = new List<object>();

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
                        object[] row = new object[3];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                foreach (var item in Rows)
                {

                    var temp = DateTime.Parse(item[0].ToString()).ToUniversalTime();
                    var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    var date = (temp - temp1).TotalMilliseconds;
                    FRD.data.Add(new object[2] { date, item[1] });
                    NFR.data.Add(new object[2] { date, item[2] });
                }
                filteredRows.Add(FRD);
                filteredRows.Add(NFR);
                return new Object[] { filteredRows };
            }
            catch (Exception ex)
            {
                return null;
            }


        }

        [HttpGet]
        public object[] AMOUNT_CLOSED_ALARMS_FRD_NFR_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(
select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount
  from ooredoo_alarms_organized
where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select * from(
  select modified_date, ROW_NUMBER() OVER(ORDER BY modified_date) RowNumber ,(select round(isnull(sum(amount),0),0)count from src where status =2  and modified_date = S.modified_date) FRD,
  (select round(isnull(sum(amount),0),0)count from src where status =4 and modified_date = S.modified_date) NFR
  from SRC S
group by modified_date ) as tempTable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
  order by 1";
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
                        object[] row = new object[4];
                        reader.GetValues(row);
                        var row1 = new object[3];
                        row1[0] = row[0].ToString().Split(' ')[0];//.Substring(0, 10);
                        row1[1] = row[2];
                        row1[2] = row[3];
                        Rows.Add(row1);
                    }
                }

                object[] count = new object[1];
                queryCommand = @"with SRC as(
select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount
  from ooredoo_alarms_organized
where cast(modified_date as date) between CAST('" + dtStartDate + @"' AS date) and CAST('" + dtEndDate + @"'AS date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select COUNT(*) from(
  select modified_date, ROW_NUMBER() OVER(ORDER BY modified_date) RowNumber ,(select round(isnull(sum(amount),0),0)count from src where status =2  and modified_date = S.modified_date) FRD,
  (select round(isnull(sum(amount),0),0)count from src where status =4 and modified_date = S.modified_date) NFR
  from SRC S
group by modified_date ) as tempTable
  order by 1";

                using (SqlConnection con = new SqlConnection())
                {

                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                        reader.GetValues(count);
                }
                string[] tableHeaders = { "Date Time", "FRD", "NFR" };
                return new object[] { count, Rows, tableHeaders };

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [HttpGet]
        public object[] COUNT_FRD_NFR_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END status, round(isnull(sum(cnt),0),0)count
from SRC S
group by CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END";
                pieChartData count = new pieChartData();
                count.name = "Count of Closed Alarms - Fraud vs. Non Fraud Distribution";
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
        public object[] COUNT_FRD_NFR_PIE_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
     @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select * from(
  select  CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END status, round(isnull(sum(cnt),0),0)count, ROW_NUMBER() OVER(ORDER BY CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END) RowNumber
from SRC S
group by CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END
) AS tempTable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
  order by 1
  ";
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
                        Rows.Add(row);
                    }
                }

                object[] count = new object[] { 2 };
                string[] tableHeaders = { "Name", "Count" };
                return new object[] { count, Rows, tableHeaders };

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [HttpGet]
        public object[] AMOUNT_FRD_NFR_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END status, round(isnull(sum(amount),0),0)amount
from SRC S
group by CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END";
                pieChartData count = new pieChartData();
                count.name = "Amount of Closed Alarms - Fraud vs. Non Fraud Distribution";
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
        public object[] AMOUNT_FRD_NFR_PIE_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
     @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select * from(
    select  CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END status, round(isnull(sum(amount),0),0)count, ROW_NUMBER() OVER(ORDER BY CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END) RowNumber
from SRC S
group by CASE status WHEN 2 THEN 'FRD' ELSE 'NFR'END
) AS tempTable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
  order by 1
  ";
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
                        Rows.Add(row);
                    }
                }

                object[] count = new object[] { 2 };
                string[] tableHeaders = { "Name", "Amount" };
                return new object[] { count, Rows, tableHeaders };

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [HttpGet]
        public object[] COUNT_9_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Status , GROUPS)
select groups,count(*) count
from SRC S
where STATUS = 2
group by groups";
                pieChartData count = new pieChartData();
                count.name = "Count of Closed Alarms - Fraud - Subscriber Group Distribution";
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
        public object[] COUNT_CLOSED_ALARMS_FRD_IMEI_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),RSLT as (select cast(modified_date as date) day,count(*)Count ,(select count(*) from ooredoo_alarms_organized where STATUS = 2 and cast(s.modified_date as date)=cast(modified_date as date)  )Tot_Count
from ooredoo_alarms_organized s
where STATUS = 2 and upper(rule_name) like '%IMEI%' and cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) group by cast(modified_date as date))
select * from (select D.Day,isnull(R.Count,0)Count, isnull(round((cast(R.count as float) /R.Tot_Count) * 100,1),0) Ratio,ROW_NUMBER() OVER(ORDER BY D.Day) RowNumber
from Rslt r right join Dates D on R.Day=D.Day) as Temptable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"";
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
                        object[] row = new object[3];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Split(' ')[0];//.Substring(0, 10); 
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),RSLT as (select cast(modified_date as date) day,count(*)Count ,(select count(*) from ooredoo_alarms_organized where STATUS = 2 and cast(s.modified_date as date)=cast(modified_date as date)  )Tot_Count
from ooredoo_alarms_organized s where STATUS = 2 and upper(rule_name) like '%IMEI%' and cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) group by cast(modified_date as date))
select Count(*) from (select D.Day,isnull(R.Count,0)Count, isnull(round((cast(R.count as float) /R.Tot_Count) * 100,1),0) Ratio,ROW_NUMBER() OVER(ORDER BY D.Day) RowNumber
from Rslt r right join Dates D on R.Day=D.Day) as Temptable";

                using (SqlConnection con = new SqlConnection())
                {

                    con.ConnectionString = connectionString;
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    string sql = queryCommand;
                    command.CommandText = sql;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                        reader.GetValues(count);
                }

                string[] tableHeaders = { "Date Time", "Count", "Ratio" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] COUNT_CLOSED_ALARMS_FRD_IMEI([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),RSLT as (select cast(modified_date as date) day,count(*)Count ,(select count(*) from ooredoo_alarms_organized where STATUS = 2 and cast(s.modified_date as date)=cast(modified_date as date)  )Tot_Count
from ooredoo_alarms_organized s
where STATUS = 2 and upper(rule_name) like '%IMEI%' and cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) group by cast(modified_date as date))
select D.Day,isnull(R.Count,0)Count, isnull(round((cast(R.count as float) /R.Tot_Count) * 100,1),0) Ratio
from Rslt r right join Dates D on R.Day=D.Day
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
                        count.data.Add(new object[2] { date, row[1] });
                        ratio.data.Add(new object[2] { date, row[2] });
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
        public object[] COUNT_CLOSED_ALARMS_FRD_IRSF([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),RSLT as (select cast(modified_date as date) day,count(*)Count ,(select count(*) from ooredoo_alarms_organized where STATUS = 2 and cast(s.modified_date as date)=cast(modified_date as date)  )Tot_Count
from ooredoo_alarms_organized s
where STATUS = 2 and upper(rule_name) like '%IRSF%' and cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) group by cast(modified_date as date))
select D.Day,isnull(R.Count,0)Count, isnull(round((cast(R.count as float) /R.Tot_Count) * 100,1),0) Ratio
from Rslt r right join Dates D on R.Day=D.Day
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
                        count.data.Add(new object[2] { date, row[1] });
                        ratio.data.Add(new object[2] { date, row[2] });
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
        public object[] COUNT_CLOSED_ALARMS_FRD_INTERNATIONAL([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),RSLT as (select cast(modified_date as date) day,count(*)Count ,(select count(*) from ooredoo_alarms_organized where STATUS = 2 and cast(s.modified_date as date)=cast(modified_date as date)  )Tot_Count
from ooredoo_alarms_organized s
where STATUS = 2 and upper(rule_name) like '%INTERNATIONAL%' and cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) group by cast(modified_date as date))
select D.Day,isnull(R.Count,0)Count, isnull(round((cast(R.count as float) /R.Tot_Count) * 100,1),0) Ratio
from Rslt r right join Dates D on R.Day=D.Day
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
        public object[] COUNT_12_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date,rule_name , USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID ,rule_name, Status , GROUPS)
select Network_ID, count (*)count
from SRC S where STATUS = 2 group by Network_ID";
                pieChartData count = new pieChartData();
                count.name = "Count of Closed Alarms - Fraud - Network Distribution";
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
        public object[] COUNT_13_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date, Display_Value, USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Display_Value, Status , GROUPS)
select Display_Value, count (*)count
from SRC S where STATUS = 2 group by Display_Value";
                pieChartData count = new pieChartData();
                count.name = "Count of Closed Alarms - Fraud - Display Value Distribution";
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
        public object[] COUNT_13B_PIE([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                //var Rows = new List<Object[]>();
                string queryCommand = @"with SRC as(select   NETWORK_ID , cast(created_date as date)created_date,cast(modified_date as date) modified_date, Display_Value, USER_ID , Status , GROUPS , count(*) cnt , sum(value) amount  from ooredoo_alarms_organized
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)" +
  @"group by NETWORK_ID , cast(created_date as date), cast(modified_date as date), USER_ID , Display_Value, Status , GROUPS)
select Display_Value, count (*)count
from SRC S where STATUS = 4 group by Display_Value";
                pieChartData count = new pieChartData();
                count.name = "Count of Closed Alarms - Non Fraud - Display Value Distribution";
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
        public object[] CHART_15A_DETAILS([FromUri] string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                var Rows = new List<object[]>();
                var count = new object[1];
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),SRC as ( select  cast(modified_date as date) Day,DATEPART(HOUR, modified_date) Hour,count(*) cnt
from  ooredoo_alarms_organized 
where status=2 AND cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)
group by cast(modified_date as date) ,DATEPART(HOUR, modified_date))
, SRC2 as(select day,max(cnt)Max_Cnt from src group by day)
select * from(
select ROW_NUMBER() OVER(ORDER BY D.Day) RowNumber ,D.Day,isnull(SRC.Hour,'') Hour,isnull(SRC.Cnt,0) Cnt from SRC join SRC2 on SRC.day=SRC2.day and SRC.Cnt=SRC2.Max_Cnt
right join Dates D on D.Day = SRC.Day 
) as temptable
where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new object[4];
                        reader.GetValues(row);
                        Rows.Add(new object[] { row[1].ToString().Split(' ')[0]/*.Substring(0, 10)*/, row[2], row[3] });
                    }
                }

                queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),SRC as ( select  cast(modified_date as date) Day,DATEPART(HOUR, modified_date) Hour,count(*) cnt
from  ooredoo_alarms_organized 
where status=2 AND cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)
group by cast(modified_date as date) ,DATEPART(HOUR, modified_date))
, SRC2 as(select day,max(cnt)Max_Cnt from src group by day)
select count(*) from (
select D.Day,SRC.Hour,SRC.Cnt from SRC join SRC2 on SRC.day=SRC2.day and SRC.Cnt=SRC2.Max_Cnt
right join Dates D on D.Day = SRC.Day ) as TempTable";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand command = con.CreateCommand();
                    command.CommandText = queryCommand;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        reader.GetValues(count);
                    }
                }

                string[] tableHeaders = { "Day", "Hour", "Count" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] CHART_15A_OLD([FromUri] string dtStartDate, string dtEndDate)
        {
            try
            {
                string queryCommand = @"with Dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) 
between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date) ),SRC as ( select  cast(modified_date as date) Day,DATEPART(HOUR, modified_date) Hour,count(*) cnt
from  ooredoo_alarms_organized 
where status=2 AND cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)
group by cast(modified_date as date) ,DATEPART(HOUR, modified_date))
, SRC2 as(select day,max(cnt)Max_Cnt from src group by day)
select D.Day,SRC.Hour,SRC.Cnt from SRC join SRC2 on SRC.day=SRC2.day and SRC.Cnt=SRC2.Max_Cnt
right join Dates D on D.Day = SRC.Day 
order by 1";

                List<object> filteredRows = new List<object>();
                List<string> daydates = new List<string>();

                chartData time = new chartData();
                time.name = "Hours";
                time.type = "scatter";
                time.yAxis = 0;
                time.data = new List<object>();

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
                        var temp = row[0].ToString();
                        daydates.Add(temp.Remove(temp.Length - 12));
                        time.data.Add(row[1]);
                    }
                }
                filteredRows.Add(time);

                return new object[] { filteredRows, daydates };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] CHART_15A(string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"With s as (select id status,name,color from status),
dates as (select distinct cast(modified_date as date) day from ooredoo_alarms_organized where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)),s_dates as(select day,Status,name,color from Dates cross join S),
 SRC as ( select cast(modified_date as date) Day,DATEPART(HOUR, modified_date) Hour,status,count(*) cnt
from  ooredoo_alarms_organized 
where cast(modified_date as date) between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)  
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
        public object[] CHART_15B(string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = @"with s as (select id status,name,color from status),
dates as (select distinct  case (cast ( datepart(wk, cast(modified_date as date))- datepart(wk,dateadd(m, DATEDIFF(M, 0, cast(modified_date as date)), 0)) + 1 as varchar)) when 1 then 'First' when 2 then 'Second' when 3 then 'Third' when 4 then 'Fourth' when 5 then 'Fifth' when 6 then 'Sixth' end   +' Week Of '+DATENAME(month,  cast(modified_date as date)) Week  from ooredoo_alarms_organized where cast(modified_date as date) 
 between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)  ),s_dates as(select Week,Status,name,color from Dates cross join S),
SRC as( select cast(modified_date as date) day,
case (cast ( datepart(wk, cast(modified_date as date))- datepart(wk,dateadd(m, DATEDIFF(M, 0, cast(modified_date as date)), 0)) + 1 as varchar)) when 1 then 'First' when 2 then 'Second' when 3 then 'Third' when 4 then 'Fourth' when 5 then 'Fifth' when 6 then 'Sixth' end   +' Week Of '+DATENAME(month,  cast(modified_date as date)) Week ,status,count(*) cnt
from  ooredoo_alarms_organized 
where cast(modified_date as date)  between cast('" + dtStartDate + @"' as date) and cast('" + dtEndDate + @"' as date)  
group by cast(modified_date as date),status,case (cast ( datepart(wk, cast(modified_date as date))- datepart(wk,dateadd(m, DATEDIFF(M, 0, cast(modified_date as date)), 0)) + 1 as varchar)) when 1 then 'First' when 2 then 'Second' when 3 then 'Third' when 4 then 'Fourth' when 5 then 'Fifth' when 6 then 'Sixth' end   +' Week Of '+DATENAME(month,  cast(modified_date as date))  )
, SRC2 as(select Week,status,max(cnt)Max_Cnt from src group by Week,status)
select s.Week,SRC.Day,s.name Status,isnull(SRC.cnt,'')cnt,SRC.status,s.color
from  SRC  join SRC2 on SRC.week=SRC2.Week  and SRC.Cnt=SRC2.Max_Cnt and SRC.status=SRC2.status 
right join s_dates s on SRC2.Week = s.Week and SRC2.status = s.status
order by 2";
                scatterChartData Dup = new scatterChartData();
                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                List<string> Colors = new List<string>();
                List<string> Categories = new List<string>();

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
                    if (!Categories.Contains(item[0].ToString()))
                        Categories.Add((item[0].ToString()));
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
                        var temp = DateTime.Parse(row[1].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[2].ToString() == range.ToString())
                            if (int.Parse(row[3].ToString()) > 0)
                                d.data.Add(new object[2] { row[1], date });
                            else d.data.Add(new object[2] { -1, date });
                    }
                    filteredRows.Add(d);
                }

                return new object[] { filteredRows, Categories };
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        void scaleSeries(List<Object> l1, List<Object> l2)
        {
            var newScaled = new List<object>();
            float avg1 = 0f, avg2 = 0f, scale;
            foreach (var item1 in l1)
            {
                avg1 += float.Parse(item1.ToString());
            }
            avg1 /= l1.Count;
            foreach (var item2 in l2)
            {
                avg2 += float.Parse(item2.ToString());
            }
            avg2 /= l2.Count;
            if (avg1 >= avg2)
            {
                scale = avg1 / avg2;
                for (int i = 0; i < l2.Count; i++)
                {
                    l2[i] = (Object)(float.Parse(l2[i].ToString()) * scale);
                }
            }
            else
            {
                scale = avg2 / avg1;
                for (int i = 0; i < l1.Count; i++)
                {
                    l1[i] = (Object)(float.Parse(l1[i].ToString()) * scale);
                }
            }

        }
    }
}

