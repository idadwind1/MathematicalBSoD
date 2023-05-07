using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MathematicalBSoD
{
    internal static class Program
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0 && args[0].ToLower() == "watchdog")
                for (; ;) WatchDogTask();
            else Application.Run(new MainForm());
        }

        public static void BSoD()
        {
#if DEBUG
            MessageBox.Show("Imagine that there's a BSoD", "Debug");
#else
            int isCritical = 1;  // we want this to be a Critical Process
            int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)
 
            Process.EnterDebugMode();  //acquire Debug Privileges
 
            // setting the BreakOnTermination = 1 for the current process
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
#endif
            Environment.Exit(0);
        }
        
        public static void WatchDogTask()
        {
            Thread.Sleep(100);
            //MessageBox.Show(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length.ToString());
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length < 6)
            {
                //MessageBox.Show("1");
                //if (args.Length > 1 && args[1] == "WithMessage")
                //{
                new Thread(() => MessageBox.Show("That's rude bro", "Ouch")).Start();
                //}
                Thread.Sleep(1000);
                BSoD();
                return;
            }
        }
    }
}
