using Foundation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace XamainIOSGraphSample
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void UIButton220_TouchUpInside(UIButton sender)
        {
            Services.GraphService graphService = new Services.GraphService();
            Task.Run(async () =>
            {
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

                            var fileContent = await graphService.DownloadFileAsync(file);

                            System.Diagnostics.Debug.WriteLine($"Downloaded file {count}.{file.Name} of {fileContent.Length / 1024} KB size.");
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
            });
        }

    }
}