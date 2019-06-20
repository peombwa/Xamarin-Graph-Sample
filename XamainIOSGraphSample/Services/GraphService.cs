using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace XamainIOSGraphSample.Services
{
    public class GraphService
    {
        public string clientID = "{ENTER_YOUR_CLIENT_ID}"; // Rememeber to sepcify the clientID.
        public string[] scopes = { "Files.Read.All" };
        public IPublicClientApplication AuthClientApp { get; private set; }
        public GraphServiceClient GraphClient { get; set; }

        public GraphService()
        {
            // var authProvider = GetAuthProvider();
            var authProvider = new DelegateAuthenticationProvider(async (request) =>
            {
                string accessToken = "YOUR_TOKEN";
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            });

            // Use AndroidClientHandler as opposed to HttpClientHandler.
            var innerHandler = new NSUrlSessionHandler { AllowAutoRedirect = false };
            var pipeline = GraphClientFactory.CreatePipeline(GraphClientFactory.CreateDefaultHandlers(authProvider), innerHandler);

            GraphClient = new GraphServiceClient(authProvider, new HttpProvider(pipeline, true, new Serializer()));
        }

        private IAuthenticationProvider GetAuthProvider()
        {
            AuthClientApp = PublicClientApplicationBuilder.Create(clientID)
                .WithRedirectUri($"msal{clientID}://auth")
                .Build();

            return new DelegateAuthenticationProvider(async (request) =>
            {
                IEnumerable<IAccount> accounts = await AuthClientApp.GetAccountsAsync();
                AuthenticationResult authResult;
                try
                {
                    authResult = await AuthClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    authResult = await AuthClientApp.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
                }
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            });
        }

        public async Task<IList<DriveItem>> GetFilesWithAttachmentsAsync()
        {
            try
            {
                List<DriveItem> files = new List<DriveItem>();
                var driveItems = await GraphClient.Me.Drive.Root.Children.Request().OrderBy("name desc").GetAsync().ConfigureAwait(false);
                return driveItems.Where(di => di.File != null).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get files to download : {ex.InnerException?.Message}");
                throw ex;
            }
        }

        public async Task<byte[]> DownloadFileAsync(DriveItem item)
        {
            try
            {
                using (var stream = await GraphClient.Drives[item.ParentReference.DriveId].Items[item.Id].Content.Request().GetAsync().ConfigureAwait(false))
                using (var outputStream = new MemoryStream())
                {
                    await stream.CopyToAsync(outputStream);
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to download file {item.Id} : {ex.InnerException?.Message}");
                throw ex;
            }
        }
    }
}
