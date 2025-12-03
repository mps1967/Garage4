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
        ISpace Garage { get; }
        IEnumerable<ISpace> Spaces();
        IEnumerable<IVehicle> Vehicles();
        ISpace? Park(IVehicle vehicle);
        IDB AsDataBase();
    }
}
