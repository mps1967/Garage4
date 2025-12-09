using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Moto:Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "Moto",
            BrandModel: "",
            Color: "Grey",
            RequiredSpaces: 1,
            IsSmall: false,
            WheelCount: 2,
            Persons: 1,
            Height: 1.5f,
            Width: 0.5f,
            Length: 1.9f,
            Depth: 0,
            Extra: ""
        );
        public Moto(IGlobals globals) : base(globals, DefaultRecord) { }
        static private VehicleRecord Force(VehicleRecord r)
        {
            r.OurId = 0;
            r.OfficialId = "";
            r.VehicleType = DefaultRecord.VehicleType;
            return r;
        }
        public Moto(IGlobals globals, VehicleRecord r) : base(globals, Force(r)) { }
    }
}
