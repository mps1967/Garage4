using Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Car:Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "Car",
            BrandModel: "",
            Color: "Grey",
            RequiredSpaces: 1,
            IsSmall: false,
            WheelCount: 4,
            Persons: 5,
            Height: 1.5f,
            Width: 1.9f,
            Length: 4.5f,
            Depth: 0,
            Extra: ""
        );

        public Car(IGlobals globals) :base(globals, DefaultRecord) {}
        static private VehicleRecord Force(VehicleRecord r)
        {
            r.OurId = 0;
            r.OfficialId = "";
            r.VehicleType = DefaultRecord.VehicleType;
            r.IsSmall = false;
            return r;
        }
        public Car(IGlobals globals, VehicleRecord r) : base(globals, Force(r)) { }

    }
}
