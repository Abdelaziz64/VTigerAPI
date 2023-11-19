using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;


namespace WebApplication1.Services
{
    public class VTigerService

    {

        public string SessionID
        {
            get { return sessionName; }
        }


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
                    Console.WriteLine($"JSON Response: {jsonResponse}"); // Add this line
                    T result = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return result;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    // Handle the case where the POST request is not successful
                    return default(T);
                }
            }
        }
        private VTigerToken MapToVTigerToken(VTigerTokenResult result)
        {
            if (result == null)
            {
                return null;
            }

            return new VTigerToken
            {
                token = result.Token,
                ServerTime = DateTimeOffset.FromUnixTimeSeconds(result.ServerTime).DateTime,
                ExpireTime = DateTimeOffset.FromUnixTimeSeconds(result.ExpireTime).DateTime
            };
        }
        private async Task<VTigerToken> GetChallengeAsync(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://demo.vtiger.com/vtigercrm/webservice.php?operation=getchallenge&username={username}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        try
                        {
                            VTigerTokenResponse tokenResponse = JsonConvert.DeserializeObject<VTigerTokenResponse>(jsonResponse);

                            if (tokenResponse != null && tokenResponse.Success)
                            {
                                return MapToVTigerToken(tokenResponse.Result);
                            }
                            else
                            {
                                Console.WriteLine("Error: Success is false in the response.");
                                return null;
                            }
                        }
                        catch (JsonException ex)
                        {
                            // Log the JSON deserialization error for debugging
                            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                            return null;
                        }
                    }
                    else
                    {
                        // Log the error or throw an exception for better debugging
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
        }

        private string sessionName;
        public string SessionName
        {
            get { return sessionName; }
        }

        private string vtigerVersion;
        public System.Version VTigerVersion
        {
            get { return new System.Version(vtigerVersion); }
        }
        private string userID;

        public string UserID
        {
            get { return userID; }
        }


        public async Task LoginAsync(string username, string accessKey)
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

                    // Further processing with loginResult, if needed
                    if (loginResult != null && loginResult.success)
                    {
                        // Successful login
                        // Do something with the loginResult if needed
                        Console.WriteLine("Login succeeded. Check the key and credentials.");

                        // Store sessionName and vtigerVersion for later use
                        sessionName = loginResult.sessionName;
                        vtigerVersion = loginResult.vtigerVersion;
                    }

                    else
                    {
                        // Log or display an error message indicating login failure
                        Console.WriteLine("Login failed. Check the key and credentials.");
                        throw new Exception("Login failed. Check the key and credentials.");
                    }
                }
                else
                {
                    // Handle the case where the token is null
                    // You might want to log an error or take appropriate action
                    Console.WriteLine("Token is null. Unable to perform login.");
                    throw new Exception("Token is null. Unable to perform login.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred during login: {ex.Message}");
                // Handle the exception, e.g., rethrow or log
                throw;
            }


        }
        private async Task<T> Create<T>(VTigerContact element)
        {
            try
            {
                // Replace "Contacts" with the actual module name for contacts in your Vtiger CRM instance
                string elementType = "Contacts";

                // Serialize the VTigerContact object to JSON
                string jsonData = JsonConvert.SerializeObject(element);
                Console.WriteLine($"JSON Data sent to server: {jsonData}");

                // Construct the parameters
                Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "operation", "create" },
            { "sessionName", SessionID },
            { "element", jsonData },
            { "elementType", elementType },
        };

                // Send a POST request to the Vtiger CRM API
                return await VTigerPostJson<T>("webservice.php", parameters);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while creating a contact: {ex.Message}");
                // Handle the exception, e.g., rethrow or log
                throw;
            }
        }

        private string URLEncodeJsonData(VTigerContact contact)
        {

            string jsonData = JsonConvert.SerializeObject(contact);
            Console.WriteLine($"JSON Data: {jsonData}");
            string urlEncodedData = WebUtility.UrlEncode(jsonData);
            return urlEncodedData;
        }

        public async Task<VTigerContact> AddContact(string firstname, string lastname, string assigned_user_id)
        {
            try
            {
                // Create a new VTigerContact instance
                VTigerContact element = new VTigerContact(lastname.Trim(), assigned_user_id);
                element.firstname = firstname;  // Set the firstname propert
                return await Create<VTigerContact>(element);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while adding a contact: {ex.Message}");
                // Handle the exception, e.g., rethrow or log
                throw;
            }
        }

        // ... existing code ...
        //


    }

}
