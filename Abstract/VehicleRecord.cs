using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public record struct VehicleRecord
    (
        int OurId = 0,
        string OfficialId = "",
        string VehicleType = "",
        string BrandModel = "",
        string Color = "Grey",
        int RequiredSpaces = 1,
        bool IsSmall = false,
        int WheelCount = 0,
        int Persons = 1,
        float Height = 0,
        float Width = 0,
        float Length = 0,
        float Depth = 0,
        string Extra = ""  // extra properties not explicitly listed.
    );
}
