using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace WO_Updater {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            using WebClient wc = new();
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted; ;
            wc.DownloadFileAsync(
                new System.Uri("https://www.dropbox.com/s/m47ixlm4ohsephc/Windows%20Optimizer.zip?dl=1"),
                Path.Combine(Environment.CurrentDirectory, "Latest.zip")
            );
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = $"Download ({progressBar1.Value}%)";
        }

        void wc_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            ZipFile.ExtractToDirectory(Path.Combine(Environment.CurrentDirectory, "Latest.zip"), Environment.CurrentDirectory, true);
            File.Delete(Path.Combine(Environment.CurrentDirectory, "Latest.zip"));
            Process p = new();
            p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Windows Optimizer.exe");
            p.Start();
            Application.Exit();
        }
    }
}