using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public record struct SpaceRecord
        // Representattion of a full (i.e. car) space or a set of car spaces.
        // if this is a leaf it is a car space.
        // otherwise it is a collection of spaces for a bigger vehicle built
        // out of a number of car spaces (leaves).
    (
        string Name,
            // A space's name has the name of its parent space as prefix.
            // A leaf's last component is is just a number, without letters.
            // an enclosing space with more leaves has its name formed as a
            // letter and a number.
            /* Size restrictions:
             * * Only apply to a vehicle intending to park in the full space,
             * * Do not apply to a vehicle intending to park in a subspace.
             */
        float Height = 0,
            // A taller vehicle cannot park.
            // if Depth = 0, the vehicle's depth is added to its height.
        float Width = 0,
            // a wider vehicle cannot park.
        float Length = 0,
            // a longer vehicle cannot park.
        float Depth = 0,  // negative for boat spaces.
            
        int SmallCount = 0
    );
}
