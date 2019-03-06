using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace RestApiSample
{
    class Program
    {
        static void Main(string[] args)
        {
            CallRefreshAsync();
            Console.ReadLine();
        }

        private static async void CallRefreshAsync()
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://eastus.asazure.windows.net/servers/abanalysisservices/models/adventureworks/") //update
            };

            // Send refresh request
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await UpdateToken());

            RefreshRequest refreshRequest = new RefreshRequest()
            {
                type = "full",
                maxParallelism = 10
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("refreshes", refreshRequest);
            response.EnsureSuccessStatusCode();
            Uri location = response.Headers.Location;
            Console.WriteLine(response.Headers.Location);

            // Check the response
            while (true) // Will exit while loop when exit Main() method (it's running asynchronously)
            {
                string output = "";

                // Refresh token if required
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await UpdateToken());

                response = await client.GetAsync(location);
                if (response.IsSuccessStatusCode)
                {
                    output = await response.Content.ReadAsStringAsync();
                }

                Console.Clear();
                Console.WriteLine(output);

                Thread.Sleep(5000);
            }
        }

        private static async Task<string> UpdateToken()
        {
            string resourceURI = "https://*.asazure.windows.net";   
            
            string authority = "https://login.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47"; // Authority address can optionally use tenant ID in place of "common". If service principal or B2B enabled, this is a requirement.
            AuthenticationContext ac = new AuthenticationContext(authority);

            ClientCredential cred = new ClientCredential("f5e4354c-7e8f-4199-b826-2f0b8264c677", "-B!A!h8}0eW+=m.#y({}dm7HI%gM[k8?"); // Native app with necessary API permissions
            AuthenticationResult ar = await ac.AcquireTokenAsync(resourceURI, cred);

            return ar.AccessToken;
        }
    }

    class RefreshRequest
    {
        public string type { get; set; }
        public int maxParallelism { get; set; }
    }
}
