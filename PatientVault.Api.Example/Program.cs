using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatientVault.Api.Client.Factory;
using PatientVault.Api.Entities.Dto;

namespace PatientVault.Api.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            _client = new PatientVaultClient(configuration, new PatientVaultPostServiceFactory());

            // This will go through every step to obtain the user's CCDA information.
            GetPatientCcdaCategory();
            Console.ReadLine();
        }

        private static PatientVaultClient _client;
        private static Guid _attachmentId;

        public static PatientVaultConfiguration GetConfiguration()
        {
            // Set the configuration
            var configuration = new PatientVaultConfiguration
            {
                ApiRootUrl = "https://patientvault.com/patientvaultapi",
                Culture = "en"
            };

            return configuration;
        }

        private static async void GetPatientCcdaCategory()
        {
            await UserAuthentication();
            await PatientRetrieveList();
            await UserActivityRetrieve();
            await GetCcdaCategory();
        }

        public static async Task UserAuthentication()
        {
            Console.WriteLine("Signing in...");

            // Insert the username and password.
            var request = new UserAuthenticationRequest
            {
                Username = "",
                Password = ""
            };

            // A successful authentication will automatically set the session id in the configuration.
            var result = await _client.UserAuthenticationAsync(request).ConfigureAwait(false);
            OutputResult(result);
        }

        public static async Task PatientRetrieveList()
        {
            Console.WriteLine("\nRetrieving patients (records)...");

            var request = new PatientRetrieveListRequest
            {
                // You can add the following properties to filter your results. 
                // If you wish to retrieve all records, comment out the Filters property.
                Filters = new Dictionary<string, string>
                {
                    { "FirstName", "Vanessa" },
                    { "LastName", "" }
                }
            };

            var result = await _client.RetrievePatientListAsync(request).ConfigureAwait(false);
            OutputResult(result);
        }

        private static async Task UserActivityRetrieve()
        {
            Console.WriteLine("\nRetrieving activities...");

            var request = new UserActivityRetrieveRequest
            {
                // You can add the following properties to filter your results.
                // If you wish to retrieve all activities, you can ignore the Filters property.
                Filters = new Dictionary<string, string>
                {
                    { "PatientId", "" },
                    { "DateFrom", "" }, // Use DateTime.ToString()
                    { "DateTo", "" },   // Use DateTime.ToString()
                    { "Year", "" }
                },

                // Activity content can be obtained as json or html. The API will return it as HTML by default.
                ContentFormatIdentifier = "json"
            };

            var result = await _client.RetrieveUserActivitiesAsync(request);

            // We select the first attachment Id of the first activity. You may choose any attachment from all activities.
            _attachmentId = result.Activities[0].AttachmentIds[0];

            OutputResult(result);
        }

        private static async Task GetCcdaCategory()
        {
            Console.WriteLine("Getting CCDA Information...");

            // We use the attachment Id of the first activity.
            var request = new PatientCategoryRequest
            {
                ActivityAttachmentId = _attachmentId,
                IncludeSections = new PatientCategoryRequest.AttachmentSection
                {
                    // Choose which sections of CCDA to include. For this example, we will include all of them.
                    IncludeAll = true
                }
            };

            var result = await _client.RetrievePatientCategoryAsync(request);
            OutputResult(result);
        }

        private static void OutputResult(object result, string title = "")
        {
            Console.WriteLine(title);
            var json = JsonConvert.SerializeObject(result);
            var parsed = JObject.Parse(json);

            foreach (var pair in parsed)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }
    }
}
