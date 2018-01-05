using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace MotionPhoto
{
    public class Picasa
    {
        private static string applicationName = "MotionPhoto";
        private readonly string credentials = "credentials.json";
        private string[] scopes = new string[] {"https://picasaweb.google.com/data/", };
        public Action<string> LogMessage;
        public static string ClientSecretFilename = "client_secret.json";
        public Picasa()
        {
        }
        private UserCredential GetCredential()
        {
            UserCredential credential = null;
            using (var stream =
                new FileStream(ClientSecretFilename, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.credentials, true)).Result;
                this.LogMessage?.Invoke("Credential file saved to: " + this.credentials);
            }
            return credential;
        }
        public void Initialize()
        {
            this.GetCredential();
        }
        public void Upload(Stream data)
        {
            UserCredential credential = this.GetCredential();

            // Create Drive API service.
            var baseClient = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
                HttpClientFactory = new Google.Apis.Http.HttpClientFactory()
            };
            var clientArgs = new Google.Apis.Http.CreateHttpClientArgs()
            {
                ApplicationName = applicationName,
                GZipEnabled = true,
                Initializers = { credential },
            };
            var factory = baseClient.HttpClientFactory;
            var client = factory.CreateHttpClient(clientArgs);
            {
                using (var content = new System.Net.Http.StreamContent(data))
                {
                    content.Headers.Add("Content-Type", "image/jpeg");
                    content.Headers.Add("Slug", "webcam");
                    var response = client.PostAsync("https://picasaweb.google.com/data/feed/api/user/default/", content).Result;
                    Console.WriteLine(response.IsSuccessStatusCode ? "Upload successfull" : "Upload failed");
                }
            }
        }
    }
}
