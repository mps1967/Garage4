using System;
using System.Collections.Generic;
using System.Text;

namespace Lib
{
    public static  class Menu
    {
        public static readonly char[] WHITESPACE = [' ', '\t', '\n', '\r'];
        public static readonly StringSplitOptions SPLITOPT =
        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        public static int Run(List<string> num_menu, string prompt, out string text)
        {
            text = "";
            for (int i = 0; i < num_menu.Count; ++i)
            {
                Console.WriteLine($"{i}. {num_menu[i]}");
            }
            Console.WriteLine();
            Console.Write(prompt);
            string sel = Console.ReadLine() ?? "";
            text = sel;
            sel = sel.Trim();
            if (sel.Length == 0) return -1;
            if (int.TryParse(sel, out int num))
            {
                if (num >= 0 && num < num_menu.Count) return num;
            }
            return num_menu.Count;
        }
        public static bool SplitSelector(string text, out string key, out string value)
        {
            key = "";
            value = "";
            string[] parts = text.Split(WHITESPACE, SPLITOPT);
            if (parts.Length == 0) return false;
            key = parts[0];
            value = string.Join(" ", parts[1..^0]);  // screetching syntax!!!
            return true;
        }
    }
}
