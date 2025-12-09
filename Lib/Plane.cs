using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    internal class Plane : Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "plane",
            BrandModel: "",
            Color: "red",
            RequiredSpaces: 18,
            IsSmall: false,
            WheelCount: 3,
            Persons: 2,
            Height: 3.5f,
            Width: 15f,
            Length: 15f,
            Depth: 0,
            Extra: "1engine"
        );
        static private VehicleRecord Force(VehicleRecord r)
        {
            r.OurId = 0;
            r.OfficialId = "";
            r.VehicleType = DefaultRecord.VehicleType;
            return r;
        }
        public Plane(IGlobals globals, VehicleRecord r) : base(globals, Force(r)) { }
        public Plane(IGlobals globals) : base(globals, DefaultRecord) { }
    }
}
