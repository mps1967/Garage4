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
            int_indexes_["IsSmall"] = new();
            int_indexes_["WheelCount"] = new();
            int_indexes_["Persons"] = new();
            int_indexes_["Height"] = new();
            int_indexes_["Width"] = new();
            int_indexes_["Length"] = new();
            int_indexes_["Depth"] = new();
            // TODO: look into Extra.
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
        private static int to_int(float f) => (int)(f * 10);
        public void AddVehicle(IVehicle vehicle)
        {
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

            string[] parts = vehicle.Record().Extra.Split(Menu.WHITESPACE, Menu.SPLITOPT);
            foreach (var p in parts)
            {
                Add("Extra", p, ourid);
            }
        }

        public SortedList<float, List<int>> GetFloatIndex(string field)
        {
            throw new NotImplementedException();
        }

        public SortedList<int, List<int>> GetIntIndex(string field)
        {
            throw new NotImplementedException();
        }

        public SortedList<string, List<int>> GetStringIndex(string field)
        {
            throw new NotImplementedException();
        }
    }
}
