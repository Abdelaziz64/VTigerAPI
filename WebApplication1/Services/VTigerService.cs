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

        public string GetDisplayContactsUrl()
        {
            // Construct the URL for the DisplayContacts view
            return $"https://demo.vtiger.com/vtigercrm/webservice.php?operation=query&sessionName={_sessionManager.SessionName}&query=SELECT+*+FROM+DisplayContacts";
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

                // Construct the query string
                string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

                // Combine the API URL and query string
                string fullUrl = $"{apiUrl}?{queryString}";

                HttpResponseMessage response;

                if (parameters.ContainsKey("operation") && ((parameters["operation"] == "login") || (parameters["operation"] == "create") || (parameters["operation"] == "delete") || (parameters["operation"] == "update")))
                {
                    // For login operation, use POST with form data
                    response = await client.PostAsync(apiUrl, new FormUrlEncodedContent(parameters));
                }
                else
                {
                    // For other operations, use GET
                    response = await client.GetAsync(fullUrl);
                }

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"JSON Response: {jsonResponse}");

                    try
                    {
                        // Attempt to deserialize as T directly
                        T result = JsonConvert.DeserializeObject<T>(jsonResponse);
                        return result;
                    }
                    catch (JsonSerializationException)
                    {
                        // If T is List<VTigerContact>, handle it accordingly
                        if (typeof(T) == typeof(List<VTigerContact>))
                        {
                            var queryResponse = JsonConvert.DeserializeObject<VTigerQueryResponse<VTigerContact>>(jsonResponse);
                            return (T)(object)queryResponse.result.ToList();
                        }

                        // Handle other cases where T is not directly deserializable
                        try
                        {
                            var queryResponse = JsonConvert.DeserializeObject<T>(jsonResponse);
                            return queryResponse;
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Error during deserialization: {ex.Message}");
                            return default;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return default;
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
                element.Firstname = firstname;
                return await Create<VTigerContact>(element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a contact: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteContactAsync(string contactId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
        {
            { "operation", "delete" },
            { "sessionName", _sessionManager.SessionName },
            { "id", contactId }
        };

                VTigerDeleteResponse deleteResponse = await VTigerPostJson<VTigerDeleteResponse>("webservice.php", parameters);

                if (deleteResponse != null && deleteResponse.success)
                {
                    Console.WriteLine($"Contact with ID {contactId} deleted successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to delete contact with ID {contactId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during contact deletion: {ex.Message}");
                throw;
            }
        }


        public async Task<List<VTigerContact>> GetContacts()
        {
            try
            {
                string query = "SELECT * FROM Contacts;";

                var parameters = new Dictionary<string, string>
        {
            { "operation", "query" },
            { "sessionName", _sessionManager.SessionName },
            { "query", query }
        };

                VTigerQueryResponse<VTigerContact> queryResponse = await VTigerPostJson<VTigerQueryResponse<VTigerContact>>("webservice.php", parameters);

                if (queryResponse != null)
                {
                    if (queryResponse.success)
                    {
                        return queryResponse.result;
                    }
                    else
                    {
                        Console.WriteLine($"Query failed. Response indicates failure. Check the error message.");
                    }
                }
                else
                {
                    Console.WriteLine("Query failed. Response is null.");
                }

                // If there is an error or the response is null, return an empty list or handle it accordingly.
                return new List<VTigerContact>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during GetContacts: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateContactAsync(string contactId, string newFirstName, string newLastName)
        {
            try
            {
                // Retrieve the contact details first
                VTigerContact existingContact = await GetContactByIdAsync(contactId);

                if (existingContact != null)
                {
                    // Update the first name and last name
                    existingContact.Firstname = newFirstName;
                    existingContact.Lastname = newLastName;

                    // Perform the update operation
                    var parameters = new Dictionary<string, string>
            {
                { "operation", "update" },
                { "sessionName", _sessionManager.SessionName },
                { "element", JsonConvert.SerializeObject(existingContact) },
                { "elementType", "Contacts" }
            };

                    VTigerUpdateResponse updateResponse = await VTigerPostJson<VTigerUpdateResponse>("webservice.php", parameters);

                    if (updateResponse != null && updateResponse.success)
                    {
                        Console.WriteLine($"Contact with ID {contactId} updated successfully.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to update contact with ID {contactId}.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"Contact with ID {contactId} not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during contact update: {ex.Message}");
                throw;
            }
        }

        private async Task<VTigerContact> GetContactByIdAsync(string contactId)
        {
            try
            {
                // Query to get the contact by ID
                string query = $"SELECT * FROM Contacts WHERE id='{contactId}';";

                var parameters = new Dictionary<string, string>
        {
            { "operation", "query" },
            { "sessionName", _sessionManager.SessionName },
            { "query", query }
        };

                VTigerQueryResponse<VTigerContact> queryResponse = await VTigerPostJson<VTigerQueryResponse<VTigerContact>>("webservice.php", parameters);

                if (queryResponse != null && queryResponse.success && queryResponse.result.Count > 0)
                {
                    return queryResponse.result[0]; // Assuming there's only one contact with the given ID
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during GetContactById: {ex.Message}");
                throw;
            }
        }






    }
}

