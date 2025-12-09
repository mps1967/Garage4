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
            g.Load();
            VehicleRegistry vr = new(globals);
            Garage g = new(globals, vr);
            IDB db = vr.Database;
            Selector sel = new(globals, db);
            sel.Run();
            List<string> menu = new();
            List<int> ourids = new();
            foreach (int o in sel.OurIdSet)
            {
                var l = vr.Line(o);
                if (string.IsNullOrEmpty(l)) continue;
                menu.Add(l);
                ourids.Add(o);
            }
            int ourid = 0;
            int sel2 = Menu.Run(menu, ":", out string text);
            if (sel2 > 0 && sel2 < menu.Count) ourid = ourids[sel2];
            ourid = vr.Run(ourid);
            IVehicle? v = vr.Get(ourid);
            Debug.Assert(v != null);
            string space = g.Park(v!);
            Console.WriteLine($"{v!.Line()} parked at {space}");
            g.Save();
            g.Load();


        }
    }
}
