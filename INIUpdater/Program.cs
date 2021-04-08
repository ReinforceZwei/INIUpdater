using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace INIUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: INIUpdater.exe <new ini file> <old ini file> [<output file>]");
                Environment.Exit(1);
            }
            var n = File.ReadAllLines(args[0]);
            var o = File.ReadAllLines(args[1]);
            var r = INIUpdater.Merge(n, o);
            if (args.Length > 2)
            {
                File.WriteAllText(args[2], string.Join("\r\n", r));
            }
            else Console.WriteLine(string.Join("\r\n", r));
        }
    }
}
