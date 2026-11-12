using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TareaPedido.Model
{
    public class Document
    {
        public string ID { get; set; }
        public string DOCNUM { get; set; }
        public bool Result { get; set; }
        public string Error_Code { get; set; }
        public string Descripcion_Error { get; set; }
    }
}
