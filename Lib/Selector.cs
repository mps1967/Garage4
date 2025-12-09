using Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lib
{
    public class Selector
    {
        private static SortedSet<string> bool_props_ = new();
        private static SortedSet<string> float_props_ = new();
        static Selector()
        {
            bool_props_.Add("IsSmall");
            float_props_.Add("Height");
            float_props_.Add("Width");
            float_props_.Add("Length");
            float_props_.Add("Depth");
        }

        private IGlobals ig_;
        private IDB db_;
        public string LowStr { get; set; } = "";
        public string HighStr { get; set; } = "";
        public int LowInt { get; set; } = 0;
        public int HighInt { get; set; } = int.MaxValue;
        public string Prop { get; set; } = "";
        public SortedSet<int> OurIdSet { get; set; } = new();

        private SortedSet<string> string_props_ = new();
        private SortedSet<string> int_props_ = new();  // i.e. bool, int or float.
        private SortedSet<string> all_props_ = new();
        private List<string> props_menu_;
        private Regex? regex_ = null;
        private string pattern_ = "";


        private string description_ = "";
        private void Accumulate(SortedSet<int> ssi)
        {
            OurIdSet.UnionWith(ssi);
        }

        bool ParseInput(string str)
        {
            if (string.IsNullOrEmpty(Prop)) return false;
            if (string.IsNullOrEmpty(str)) return false;
            if (string_props_.Contains(Prop)) return ParseStringInput(str);
            if (bool_props_.Contains(Prop)) return ParseBoolInput(str);
            if (int_props_.Contains(Prop)) return ParseIntInput(str);
            if (float_props_.Contains(Prop)) return ParseFloatInput(str);
            Debug.Assert(false);
            return false;
        }

        static string EmptyIfInf(string s)
        {
            if (s == "_") return "";
            return s;
        }
        static string InfIfEmpty(string s)
        {
            if (s.Length == 0) return "_";
            return s;
        }

        bool ParseStringInput(string str)
        {
            if (!Menu.SplitSelector(str, out string k, out string v)) return false;
            if (v.Length == 0)
            {
                // checkin if this is a prefix match.
                if (str.IndexOf('|') == (str.Length - 1))
                {
                    LowStr = str.Substring(0, str.Length - 1);
                    if (LowStr.Length == 0) return false;
                    StringBuilder sb = new();
                    sb.Append(LowStr.Substring(0, LowStr.Length - 1));
                    var last = LowStr[LowStr.Length - 1];
                    sb.Append(1 + last);
                    HighStr = sb.ToString();
                    description_ = $"{LowStr} <= {Prop} < {HighStr}";
                    return true;
                }

                // this is a pattern.
                RegexOptions opt = RegexOptions.IgnoreCase
                    | RegexOptions.Singleline
                    | RegexOptions.Compiled
                    | RegexOptions.IgnoreCase
                    | RegexOptions.NonBacktracking;
                try
                {
                    regex_ = new Regex(str, opt);
                    pattern_ = str;
                    description_ = $"{Prop} matches {pattern_}";
                }
                catch
                {
                    pattern_ = "";
                    description_ = "";
                    return false;
                }
                return true;
            }
            LowStr = EmptyIfInf(k);
            HighStr = EmptyIfInf(v);
            if (LowStr.Length > 0 && HighStr.Length > 0)
            {
                if (LowStr.CompareTo(HighStr) > 0) return false;
            }
            string l = InfIfEmpty(LowStr);
            string h = InfIfEmpty(HighStr);
            description_ = $"{l} <= {Prop} < {h}";
            return true;
        }

        bool ParseBoolInput(string str)
        {
            if (!Menu.SplitSelector(str, out string k, out string v)) return false;
            // no bool range, just one value.
            bool value = false;
            if (v.Length != 0) return false;
            if (!bool.TryParse(k, out bool val))
            {
                // maybe it parses as int...
                if (!int.TryParse(k, out int val1)) return false;  // neither bool, nor int.
                if (val1 < 0 || val1 > 1) return false;
                value = val1 == 1;
            }
            else
            {
                value = val;
            }
            LowInt = HighInt = value ? 1 : 0;
            description_ = $"{Prop} == {value}";
            return true;
        }

        static bool ParseInt(string s, out bool is_inf, out int value)
        {
            if (s == "_")
            {
                is_inf = true;
                value = 0;
                return true;
            }
            is_inf = false;
            if (!int.TryParse(s, out value)) return false;
            if (value < 0) value = 0;
            return true;
        }

        static bool ParseFloat(string s, out bool is_inf, out float value)
        {
            if (s == "_")
            {
                is_inf = true;
                value = 0;
                return true;
            }
            is_inf = false;
            if (!float.TryParse(s, out value)) return false;
            if (value < 0) value = 0;
            float limit = ((float)(int.MaxValue)) / 10.0f - 1.0f;
            if (value > limit)
            {
                is_inf = true;
                value = 0;
            }
            return true;
        }

        bool ParseIntInput(string str)
        {
            if (!Menu.SplitSelector(str, out string k, out string v)) return false;
            if (!ParseInt(k, out bool is_inf_k, out int ik)) return false;
            if (!ParseInt(v, out bool is_inf_v, out int iv)) return false;
            LowInt = ik;
            HighInt = is_inf_v ? int.MaxValue : iv;
            if (LowInt > HighInt) return false;
            string l = ik.ToString();
            string h = is_inf_v ? "_" : iv.ToString();
            description_ = $"{l} <= {Prop} < {h}";
            return true;
        }

        bool ParseFloatInput(string str)
        {
            if (!Menu.SplitSelector(str, out string k, out string v)) return false;
            if (!ParseFloat(k, out bool is_inf_k, out float fk)) return false;
            if (!ParseFloat(v, out bool is_inf_v, out float fv)) return false;
            LowInt = is_inf_k ? 0 : (int)(fk * 10.0f);
            HighInt = is_inf_v ? int.MaxValue : (int)(fv * 10.0f);
            if (LowInt > HighInt) return false;
            float lf = (float)LowInt;
            lf /= 10f;
            float hf = (float)HighInt;
            hf /= 10f;
            string l = is_inf_k ? "0" : lf.ToString();
            string h = is_inf_v ? "_" : hf.ToString();
            description_ = $"{l} <= {Prop} < {h}";
            return true;
        }


        public Selector(IGlobals ig, IDB db)
        {
            ig_ = ig;
            db_ = db;
            string_props_.UnionWith(db_.GetFields(""));
            int_props_.UnionWith(db_.GetFields(0));
            all_props_.UnionWith(string_props_);
            all_props_.UnionWith(int_props_);
            props_menu_ = all_props_.ToList();
        }

        void BetweenInt(SortedList<int, SortedSet<int>> ix)
        {
            int start = InsPoint<int>(LowInt, ix);
            int end = InsPoint<int>(HighInt, ix);
            for (int i = start; i < end; ++i)
            {
                Accumulate(ix.GetValueAtIndex(i));
            }
        }

        void BetweenStr(SortedList<string, SortedSet<int>> ix)
        {
            int start = 0;
            if (LowStr.Length > 0) start = InsPoint<string>(LowStr, ix);
            int end = ix.Count();
            if (HighStr.Length > 0) end = InsPoint<string>(HighStr, ix);
            for (int i = start; i < end; ++i)
            {
                Accumulate(ix.GetValueAtIndex(i));
            }
        }

        private void QueryInt()
        {
            var ix = db_.GetIndex(Prop, 0);
            BetweenInt(ix);
        }

        private void Query()
        {
            OurIdSet = new();
            if (string_props_.Contains(Prop)) QueryString();
            else QueryInt();
        }

        void QueryString()
        {
            var ix = db_.GetIndex(Prop, "");
            if (regex_ == null)
            {
                BetweenStr(ix);
                return;
            }

        }

        public bool Run()
        {
            while (true)
            {
                int sel = Menu.Run(props_menu_, "prop to query :", out string prop);
                if (sel < 0) return false;
                if (sel < props_menu_.Count) prop = props_menu_[sel];
                if (!all_props_.Contains(prop)) return false;
                Prop = prop;
                Console.WriteLine($"Property selected: {Prop}");
                Console.WriteLine("One value for exact match or pattern.");
                Console.WriteLine("Two values for range");
                Console.WriteLine("_ (underscore) for no upper or lower limit in range.");
                Console.WriteLine("All numbers are positive.");
                List<string> menu = new();
                if (description_.Length > 0)
                {
                    Console.WriteLine(description_);
                    menu.Add("ExecuteQuery");
                }
                string prompt = "Enter value or range or pattern :";
                int sel2 = Menu.Run(menu, prompt, out string str);
                if (sel2 < 0) continue;
                if (sel2 == 1)
                {
                    ParseInput(str);
                    continue;
                }
                Query();
                return true;
            }

        }

        static public int InsPoint<T>(
            T v,
            SortedList<T, SortedSet<int>> ix
            ) where T : IComparable<T>
            // Binary Search.
        {
            int low = 0;
            int high = ix.Count;
            while (low < high)
            {
                int mid = (low + high) / 2;
                T midK = ix.GetKeyAtIndex(mid);
                int ord = v.CompareTo(midK);
                if (ord < 0) high = mid;
                else low = mid;
            }
            return low;
        }
        void PatternMatching(
            SortedList<string, SortedSet<int>> ix)
        {
            try { PatternMatchingWork(ix); }
            catch { }
        }
        void PatternMatchingWork(SortedList<string, SortedSet<int>> ix)
        {
            if (regex_ == null) return;
            foreach (KeyValuePair<string, SortedSet<int>> kv in ix)
            {
                var m = regex_.Match(kv.Key);
                if (!m.Success) continue;
                Accumulate(kv.Value);
            }
        }

    }
}