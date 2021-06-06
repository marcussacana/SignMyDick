using SignMyDick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vestris.ResourceLib;

namespace SignMyDickInstaller
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            Log("SignMyDick Installer v" + FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion.ToString());

            if (!File.Exists(SignMyDickDll)) {
                MessageBox.Show($"Module Not Found: {Path.GetFileName(SignMyDickDll)}", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }    
        }

        string SignMyDickDll => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SignMyDick.dll");
        string OriLauncherExe => Path.Combine(Path.GetDirectoryName(LauncherExe), Path.GetFileNameWithoutExtension(LauncherExe) + "_ori.exe");
        string LauncherExe;
        string LauncherPath;
        string ElfPath;
        private void button1_Click(object sender, EventArgs e)
        {
            BrowserFinderDialog.ShowDialog();
        }

        private void BrowserFinderDialog_FileOk(object sender, CancelEventArgs e)
        {
            
            tbBrowserPath.Text = BrowserFinderDialog.FileName;
            DetectEnvironment(BrowserFinderDialog.FileName, out ElfPath, out LauncherPath, out LauncherExe);
            if (ElfPath != null && LauncherPath != null && LauncherExe != null)
            {
                Log("Environment:");
                Log($"Elf Path: {ElfPath}");
                Log($"Launcher Path: {LauncherPath}");
                btnInstall.Enabled = true;
                btnUninstall.Enabled = true;
            }
        }

        public static void DetectEnvironment(string MainExecutable, out string ElfPath, out string LauncherPath, out string LauncherExe) {
            ElfPath = "???";
            LauncherPath = "???";
            LauncherExe = MainExecutable;
            string Name = Path.GetFileNameWithoutExtension(MainExecutable);
            string ExeDir = Path.GetDirectoryName(MainExecutable);
            LauncherPath = ExeDir.TrimEnd('\\', '/');
            string[] Files = Directory.GetFiles(ExeDir, "*_elf.dll");
            if (Files.Length == 0)
            {
                string[] Dirs = Directory.GetDirectories(ExeDir);
                foreach (var Dir in Dirs)
                {
                    Files = Directory.GetFiles(Dir, "*_elf.dll");
                    if (Files.Length != 0)
                        break;
                }
            }
            else
            {
                LauncherPath = Path.GetDirectoryName(LauncherPath);
                var LFiles = Directory.GetFiles(LauncherPath, "*.exe");
                LFiles = (from x in LFiles where Path.GetFileNameWithoutExtension(x).ToLower() == Name.ToLower() select x).ToArray();
                if (LFiles.Length == 0)
                    Files = new string[0];
                LauncherExe = LFiles.First();
            }
            if (Files.Length != 1)
            {
                ElfPath = null;
                LauncherPath = null;
                LauncherExe = null;
                MessageBox.Show("Unsupported Browser", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ElfPath = Files.Single();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            CloseBrowser(tbBrowserPath.Text);
            Log("Searching Elf Patch Offset...");
            byte[] ElfData = File.ReadAllBytes(ElfPath);
            int IndexOf = SearchString(ElfData, "VERSION");
            if (IndexOf == -1)
                IndexOf = SearchString(ElfData, "Version");
            if (IndexOf == -1)
                IndexOf = SearchString(ElfData, "version");
            if (IndexOf == -1) {
                IndexOf = SearchString(ElfData, "SigMyDk");
                if (IndexOf == -1) {
                    Log("Failed to find VERSION.DLL Import in the Elf Library");
                    MessageBox.Show("Unsupported Browser", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Log("Already Installed");
                MessageBox.Show("SignMyDick is already Installed", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Log($"Elf Patch Offset: 0x{IndexOf:X8}");
            Encoding.ASCII.GetBytes("SigMyDk").CopyTo(ElfData, IndexOf);
            File.WriteAllBytes(ElfPath, ElfData);
            File.Copy(SignMyDickDll, Path.Combine(LauncherPath, "SigMyDk.dll"), true);
            Log($"{Path.GetFileName(SignMyDickDll)} Copied.");

            Program.Settings.SetValue("ElfOffset", IndexOf.ToString());
            if (ckAutoReinstall.Checked) {
                Log("Installing Launcher...");
                if (File.Exists(OriLauncherExe)) {
                    Log($"Deleting {Path.GetFileName(OriLauncherExe)}...");
                    File.Delete(OriLauncherExe);
                }
                File.Move(LauncherExe, OriLauncherExe);
                Log($"{Path.GetFileName(LauncherExe)} Renamed.");

                var BrowserResInfo = GetIconResouceFromFile(OriLauncherExe);
                var LauncherResInfo = GetIconResouceFromFile(Application.ExecutablePath);
                LauncherResInfo.Icons = BrowserResInfo.Icons;
                File.Copy(Application.ExecutablePath, LauncherExe);
                LauncherResInfo.SaveTo(LauncherExe);
                Log($"Launcher Genareted.");

                Program.Settings.Save();
                var BrowserIni = Path.Combine(LauncherPath, "SignMyDick.ini");
                if (File.Exists(BrowserIni))
                    File.Delete(BrowserIni);
                File.Move(Program.IniPath, BrowserIni);
                Log($"Initial Settings Genareted.");
            }
            Log($"Success.");
        }

        private IconDirectoryResource GetIconResouceFromFile(string File) {
            var Resource = new IconDirectoryResource();
            try
            {
                Resource.LoadFrom(File);
            }
            catch
            {
                Resource = null;
                // Above fails on some files (possibly due to larger icons?) but this approach works
                using (ResourceInfo vi = new ResourceInfo())
                {
                    vi.Load(File);
                    var resKey = vi.Resources.Keys.FirstOrDefault((v) => { try { return v.ResourceType == Kernel32.ResourceTypes.RT_GROUP_ICON; } catch { return false; } });
                    if (resKey != null)
                    {
                        var res = vi.Resources[resKey];
                        Resource = res[0] as IconDirectoryResource;
                    }
                }
            }
            return Resource;
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(LauncherPath, "SigMyDk.dll"))) {
                MessageBox.Show("The SignMyDick isn't installed.", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CloseBrowser(tbBrowserPath.Text);
            Log("Starting Uninstaller...");
            File.Delete(Path.Combine(LauncherPath, "SigMyDk.dll"));
            Log("SigMyDk.dll Deleted.");
            if (File.Exists(OriLauncherExe)) {
                File.Delete(LauncherExe);
                File.Move(OriLauncherExe, LauncherExe);
                Log($"{Path.GetFileName(LauncherExe)} Restored.");
            }

            var BrowserIni = Path.Combine(LauncherPath, "SignMyDick.ini");
            if (File.Exists(BrowserIni)) {
                File.Delete(BrowserIni);
                Log($"{Path.GetFileName(BrowserIni)} Deleted.");
            }

            var ElfData = File.ReadAllBytes(ElfPath);
            var Index = SearchString(ElfData, "SigMyDk");
            if (Index != -1) {
                Encoding.ASCII.GetBytes("VERSION").CopyTo(ElfData, Index);
                File.WriteAllBytes(ElfPath, ElfData);
                Log("Elf Unpatched.");
            }
            Log("Success.");
        }

        public static int SearchString(byte[] Array, string Search) => SearchArray(Array, Encoding.ASCII.GetBytes(Search));
        public static int SearchArray(byte[] Array, byte[] Search) {
            for (int i = 0; i < Array.Length; i++) {
                if (EqualsAt(Array, Search, i))
                    return i;
            }
            return -1;
        }
        public static bool EqualsAt(byte[] Array, string Search, int At) => EqualsAt(Array, Encoding.ASCII.GetBytes(Search), At);

        public static bool EqualsAt(byte[] Array, byte[] Data, int At) {
            if (At + Data.Length >= Array.Length)
                return false;
            for (int i = 0; i < Data.Length; i++) {
                if (Data[i] != Array[i + At])
                    return false;
            }
            return true;
        }

        public static void CloseBrowser(string ExePath) {
            bool Killed = false;
            var CurrentProcId = Process.GetCurrentProcess().Id;
            
            foreach (var Proc in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExePath))) {
                if (Proc.Id == CurrentProcId)
                    continue;               
                Proc.Kill();
                Killed = true;
            }

            foreach (var Proc in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExePath) + "_ori")) {
                if (Proc.Id == CurrentProcId)
                    continue;               
                Proc.Kill();
                Killed = true;
            }

            if (Killed)
                Thread.Sleep(5000);
        }

        private void Log(string Message)
        {
            tbLog.Text += Message + "\r\n";
            tbLog.SelectionStart = tbLog.Text.Length;
            tbLog.ScrollToCaret();
            Application.DoEvents();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
