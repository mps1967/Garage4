using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public interface ISpace
    {
        string Name { get; }

        ISpace? Parent{ get; }
            // The immediately bigger "owning" space.
            // For example a space where a car can park lies within a space for a bus.
            // A place for a bus is composed of a number of car spaces.
        IEnumerable<ISpace> Spaces();
        IEnumerable<IVehicle> Vehicles();
        ISpace? Park(IVehicle vehicle);
        bool Unpark(IVehicle vehicle);
    }
}
