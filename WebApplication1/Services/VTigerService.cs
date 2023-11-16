using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;


namespace WebApplication1.Services
{
    public class VTigerService

    {
        private static string GetMD5Hash(string input)
        {
            if ((input == null) || (input.Length == 0))
            {
                return string.Empty;
            }
            byte[] data;
            using (MD5 md5Hasher = MD5.Create())
                data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private async Task<T> VTigerPostJson<T>(string endpoint, Dictionary<string, string> parameters)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://demo.vtiger.com/vtigercrm/{endpoint}";

                HttpResponseMessage response = await client.PostAsync(apiUrl, new FormUrlEncodedContent(parameters));

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    T result = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return result;
                }
                else
                {
                    // Handle the case where the POST request is not successful
                    return default(T);
                }
            }
        }

        private async Task<VTigerToken> GetChallengeAsync(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://demo.vtiger.com/vtigercrm/webservice.php?operation=getchallenge&username={username}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    VTigerToken result = JsonConvert.DeserializeObject<VTigerToken>(jsonResponse);
                    return result;
                }
                else
                {
                    // Handle the case where the GET request is not successful
                    return null;
                }
            }
        }

        public async Task<bool> LoginAsync(string username, string accessKey)
        {
            try
            {
                VTigerToken token = await GetChallengeAsync(username);

                // Check if token is not null before accessing its properties
                if (token != null)
                {
                    string key = GetMD5Hash(token.token + accessKey);

                    var loginParams = new Dictionary<string, string>
            {
                { "operation", "login" },
                { "username", username },
                { "accessKey", key }
            };

                    VTigerLogin loginResult = await VTigerPostJson<VTigerLogin>("webservice.php", loginParams);

                    // Log the key
                    Console.WriteLine($"Calculated Key: {key}");

                    // Further processing with loginResult, if needed
                    if (loginResult != null && loginResult.success)
                    {
                        // Successful login
                        // Do something with the loginResult if needed
                        return true;
                    }
                    else
                    {
                        // Log or display an error message indicating login failure
                        Console.WriteLine("Login failed. Check the key and credentials.");
                        return false;
                    }
                }
                else
                {
                    // Handle the case where the token is null
                    // You might want to log an error or take appropriate action
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred during login: {ex.Message}");
                // Handle the exception, e.g., rethrow or log
                return false;
            }
        }



    }
}
