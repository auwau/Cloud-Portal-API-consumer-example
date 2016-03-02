using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CloudPortal.Web.Api.ConsumerExample
{
    class Program
    {
        //Change these constants before you begin - they need to match your settings

        /// <summary>
        /// Base Address to the API
        /// </summary>
        private const string BaseAddress = "https://localhost:44300";

        /// <summary>
        /// Client Id for your application - create it in the portal: Settings->API Access
        /// </summary>
        private const string ClientId = "9ea349a5-deff-4b0e-8b99-30e63d055dc1";
        
        /// <summary>
        /// Origin of your application - this needs to match what you specified in Settings->API Access
        /// </summary>
        private const string Origin = "https://testapp.com";

        /// <summary>
        /// User Name for your user
        /// </summary>
        private const string UserName = "api@domain.tld";

        /// <summary>
        /// Password for your user
        /// </summary>
        private const string Password = "123456";


        static void Main(string[] args)
        {
            ExistingUserAsync().Wait();
            Console.ReadKey();
        }

        /// <summary>
        /// Call the API using an existing user from the user database
        /// </summary>
        /// <returns></returns>
        private static async Task ExistingUserAsync()
        {
            using (var client = new HttpClient())
            {
                //Set the Base URI
                client.BaseAddress = new Uri(BaseAddress);

                //Set Origin - this needs to be the same as you specified in the portal
                client.DefaultRequestHeaders.Add("ORIGIN", Origin);

                //Setup the token request
                var tokenRequest = new FormUrlEncodedContent(new[] 
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("grant_type","password"), 
                    new KeyValuePair<string, string>("username",UserName), 
                    new KeyValuePair<string, string>("password",Password)
                });

                //Post token request and receive the access token
                var response = await client.PostAsync("v1/oauth", tokenRequest);

                //Serialize the received JSON into an AccessToken object
                //This example uses the Microsoft.AspNet.WebApi.Client package to do this
                var accessToken = await response.Content.ReadAsAsync<AccessToken>();

                //For subsequent calls, add authorization header containing the access token
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Bearer " + accessToken.access_token);

                //Load Active User Profile
                response = await client.GetAsync("v1/me");

                if (response.IsSuccessStatusCode)
                {
                    //Let's serialize the JSON response into a dynamic object
                    //This example uses the Microsoft.AspNet.WebApi.Client package to do this
                    dynamic user = await response.Content.ReadAsAsync<ExpandoObject>();

                    //Read data from serialized object
                    //Json.NET will always deserialize integers to Int64/long to prevent overflow
                    long businessUnitId = user.businessUnit.id;

                    var username = user.name;

                    //Output result
                    Console.WriteLine("Response from Consumer Test using Username/Password");
                    Console.WriteLine("User name: " + username);
                    Console.WriteLine("Current Business Unit Id: " + businessUnitId);
                    Console.WriteLine(); //Empty line
                }
                else
                {
                    //Error handling
                    //Check if the access token has expired - renew it

                    var content = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("StatusCode: " + response.StatusCode);
                    Console.WriteLine("Content: " + content);
                    return;
                }

                //Let's try to renew the token
                Console.WriteLine("Old token: " + accessToken.access_token);
                Console.WriteLine(); //Empty line
                accessToken = await RefreshToken(accessToken);
                Console.WriteLine("New token: " + accessToken.access_token);
                Console.WriteLine(); //Empty line
            }
        }

        /// <summary>
        /// Refresh the access token using the refresh token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<AccessToken> RefreshToken(AccessToken token)
        {
            using (var client = new HttpClient())
            {
                //Set the Base URI
                client.BaseAddress = new Uri(BaseAddress);

                //Set Origin - this needs to be the same as you specified in the portal
                client.DefaultRequestHeaders.Add("ORIGIN", Origin);

                //Setup the token request
                var tokenRequest = new FormUrlEncodedContent(new[] 
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("grant_type","refresh_token"), 
                    new KeyValuePair<string, string>("refresh_token",token.refresh_token)
                });
                
                //Post token request and receive the access token
                var response = await client.PostAsync("v1/oauth", tokenRequest);

                //Serialize the received JSON into an AccessToken object
                //This example uses the Microsoft.AspNet.WebApi.Client package to do this
                var accessToken = await response.Content.ReadAsAsync<AccessToken>();

                return accessToken;
            }
        }
    }

    public class AccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
