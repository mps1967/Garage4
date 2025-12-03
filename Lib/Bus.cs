using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    internal class Bus:Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "Bus",
            BrandModel: "",
            Color: "Red",
            RequiredSpaces: 6,
            IsSmall: false,
            WheelCount: 4,
            Persons: 45,
            Height: 3.5f,
            Width: 2.5f,
            Length: 15f,
            Depth: 0,
            Extra: ""
        );
        public Bus(IGlobals globals) : base(globals, DefaultRecord) { }
    }
}
