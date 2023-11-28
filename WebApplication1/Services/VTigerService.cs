using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly SessionManager _sessionManager;

        public VTigerService(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public async Task LoginAsync(string username, string accessKey)
        {
            try
            {
                VTigerToken token = await GetChallengeAsync(username);

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

                    if (loginResult != null && loginResult.success)
                    {
                        Console.WriteLine("Login succeeded. Check the key and credentials.");
                        _sessionManager.SessionName = loginResult.sessionName;
                        _sessionManager.VtigerVersion = loginResult.vtigerVersion;

                        // Store sessionName and vtigerVersion for later use
                        // Note: You can still store these values as class properties if needed
                    }
                    else
                    {
                        Console.WriteLine("Login failed. Check the key and credentials.");
                        throw new Exception("Login failed. Check the key and credentials.");
                    }
                }
                else
                {
                    Console.WriteLine("Token is null. Unable to perform login.");
                    throw new Exception("Token is null. Unable to perform login.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during login: {ex.Message}");
                throw;
            }
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
                    Console.WriteLine($"JSON Response: {jsonResponse}");
                    T result = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return result;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
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
                            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
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

        private async Task<T> Create<T>(VTigerContact element)
        {
            try
            {
                string elementType = "Contacts";
                string jsonData = JsonConvert.SerializeObject(element);
                Console.WriteLine($"JSON Data sent to server: {jsonData}");

                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "operation", "create" },
                    { "sessionName", _sessionManager.SessionName }, // Use SessionName from SessionManager
                    { "element", jsonData },
                    { "elementType", elementType },
                };

                return await VTigerPostJson<T>("webservice.php", parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating a contact: {ex.Message}");
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
                VTigerContact element = new VTigerContact(lastname.Trim(), assigned_user_id);
                element.firstname = firstname;
                return await Create<VTigerContact>(element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a contact: {ex.Message}");
                throw;
            }
        }
    }
}
