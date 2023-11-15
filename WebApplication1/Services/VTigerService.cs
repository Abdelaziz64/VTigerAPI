using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
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

        private async Task<T> VTigerGetJson<T>(string endpoint, string queryString, bool authenticate)
        {
            using (HttpClient client = new HttpClient())
            {

                string apiUrl = $"https://demo.vtiger.com/vtigercrm/{endpoint}?{queryString}";


                HttpResponseMessage response = await client.GetAsync(apiUrl);


                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    T result = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return result;
                }
                else
                {

                    return default(T);
                }
            }
        }
        private async Task<VTigerToken> GetChallengeAsync(string username)
        {
            return await VTigerGetJson<VTigerToken>("getchallenge", String.Format("username={0}", username), false);
        }

        public async Task LoginAsync(string username, string accessKey)
        {
            VTigerToken token = await GetChallengeAsync(username);

            string key = GetMD5Hash(token.token + accessKey);
//test

            VTigerLogin loginResult = await VTigerGetJson<VTigerLogin>("login",
                String.Format("username={0}&accessKey={1}", username, key), true);


        }


    }
}
