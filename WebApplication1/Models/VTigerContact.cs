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
        public string AssignedUserId { get; set; }

        // Additional properties as needed

        // Constructor to initialize required properties
        public VTigerContact(string lastname, string assignedUserId)
        {
            this.lastname = lastname;
            this.AssignedUserId = assignedUserId;
        }

        // Add other constructors or methods as needed
    }
}