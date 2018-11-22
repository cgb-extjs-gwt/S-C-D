using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public static class Config
    {
        public static string SpServiceDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceDomain"];
            }
        }
        public static string SpServiceAccount
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceAccount"];
            }
        }
        public static string SpServicePassword
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServicePassword"];
            }
        }
        public static string CalculatiolToolWeb
        {
            get
            {
                return ConfigurationManager.AppSettings["CalculatiolToolWeb"];
            }
        }
        public static string CalculatiolToolList
        {
            get
            {
                return ConfigurationManager.AppSettings["CalculatiolToolList"];
            }
        }
        public static string CalculatiolToolFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["CalculatiolToolFolder"];
            }
        }
        public static string CalculatiolToolFileName
        {
            get
            {
                return ConfigurationManager.AppSettings["CalculatiolToolFileName"];
            }
        }
        public static string CalculatiolToolInputFileName
        {
            get
            {
                return ConfigurationManager.AppSettings["CalculatiolToolInputFileName"];
            }
        }
        public static string ProActiveWgList
        {
            get
            {
                return ConfigurationManager.AppSettings["ProActiveWgList"];
            }
        }
    }
}
