using SignMyDick;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignMyDickInstaller
{
    static class Program
    {
        public static Ini Settings;
        public static string IniPath => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SignMyDick.ini");
        public static string OriLauncher => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "_ori.exe");
        private static string SignMyDickDll => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SigMyDk.dll");
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            Settings = new Ini(IniPath, "Installer");
            if (!File.Exists(SignMyDickDll)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);
                Application.Run(new MainWindow());
                return;
            }

            MainWindow.DetectEnvironment(Application.ExecutablePath, out string ElfPath, out string _, out string _);
            int Offset = int.Parse(Settings.GetValue("ElfOffset"));
            byte[] ElfData = File.ReadAllBytes(ElfPath);
            if (!MainWindow.EqualsAt(ElfData, "SigMyDk", Offset)) {
                MainWindow.CloseBrowser(Application.ExecutablePath);
                MainWindow.CloseBrowser(OriLauncher);
                Repatch(ElfData, ElfPath);
            }

            string LArgs = ParseArguments(Args);
            Process.Start(OriLauncher, LArgs);
        }

        private static void Repatch(byte[] ElfData, string ElfPath)
        {
            int IndexOf = MainWindow.SearchString(ElfData, "VERSION");
            if (IndexOf == -1)
                IndexOf = MainWindow.SearchString(ElfData, "Version");
            if (IndexOf == -1)
                IndexOf = MainWindow.SearchString(ElfData, "version");
            if (IndexOf == -1)
            {
                IndexOf = MainWindow.SearchString(ElfData, "SigMyDk");
                if (IndexOf == -1)
                {
                    MessageBox.Show("Unsupported Browser", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } else {
                Encoding.ASCII.GetBytes("SigMyDk").CopyTo(ElfData, IndexOf);
            }
            Settings.SetValue("ElfOffset", IndexOf.ToString());
            File.WriteAllBytes(ElfPath, ElfData);
        }

        private static string ParseArguments(string[] Args) {
            string Line = "";
            foreach (var Arg in Args) {
                if (Arg.Contains(" "))
                    Line += $"\"{Arg}\" ";
                else
                    Line += Arg + " ";
            }
            return Line.TrimEnd(' ');
        }
    }
}
