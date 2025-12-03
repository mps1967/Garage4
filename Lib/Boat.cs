using Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    internal class Boat:Vehicle
    {
        public static readonly VehicleRecord DefaultRecord = new VehicleRecord
        (
            OurId: 0,
            OfficialId: "",
            VehicleType: "boat",
            BrandModel: "",
            Color: "white",
            RequiredSpaces: 1,
            IsSmall: false,
            WheelCount: 0,
            Persons: 5,
            Height: 3.5f,
            Width: 2.5f,
            Length: 15f,
            Depth: 2f,
            Extra: ""
        );
        public Boat(IGlobals globals) : base(globals, DefaultRecord) { }
    }
