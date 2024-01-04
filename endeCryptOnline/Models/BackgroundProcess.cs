namespace endeCryptOnline.Models
{
    public class BackgroundProcess : BackgroundService
    {

        public int IntervalSec { get; } = 60;

        private readonly IWebHostEnvironment _rootEnv;

        public BackgroundProcess(IWebHostEnvironment env)
        {
            _rootEnv = env; //for root path
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000 * IntervalSec, stoppingToken);
                backgroundReminder();
            }
        }

        private void backgroundReminder()
        {
            try
            {
                string webRootPath = _rootEnv.WebRootPath;
                PrepareFiles.CleanUploadSpace(Path.Combine(webRootPath, "uploads/"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("backgroundReminder Error: "+ ex.Message);
            }
         
        }

    }
}
