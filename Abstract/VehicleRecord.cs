using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public record struct VehicleRecord
    (
        int OurId,
        string OfficialId,
        string VehicleType,
        string BrandModel,
        string Color,
        int RequiredSpaces,
        bool IsSmall,
        int WheelCount,
        int Persons,
        float Height,
        float Width,
        float Length,
        float Depth,
        string Extra  // extra properties not explicitly listed.
    );
}
