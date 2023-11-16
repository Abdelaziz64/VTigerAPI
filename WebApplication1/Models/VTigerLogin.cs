namespace WebApplication1.Models
{
    public class VTigerLogin
    {
        public string sessionName { get; set; }
        public string vtigerVersion { get; set; }
        public string version { get; set; }
        public string userId { get; set; }

        public bool success { get; set; }
        public VTigerLoginResult result { get; set; }


    }
}
