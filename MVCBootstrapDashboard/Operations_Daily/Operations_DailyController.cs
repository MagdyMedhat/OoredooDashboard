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

namespace MVCBootstrapDashboard.AIR_REFUND
{
    public class Operations_DailyController : ApiController
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        //connectionString;
        class chartData
        {
            public string name;
            public string type;
            public int yAxis;
            public List<object> data;
        };

        [HttpGet]
        public object[] MSC_COUNT_DURATION_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = queryCommand = @"select * from(
                                                    select date_Time,
                                                    Measure_From,
                                                    Measure_To,
                                                    net_count Total_Number_Of_CDRs,
                                                    sum_net_duration Total_Duration
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total Number Of CDRs", "Total Duration" };
                return new object[] { count, Rows, tableHeaders};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] MSC_COUNT_DURATION([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time,
Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(net_count)Total_Number_Of_CDRs,
sum(sum_net_duration) Total_Duration
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";


                List<object> filteredRows = new List<object>();
                //List<string> daydates = new List<string>();
                List<string> ranges = new List<string>();

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
                        Rows.Add(row);
                    }
                }
                //foreach (var item in Rows)
                //{
                //    if (!daydates.Contains(item[0].ToString()))
                //        daydates.Add(item[0].ToString());

                //}

                //for (int i = 0; i < daydates.Count; i++)
                //{
                //    daydates[i] = daydates[i].Remove(daydates[i].Length - 12);
                //}
                    

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total Number of CDRs";
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total Duration";
                    Duration.data = new List<object>();


                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[1].ToString() == range.ToString())
                        {
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2]{date, row[3]});
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);

                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] IN_TOTALCOUNT_DURATION_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time, Measure_From,Measure_To,total_cdrs Total_Number_Of_CDRs, total_duration Total_Duration
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total Number Of CDRs", "Total Duration" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] IN_TOTALCOUNT_DURATION([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_cdrs)Total_Number_Of_CDRs, sum(total_duration) Total_Duration
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        Rows.Add(row);
                    }
                }

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total_Number_Of_CDRs";
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total_Duration";
                    Duration.data = new List<object>();


                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[1].ToString() == range.ToString())
                        {
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2]{date, row[3]});
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);

                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] MSC_VS_IN_CWITHV_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time,
Measure_From,
Measure_To,
net_count Total_Number_Of_MSC_CDRs,
total_cdrs Total_Number_Of_IN_CDRs,
round((net_count-total_cdrs)/net_count,4)*100 Variance
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[6];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total Number Of MSC CDRs", "Total Number Of IN CDRs", "Variance" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] MSC_VS_IN_CWITHV([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = null;
                if (ORIGINHOSTNAMEs[0].Substring(0,1).ToLower() == "n")
                {
                    queryCommand = @"select cast(date_Time as date) date_Time,
                                Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(net_count)Total_Number_Of_MSC_CDRs,
                                sum(total_cdrs)Total_Number_Of_IN_CDRs,
                                round((sum(net_count)-sum(total_cdrs))/sum(net_count),4)*100 Variance 
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                }
                else if (ORIGINHOSTNAMEs[0].Substring(0,1).ToLower() == "a")
                {
                    queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(net_count)Total_Number_Of_MSC_CDRs,sum(total_cdrs)Total_Number_Of_IN_CDRs,abs(round((sum(net_count)-sum(total_cdrs))/sum(net_count),4)*100) Variance 
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                }


                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }               

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total Number of MSC CDRs";
                    CDR.type = "column";
                    CDR.yAxis = 0;
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total Number of IN CDRs";
                    Duration.type = "column";
                    Duration.yAxis = 0;
                    Duration.data = new List<object>();

                    chartData variance = new chartData();
                    variance.name = range + " Variance";
                    variance.yAxis = 1;
                    variance.data = new List<object>();

                    foreach (var row in Rows)
                    {
                        if (row[1].ToString() == range.ToString())
                        {
                            var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                            var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            var date = (temp - temp1).TotalMilliseconds;
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2]{date, row[3]});
                            variance.data.Add(new object[2]{date, row[4]});
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);
                    filteredRows.Add(variance);
                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] MSC_VS_IN_DWITHV_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time,
