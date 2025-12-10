using Abstract;
using Lib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garage4
{
    internal class Garage
    {
        public string FileName { get; set; }
        public ISpace Root { get; private set; }

        private IGlobals ig_;
        private IRegistry ir_;

        public Garage(IGlobals ig, IRegistry ir) 
        {
            ig_ = ig;
            ir_ = ir;
            FileName = Path.Join([ig.DataRoot, "Application", "garage.json"]);
            Space r = new Space(GarageDef.Records["G"], Children("G"));
            r.SetParent(null);
            Root = r;
            foreach (var kv in SpaceList(Root)) SpaceDict[kv.Key] = kv.Value;
        }

        public SortedDictionary<string, ISpace> SpaceDict = new();  // space name to ISpace.
        public SortedDictionary<int, string> ParkedDict = new();  // OurId to space name.
        public string Park(IVehicle v)
        {
            ISpace? space = Root.Park(v);
            if (space == null) return "";
            ParkedDict[v.OurId()] = space.Name;
            return space.Name;
        }

        public string UnPark(IVehicle v)
        {
            if (!ParkedDict.TryGetValue(v.OurId(), out string? sname)) return "";
            if (string.IsNullOrEmpty(sname)) return "";
            if (!SpaceDict.TryGetValue(sname, out ISpace? sp)) return "";
            if (sp == null) return "";
            if (sp!.Unpark(v)) return sp.Name;
            return "";
        }

        static IEnumerable<Space> Children(string name)
            // Creates the children spaces and all spaces starting from the children of the root.
        {
            if (GarageDef.Tree.ContainsKey(name))
            {
                foreach (string c in GarageDef.Tree[name])
                {
                    SpaceRecord r = GarageDef.Records[c];
                    IEnumerable<Space> spaces = Children(c);
                    yield return new Space(r, spaces);
                }
            }
        }
        static IEnumerable<KeyValuePair<string, ISpace>> SpaceList(ISpace sp)
        {
            yield return new(sp.Name, sp);
            foreach (var s in sp.Spaces())
            {
                foreach (var kv in SpaceList(s))
                {
                    yield return kv;
                }
            }
        }
        public string Json()
        {
            return System.Text.Json.JsonSerializer.Serialize<SortedDictionary<int, string>>(ParkedDict);
        }
        public void MustSave() { System.IO.File.WriteAllText(FileName, Json()); }
        public bool Save()
        {
            try { MustSave(); return true; }
            catch { return false; }
        }
        public void MustLoad()
        {
            string? json = System.IO.File.ReadAllText(FileName);
            if (string.IsNullOrEmpty(json)) return;
            SortedDictionary<int, string>? parked_dict = 
                System
                .Text
                .Json
                .JsonSerializer
                .Deserialize<SortedDictionary<int, string>>(json!);
            if (parked_dict == null) return;
            ParkedDict = parked_dict;
            foreach (var kv in ParkedDict)
            {
                IVehicle? v = ir_.Get(kv.Key);
                if (v == null) continue;
                Space? sp = SpaceDict[kv.Value] as Space;
                if (sp == null) continue;
                sp.ForcePark(v);
            }
        }
        public bool Load()
        {
            try { MustLoad(); return true; }
            catch { return false; }
        }
    }
}
