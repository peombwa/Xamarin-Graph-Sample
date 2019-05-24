using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Microsoft.Identity.Client;
using Xamarin_Graph_Sample_Android.Services;

namespace Xamarin_Graph_Sample_Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        GraphService graphService;
        ProgressBar pbProgess;
        TextView txtMessage;
        FloatingActionButton fab;
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            graphService = new GraphService(this);

            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            pbProgess = FindViewById<ProgressBar>(Resource.Id.pbProgess);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);

            fab.Click += FabOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            txtMessage.Text = GetString(Resource.String.txt_downloading_message);
            pbProgess.Visibility = ViewStates.Visible;
            fab.Visibility = ViewStates.Invisible;

            Task.Run(async () => {
                var filesToDownload = await graphService.GetFilesWithAttachmentsAsync();

                List<Task> tasks = new List<Task>();
                int count = 0;
                int totalByteCount = 0;
                SemaphoreSlim semaphore = new SemaphoreSlim(10);
                foreach (var file in filesToDownload)
                {
                    var downloadFileTask = Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync();

                            count++;
                            string message = $"Downloading file {count} of {filesToDownload.Count}.";
                            System.Diagnostics.Debug.WriteLine(message);

                            RunOnUiThread(() => {
                                txtMessage.Text = message;
                            });

                            var fileContent = await graphService.DownloadFileAsync(file);

                            System.Diagnostics.Debug.WriteLine($"Downloaded file {count}.{file.Name} of {fileContent.Length/ 1024} KB size.");
                            totalByteCount += fileContent.Length;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    tasks.Add(downloadFileTask);
                }

                Task.WaitAll(tasks.ToArray());

                RunOnUiThread(() => {
                    pbProgess.Visibility = ViewStates.Invisible;
                    fab.Visibility = ViewStates.Visible;
                    txtMessage.Text = GetString(Resource.String.txt_default_message);

                    View view = (View)sender;
                    Snackbar.Make(view, $"Downloaded {count} files of total size {totalByteCount / 1048576} MB.", Snackbar.LengthLong)
                        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
                });
            });
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

