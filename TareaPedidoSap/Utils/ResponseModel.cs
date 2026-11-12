using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace K66_API.Models
{
    public class ResponseModel
    {
        public class Response
        {
            public string ID { get; set; }

            public string DOCNUM { get; set; }
            public bool result { get; set; }
            public string error_code { get; set; }
            public string descripcion_error { get; set; }
        }

        public Response RespuestaSap(bool result, string ID, string error_code, string descripcion_error, string DOCNUM)
        {       
            var RESSAP = new Response();
            RESSAP.ID = ID;
            RESSAP.DOCNUM = DOCNUM;
            RESSAP.result = result;     
            RESSAP.error_code = error_code;
            RESSAP.descripcion_error = descripcion_error;
            return RESSAP;
        }
    }
}