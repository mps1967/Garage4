using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abstract;

namespace Lib
{
    public class Globals : IGlobals
    {
        public string DataRoot { get; } = BuildDataRoot();
        private SortedDictionary<string, IBlueprint> blueprints_ = new();
        private int number_ = GetTime() * 1000;
        public void Add(IBlueprint bp)
        {
            blueprints_[bp.Name] = bp;
        }

        public IBlueprint GetBlueprint(string name)
        {
            return blueprints_[name];
        }
        private static string ThisFile([System.Runtime.CompilerServices.CallerFilePath] string fn = "") {return fn; }

        private static int GetTime()
        {
            var start = new DateTime(2025, 12, 1);
            var now = DateTime.Now;
            TimeSpan interval = now - start;
            return interval.Seconds;
        }
        private static string BuildDataRoot()
        {
            string mps_root = @"C:\Users\mps\source\repos\Garage4\Data";
            string fn = ThisFile();
            // fn C:\Users\mps\source\repos\Garage4\Lib\Globals.cs
            // dir C:\Users\mps\source\repos\Garage4\Lib\
            // dirdir C:\Users\mps\source\repos\Garage4\
            if (string.IsNullOrEmpty(fn)) return mps_root;
            string? dir = Path.GetDirectoryName(fn!);
            if (string.IsNullOrEmpty(dir)) return mps_root;
            string? dirdir = Path.GetDirectoryName(dir!);
            if (string.IsNullOrEmpty(dirdir)) return mps_root;
            return Path.Join(dirdir, "Data");
        }
        public int NewNumber() { return ++number_; }
            // The budget of number between 2 sessions is the time in seconds between the sessions * 1000.
            // This seems as long as I don't start the program more often then every second.
            // I could prevent that by sleeping for 1 second --> not a serious enough matter to do it.
    }
}
