using System;
using System.Diagnostics;
using System.IO;

namespace SR_Manager
{
    internal static class Global
    {
        public static async void WriteLog(string msg, bool error = false)
        {
            try
            {
                var filename = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);
                var directory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
                using (var fw = File.AppendText($"{directory}\\{filename?.Replace(".exe", "")}_{DateTime.Now:yyyyMMdd}.log"))
                {
                    var timenow = DateTime.Now;

                    if (!error)
                    {
                        var fontcolor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($@"[{timenow.Year}/{timenow.Month:D2}/{timenow.Day:D2} {timenow:hh}:{timenow.Minute:D2}:{timenow.Second:D2} {timenow:tt}] {msg}");
                        Console.ForegroundColor = fontcolor;
                        
                        await fw.WriteLineAsync($"[{timenow.Year}/{timenow.Month:D2}/{timenow.Day:D2} {timenow:hh}:{timenow.Minute:D2}:{timenow.Second:D2} {timenow:tt}] {msg}");
                    }
                    else
                    {
                        var fontcolor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($@"[{timenow.Year}/{timenow.Month:D2}/{timenow.Day:D2} {timenow:hh}:{timenow.Minute:D2}:{timenow.Second:D2} {timenow:tt}] An error has been logged, please check the log file.");
                        Console.ForegroundColor = fontcolor;

                        var errormsg = msg.Split(new[] {"Exception: ", " at ", " in "}, StringSplitOptions.None);
                        await fw.WriteLineAsync($"[{timenow.Year}/{timenow.Month:D2}/{timenow.Day:D2} {timenow:hh}:{timenow.Minute:D2}:{timenow.Second:D2} {timenow:tt}] {errormsg[1]} {errormsg[3]}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    internal class Modules
    {
        public string DropDownName { get; set; }
        public string ActualName { get; set; }
        public string ActualPath { get; set; }

        public Modules(string list, string name, string path) {
            DropDownName = list;
            ActualName = name;
            ActualPath = path;
        }
    }
}
