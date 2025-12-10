using Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
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
            var r = record_;
            try 
            {
                MustLoad(Filename(filename));
                return true;
            }
            catch { record_ = r;  return false; }
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
        private string Json()
        {
            return System.Text.Json.JsonSerializer.Serialize<VehicleRecord>(record_);
        }

        public bool Save(string? filename = null)
        {
            try { MustSave(Filename(filename)); return true; }
            catch { return false; }
        }
        public void MustSave(string filename)
        {
            System.IO.File.WriteAllText(filename, Json());
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
        public string Line()
        {
            string ourid = "New";
            if (record_.OurId != 0) ourid = $"{record_.OurId}";
            string oi = $"[{record_.OfficialId}]";
            if (string.IsNullOrEmpty(record_.OfficialId)) oi = "";
            string small = record_.IsSmall ? "I" : "W";
            float depth = (float)(Math.Abs(record_.Depth));
            string dim = ""
                + $"H{record_.Height:F1} W{record_.Width:F1} "
                + $"L{record_.Length:F1} D{depth:F1} "
                + $"{record_.WheelCount}W {record_.Persons}P"
                + $"{record_.RequiredSpaces}S{small}";
            return $"{ourid}: {record_.VehicleType} {oi} "
                + $"{record_.Color} {record_.BrandModel} {dim} {record_.Extra}";
        }
    }
}
