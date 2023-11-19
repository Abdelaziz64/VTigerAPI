namespace WebApplication1.Models
{
    public class VTigerLogin
    {
        public bool success { get; set; }
        public VTigerLoginResult result { get; set; }

        // Add a property to directly access sessionName
        public string sessionName => result?.sessionName;

        public string userId => result?.userId;

        // Add a property to directly access vtigerVersion
        public string vtigerVersion => result?.vtigerVersion;
    }
}
