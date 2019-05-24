using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Xamarin.Android.Net;

namespace Xamarin_Graph_Sample_Android.Services
{
    public class GraphService
    {
        public string clientID = "{YOUR_CLIENT_ID}"; // Rememeber to sepcify the clientID in your AndroidManifest.xml as well.
        public string[] scopes = { "Files.ReadWrite.All" };
        public IPublicClientApplication AuthClientApp { get; private set; }
        public GraphServiceClient GraphClient { get; set; }

        public GraphService(Activity activity)
        {
            AuthClientApp = PublicClientApplicationBuilder.Create(clientID)
                .WithRedirectUri($"msal{clientID}://auth")
                .Build();

            var authProvider = new DelegateAuthenticationProvider(async (request) =>
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
                    .WithParentActivityOrWindow(activity)
                    .ExecuteAsync();
                }
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            });

            // Use AndroidClientHandler as opposed to HttpClientHandler.
            var innerHandler = new AndroidClientHandler { AllowAutoRedirect = false };
            var pipeline = GraphClientFactory.CreatePipeline(GraphClientFactory.CreateDefaultHandlers(authProvider), innerHandler);

            GraphClient = new GraphServiceClient(authProvider, new HttpProvider(pipeline, true, new Serializer()));
        }

        public async Task<IList<DriveItem>> GetFilesWithAttachmentsAsync()
        {
            try
            {
                List<DriveItem> files = new List<DriveItem>();
                var driveItems = await GraphClient.Me.Drive.Root.Children.Request().OrderBy("name desc").GetAsync();
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
                using (var stream = await GraphClient.Drives[item.ParentReference.DriveId].Items[item.Id].Content.Request().GetAsync())
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