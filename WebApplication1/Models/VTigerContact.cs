using Newtonsoft.Json;

namespace WebApplication1.Models
{
    public class VTigerContact
    {
        // Properties representing the contact fields
        [JsonProperty("firstname")]
        public string firstname { get; set; }

        [JsonProperty("lastname")]
        public string lastname { get; set; }

        [JsonProperty("assigned_user_id")]
        public string assigned_user_id { get; set; }

        // Additional properties as needed

        // Constructor to initialize required properties
        public VTigerContact(string lastname, string assigned_user_id)
        {
            this.lastname = lastname;
            this.assigned_user_id = assigned_user_id;
        }

        // Add other constructors or methods as needed
    }
}