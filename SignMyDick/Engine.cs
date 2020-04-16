using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SignMyDick
{
    static class Engine
    {
        static string Class;
        static string Title;
        static bool SetupRequired => Class == null || Title == null;
        static ShowWindow Hook;

        static Ini Settings;
        static bool Initialized = false;
        static bool Persistent = false;
        public static void Initialize() {
            if (Initialized)
                return;
            Initialized = true;
            
            string IniPath = Path.Combine(Wrapper.CurrentDllPath, "SignMyDick.ini");

            Settings = new Ini(IniPath, "SignMyDick");

            Title = Settings.GetValue("Title");
            Class = Settings.GetValue("Class");
            var PersistentValue = Settings.GetValue("Persistent");
            if (PersistentValue != null) {
                PersistentValue = PersistentValue.Trim().ToLower();
                Persistent = PersistentValue == "true" || PersistentValue == "1";
            }
            Hook = new ShowWindow();

            if (SetupRequired)
            {
                MessageBox.Show("The SignMyDick isn't configured yet, I will ask you some things to confirm what is the Unsigned Alert, you just need reply Yes or No.", "SignMyDick", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hook.BeforeShowWindow += Setup;
            }
            else
                Hook.BeforeShowWindow += BeforeShowWindow;

            Hook.Install();
        }

        public static void BeforeShowWindow(object Sender, ShowWindowEventArgs Args)
        {
            if (Args.Title == Title && Args.Class == Class) {
                Args.nCmdShow = CMDShow.SW_HIDE;
                if (!Persistent)
                    Hook.Uninstall();
            }
        }

        readonly static string[] Invalids = new[] { "Chrome Legacy Window" };
        public static void Setup(object Sender, ShowWindowEventArgs Args) {
            if (Args.Title == "SignMyDick")
                return;           

            if (string.IsNullOrWhiteSpace(Args.Title) || string.IsNullOrWhiteSpace(Args.Class))
                return;
            if (Invalids.Contains(Args.Title) || Invalids.Contains(Args.Class))
                return;

            Hook.Uninstall();
            var DRst = MessageBox.Show($"This is the Unsigned Alert?\n[{Args.Class}] {Args.Title}", "SignMyDick", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DRst == DialogResult.Yes) {
                try
                {
                    Settings.SetValue("Title", Args.Title);
                    Settings.SetValue("Class", Args.Class);
                    Settings.SetValue("Persistent", "false");
                    Settings.Save();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }

                Hook.BeforeShowWindow -= Setup;
            }
            Hook.Install();
        }
    }
}
