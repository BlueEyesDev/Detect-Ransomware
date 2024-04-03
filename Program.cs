using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace detect_ransomware
{
    internal class Program
    {
        static List<int> Run_PIDS = new List<int>();
        static List<regedit> Run_regedit = new List<regedit>();
        static List<taskfile> Tasks = new List<taskfile>();
        static void Main(string[] args)
        {
            RunAs();
            Run_PIDS = new List<int>(Process.GetProcesses().Select(process => process.Id));
            Run_regedit = reg();
            Tasks = HashTask();

            ListDirs("C:\\Users\\");
            Console.ReadLine();
        }
        static List<taskfile> HashTask()
        {
            List<taskfile> tasklist = new List<taskfile>();
            string[] files = Directory.GetFiles("C:\\Windows\\System32\\Tasks", "*.*", SearchOption.AllDirectories);
            Parallel.ForEach(files, name =>
            {
                try
                {
                    tasklist.Add(new taskfile()
                    {
                        Hash = Checksum(name, SHA256.Create()).Hexa,
                        Path = name
                    });
                }
                catch (Exception) { }
            });
            return tasklist;
        }
      
        static List<regedit> reg()
        {
            List<regedit> regs = new List<regedit>();
            Parallel.ForEach(new string[] {
                 "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                 "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                 "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                 "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
            }, reg =>
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(reg))
                    {
                        if (key != null)
                        {
                            string[] valueNames = key.GetValueNames();
                            Parallel.ForEach(valueNames, name => {
                                try
                                {
                                    regs.Add(new regedit()
                                    {
                                        Hash = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(UTF8Encoding.UTF8.GetBytes($"{reg}{name}{key.GetValue(name)}"))).Replace("-", string.Empty),
                                        Registry = key,
                                        Name = name,
                                        Value = key.GetValue(name).ToString()
                                    }); 
                                }
                                catch (Exception) { }
                            });
                        }
                    }
                }
                catch (Exception) { }
            });
            return regs;
        }
      
        static List<string> ListDirs(string chemin)
        {
            List<string> alldirs = new List<string>();
                string[] Dirs = Directory.GetDirectories(chemin, "*.*");
                alldirs.AddRange(alldirs);
                Parallel.ForEach(Dirs, Dir => {
                    try {
                        alldirs.AddRange(ListDirs(Dir));
                    }
                    catch (Exception) { }
                });
                try {
                    string[] files = Directory.GetFiles(chemin, "*.txt");
                    if (files.Length > 0) {
                        Parallel.ForEach(files, fs => {
                            try {
                                if (fs.Contains("0.txt") || fs.Contains("_.txt")) {
                                    Watcher(chemin, fs);
                                }
                            }
                            catch (Exception) { }
                        });
                    } else {
                        try { 
                            File.Create($"{chemin}/0.txt");
                            File.SetAttributes($"{chemin}/0.txt", File.GetAttributes($"{chemin}/0.txt") | FileAttributes.Hidden);
                        Watcher(chemin, "0.txt");
                        } catch {  }
                            
                        try {
                            File.Create($"{chemin}/_.txt");
                            File.SetAttributes($"{chemin}/_.txt", File.GetAttributes($"{chemin}/_.txt") | FileAttributes.Hidden);
                            Watcher(chemin, "_.txt");
                        } catch { }

                    }
                }
                catch (Exception) { }
            return alldirs;
        }
        public static void Watcher(string dir, string file) {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dir;
            watcher.Filter = file.Split('\\').Last();
            watcher.Changed += (sender, e) =>
            {
                List<int> pidsKill = new List<int>(Process.GetProcesses().Select(process => process.Id));
                List<int> pidsToKill = pidsKill.Except(Run_PIDS).ToList();
                List<taskfile> checktask = HashTask();
                List<regedit>  regedits = new List<regedit>();
                Parallel.ForEach(pidsToKill, pid => {
                    try
                    {
                        Process.GetProcessById(pid).Kill();
                    }
                    catch (Exception) { }
                });

                List<taskfile> check =  checktask.Where(item1 => !Tasks.Any(item2 => item1.Hash == item2.Hash)).ToList();
                Parallel.ForEach(check, l => {
                    try
                    {
                        File.Delete(l.Path);
                    }
                    catch (Exception) { }
                });

                List<regedit> checkr = Run_regedit.Where(item1 => !regedits.Any(item2 => item1.Hash == item2.Hash)).ToList();
                Parallel.ForEach(checkr, l => {
                    try
                    {
                        l.Registry.DeleteValue(l.Name);
                    }
                    catch (Exception) { }
                });


            };
            watcher.Deleted += (sender, e) =>
            {
                List<int> pidsKill = new List<int>(Process.GetProcesses().Select(process => process.Id));
                List<int> pidsToKill = pidsKill.Except(Run_PIDS).ToList();
                List<taskfile> checktask = HashTask();
                List<regedit> regedits = new List<regedit>();
                Parallel.ForEach(pidsToKill, pid => {
                    try
                    {
                        Process.GetProcessById(pid).Kill();
                    }
                    catch (Exception) { }
                });

                List<taskfile> check = checktask.Where(item1 => !Tasks.Any(item2 => item1.Hash == item2.Hash)).ToList();
                Parallel.ForEach(check, l => {
                    try
                    {
                        File.Delete(l.Path);
                    }
                    catch (Exception) { }
                });

                List<regedit> checkr = Run_regedit.Where(item1 => !regedits.Any(item2 => item1.Hash == item2.Hash)).ToList();
                Parallel.ForEach(checkr, l => {
                    try
                    {
                        l.Registry.DeleteValue(l.Name);
                    }
                    catch (Exception) { }
                });

            };
            watcher.EnableRaisingEvents = true;

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
        public static (string Hexa, byte[] Binary) Checksum(string path, HashAlgorithm hash)
        {
            using (FileStream Data = File.OpenRead(path))
            {
                byte[] ComputeHash = hash.ComputeHash(Data);
                string hexa = BitConverter.ToString(ComputeHash).Replace("-", string.Empty).ToLower();
                return (hexa, ComputeHash);
            }
        }
        class taskfile
        {
            public string Hash { get; set; }
            public string Path { get; set; }
        }
        class regedit
        {
            public string Hash { get; set; }
            public RegistryKey Registry { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
