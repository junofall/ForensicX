using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ElevateHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logFile = Path.Combine(currentDirectory, "ElevateHelper.log");
            File.WriteAllText(logFile, $"ElevateHelper.exe: Received {args.Length} args\n");

            if (args.Length > 0)
            {
                var exePath = args[0];
                File.AppendAllText(logFile, $"ElevateHelper.exe: Received path: {exePath}\n");

                var startInfo = new ProcessStartInfo(exePath)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    File.AppendAllText(logFile, "ElevateHelper.exe: Attempting to elevate process.\n");
                    Process.Start(startInfo);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    // The user refused to allow privileges elevation.
                    // You can handle this situation if needed.
                    File.AppendAllText(logFile, "ElevateHelper.exe: " + e.Message + "\n");
                }
            }
            else
            {
                File.AppendAllText(logFile, "ElevateHelper.exe: No args\n");
            }
        }
    }
}