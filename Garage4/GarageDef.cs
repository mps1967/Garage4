using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Garage4
{
    internal static class GarageDef
        // This will evventually be loaded from a config file.
        // This class defines the Garage from a collection os Spacerecords.
    {
       public static readonly SortedDictionary<string, SpaceRecord> Records;
       public static readonly SortedDictionary<string, List<string>> Tree;

        private static SpaceRecord BoatSpace(string name)
            { return new SpaceRecord(name, 10f, 3f, 10f, -2f); }
        private static SpaceRecord CarSpace(string name)
        { return new SpaceRecord(name, 4f, 2.5f, 5f, 0); }
        private static SpaceRecord BusSpace(string name)
        { return new SpaceRecord(name, 4f, 3f, 15f, 0); }
        private static SpaceRecord PlaneSpace(string name)
        { return new SpaceRecord(name, 4f, 15f, 15f, 0); }

        private static void AddCarSpaces(string name, int num)
        {
            if (!Tree.ContainsKey(name)) Tree.Add(name, new());
            for (int i = 0; i < num; ++i)
            {
                string cn = $"{name}.{i}";
                Records[cn] = CarSpace(cn);
                Tree[name].Add(cn);
            }
        }
        private static void AddBusSpaces(string name, int num)
        {
            if (!Tree.ContainsKey(name)) Tree.Add(name, new());
            for (int i = 0; i < num; ++i)
            {
                string bn = $"{name}.B{i}";
                Records[bn] = BusSpace(bn);
                Tree[name].Add(bn);
                AddCarSpaces(bn, 6);
            }
        }
        private static void AddPlaneSpaces(string name, int num)
        {
            if (!Tree.ContainsKey(name)) Tree.Add(name, new());
            for (int i = 0; i < num; ++i)
            {
                string pn = $"{name}.P{i}";
                Records[pn] = PlaneSpace(pn);
                Tree[name].Add(pn);
                AddBusSpaces(pn, 3);
            }
        }

        private static void AddGarage(string name)
        {
            if (!Tree.ContainsKey(name)) Tree.Add(name, new());
            Records[name] =new SpaceRecord(name);
            AddPlaneSpaces(name, 2); // 2* 18 = 36
            AddBusSpaces(name, 2);  // 2*6 = 12
            AddCarSpaces(name, 3);
            for (int i = 0; i < 3; ++i)
            {
                string bbn = $"{name}.{i + 3}";
                Records[bbn] = BoatSpace(bbn);  // 0-2 are cars. 3-6 are boats.
                Tree[name].Add(bbn);
            }
        }

        static GarageDef()
        {
            Records = new();
            Tree = new();
            AddGarage("G");
            AddGarage("G.A");
            AddGarage("G.B");
            Tree["G"].Add("G.A");
            Tree["G"].Add("G.B");
        }

        public static IEnumerable<SpaceRecord> Subspaces(string name)
        {
            foreach  (var sub in Tree[name])
            {
                yield return Records[sub];
            }
        }
    }
}