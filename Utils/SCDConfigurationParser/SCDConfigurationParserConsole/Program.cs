using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SCDConfigurationParser;

namespace SCDConfigurationParserConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string scdConnectionString = GetConnectionStringByName("SCDDataBase");
            if (String.IsNullOrEmpty(scdConnectionString)) return;
            SqlConnection scdDbConnection = new SqlConnection(scdConnectionString);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string scdConfigurationFileName=config.AppSettings.Settings["SCDConfigurationFile"].Value;

            var scdConfiguration = new SCDConfigurationParser.SCDConfigurationParser(scdConfigurationFileName, scdDbConnection);
            scdConfiguration.CreateDBStructure();
        }

       
        static string GetConnectionStringByName(string name)
        {
            string returnValue = null;
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }


      

    }
}

