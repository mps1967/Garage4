using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Bike:Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "Bike",
            BrandModel: "",
            Color: "Grey",
            RequiredSpaces: 1,
            IsSmall: true,
            WheelCount: 2,
            Persons: 1,
            Height: 1.5f,
            Width: 0.5f,
            Length: 1.9f,
            Depth: 0,
            Extra: ""
        );
        public Bike(IGlobals globals) : base(globals, DefaultRecord) { }
    }
}
