using System;
using System.Configuration;
using K66_API.Utils;
using SAPbobsCOM;

// Sustituto exclusivo de pruebas: nunca carga COM ni abre conexiones de red.
namespace SAPbobsCOM
{
    public enum BoDataServerTypes
    {
        dst_MSSQL2005, dst_MSSQL2008, dst_MSSQL2012, dst_MSSQL2014,
        dst_MSSQL2016, dst_MSSQL2017, dst_MSSQL2019, dst_HANADB,
        dst_MSSQL, dst_DB_2, dst_MAXDB, dst_SYBASE
    }

    public enum BoSuppLangs { ln_Spanish }

    public class Company
    {
        public static Company LastConnection;
        public static int ConnectCalls;
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CompanyDB { get; set; }
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string SLDServer { get; set; }
        public BoDataServerTypes DbServerType { get; set; }
        public BoSuppLangs language { get; set; }
        public bool UseTrusted { get; set; }
        public bool Connected { get; private set; }

        public int Connect()
        {
            ConnectCalls++;
            // La instantanea comprueba la configuracion ANTES de Connect.
            LastConnection = new Company {
                SLDServer = SLDServer, CompanyDB = CompanyDB,
                UseTrusted = UseTrusted, Server = Server,
                UserName = UserName, Password = Password,
                DbUserName = DbUserName, DbPassword = DbPassword,
                DbServerType = DbServerType
            };
            Connected = true;
            return 0;
        }

        public void Disconnect() { Connected = false; }
        public int GetLastErrorCode() { return 0; }
        public string GetLastErrorDescription() { return ""; }
    }
}

internal static class SldConnectionTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Configure(string sld)
    {
        // Solo modifica la configuracion del ejecutable de pruebas en TEMP.
        Configuration config = ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None);
        config.AppSettings.Settings.Clear();
        config.AppSettings.Settings.Add("server", "test-hana:30013");
        config.AppSettings.Settings.Add("UserName", "test-sap-user");
        config.AppSettings.Settings.Add("Password", "test-sap-password");
        config.AppSettings.Settings.Add("DbUserName", "test-db-user");
        config.AppSettings.Settings.Add("DbPassword", "test-db-password");
        config.AppSettings.Settings.Add("CompanyDB", "TEST_COMPANY");
        config.AppSettings.Settings.Add("SQLType", "HANADB");
        if (sld != null) config.AppSettings.Settings.Add("SLDServer", sld);
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }

    private static void VerifyConnection(Action connect, string company, bool trusted)
    {
        Company.ConnectCalls = 0;
        Company.LastConnection = null;
        connect();
        Company result = Company.LastConnection;
        Check(Company.ConnectCalls == 1, "Debe conectar una sola vez.");
        Check(result.SLDServer == "test-sld:40000", "SLD explicito incorrecto.");
        Check(result.CompanyDB == company, "Cambio el mapeo de sociedad.");
        Check(result.UseTrusted == trusted, "Cambio UseTrusted.");
        Check(result.Server == "test-hana:30013", "Cambio el servidor HANA.");
        Check(result.DbServerType == BoDataServerTypes.dst_HANADB, "Cambio el tipo de BD.");
        Check(result.UserName == "test-sap-user" && result.Password == "test-sap-password",
            "Cambiaron las credenciales SAP.");
        Check(result.DbUserName == "test-db-user" && result.DbPassword == "test-db-password",
            "Cambiaron las credenciales de BD.");
        Check(ApplicationContext.SBOCompany.Connected, "No se publico la conexion.");
        ApplicationContext.UnsetApplication();
        Check(!ApplicationContext.SBOCompany.Connected, "No se desconecto.");
    }

    private static void VerifyMissingSetting(Action connect, string sld)
    {
        Configure(sld);
        Company.ConnectCalls = 0;
        bool rejected = false;
        try { connect(); }
        catch (ConfigurationErrorsException ex)
        {
            rejected = ex.Message.Contains("appSettings/SLDServer");
        }
        Check(rejected, "Debe explicar que falta la configuracion SLDServer.");
        Check(Company.ConnectCalls == 0, "No debe intentar conectar sin SLDServer.");
    }

    public static int Main()
    {
        try
        {
            Configure("  test-sld:40000  ");
            VerifyConnection(delegate { ApplicationContext.SetApplication(); }, "TEST_COMPANY", false);
            VerifyConnection(delegate { ApplicationContext.SetApplication("GRACO"); }, "SBO_GRACO", true);
            VerifyConnection(delegate { ApplicationContext.SetApplication("BOLIK"); }, "SBOBOLIK", true);
            VerifyConnection(delegate { ApplicationContext.SetApplication("ESCOCESA"); }, "SBOESCOCESA", true);
            VerifyConnection(delegate { ApplicationContext.SetApplication("ESCOBAS"); }, "SBOESCOCESA", true);

            Action[] paths = {
                delegate { ApplicationContext.SetApplication(); },
                delegate { ApplicationContext.SetApplication("GRACO"); }
            };
            foreach (Action path in paths)
                foreach (string missing in new string[] { null, "", "   " })
                    VerifyMissingSetting(path, missing);

            Console.WriteLine("OK: 5 conexiones configuradas y 6 casos sin SLD rechazados; sin acceso a SAP.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
