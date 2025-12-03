using Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Vehicle(IGlobals globals, VehicleRecord record) : IVehicle
    {
        protected IGlobals globals_ = globals;
        protected VehicleRecord record_ = record;

        public bool Load(string? filename = null)
        {
            try 
            {
                MustLoad(Filename(filename));
                return true;
            }
            catch { return false; }
        }

        private void MustLoad(string fn)
        {
            string json = System.IO.File.ReadAllText(fn);
            if (string.IsNullOrEmpty(json)) return;
            record_ = System.Text.Json.JsonSerializer.Deserialize<VehicleRecord>(json!);
        }

        public int OurId()
        {
            return record_.OurId;
        }

        public VehicleRecord Record()
        {
            return record_;
        }

        public bool Save(string? filename = null)
        {
            throw new NotImplementedException();
        }
        private string Filename(string? filename)
        {
            if (filename != null) return filename!;
            if (record_.OurId != 0) return Path.Join([
                globals_.DataRoot,
                "Vehicles",
                $"{record_.OurId}.json"]);
            return Path.Join([
                Environment.GetEnvironmentVariable("TEMP"),
                $"{globals_.NewNumber()}.json"]);
        }
    }
}
