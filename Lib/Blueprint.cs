using Abstract;
using BPSet = System.Collections.Generic.SortedSet<string>;
namespace Lib
{
    public class Blueprint (IGlobals globals, string name) : IBlueprint
    {
        // The 3 available blueprints.
        public static readonly string VEHTYPES = "VehicleTypes";
        public static readonly string BRANDMODEL = "BrandModel";
        public static readonly string COLOR = "Color";
        public string Name { get; } = name;
        public BPSet Set { get; private set; } = new();

        private IGlobals globals_ = globals;
        private string filename_ = Path.Join([globals.DataRoot, "Application", $"{name}.json"]);
        public bool Load()
        {
            try { MustLoad(); return true; }
            catch { return false; }
        }
        public void MustLoad()
        {
            string? json =System.IO.File.ReadAllText(filename_);
            if (string.IsNullOrEmpty(json)) return;
            BPSet? s = System.Text.Json.JsonSerializer.Deserialize<BPSet>(json!);
            if (s == null) return;
            Set = s!;
        }
        private string Json() { return System.Text.Json.JsonSerializer.Serialize<BPSet>(Set); }

        public void MustSave() { System.IO.File.WriteAllText(filename_, Json()); }
        public bool Save()
        {
            try { MustSave(); return true; }
            catch { return false; }
        }

    }
}
