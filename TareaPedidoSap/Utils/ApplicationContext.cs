using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace K66_API.Utils
{
    public class ApplicationContext
    {
        public static string ConnectionString { get; set; }
        public static Company SBOCompany { get; set; }
        public static string SBOError { get { return "Error (" + SBOCompany.GetLastErrorCode() + "): " + SBOCompany.GetLastErrorDescription(); } }

        private static string GetSldServer()
        {
            string sldServer = ConfigurationManager.AppSettings["SLDServer"];
            if (string.IsNullOrWhiteSpace(sldServer))
            {
                throw new ConfigurationErrorsException(
                    "Debe configurar appSettings/SLDServer en TareaPedidoSap.exe.config (servidor:puerto).");
            }

            return sldServer.Trim();
        }

        public static void SetApplication()
        {
            SBOCompany = new Company();
            Company B1Company = new Company();
            B1Company.Server = System.Configuration.ConfigurationManager.AppSettings["server"];
            B1Company.UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
            B1Company.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
            B1Company.CompanyDB = System.Configuration.ConfigurationManager.AppSettings["CompanyDB"];
            B1Company.DbUserName = System.Configuration.ConfigurationManager.AppSettings["DbUserName"];
            B1Company.DbPassword = System.Configuration.ConfigurationManager.AppSettings["DbPassword"];
            switch (System.Configuration.ConfigurationManager.AppSettings["SQLType"])
            {
                case "MSSQL2005": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2005; break;
                case "MSSQL2008": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2008; break;
                case "MSSQL2012": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2012; break;
                case "MSSQL2014": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2014; break;
                case "MSSQL2016": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2016; break;
                case "MSSQL2017": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2017; break;
                case "MSSQL2019": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2019; break;
                case "HANADB": B1Company.DbServerType = BoDataServerTypes.dst_HANADB; break;
                case "MSSQL": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL; break;
                case "DB_2": B1Company.DbServerType = BoDataServerTypes.dst_DB_2; break;
                case "MAXDB": B1Company.DbServerType = BoDataServerTypes.dst_MAXDB; break;
                case "SYBASE": B1Company.DbServerType = BoDataServerTypes.dst_SYBASE; break;
            }
            B1Company.language = SAPbobsCOM.BoSuppLangs.ln_Spanish;
            B1Company.UseTrusted = false;
            B1Company.SLDServer = GetSldServer();
            int ret = B1Company.Connect();
            if (ret != 0)
            {
                string errMsg = B1Company.GetLastErrorDescription();
                int ErrNo = B1Company.GetLastErrorCode();
                throw new Exception(" Error" + ErrNo + " Codigo" + errMsg);
            }
            SBOCompany = B1Company;
        }

        public static void SetApplication(string empresa)
        {
            if (empresa.Equals( "BOLIK"))
            {
                empresa= "SBOBOLIK";
            }
            else if (empresa.Equals("GRACO"))
            {
                empresa = "SBO_GRACO";
            }
            else if (empresa.Equals("ESCOBAS")|| empresa.Equals("ESCOCESA"))
            {
                empresa = "SBOESCOCESA";
            }
            else
            {
                empresa = "SIN NOMBRE";
            }
            SBOCompany = new Company();
            Company B1Company = new Company();
            B1Company.Server = ConfigurationManager.AppSettings["server"];
            B1Company.UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
            B1Company.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];

            B1Company.CompanyDB = empresa;
            B1Company.DbUserName = System.Configuration.ConfigurationManager.AppSettings["DbUserName"];
            B1Company.DbPassword = System.Configuration.ConfigurationManager.AppSettings["DbPassword"];
            switch (System.Configuration.ConfigurationManager.AppSettings["SQLType"])
            {
                case "MSSQL2005": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2005; break;
                case "MSSQL2008": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2008; break;
                case "MSSQL2012": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2012; break;
                case "MSSQL2014": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2014; break;
                case "MSSQL2016": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2016; break;
                case "MSSQL2017": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2017; break;
                case "MSSQL2019": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL2019; break;
                case "HANADB": B1Company.DbServerType = BoDataServerTypes.dst_HANADB; break;
                case "MSSQL": B1Company.DbServerType = BoDataServerTypes.dst_MSSQL; break;
                case "DB_2": B1Company.DbServerType = BoDataServerTypes.dst_DB_2; break;
                case "MAXDB": B1Company.DbServerType = BoDataServerTypes.dst_MAXDB; break;
                case "SYBASE": B1Company.DbServerType = BoDataServerTypes.dst_SYBASE; break;
            }
            B1Company.language = SAPbobsCOM.BoSuppLangs.ln_Spanish;
            B1Company.UseTrusted = true;
            B1Company.SLDServer = GetSldServer();
            int ret = B1Company.Connect();
            if (ret != 0)
            {
                string errMsg = B1Company.GetLastErrorDescription();
                int ErrNo = B1Company.GetLastErrorCode();
                throw new Exception(" Error:" + ErrNo + ", Codigo:" + errMsg +","+ B1Company.CompanyDB);
            }
            SBOCompany = B1Company;
        }

        public static string SetApplicationHana(string empresa)
        {
            if (empresa.Equals("BOLIK"))
            {
                return ConfigurationManager.AppSettings["CnnBolik"];
            }
            else if (empresa.Equals("GRACO"))
            {
                return ConfigurationManager.AppSettings["CnnGraco"];
            }
            else if (empresa.Equals("ESCOBAS"))
            {
                return ConfigurationManager.AppSettings["CnnFaes"];
            }
            else
            {
                return ConfigurationManager.AppSettings["CnnGraco"];
            }
        }

        public static string getEmpresa(string empresa)
        {
            if (empresa.Equals("BOLIK"))
            {
                return "SBOBOLIK";
            }
            else if (empresa.Equals("GRACO"))
            {
                return "SBO_GRACO";
            }
            else if (empresa.Equals("ESCOBAS"))
            {
                return "SBOESCOCESA";
            }
            else
            {
                return "SBO_GRACO";
            }
        }

        internal static string getProcedure(string OP, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10)
        {
            string query = @"CALL SBO_GRACO.GET_PROCEDURE('" + OP + "','" + param1 + "','" + param2 + "','" + param3 + "','" + param4 + "','" + param5 + "','" + param6 + "','" + param7 + "','" + param8 + "','" + param9 + "','" + param10 + "')"; ;
            //if (SBOCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            //{
            //    query 
            //}
            //else
            //{
            //    query = @"EXEC GET_PROCEDURE '" + OP + "','" + param1 + "','" + param2 + "','" + param3 + "','" + param4 + "','" + param5 + "','" + param6 + "','" + param7 + "','" + param8 + "','" + param9 + "','" + param10 + "'";
            //}
            return query;
        }

        internal static string getProcedure(string OP, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string empresa)
        {
            string query = @"CALL "+empresa+@".GET_PROCEDURE('" + OP + "','" + param1 + "','" + param2 + "','" + param3 + "','" + param4 + "','" + param5 + "','" + param6 + "','" + param7 + "','" + param8 + "','" + param9 + "','" + param10 + "')"; ;
            return query;
        }

        public static void UnsetApplication()
        {
            if(SBOCompany != null)
            {
                if (SBOCompany.Connected == true)
                {
                    SBOCompany.Disconnect();
                }
            }        
        }

        internal static bool existepathLOG()
        {
            try
            {
                string ruta = System.Configuration.ConfigurationManager.AppSettings["Ruta"];
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void _writeToApplicationEventLog(string logMessage, int eventID, EventLogEntryType logType)
        {
            string sourceName = "DidelcoApi";
            string myLogName = "ApiLog";
            string messageFile = "didelcoApi.log";
            if (!EventLog.SourceExists(sourceName))
            {
                EventSourceCreationData mySourceData = new EventSourceCreationData(sourceName, myLogName);
                mySourceData.MessageResourceFile = messageFile;
                mySourceData.CategoryResourceFile = messageFile;
                mySourceData.CategoryCount = 1;
                mySourceData.ParameterResourceFile = messageFile;
                EventLog.CreateEventSource(mySourceData);
            }

            myLogName = EventLog.LogNameFromSourceName(sourceName, ".");

            using (EventLog eventLog = new EventLog(myLogName, ".", sourceName))
            {
                eventLog.Source = sourceName;
                eventLog.WriteEntry(logMessage, logType, eventID);
            }

        }
    }

}