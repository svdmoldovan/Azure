using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Client
{
    public class Program
    {
        public static async Task Main()
        {
            Console.Title = "External API Client";
            Console.WriteLine("Press a key to do it");
            Console.ReadKey();

            var client = new HttpClient();

            var disco = await
                client.GetDiscoveryDocumentAsync("https://localhost:5000");

            var response = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,

                    ClientId = "attendeeposter",
                    ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A",
                    Scope = "confarch_api"
                });


            if (response.IsError)
            {
                Console.WriteLine(response.Error);
                Console.ReadLine();
                return;
            }

            Console.WriteLine(response.AccessToken);
            Console.WriteLine("\n\n");

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(response.AccessToken);

            var apiResponse = await apiClient
                .PostAsync("https://localhost:5002/Attendee/1/Roland", null);
            if (!apiResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(apiResponse.StatusCode);
            }
            else
            {
                Console.WriteLine("Attendee posted");
            }
            Console.ReadLine();
        }
    }
}