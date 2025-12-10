using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class DB : IDB
    {
        public int Count { get; set; } = 0;
        private SortedDictionary<
            string,  // string field name
            SortedList<
                string,  // string field value
                SortedSet<int>  // OurId where the field has the value.
                >
            > string_indexes_ = new();
        private SortedDictionary<
            string,  // string field name
            SortedList<
                int,  // string field value
                SortedSet<int>  // OurId where the field has the value.
                >
            > int_indexes_ = new();

        public DB()
        {
            string_indexes_["OfficialId"] = new();
            string_indexes_["VehicleType"] = new();
            string_indexes_["BrandModel"] = new();
            string_indexes_["Color"] = new();
            string_indexes_["Line"] = new();
            int_indexes_["IsSmall"] = new();
            int_indexes_["WheelCount"] = new();
            int_indexes_["RequiredSpaces"] = new();
            int_indexes_["Persons"] = new();
            int_indexes_["Height"] = new();
            int_indexes_["Width"] = new();
            int_indexes_["Length"] = new();
            int_indexes_["Depth"] = new();
            // TODO: look into Extra.
        }

        public DB(DB parent, SortedSet<int> filter)
        {
            foreach (var kv in parent.string_indexes_) InitStringIndex(kv.Key, kv.Value, filter);
            foreach (var kv in parent.int_indexes_) InitIntIndex(kv.Key, kv.Value, filter);
        }

        void InitStringIndex(
            string field_name,
            SortedList<string, SortedSet<int>> parent_sl,
            SortedSet<int> filter)
        {
            string_indexes_[field_name] = new();
            foreach (var kv in parent_sl) InitStringSortedList(field_name, kv.Key, kv.Value, filter);
        }
        void InitIntIndex(
            string field_name,
            SortedList<int, SortedSet<int>> parent_sl,
            SortedSet<int> filter)
        {
            int_indexes_[field_name] = new();
            foreach (var kv in parent_sl) InitIntSortedList(field_name, kv.Key, kv.Value, filter);
        }

        void InitStringSortedList(string field_name, string value, SortedSet<int> parent_idset, SortedSet<int> filter)
        {
            string_indexes_[field_name][value] = new();
            string_indexes_[field_name][value].UnionWith(parent_idset);
            string_indexes_[field_name][value].IntersectWith(filter);
        }
        void InitIntSortedList(string field_name, int value, SortedSet<int> parent_idset, SortedSet<int> filter)
        {
            int_indexes_[field_name][value] = new();
            int_indexes_[field_name][value].UnionWith(parent_idset);
            int_indexes_[field_name][value].IntersectWith(filter);
        }

        void AddL2(SortedList<string, SortedSet<int>> sl2, string field_value, int ourid)
        {
            if (!sl2.ContainsKey(field_value)) sl2[field_value] = new();
            sl2[field_value].Add(ourid);
        }
        void Add(string field_name, string field_value, int ourid)
        {
            AddL2(string_indexes_[field_name], field_value, ourid);
        }
        void AddL2(SortedList<int, SortedSet<int>> sl2, int field_value, int ourid)
        {
            if (!sl2.ContainsKey(field_value)) sl2[field_value] = new();
            sl2[field_value].Add(ourid);
        }
        void Add(string field_name, int field_value, int ourid)
        {
            AddL2(int_indexes_[field_name], field_value, ourid);
        }
        private static int to_int(bool b) { return b ? 1 : 0; }
        private static int to_int(float f) => Math.Abs((int)(f * 10));
        public void AddVehicle(IVehicle vehicle)
        {
            Count += 1;
            int ourid = vehicle.OurId();
            Add("OfficialId", vehicle.Record().OfficialId, ourid);
            Add("VehicleType", vehicle.Record().VehicleType, ourid);
            Add("BrandModel", vehicle.Record().BrandModel, ourid);
            Add("Color", vehicle.Record().Color, ourid);
            Add("RequiredSpaces", vehicle.Record().RequiredSpaces, ourid);
            Add("IsSmall", to_int(vehicle.Record().IsSmall), ourid);
            Add("WheelCount", vehicle.Record().WheelCount, ourid);
            Add("Persons", vehicle.Record().Persons, ourid);
            Add("Height", to_int(vehicle.Record().Height), ourid);
            Add("Length", to_int(vehicle.Record().Length), ourid);
            Add("Depth", to_int(vehicle.Record().Depth), ourid);

            string[] parts = vehicle.Line().Split(Menu.WHITESPACE, Menu.SPLITOPT);
            foreach (var p in parts)
            {
                Add("Line", p, ourid);  // includes extra.
            }
        }

        public IEnumerable<string> GetFields(string unused) { return string_indexes_.Keys;  }
        public IEnumerable<string> GetFields(int unused) { return int_indexes_.Keys; }

        public SortedList<int, SortedSet<int>> GetIndex(string field, int unused)
        {
            return int_indexes_[field];
        }
        public SortedList<string, SortedSet<int>> GetIndex(string field, string unused)
        {
            return string_indexes_[field];
        }

        public void Load(IGlobals ig)
        {
            string vdir = Path.Join(ig.DataRoot, "Vehicles");
            foreach (var f in Directory.EnumerateFiles(vdir))
            {
                if (!f.EndsWith(".json")) continue;
                string fn = Path.GetFileName(f);
                string numf = fn.Substring(0, fn.Length - 5);
                if (!int.TryParse(numf, out int ourid)) continue;
                VehicleRecord r = new();
                r.OurId = ourid;
                Vehicle v = new(ig, r);
                if (!v.Load()) continue;
                AddVehicle(v);
            }
        }
    }
}
