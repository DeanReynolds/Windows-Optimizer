using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http.Headers;
using Task = System.Threading.Tasks.Task;

namespace Windows_Optimizer
{
    public partial class MainWindow : Form
    {
        const int ItemsPerRow = 12;
        readonly int anItemWidth;

        readonly List<AnItem> AllTheItems = new();

        public MainWindow()
        {
            InitializeComponent();
            AnItem.Check = imageList1.Images["check"];
            AnItem.Warn = imageList1.Images["warn"];
            anItemWidth = new AnItem().Width;
        }

        public void AddAnItem(Func<AnItem, bool> check, Action<AnItem> ifIncomplete, Action<AnItem> ifComplete, Action<AnItem> apply, string tooltip = "") {
            var anItem = new AnItem(check, ifIncomplete, ifComplete, apply, tooltip);
            anItem.RunCheck();
            int col = AllTheItems.Count / ItemsPerRow, row = AllTheItems.Count % ItemsPerRow;
            anItem.Location = new(0 + (col * anItemWidth), row * 20);
            AllTheItems.Add(anItem);
            this.Controls.Add(anItem);
        }

        public T RegGet<T>(string key, string value, T returnIfError)
        {
            var reg = Registry.GetValue(key, value, returnIfError);
            if (reg == null)
                return returnIfError;
            return (T)reg;
        }

        public void RegSet<T>(string key, string value, T setTo, RegistryValueKind valueKind)
        {
            Registry.SetValue(key, value, setTo, valueKind);
        }

        public string GetRequest(string uri, bool jsonaccept)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(uri);

            if (jsonaccept)
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var res = client.GetStringAsync(uri).Result;
            return res;
        }

