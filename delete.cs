using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace delete
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunAs();
            ListDirs("c:\\");
        }
        static List<string> ListDirs(string chemin)
        {
            List<string> alldirs = new List<string>();

                string[] Dirs = Directory.GetDirectories(chemin, "*.*");
                alldirs.AddRange(alldirs);
                Parallel.ForEach(Dirs, Dir => {
                    try
                    {
                        alldirs.AddRange(ListDirs(Dir));
                    }
                    catch (Exception) { }
                });
                try
                {
                    string[] files = Directory.GetFiles(chemin, "*.txt");
                    if (files.Length > 0)
                    {
                        Parallel.ForEach(files, fs => {
                            try
                            {
                                string filename = fs.Split('\\').Last();
                                if (filename == "0.txt" || filename == "_.txt")
                                {
                                    Console.WriteLine(fs);
                                    File.Delete(fs);
                                }
                            }
                            catch (Exception) { }
                        });
                    }
                }
                catch (Exception) { }
            return alldirs;
        }
        public static void RunAs()
        {

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        Verb = "runas",
                        Arguments = string.Join(" ", Environment.GetCommandLineArgs())
                    });
                }
                catch { }
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
