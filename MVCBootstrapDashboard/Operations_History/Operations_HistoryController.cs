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

    public class Operations_HistoryController : ApiController
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        class chartData
        {
            public string name;
            public List<object> data;
        };        
        [HttpGet]
        public object[] Get_TAPIN_Details([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select cast(Day_Date as date) Day_Date,
                                                    Cast(Measure_From as date) Measure_From,
                                                    cast(Measure_To as date) Measure_To,
                                                    total_tap_in_consd_cdrs Total_Number_Of_CDRs
                                                     ,ROW_NUMBER() OVER(ORDER BY Day_Date DESC) AS RowNumber 
                                        from RA_TAPIN_Data_Organized
                                        where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by day_date,Measure_From";

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
                queryCommand = "select COUNT(*) FROM RA_TAPIN_Data_Organized where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                return new object[] {count, Rows};
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpGet]
        public object[] Get_TAPIN([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(Day_Date as date) Day_Date, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_tap_in_consd_cdrs)Total_Number_Of_CDRs
                                from RA_TAPIN_Data_Organized
                                where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(Day_Date as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                int totalCount = 0;
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
                        totalCount += int.Parse(row[2].ToString());
                     }
                }
                
                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData d = new chartData();
                    d.name = range;
                    d.data = new List<object>();
                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[1].ToString() == range.ToString())
                            d.data.Add(new object[2]{date, row[2]});
                    }
                    filteredRows.Add(d);
                }
                return new object[] {filteredRows, totalCount};
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpGet]
        public object[] Get_BILLING_Details([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate, int PageNumber, int RowsPerPage)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select * from(
                                                    select cast(Day_Date as date) Day_Date,
                                                    Cast(Measure_From as date) Measure_From,
                                                    cast(Measure_To as date) Measure_To,
                                                    total_bill_cdrs Total_Number_Of_CDRs
                                                     ,ROW_NUMBER() OVER(ORDER BY Day_Date DESC) AS RowNumber 
                                        from RA_TAPIN_Data_Organized
                                        where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        ) As TempTable
                                        where RowNumber BETWEEN " + ((PageNumber - 1) * RowsPerPage + 1) + @" AND " + (PageNumber * RowsPerPage) + @"
                                        order by day_date,Measure_From";

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
                queryCommand = "select COUNT(*) FROM RA_TAPIN_Data_Organized where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)";

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

                return new object[] { count, Rows };
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpGet]
        public object[] Get_BILLING([FromUri] string[] ORIGINHOSTNAMEs, string dtStartDate, string dtEndDate)
        {
            try
            {
                List<object[]> Rows = new List<object[]>();

                string queryCommand = @"select cast(Day_Date as date) Day_Date, Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)  Range
                                ,sum(total_bill_cdrs)Total_Number_Of_CDRs
                                from RA_TAPIN_Data_Organized
                                where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                group by cast(Day_Date as date),Cast(Measure_From as varchar) +' - ' +Cast(Measure_To as varchar)
                                order by 1";
                List<object> filteredRows = new List<object>();
                List<string> ranges = new List<string>();
                int totalCount = 0;

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
                        totalCount += int.Parse(row[2].ToString());
                    }
                }
                

                foreach (var item in Rows)
                {
                    if (!ranges.Contains(item[1].ToString()))
                        ranges.Add(item[1].ToString());

                }
                foreach (var range in ranges)
                {
                    chartData d = new chartData();
                    d.name = range;
                    d.data = new List<object>();
                    foreach (var row in Rows)
                    {
                        var temp = DateTime.Parse(row[0].ToString()).ToUniversalTime();
                        var temp1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        var date = (temp - temp1).TotalMilliseconds;
                        if (row[1].ToString() == range.ToString())
                            d.data.Add(new object[2]{date, row[2]});
                    }
                    filteredRows.Add(d);
                }
                return new object[] { filteredRows, totalCount };
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
                Rows.Add("TAPIN Report");
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

                string queryCommand = @"select cast(Day_Date as date) Day_Date,
                                                    Cast(Measure_From as date) Measure_From,
                                                    cast(Measure_To as date) Measure_To,
                                                    total_bill_cdrs, total_tap_in_consd_cdrs
                                        from RA_TAPIN_Data_Organized
                                        where Day_Date between CAST('" + dtStartDate + @"' AS date) And CAST('" + dtEndDate + @"' AS date)
                                        order by day_date,Measure_From";

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

        private object[] FillAIR_Header_Details(object[] Row)
        {
            Row[0] = "ORIGINHOSTNAME";
            Row[1] = "ORIGINTIMESTAMP";
            Row[2] = "CURRENTSERVICECLASS";
            Row[3] = "ACCOUNTNUMBER";
            Row[4] = "B_SUBNO";
            Row[5] = "TRANSACTIONAMOUNT";
            Row[6] = "RANSACTIONTYPE";
            Row[7] = "ACCOUNTBALANCE";
            return Row;
        }
    }
}
