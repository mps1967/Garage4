using Abstract;
using Lib;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace Garage4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Globals globals = new();
            Directory.CreateDirectory(globals.DataRoot);
            string appdir = Path.Join(globals.DataRoot, "Application");
            Directory.CreateDirectory(appdir);
            string vehicle_dir = Path.Join(globals.DataRoot, "Vehicles");
            Directory.CreateDirectory(vehicle_dir);
            Blueprint bpVehicleType = new(globals, "VehicleTypes");
            globals.Add(bpVehicleType);
            bpVehicleType.Load();
            Blueprint bpBrandModel = new(globals, "BrandModel");
            globals.Add(bpBrandModel);
            bpBrandModel.Load();
            Blueprint bpColor = new(globals, "Color");
            globals.Add(bpColor);
            bpColor.Load();
            foreach (string c in Enum.GetNames(typeof(ConsoleColor)))
            {
                bpColor.Set.Add(c);
            }
            bpColor.Save();
            VehicleRegistry vr = new(globals);
            Garage g = new(globals, vr);
            g.Load();
            Console.WriteLine($"{g.ParkedDict.Count} vehicles in the garage.");
            IDB db = vr.Database;
            DB? _db = db as DB;
            int count = _db == null ? 0 : _db.Count;
            Console.WriteLine($"Indexed {count} vehicles.");
            Selector sel = new(globals, db);
            while (!sel.Run());  // yes, till a query is run!
            List<string> menu = new();
            List<int> ourids = new();
            Console.WriteLine($"{sel.OurIdSet.Count} vehicles.");
            foreach (int o in sel.OurIdSet)
            {
                var l = vr.Line(o);
                if (string.IsNullOrEmpty(l)) continue;
                menu.Add(l);
                ourids.Add(o);
            }
            int ourid = 0;
            if (menu.Count > 0)
            {
                int sel2 = Menu.Run(menu, ":", out string text);
                if (sel2 > 0 && sel2 < menu.Count) ourid = ourids[sel2];
            }
            ourid = vr.Run(ourid);
            IVehicle? v = vr.Get(ourid);
            Debug.Assert(v != null);
            string space = g.Park(v!);
            Console.WriteLine($"{v!.Line()} parked at {space}");
            g.Save();
            g.Load();
            Console.WriteLine($"{g.ParkedDict.Count} vehicles in the garage.");


        }
    }
}
