using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APIServices.Models
{
    public class User
    {
        public long USERID { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string USERNAME { get; set; }
        [JsonIgnore]
        public string PASSWORD { get; set; }
        public string CONNECTIONID { get; set; }
    }
}