        public Guid GetActivePowerPlan()
        {
            Guid plan = Guid.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\cimv2\\power", "SELECT * FROM Win32_PowerPlan");

            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["IsActive"].ToString() == "True")
                    Guid.TryParse(obj["InstanceID"].ToString().Substring(21, 36), out plan);
            } //also might need future proofing, but for now it's ok

            //if the Guid is empty, then an error occured, if not
            //the plan should have a proper Guid.

            return plan;
        }

        public bool isNvidia()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["Name"].ToString().Contains("NVIDIA"))
                    return true;
            }

            return false;
        }

        public void ImportNIP()
        {
            if (!isNvidia())
                return;

            try
            {
                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "nvidiaProfileInspector.exe"), Properties.Resources.nvidiaProfileInspector);
                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "nv_profile.nip"), Properties.Resources.nv_profile);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("NIP Importing failed. Access to the temporary folder was denied.");
            }


            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(Path.GetTempPath(), "nvidiaProfileInspector.exe");
            p.StartInfo.Arguments = $"-silentImport {Path.Combine(Path.GetTempPath(), "nv_profile.nip")}";

            p.Start();
            p.WaitForExit();

            File.Delete(Path.Combine(Path.GetTempPath(), "nvidiaProfileInspector.exe"));
            File.Delete(Path.Combine(Path.GetTempPath(), "nv_profile.nip"));
        }

        public void ImportPowerPlan()
        {
            try
            {
                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "pwrplan.pow"), Properties.Resources.powerplan);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Power plan importing failed. Access to the temporary folder was denied.");
            }

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.Start();

            using (StreamWriter sw = p.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("powercfg -delete 77777777-7777-7777-7777-777777777777"); // <-- this is just in case it exists already, it imports it again.
                    sw.WriteLine($"powercfg -import {Path.Combine(Path.GetTempPath(), "pwrplan.pow")} 77777777-7777-7777-7777-777777777777");
                    sw.WriteLine("powercfg -SETACTIVE \"77777777-7777-7777-7777-777777777777\"");
                }
            }

            p.WaitForExit();

            File.Delete(Path.Combine(Path.GetTempPath(), "pwrplan.pow"));
        }

        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            HttpClient client = new HttpClient();

            using (var s = await client.GetStreamAsync(requestUri))
            {
                using (var fs = new FileStream(filename, FileMode.Create))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }

        public static Task DownloadSync(string requestUri, string filename)
        {
            if (requestUri == null)
                throw new ArgumentNullException("requestUri");

            return DownloadAsync(new Uri(requestUri), filename);
        }

        public void InstallOTR()
        {
            string json = GetRequest("https://github.com/TorniX0/OpenTimerResolution/releases/latest", true);
            var obj = JObject.Parse(json);
            string ver = string.Empty;

            DialogResult msgbox = MessageBox.Show("Do you want the self-contained version? (with the built-in framework)", "Windows Optimizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            switch (msgbox)
            {
                case DialogResult.Yes:
                    ver = "OpenTimerResolution_s";
                    break;
                case DialogResult.No:
                    ver = "OpenTimerResolution";
                    break;
            }

            string url = $"https://github.com/TorniX0/OpenTimerResolution/releases/download/{(string)obj["tag_name"]}/{ver}.exe";

            try
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"OpenTimerResolution"));
                Task.Run(() => DownloadSync(url, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"OpenTimerResolution\OpenTimerResolution.exe"))).GetAwaiter().GetResult();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Installing OpenTimerRes failed. Access to the temporary folder was denied.");
            }

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Verb = "runas";
            p.StartInfo.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"OpenTimerResolution\");
            p.StartInfo.CreateNoWindow = true;

            p.Start();

            using (StreamWriter sw = p.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine($@"OpenTimerResolution.exe -silentInstall");
                }
            }

            // we do all of this, to avoid the process sticking to the Windows Optimizer process.

            p.WaitForExit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Game Bar On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Game Bar Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Game Bar", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Turns off Xbox Game Bar");

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1) == 1;
            }, (AnItem me) => {
                me.SetText("Power Throttling On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Power Throttling Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Power Throttling", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 0) == -1
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Network Throttling On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Network Throttling Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Network Throttling", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", -1, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Affinity", 1) == 0
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Background Only", "") == "False"
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Clock Rate", 1) == 10000
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 1) == 8
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 1) == 6
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "") == "High"
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", "") == "High";
            }, (AnItem me) => {
                me.SetText("Games Scheduling Off", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Games Scheduling On", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Enabling Games Scheduling", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Affinity", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Background Only", "False", RegistryValueKind.String);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Clock Rate", 10000, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High", RegistryValueKind.String);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", "High", RegistryValueKind.String);
                me.RunCheck();
            }, "Sets some global games' properties to make them more stable");
            AddAnItem((AnItem me) => {
                var tempFiles = GetFiles(Path.GetTempPath(), false).Count()
                    + GetFiles($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\Temp", false).Count();
                return tempFiles <= 0;
            }, (AnItem me) => {
                var tempFiles = GetFiles(Path.GetTempPath(), false).Count()
                    + GetFiles($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\Temp", false).Count();
                me.SetText(tempFiles.ToString("#,## temp files"), Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("No temp files", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                var tempFiles = new List<string>(GetFiles(Path.GetTempPath(), false));
                var tempFolders = new List<string>(GetFiles(Path.GetTempPath(), true));
                tempFiles.AddRange(GetFiles($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\Temp", false));
                tempFolders.AddRange(GetFiles($@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\Temp", true));
                me.SetText($"Cleaning {tempFiles.Count:#,##} temp files...", Color.Black, FontStyle.Bold);
                foreach (var file in tempFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
                foreach (var folder in tempFolders)
                {
                    try
                    {
                        Directory.Delete(folder);
                    }
                    catch { }
                }
                me.RunCheck();
            }, "Tries to delete all temp files from multiple windows directories");
            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowSleepOption", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Sleep On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Sleep Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Sleep", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowSleepOption", 0, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Hides the Sleep option from the start menu");
            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Hibernate On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Hibernate Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Hibernate", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Hides the Hibernate option from the start menu");
            AddAnItem((AnItem me) => {
            return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 0) == 1;
            }, (AnItem me) => {
                me.SetText("Auto Maintenance On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Auto Maintenance Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Auto Maintenance", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "") == "0";
            }, (AnItem me) => {
                me.SetText("Menu Show Delay On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Menu Show Delay Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Menu Show Delay", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "0", RegistryValueKind.String);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "0") == "";
            }, (AnItem me) => {
                me.SetText("Classic Context Menu Off", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Classic Context Menu On", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Enabling Classic Context Menu", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0) == 1;
            }, (AnItem me) => {
                me.SetText("Background Apps On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Background Apps Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Background Apps", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Windows Widgets On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Windows Widgets Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Windows Widgets", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\Maps", "AutoUpdateEnabled", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Windows Maps On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Windows Maps Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Windows Maps", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\Maps", "AutoUpdateEnabled", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "EnablePerProcessSystemDPI", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Fix Blurry Apps On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText("Fix Blurry Apps Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Fix Blurry Apps", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "EnablePerProcessSystemDPI", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "") == "0"
                && RegGet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", "") == "10";
            }, (AnItem me) => {
                me.SetText("EPP Off | Mouse 6/11", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                var epp = RegGet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "") == "0" ? "Off" : "On";
                me.SetText($"EPP {epp} | Mouse {int.Parse(RegGet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", "2")) / 2 + 1}/11", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling EPP & Setting Mouse 6/11", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", "10", RegistryValueKind.String);
                me.RunCheck();
            });

            var v = RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", "StartMenu_Start_Time", Array.Empty<byte>());
            for (int i = 0; i < v.Length; i++)
                Debug.WriteLine(v[i]);

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "") == "0"
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "EnableAeroPeek", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "AlwaysHibernateThumbnails", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "IconsOnly", 0) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "DragFullWindows", "") == "1"
                && RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "FontSmoothing", "") == "2"
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 1) == 0
                && Enumerable.SequenceEqual(RegGet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "UserPreferencesMask", Array.Empty<byte>()), new byte[] { 144, 18, 2, 128, 16, 0, 0, 0 });
            }, (AnItem me) => {
                me.SetText("Visual Effects On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Visual Effects Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Visual Effects", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0", RegistryValueKind.String);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "EnableAeroPeek", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "AlwaysHibernateThumbnails", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "IconsOnly", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "DragFullWindows", "1", RegistryValueKind.String);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "FontSmoothing", "2", RegistryValueKind.String);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\Desktop", "UserPreferencesMask", new byte[] { 144, 18, 2, 128, 16, 0, 0, 0 }, RegistryValueKind.Binary);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Hide File Ext On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Hide File Ext Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Hide File Ext", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", 1) == 0
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Cortana On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Cortana Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Cortana", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 0) == 1;
            }, (AnItem me) => {
                me.SetText("Web Search On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Web Search Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Web Search", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Transparency Effects On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Transparency Effects Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Transparency Effects", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "SafeSearchMode", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Safe Search On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Safe Search Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Safe Search", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Cloud Search On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Cloud Search Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Cloud Search", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "DeviceHistoryEnabled", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Device History On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Device History Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Device History", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "DeviceHistoryEnabled", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 0) == 1
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 1) == 0
                && Enumerable.SequenceEqual(RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", "StartMenu_Start_Time", Array.Empty<byte>()), new byte[] { 13, 180, 116, 198, 31, 253, 214, 1 })
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Privacy Exposed", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Privacy Maintained", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Fixing Exposed Privacy", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", "StartMenu_Start_Time", new byte[] { 13, 180, 116, 198, 31, 253, 214, 1 }, RegistryValueKind.Binary);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Disables some privacy-breaching features of windows");
            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack", "ShowedToastAtLevel", 0) == 1
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Diagnostic Data On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Diagnostic Data Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Diagnostic Data", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack", "ShowedToastAtLevel", 1, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Disables windows diagnostic data reporting and telemetry");
            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Activity History On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Activity History Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Activity History", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0) == 1
                && RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 1) == 0;
            }, (AnItem me) => {
                me.SetText("Apps Run in BG On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Apps Run in BG Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Apps Run in BG", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "EnableAutoTray", 1) == 0
                && RegGet(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 0) == 1;
            }, (AnItem me) => {
                me.SetText("All Icons in Taskbar Off", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"All Icons in Taskbar On", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling All Icons in Taskbar", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "EnableAutoTray", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, RegistryValueKind.DWord);
                me.RunCheck();
            });

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PenWorkspace", "PenWorkspaceAppSuggestionsEnabled", 1) == 0
                && RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 0) == 1;
            }, (AnItem me) => {
                me.SetText("Recommend Apps On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Recommend Apps Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Recommend Apps", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PenWorkspace", "PenWorkspaceAppSuggestionsEnabled", 0, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Disables automatic recommended apps");

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\gupdate", "Start", 0) == 4
                && RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\gupdatem", "Start", 0) == 4
                && RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdate", "Start", 0) == 4
                && RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdatem", "Start", 0) == 4
                && RegGet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MozillaMaintenance", "Start", 0) == 4;
            }, (AnItem me) => {
                me.SetText("Browser Update Services On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Browser Update Services Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Browser Update Services", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\gupdate", "Start", 4, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\gupdatem", "Start", 4, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdate", "Start", 4, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdatem", "Start", 4, RegistryValueKind.DWord);
                RegSet(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MozillaMaintenance", "Start", 4, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Disables automatic browser updates/maintenance (Edge, Firefox)");

            AddAnItem((AnItem me) => {
                return RegGet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0) == 1;
            }, (AnItem me) => {
                me.SetText("Auto Windows Updates On", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Auto Windows Updates Off", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Disabling Auto Windows Updates", Color.Black, FontStyle.Bold);
                RegSet(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                me.RunCheck();
            }, "Disables automatic windows updates");

            AddAnItem((AnItem me) => {
                return GetActivePowerPlan() == new Guid("77777777-7777-7777-7777-777777777777");
            }, (AnItem me) => {
                me.SetText("Bitsum Power Plan Inactive", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"Bitsum Power Plan Active", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Activating Bitsum Power Plan", Color.Black, FontStyle.Bold);
                ImportPowerPlan();
                me.RunCheck();
            }, "Installs and activates the \"Bitsum Power Plan\", a superior power plan designed for better performance");

            AddAnItem((AnItem me) => {
                using (TaskService ts = new TaskService())
                {
                    return ts.RootFolder.AllTasks.Any(t => t.Name == "OpenTimerRes");
                }
            }, (AnItem me) => {
                me.SetText("OpenTimerRes Not Installed", Color.DimGray, FontStyle.Regular);
                me.GoWarn();
            }, (AnItem me) => {
                me.SetText($"OpenTimerRes Installed", Color.DimGray, FontStyle.Regular);
                me.GoCheck();
            }, (AnItem me) => {
                me.SetText("Installing OpenTimerRes", Color.Black, FontStyle.Bold);

                InstallOTR();

                using (TaskService ts = new TaskService())
                {
                    while (!ts.RootFolder.AllTasks.Any(t => t.Name == "OpenTimerRes"))
                    {
                        if (ts.RootFolder.AllTasks.Any(t => t.Name == "OpenTimerRes"))
                            break;
                    }
                }
                // probably not the best method to check, but did this anyway since it takes a bit to install the schedule.
                // maybe will be replaced soon.

                me.RunCheck();
            }, "Installs and runs OpenTimerResolution, which will automatically set\nthe timer resolution to 0.5ms and clean your memory cache automatically in the background");

            // NOTE:
            // this is a placeholder until we find a method that can actually verify it properly

            if (isNvidia())
            {
                bool optimized = false;
                AddAnItem((AnItem me) =>
                {
                    return optimized == true;
                }, (AnItem me) =>
                {
                    me.SetText("NVIDIA Settings Unoptimized", Color.DimGray, FontStyle.Regular);
                    me.GoWarn();
                }, (AnItem me) =>
                {
                    me.SetText($"NVIDIA Settings Optimized", Color.DimGray, FontStyle.Regular);
                    me.GoCheck();
                }, (AnItem me) =>
                {
                    me.SetText("Optimizing NVIDIA Settings", Color.Black, FontStyle.Bold);
                    ImportNIP();
                    optimized = true;
                    me.RunCheck();
                }, "Optimizes the NVIDIA Settings from your NVIDIA Control Panel (and some hidden ones that you can't see there)");
            }
            this.Size = new((((AllTheItems.Count - 1) / ItemsPerRow) + 1) * anItemWidth + 20, (ItemsPerRow * 20) + 39 + btnStart.Size.Height + 6);
            btnStart.Location = new(5, (ItemsPerRow * 20) + 1);
            btnUncheck.Location = new(btnStart.Right + 3, btnStart.Top);
            panel1.Location = new(btnUncheck.Right + 3, btnUncheck.Top);
            panel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            foreach (var item in AllTheItems)
                if (item.IsChecked && !item.RunCheck())
                    item.Apply();
        }

        private void btnUncheck_Click(object sender, EventArgs e)
        {
            foreach (var item in AllTheItems)
                if (item.IsChecked)
                    item.SetChecked(false);
        }

        static IEnumerable<string> GetFiles(string path, bool directories)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                        queue.Enqueue(subDir);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[]? files = null;
                try
                {
                    if (directories)
                        files = Directory.GetDirectories(path);
                    else
                        files = Directory.GetFiles(path);
                }
                catch (Exception) {}

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }

        private void tornixLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/TorniX0") { CreateNoWindow = true });
        }

        private void freethyLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.youtube.com/c/FR33THY") { CreateNoWindow = true });
        }

        private void dcrewLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/DeanReynolds") { CreateNoWindow = true });
        }
    }
}