Measure_From,
Measure_To,
sum_net_Duration Total_MSC_Duration,
total_duration Total_IN_Duration,
round((sum_net_Duration-total_duration)/sum_net_Duration,4)*100 Variance 
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[6];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total MSC Duration", "Total IN Duration", "Variance" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] MSC_VS_IN_DWITHV([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();
                string queryCommand = null;
                if (ORIGINHOSTNAMEs[0].Substring(0,1).ToLower() == "n")
                {
                    queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(sum_net_Duration)Total_MSC_Duration,sum(total_duration)Total_IN_Duration,round((sum(sum_net_Duration)-sum(total_duration))/sum(sum_net_Duration),4)*100 Variance 
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                }
                else if (ORIGINHOSTNAMEs[0].Substring(0,1).ToLower() == "a")
                {
                    queryCommand = @"select cast(date_Time as date) date_Time,
                                            Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(sum_net_Duration)Total_MSC_Duration,
                                sum(total_duration)Total_IN_Duration,
                                abs(round((sum(sum_net_Duration)-sum(total_duration))/sum(sum_net_Duration),4)*100) Variance 
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                }

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
               

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total MSC Duration";
                    CDR.type = "column";
                    CDR.yAxis = 0;
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total IN Duration";
                    Duration.type = "column";
                    Duration.yAxis = 0;
                    Duration.data = new List<object>();

                    chartData variance = new chartData();
                    variance.name = range + " Variance";
                    variance.yAxis = 1;
                    variance.data = new List<object>();

                    foreach (var row in Rows)
                    {
                        if (row[1].ToString() == range.ToString())
                        {
                            var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                            var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            var date = (temp - temp1).TotalMilliseconds;
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2] { date, row[3] });
                            variance.data.Add(new object[2] { date, row[4] });
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);
                    filteredRows.Add(variance);
                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INMAD_VS_INMAC_VS_COST_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time, Measure_From, Measure_To,total_duration_Without_Prom Total_IN_MA_Duration,total_cdr_Without_Prom Total_IN_MA_Count,total_cost_Without_Prom Total_IN_MA_Cost
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[6];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total IN MA Duration", "Total IN MA Count", "Total IN MA Cost" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INMAD_VS_INMAC_VS_COST([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_duration_Without_Prom)Total_IN_MA_Duration,sum(total_cdr_Without_Prom)Total_IN_MA_Count,sum(total_cost_Without_Prom)Total_IN_MA_Cost
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total_IN_MA_Duration";
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total_IN_MA_Count";
                    Duration.data = new List<object>();

                    chartData cost = new chartData();
                    cost.name = range + " Total_IN_MA_Cost";
                    cost.data = new List<object>();

                    foreach (var row in Rows)
                    {
                        if (row[1].ToString() == range.ToString())
                        {
                            var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                            var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            var date = (temp - temp1).TotalMilliseconds;
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2] { date, row[3] });
                            cost.data.Add(new object[2] { date, row[4] });
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);
                    filteredRows.Add(cost);

                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INDAD_VS_INDAC_VS_COST_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time,
