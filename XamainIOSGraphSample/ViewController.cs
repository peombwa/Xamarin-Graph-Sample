using Foundation;
using System;
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
            RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            Services.GraphService graphService = new Services.GraphService();
            try
            {
                var files = await graphService.GetFilesWithAttachmentsAsync();

                foreach (var file in files)
                {
                    var content = await graphService.DownloadFileAsync(file);
                }

            }
            catch (Exception ex)
            {

            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}