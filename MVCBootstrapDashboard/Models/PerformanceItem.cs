using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBootstrapDashboard.Models
{
    public class PerformanceItem
    {
        public int ID { get; set; }
        public string Financial { get; set; }
        public short Year { get; set; }
        public short Month { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
    }

    public class PerformanceMonthlyTrendItem
    {
        public string LastMonthMonthName { get; set; }
        public string CurerntMonthName { get; set; }
        public string BudgetName { get; set; }

        public double LastMonthMonthValue { get; set; }
        public double CurerntMonthValue { get; set; }
        public double BudgetValue { get; set; }

        public double LastMonthMonthPercentage { get; set; }
        public double CurerntMonthPercentage { get; set; }
        public double BudgetPercentage { get; set; }


        public string CurrentLastDiff { get; set; }
        public string CurrentBudgetDiff { get; set; }
    }

    public class TableRecord
    {
        public string Financial { get; set; }
        public double CurrentActual { get; set; }
        public double CurrentBudget { get; set; }
        public double LastActual { get; set; }
        public double DifferenceCurrentActVsCurrentBud { get; set; }
        public double DifferenceCurrentActVsLastAct { get; set; }
        public double VarianceCurrentActVsCurrentBud { get; set; }
        public double VarianceCurrentActVsLastAct { get; set; }
        public double PercentageRevCurrentActual { get; set; }
        public double PercentageRevCurrentBudget { get; set; }
        public double PercentageRevLastActual { get; set; }
    }


    public class EBITDABridgeRecords 
    {
        public int Length { get; set; }
        public List<string> strParticulars { get; set; }
        public List<double> dInput { get; set; }
        public double[] dSumInput { get; set; }
        public double[] dTop { get; set; }
        public double[] dBase { get; set; }
        public double[] dElevation { get; set; }
        public double[] dNegative { get; set; }
        public double[] dPositive { get; set; }
        public double[] dResult { get; set; }
    }
}