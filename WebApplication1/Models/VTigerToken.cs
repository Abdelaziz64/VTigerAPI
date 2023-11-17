using Newtonsoft.Json;

namespace WebApplication1.Models
{

        public class VTigerToken
        {
            public string token { get; set; }
            public DateTime ServerTime { get; set; }
            public DateTime ExpireTime { get; set; }
        }

}