using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBootstrapDashboard.Models
{
    public static class HelperFunction
    {

        public static string Convert_Array_To_IN_String(string[] INPUT)
        {
            string OUTPUT_STTRING = null;
            try
            {
                OUTPUT_STTRING = "('";
                for (int i = 0; i < INPUT.Length; i++)
                {
                    OUTPUT_STTRING += INPUT[i];

                    if (i != INPUT.Length - 1)
                        OUTPUT_STTRING += "', '";
                }
                OUTPUT_STTRING += "')";
                return OUTPUT_STTRING;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}