Measure_From,
Measure_To,
total_d_With_Prom_with_d Total_IN_DA_Duration,
total_cdr_With_Prom Total_IN_DA_Count,
total_c_With_Prom_with_d Total_IN_DA_Cost
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[6];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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


                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total IN DA Duration", "Total IN DA Count", "Total IN DA Cost" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INDAD_VS_INDAC_VS_COST([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time,
Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_d_With_Prom_with_d)Total_IN_DA_Duration,
sum(total_cdr_With_Prom)Total_IN_DA_Count,
sum(total_c_With_Prom_with_d)Total_IN_DA_Cost
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }
                

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total_IN_DA_Duration";
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total_IN_DA_Count";
                    Duration.data = new List<object>();

                    chartData cost = new chartData();
                    cost.name = range + " Total_IN_DA_Cost";
                    cost.data = new List<object>();


                    foreach (var row in Rows)
                    {
                        if (row[1].ToString() == range.ToString())
                        {
                            var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                            var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            var date = (temp - temp1).TotalMilliseconds;
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2] { date, row[3] });
                            cost.data.Add(new object[2] { date, row[4] });
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);
                    filteredRows.Add(cost);

                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INMAD_VS_INDAD_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time, Measure_From ,Measure_To,total_duration_Without_Prom Total_IN_MA_Duration,total_d_With_Prom_with_d Total_IN_DA_Duration
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total IN MA Duration", "Total IN DA Duration"};
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] INMAD_VS_INDAD([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_duration_Without_Prom)Total_IN_MA_Duration,sum(total_d_With_Prom_with_d)Total_IN_DA_Duration
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                        Rows.Add(row);
                    }
                }
                

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total_IN_MA_Duration";
                    CDR.data = new List<object>();

                    chartData Duration = new chartData();
                    Duration.name = range + " Total_IN_DA_Duration";
                    Duration.data = new List<object>();


                    foreach (var row in Rows)
                    {
                        if (row[1].ToString() == range.ToString())
                        {
                            var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                            var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            var date = (temp - temp1).TotalMilliseconds;
                            CDR.data.Add(new object[2]{date, row[2]});
                            Duration.data.Add(new object[2] { date, row[3] });
                        }
                    }
                    filteredRows.Add(CDR);
                    filteredRows.Add(Duration);

                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] FILTER_OUT_DETAILS([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select date_Time, Measure_From,Measure_To,filt_msc_count Total_Number_Of_CDRs
                                                    ,ROW_NUMBER() OVER(ORDER BY date_Time DESC) AS RowNumber 
                                        from RA_MSC_vs_IN_Data_organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by 1 ";

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
                        row[0] = row[0].ToString().Substring(0, 10);
                        row[1] = row[1].ToString().Substring(0, 10);
                        row[2] = row[2].ToString().Substring(0, 10);
                        Rows.Add(row);
                    }
                }

                object[] count = new object[1];
                queryCommand = "select COUNT(*) FROM RA_MSC_vs_IN_Data_organized where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                string[] tableHeaders = { "Date Time", "Measure From", "Measure To", "Total Number Of CDRs" };
                return new object[] { count, Rows, tableHeaders };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public object[] FILTER_OUT([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(filt_msc_count)Total_Number_Of_CDRs
                                from RA_MSC_vs_IN_Data_organized
                                where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(date_Time as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";

                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();

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
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData CDR = new chartData();
                    CDR.name = range + " Total Number of CDRs";
                    CDR.data = new List<object>();


                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[1].ToString() == range.ToString())
                            CDR.data.Add(new object[2]{date, row[2]});
                    }
                    filteredRows.Add(CDR);
                }
                return new object[] { filteredRows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public List<string> GetORIGINHOSTNAMEs()
        {
            try
            {
                List<string> Rows = new List<string>();
                Rows.Add("Normal Variance");
                Rows.Add("Absolute Variance");
                return Rows;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public List<object[]> ExportALL([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(date_Time as date) date_Time,
                                                    Cast(Measure_From as date) Measure_From,
                                                    cast(Measure_To as date) Measure_To,
                                                    total_bill_cdrs, total_tap_in_consd_cdrs
                                        from RA_TAPIN_Data_Organized
                                        where date_Time between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        order by date_Time,Measure_From";

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
                        object[] row = new object[5];
                        reader.GetValues(row);
                        Rows.Add(row);
                    }
                }

                return Rows;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
