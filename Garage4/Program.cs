using Abstract;
using Lib;
namespace Garage4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Globals globals = new();
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

        }
    }
}
