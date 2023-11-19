namespace WebApplication1.Models
{
    public class VTigerCreateResult
    {
        public bool success { get; set; }
        public VTigerCreateResultData result { get; set; }
    }

    public class VTigerCreateResultData
    {
        public int id { get; set; } // Assuming the ID of the created record is returned, adjust it accordingly
        public string label { get; set; }
        // You can include other properties based on the expected response from the VTiger CRM
    }
}
