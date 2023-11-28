namespace WebApplication1.Models
{
    public class VTigerQueryResponse<T>
    {
        public bool success { get; set; }

        public List<T> result { get; set; }
    }

}
