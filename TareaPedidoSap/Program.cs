using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TareaPedido.BL;

namespace TareaPedidoSap
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CultureInfo culture = new CultureInfo("en-GT"); // Fuerza formato YYYY-MM-DD
                culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
                culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
                culture.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
                culture.DateTimeFormat.DateSeparator = "-";

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                string cadenaConexionSQL = Properties.Settings.Default.cadenaConexionSQL;
                string urlApi = Properties.Settings.Default.urlApi;

                new Sincronizador().procesarPedidos(cadenaConexionSQL, urlApi);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
